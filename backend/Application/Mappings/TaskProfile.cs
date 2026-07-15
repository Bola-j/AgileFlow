using AgileFlow.Domain.Entities;
using Application.DTOs.Dashboard;
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
                .ForMember(dest => dest.ApprovalStatus,
                    opt => opt.MapFrom(src => src.ApprovalStatus.HasValue ? src.ApprovalStatus.Value.ToString() : null))
                .ForMember(dest => dest.Assignees,
                    opt => opt.MapFrom(src => src.UserTasks.Where(ut => !ut.IsDeleted)));

            CreateMap<ProjectTask, MyTaskResponse>()
                .IncludeBase<ProjectTask, TaskSummaryResponse>();

            CreateMap<ProjectTask, TaskDetailResponse>()
                .IncludeBase<ProjectTask, TaskSummaryResponse>()
                .ForMember(dest => dest.Description,
                    opt => opt.MapFrom(src => src.Description))
                .ForMember(dest => dest.Dependencies,
                    opt => opt.MapFrom(src => src.TaskDependents))
                .ForMember(dest => dest.Commits,
                    opt => opt.MapFrom(src => src.Commits.Where(commit => !commit.IsDeleted).OrderByDescending(commit => commit.CreatedAt)))
                .ForMember(dest => dest.Comments,
                    opt => opt.MapFrom(src => src.Comments.Where(comment => !comment.IsDeleted).OrderByDescending(comment => comment.CreatedAt)));

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

            CreateMap<TaskActivityLog, TaskActivityLogResponse>()
                .ForMember(dest => dest.AppUserName, opt => opt.MapFrom(src =>
                    src.AppUser == null
                        ? "Unknown User"
                        : string.IsNullOrWhiteSpace((src.AppUser.First_Name + " " + src.AppUser.Last_Name).Trim())
                            ? src.AppUser.UserName ?? src.AppUser.Email ?? src.AppUserId
                            : (src.AppUser.First_Name + " " + src.AppUser.Last_Name).Trim()));

            CreateMap<TaskDependent, TaskDependencyResponse>()
                .ForMember(dest => dest.DependencyTaskId, opt => opt.MapFrom(src => src.DependedTaskId))
                .ForMember(dest => dest.Title, opt => opt.MapFrom(src => src.DependedTask.Title))
                .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.DependedTask.Status.ToString()))
                .ForMember(dest => dest.ApprovalStatus, opt => opt.MapFrom(src => src.DependedTask.ApprovalStatus.HasValue ? src.DependedTask.ApprovalStatus.Value.ToString() : null));

            CreateMap<Commit, TaskCommitResponse>()
                .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.Status.ToString()))
                .ForMember(dest => dest.AppUserName, opt => opt.MapFrom(src =>
                    src.AppUser == null
                        ? "Unknown User"
                        : string.IsNullOrWhiteSpace((src.AppUser.First_Name + " " + src.AppUser.Last_Name).Trim())
                            ? src.AppUser.UserName ?? src.AppUser.Email ?? src.AppUserId
                            : (src.AppUser.First_Name + " " + src.AppUser.Last_Name).Trim()));

            CreateMap<Comment, TaskCommentResponse>()
                .ForMember(dest => dest.AppUserName, opt => opt.MapFrom(src =>
                    src.AppUser == null
                        ? "Unknown User"
                        : string.IsNullOrWhiteSpace((src.AppUser.First_Name + " " + src.AppUser.Last_Name).Trim())
                            ? src.AppUser.UserName ?? src.AppUser.Email ?? src.AppUserId
                            : (src.AppUser.First_Name + " " + src.AppUser.Last_Name).Trim()));
        }
    }
}
