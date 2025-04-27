using AutoMapper;
using TaskFlowAPI.DTOs;
using TaskFlowAPI.Models;
using TaskFlowAPI.Models.Enums;

namespace TaskFlow.API.Helpers
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            CreateMap<Project, ProjectDto>()
                .ForMember(dest => dest.CreatedBy, opt => opt.MapFrom(src => src.CreatedBy.Username));

            CreateMap<Project, ProjectDto>()
              .ForMember(d => d.CreatedBy, o => o.MapFrom(s => s.CreatedBy.Username))
              .ForMember(d => d.Admins, o => o.MapFrom(s =>
                   s.ProjectUsers
                    .Where(pu => pu.Role == ProjectRole.Admin)
                    .Select(pu => new AdminDto
                    {
                        UserId = pu.UserId,
                        Username = pu.User!.Username
                    })
                    .ToList()
              ));


            CreateMap<CreateProjectDto, Project>();

            CreateMap<TaskItem, TaskDto>()
                .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.Status.ToString()))
                .ForMember(dest => dest.ProjectTitle, opt => opt.MapFrom(src => src.Project.Title))
                .ForMember(dest => dest.CreatedBy, opt => opt.MapFrom(src => src.CreatedBy.Username))
                .ForMember(dest => dest.AssignedToId, opt => opt.MapFrom(src => src.AssignedToId))
                .ForMember(dest => dest.AssignedTo, opt => opt.MapFrom(src => src.AssignedTo != null ? src.AssignedTo.Username : null));


            CreateMap<CreateTaskDto, TaskItem>();

            CreateMap<TaskTimeLog, TimeLogDto>()
                .ForMember(dest => dest.Username, opt => opt.MapFrom(src => src.User.Username));

            CreateMap<CreateTimeLogDto, TaskTimeLog>();

            CreateMap<User, UserDto>();

            CreateMap<AddProjectAdminDto, ProjectUser>();

        }
    }
}
