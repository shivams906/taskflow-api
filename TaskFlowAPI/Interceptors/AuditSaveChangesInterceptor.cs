using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore;
using TaskFlowAPI.Models;
using System.Text.Json;
using TaskFlowAPI.Interfaces;
using Microsoft.Extensions.Configuration.UserSecrets;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using TaskFlowAPI.Models.Enum;

namespace TaskFlowAPI.Interceptors
{
    public class AuditSaveChangesInterceptor : SaveChangesInterceptor
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly ICurrentSessionProvider _currentSessionProvider;

        public AuditSaveChangesInterceptor(IHttpContextAccessor httpContextAccessor, ICurrentSessionProvider currentSessionProvider)
        {
            _httpContextAccessor = httpContextAccessor;
            _currentSessionProvider = currentSessionProvider;
        }

        public override ValueTask<InterceptionResult<int>> SavingChangesAsync(
            DbContextEventData eventData,
            InterceptionResult<int> result,
            CancellationToken cancellationToken = default)
        {
            var context = eventData.Context;
            if (context == null) return base.SavingChangesAsync(eventData, result, cancellationToken);

            var userId = _currentSessionProvider.GetUserId() ?? throw new Exception("User ID not found");

            var entries = context.ChangeTracker.Entries()
                .Where(e => e.State is EntityState.Added or EntityState.Modified or EntityState.Deleted)
                .Where(e => e.Entity is not AuditLog)
                .Select(x => CreateAuditLog(userId, x))
                .ToList();

            context.Set<AuditLog>().AddRange(entries);

            return base.SavingChangesAsync(eventData, result, cancellationToken);
        }

        private AuditLog CreateAuditLog(Guid userId, EntityEntry entry)
        {
            var auditEntry = new AuditLog
            {
                TableName = entry.Entity.GetType().Name,
                UserId = userId,
            };
            
            // Primary key
            var keyNames = entry.Properties
                .Where(p => p.Metadata.IsPrimaryKey())
                .Select(p => $"{p.Metadata.Name}={p.CurrentValue}")
                .ToList();
            auditEntry.KeyValues = string.Join(",", keyNames);

            var oldValues = new Dictionary<string, object?>();
            var newValues = new Dictionary<string, object?>();
            var changedColumns = new List<string>();

            foreach (var prop in entry.Properties)
            {
                if (prop.IsTemporary) continue;
                // Filter properties that should not appear in the audit list
                if (prop.Metadata.Name.Equals("PasswordHash")) continue;

                switch (entry.State)
                {
                    case EntityState.Added:
                        newValues[prop.Metadata.Name] = prop.CurrentValue;
                        auditEntry.AuditType = AuditType.Create;
                        break;
                    case EntityState.Deleted:
                        oldValues[prop.Metadata.Name] = prop.OriginalValue;
                        auditEntry.AuditType = AuditType.Delete;
                        break;
                    case EntityState.Modified:
                        if (!Equals(prop.OriginalValue, prop.CurrentValue))
                        {
                            changedColumns.Add(prop.Metadata.Name);
                            oldValues[prop.Metadata.Name] = prop.OriginalValue;
                            newValues[prop.Metadata.Name] = prop.CurrentValue;
                            auditEntry.AuditType = AuditType.Update;
                        }
                        break;
                }
            }

            //foreach (var reference in entry.References.Where(x => x.IsModified))
            //{
            //    var referenceName = reference.EntityEntry.Entity.GetType().Name;
            //    changedColumns.Add(referenceName);
            //}

            //foreach (var navigation in entry.Navigations.Where(x => x.Metadata.IsCollection && x.IsModified))
            //{
            //    if (navigation.CurrentValue is not IEnumerable<object> enumerable)
            //    {
            //        continue;
            //    }

            //    var collection = enumerable.ToList();
            //    if (collection.Count == 0)
            //    {
            //        continue;
            //    }

            //    var navigationName = collection.First().GetType().Name;
            //    changedColumns.Add(navigationName);
            //}

            auditEntry.ChangedColumns = string.Join(",", changedColumns);
            auditEntry.OldValues = oldValues.Any() ? JsonSerializer.Serialize(oldValues) : null;
            auditEntry.NewValues = newValues.Any() ? JsonSerializer.Serialize(newValues) : null;
            return auditEntry;
        }
    }
}
