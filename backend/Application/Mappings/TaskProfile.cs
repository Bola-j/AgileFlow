using AgileFlow.Domain.Entities;
using Application.DTOs.Tasks;
using AutoMapper;
using Domain.Entities;

namespace Application.Mappings
{
    public class TaskProfile : Profile
    {
        public TaskProfile()
        {
            CreateMap<ProjectTask, TaskSummaryResponse>()
                .ForMember(dest => dest.Status,
                    opt => opt.MapFrom(src => src.Status.ToString()))
                .ForMember(dest => dest.Priority,
                    opt => opt.MapFrom(src => src.Priority.ToString()))
                .ForMember(dest => dest.Assignees,
                    opt => opt.MapFrom(src => src.UserTasks.Where(ut => !ut.IsDeleted)));

            CreateMap<ProjectTask, TaskDetailResponse>()
                .IncludeBase<ProjectTask, TaskSummaryResponse>()
                .ForMember(dest => dest.Description,
                    opt => opt.MapFrom(src => src.Description));

            CreateMap<UserTask, TaskAssigneeResponse>()
                .ForMember(dest => dest.UserId,
                    opt => opt.MapFrom(src => src.AppUserId))
                .ForMember(dest => dest.Email,
                    opt => opt.MapFrom(src => src.AppUser.Email))
                .ForMember(dest => dest.FullName,
                    opt => opt.MapFrom(src =>
                        string.IsNullOrWhiteSpace((src.AppUser.First_Name + " " + src.AppUser.Last_Name).Trim())
                            ? src.AppUser.UserName ?? src.AppUser.Email ?? src.AppUserId
                            : (src.AppUser.First_Name + " " + src.AppUser.Last_Name).Trim()));
        }
    }
}
