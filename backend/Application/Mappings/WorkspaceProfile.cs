using AgileFlow.Domain.Entities;
using Application.DTOs.Workspace;
using AutoMapper;
using Domain.Entities;

namespace Application.Mappings
{
    public class WorkspaceProfile : Profile
    {
        public WorkspaceProfile()
        {

            CreateMap<Workspace, WorkspaceSummaryResponse>()
                .ForMember(dest => dest.ProjectCount,
                    opt => opt.MapFrom(src => src.Projects.Count(p => !p.IsDeleted)))
                .ForMember(dest => dest.MemberCount,
                    opt => opt.MapFrom(src => src.UserWorkspaces.Count(uw => !uw.IsDeleted)));

            CreateMap<Workspace, WorkspaceResponse>()
                .ForMember(dest => dest.Projects,
                    opt => opt.MapFrom(src => src.Projects.Where(p => !p.IsDeleted)))
                .ForMember(dest => dest.Members,
                    opt => opt.MapFrom(src => src.UserWorkspaces.Where(uw => !uw.IsDeleted)));

            CreateMap<Project, WorkspaceProjectResponse>()
                .ForMember(dest => dest.Status,
                    opt => opt.MapFrom(src => src.Status.ToString()));

            CreateMap<UserWorkspace, WorkspaceMemberResponse>()
                .ForMember(dest => dest.UserId,
                    opt => opt.MapFrom(src => src.AppUserId))
                .ForMember(dest => dest.FullName,
                    opt => opt.MapFrom(src => $"{src.AppUser.First_Name} {src.AppUser.Last_Name}"))
                .ForMember(dest => dest.Email,
                    opt => opt.MapFrom(src => src.AppUser.Email))
                .ForMember(dest => dest.ProfilePicture,
                    opt => opt.MapFrom(src => src.AppUser.Profile_Picture))
                .ForMember(dest => dest.Role,
                    opt => opt.MapFrom(src => src.Role.ToString()));
        }
    }
}