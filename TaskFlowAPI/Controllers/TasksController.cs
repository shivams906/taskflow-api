using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using TaskFlowAPI.Data;
using TaskFlowAPI.DTOs;
using TaskFlowAPI.Interfaces;
using TaskFlowAPI.Models;
using TaskFlowAPI.Models.Enums;

namespace TaskFlowAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class TasksController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly IMapper _mapper;
        private readonly ICurrentSessionProvider _currentSessionProvider;

        public TasksController(AppDbContext context, IMapper mapper, ICurrentSessionProvider currentSessionProvider)
        {
            _context = context;
            _mapper = mapper;
            _currentSessionProvider = currentSessionProvider;
        }

        // ✅ 1. Get all tasks for a project (for admins)
        [HttpGet("project/{projectId}")]
        public async Task<IActionResult> GetTasksForProject(Guid projectId)
        {
            var userId = _currentSessionProvider.GetUserId() ?? throw new Exception("User ID not found");

            var isAdmin = await _context.ProjectUsers
                .AnyAsync(pu => pu.ProjectId == projectId && pu.UserId == userId && pu.Role == ProjectRole.Admin);

            if (!isAdmin)
                return Forbid("Only admins can view project tasks.");

            var tasks = await _context.Tasks
                .Include(t => t.AssignedTo)
                .Include(t => t.CreatedBy)
                .Where(t => t.ProjectId == projectId)
                .Select(t => _mapper.Map<TaskDto>(t))
                .ToListAsync();

            //var dtos = _mapper.Map<List<TaskDto>>(tasks);

            return Ok(tasks);
        }

        // ✅ 2. Get tasks assigned to logged-in user
        [HttpGet("my")]
        public async Task<IActionResult> GetMyTasks()
        {
            var userId = _currentSessionProvider.GetUserId() ?? throw new Exception("User ID not found");

            var tasks = await _context.Tasks
                .Include(t => t.Project)
                .Where(t => t.AssignedToId == userId)
                .Select(t => _mapper.Map<TaskDto>(t))
                .ToListAsync();

            //var dtos = _mapper.Map<List<TaskDto>>(tasks);

            return Ok(tasks);
        }

        [HttpGet("{taskId}")]
        public async Task<IActionResult> GetTaskById(Guid taskId)
        {
            var userId = _currentSessionProvider.GetUserId() ?? throw new Exception("User ID not found");
            var task = await _context.Tasks
                .Include(t => t.AssignedTo)
                .FirstOrDefaultAsync(t => t.Id == taskId);
            if (task == null)
                return NotFound("Task not found.");
            // Check if the user is assigned to the task or an admin
            bool isAdmin = await _context.ProjectUsers
                .AnyAsync(pu => pu.ProjectId == task.ProjectId && pu.UserId == userId && pu.Role == ProjectRole.Admin);
            bool isAssigned = task.AssignedToId == userId;
            if (!isAdmin && !isAssigned)
                return Forbid("Not authorized to view this task.");
            var dto = _mapper.Map<TaskDto>(task);
            return Ok(dto);
        }

        // ✅ 3. Create a task (admin only)
        [HttpPost]
        public async Task<IActionResult> CreateTask([FromBody] CreateTaskDto task)
        {
            var userId = _currentSessionProvider.GetUserId() ?? throw new Exception("User ID not found");

            // Check admin access
            var isAdmin = await _context.ProjectUsers
                .AnyAsync(pu => pu.ProjectId == task.ProjectId && pu.UserId == userId && pu.Role == ProjectRole.Admin);

            if (!isAdmin)
                return Forbid("Only project admins can create tasks.");

            var taskItem = _mapper.Map<TaskItem>(task);

            taskItem.Id = Guid.NewGuid();
            taskItem.CreatedById = userId;
            taskItem.CreatedAtUtc = DateTime.UtcNow;

            _context.Tasks.Add(taskItem);
            await _context.SaveChangesAsync();

            return Ok(task);
        }

        // ✅ 4. Update task status (assigned user or admin)
        [HttpPut("{taskId}/status")]
        public async Task<IActionResult> UpdateStatus(Guid taskId, [FromBody] UpdateTaskStatusDto dto)
        {
            var userId = _currentSessionProvider.GetUserId() ?? throw new Exception("User ID not found");
            var task = await _context.Tasks.Include(t => t.Project).FirstOrDefaultAsync(t => t.Id == taskId);

            if (task == null)
                return NotFound("Task not found.");

            bool isAdmin = await _context.ProjectUsers
                .AnyAsync(pu => pu.ProjectId == task.ProjectId && pu.UserId == userId && pu.Role == ProjectRole.Admin);

            bool isAssignedUser = task.AssignedToId == userId;

            if (!isAdmin && !isAssignedUser)
                return Forbid("Not allowed to update this task.");

            // Convert string to enum
            if (!Enum.TryParse<TaskItemStatus>(dto.NewStatus, true, out var statusEnum))
                return BadRequest("Invalid task status.");

            task.Status = statusEnum;
            task.UpdatedById = userId;
            task.UpdatedAtUtc = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            return Ok("Task status updated.");
        }

        // ✅ 5. Add time log (assigned user only)
        [HttpPost("{taskId}/log-time")]
        public async Task<IActionResult> LogTime(Guid taskId, [FromBody] CreateTimeLogDto dto)
        {
            var userId = _currentSessionProvider.GetUserId() ?? throw new Exception("User ID not found");

            var task = await _context.Tasks.FirstOrDefaultAsync(t => t.Id == taskId);

            if (task == null || task.AssignedToId != userId)
                return Forbid("Only the assigned user can log time.");

            var log = _mapper.Map<TaskTimeLog>(dto);

            log.Id = Guid.NewGuid();
            log.TaskItemId = taskId;
            log.UserId = userId;
            log.CreatedById = userId;
            log.CreatedAtUtc = DateTime.UtcNow;

            _context.TaskTimeLogs.Add(log);
            await _context.SaveChangesAsync();

            return Ok("Time log saved.");
        }

        [HttpGet("{taskId}/logs")]
        public async Task<IActionResult> GetTimeLogsForTask(Guid taskId, [FromQuery] bool onlyMine = false)
        {
            var userId = _currentSessionProvider.GetUserId() ?? throw new Exception("User ID not found");

            var task = await _context.Tasks
                .Include(t => t.Project)
                .FirstOrDefaultAsync(t => t.Id == taskId);

            if (task == null)
                return NotFound("Task not found.");

            // Only project admins or assigned user can view logs
            bool isAdmin = await _context.ProjectUsers
                .AnyAsync(pu => pu.ProjectId == task.ProjectId && pu.UserId == userId && pu.Role == ProjectRole.Admin);
            bool isAssigned = task.AssignedToId == userId;

            if (!isAdmin && !isAssigned)
                return Forbid("Not authorized to view logs for this task.");

            var logsQuery = _context.TaskTimeLogs
               .Where(t => t.TaskItemId == taskId);

            if (onlyMine)
            {
                logsQuery = logsQuery.Where(t => t.UserId == userId);
            }

            var logs = await logsQuery
                .Include(l=>l.User)
                .OrderByDescending(l => l.StartTime)
                .Select(l=>_mapper.Map<TimeLogDto>(l))
                .ToListAsync();

            return Ok(logs);
        }


        [HttpPut("{taskId}")]
        public async Task<IActionResult> UpdateTask(Guid taskId, [FromBody] CreateTaskDto updated)
        {
            var userId = _currentSessionProvider.GetUserId() ?? throw new Exception("User ID not found");

            var task = await _context.Tasks
                .Include(t => t.Project)
                .FirstOrDefaultAsync(t => t.Id == taskId);

            if (task == null)
                return NotFound("Task not found.");

            bool isAdmin = await _context.ProjectUsers
                .AnyAsync(pu => pu.ProjectId == task.ProjectId && pu.UserId == userId && pu.Role == ProjectRole.Admin);

            if (!isAdmin)
                return Forbid("You don't have permission to update this task.");

            // Update fields
            task.Title = updated.Title;
            task.Description = updated.Description;
            task.UpdatedById = userId;
            task.UpdatedAtUtc = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            var dto = _mapper.Map<TaskDto>(task);
            return Ok(dto);
        }

        [HttpDelete("{taskId}")]
        public async Task<IActionResult> DeleteTask(Guid taskId)
        {
            var userId = _currentSessionProvider.GetUserId() ?? throw new Exception("User ID not found");

            var task = await _context.Tasks
                .Include(t => t.Project)
                .FirstOrDefaultAsync(t => t.Id == taskId);

            if (task == null)
                return NotFound("Task not found.");

            bool isAdmin = await _context.ProjectUsers
                .AnyAsync(pu => pu.ProjectId == task.ProjectId && pu.UserId == userId && pu.Role == ProjectRole.Admin);

            if (!isAdmin)
                return Forbid("Only project admins can delete tasks.");

            _context.Tasks.Remove(task);
            await _context.SaveChangesAsync();

            return Ok("Task deleted.");
        }

        [HttpPut("{taskId}/assign")]
        public async Task<IActionResult> AssignTask(Guid taskId, [FromBody] AssignUserToTaskDto dto)
        {
            var userId = _currentSessionProvider.GetUserId() ?? throw new Exception("User ID not found");

            var task = await _context.Tasks
                .Include(t => t.Project)
                .FirstOrDefaultAsync(t => t.Id == taskId);

            if (task == null)
                return NotFound("Task not found.");

            bool isAdmin = await _context.ProjectUsers
                .AnyAsync(pu => pu.ProjectId == task.ProjectId && pu.UserId == userId && pu.Role == ProjectRole.Admin);

            if (!isAdmin)
                return Forbid("Only project admins can assign tasks.");

            var targetUser = await _context.Users.FindAsync(dto.UserId);
            if (targetUser == null)
                return BadRequest("User not found.");

            task.AssignedToId = dto.UserId;
            task.UpdatedById = userId;
            task.UpdatedAtUtc = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            return Ok("Task assigned.");
        }

        [HttpPut("{taskId}/unassign")]
        public async Task<IActionResult> UnassignTask(Guid taskId)
        {
            var userId = _currentSessionProvider.GetUserId() ?? throw new Exception("User ID not found");

            var task = await _context.Tasks
                .Include(t => t.Project)
                .FirstOrDefaultAsync(t => t.Id == taskId);

            if (task == null)
                return NotFound("Task not found.");

            var isCreator = task.Project.CreatedById == userId;
            var isAdmin = await _context.ProjectUsers
                .AnyAsync(pu => pu.ProjectId == task.ProjectId && pu.UserId == userId && pu.Role == ProjectRole.Admin);

            if (!isCreator && !isAdmin)
                return Forbid("Only project creator or admins can unassign this task.");

            task.AssignedToId = null;
            task.UpdatedById = userId;
            task.UpdatedAtUtc = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            return Ok("Task unassigned.");
        }


        [HttpGet("statuses")]
        public IActionResult GetStatuses()
        {
            var statuses = Enum.GetNames(typeof(TaskItemStatus));
            return Ok(statuses);
        }

    }
}
