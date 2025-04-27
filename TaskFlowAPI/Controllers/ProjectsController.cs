using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using TaskFlowAPI.Data;
using TaskFlowAPI.DTOs;
using TaskFlowAPI.Models;
using TaskFlowAPI.Models.Enums;

namespace TaskFlowAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class ProjectsController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly IMapper _mapper;

        public ProjectsController(AppDbContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        [HttpGet]
        public async Task<IActionResult> GetMyProjects()
        {
            var userId = GetUserId();

            var projects = await _context.Projects
                .Where(p => p.CreatedById == userId ||
                            p.ProjectUsers.Any(pu => pu.UserId == userId && pu.Role == ProjectRole.Admin))
                .Include(p => p.ProjectUsers)!
                    .ThenInclude(pu => pu.User)
                .Include(p => p.CreatedBy)
                .Select(p => _mapper.Map<ProjectDto>(p))
                .ToListAsync();

            return Ok(projects);
        }

        [HttpPost]
        public async Task<IActionResult> CreateProject([FromBody] CreateProjectDto dto)
        {
            var userId = GetUserId();
            var project = _mapper.Map<Project>(dto);
            project.Id = Guid.NewGuid();
            project.CreatedById = userId;
            
            _context.Projects.Add(project);

            // Add creator as Admin
            var projectUser = new ProjectUser
            {
                ProjectId = project.Id,
                UserId = userId,
                Role = ProjectRole.Admin
            };

            _context.ProjectUsers.Add(projectUser);

            await _context.SaveChangesAsync();
            await _context.Entry(project).Reference(p => p.CreatedBy).LoadAsync();
            var projectDto = _mapper.Map<ProjectDto>(project);
            return Ok(projectDto);
        }

        [HttpPost("add-admin")]
        public async Task<IActionResult> AddProjectAdmin([FromBody] AddProjectAdminDto dto)
        {
            var userId = GetUserId();

            // Check if current user is the creator of the project
            var project = await _context.Projects
                .FirstOrDefaultAsync(p => p.Id == dto.ProjectId && p.CreatedById == userId);

            if (project == null)
                return Forbid("Only the project creator can add admins.");

            // Check if user already added
            bool alreadyAdmin = await _context.ProjectUsers
                .AnyAsync(pu => pu.ProjectId == dto.ProjectId && pu.UserId == dto.UserId);

            if (alreadyAdmin)
                return BadRequest("User is already an admin of this project.");

            var projectUser = _mapper.Map<ProjectUser>(dto);

            _context.ProjectUsers.Add(projectUser);
            await _context.SaveChangesAsync();

            return Ok("Admin added successfully.");
        }

        [HttpDelete("remove-admin")]
        public async Task<IActionResult> RemoveAdmin([FromBody] AddProjectAdminDto dto)
        {
            var currentUserId = GetUserId();

            // Only creator can remove admin
            var project = await _context.Projects
                .FirstOrDefaultAsync(p => p.Id == dto.ProjectId && p.CreatedById == currentUserId);

            if (project == null)
                return Forbid("Only the project creator can remove admins.");

            var projectUser = await _context.ProjectUsers
                .FirstOrDefaultAsync(pu => pu.ProjectId == dto.ProjectId && pu.UserId == dto.UserId && pu.Role == ProjectRole.Admin);

            if (projectUser == null)
                return NotFound("User is not an admin of this project.");

            _context.ProjectUsers.Remove(projectUser);
            await _context.SaveChangesAsync();

            return Ok("Admin removed.");
        }


        [HttpGet("{id}")]
        public async Task<IActionResult> GetProjectById(Guid id)
        {
            var userId = GetUserId();

            var project = await _context.Projects
                .Include(p => p.ProjectUsers)!
                    .ThenInclude(pu => pu.User)
                .Include(p => p.Tasks)
                .Include(p => p.CreatedBy)
                .FirstOrDefaultAsync(p =>
                    p.Id == id &&
                    (p.CreatedById == userId || p.ProjectUsers.Any(pu => pu.UserId == userId && pu.Role == ProjectRole.Admin)));

            if (project == null)
                return NotFound("Project not found or you do not have access.");

            var projectDto = _mapper.Map<ProjectDto>(project);

            return Ok(projectDto);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateProject(Guid id, [FromBody] CreateProjectDto updated)
        {
            var userId = GetUserId();

            var project = await _context.Projects
                .Include(p => p.CreatedBy)
                .FirstOrDefaultAsync(p => p.Id == id && p.CreatedById == userId);

            if (project == null)
                return Forbid("Only the project creator can update this project.");

            project.Title = updated.Title;
            project.Description = updated.Description;

            await _context.SaveChangesAsync();

            var projectDto = _mapper.Map<ProjectDto>(project);

            return Ok(projectDto);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteProject(Guid id)
        {
            var userId = GetUserId();

            var project = await _context.Projects
                .Include(p => p.Tasks)
                .Include(p => p.ProjectUsers)
                .FirstOrDefaultAsync(p => p.Id == id && p.CreatedById == userId);

            if (project == null)
                return Forbid("Only the creator can delete this project.");

            // Remove related tasks and users first
            _context.Tasks.RemoveRange(project.Tasks);
            _context.ProjectUsers.RemoveRange(project.ProjectUsers);
            _context.Projects.Remove(project);

            await _context.SaveChangesAsync();

            return Ok("Project deleted.");
        }


        private Guid GetUserId()
        {
            return Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? throw new Exception("User ID not found"));
        }
    }
}
