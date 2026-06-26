using AgileFlow.Domain.Entities;
using Application.DTOs.Sprint;
using AutoMapper;

namespace Application.Mappings
{
    public class SprintProfile : Profile
    {
        public SprintProfile()
        {
            CreateMap<Sprint, SprintResponse>()
                .ForMember(dest => dest.Status,
                    opt => opt.MapFrom(src => src.Status.ToString()))
                .ForMember(dest => dest.TaskCount,
                    opt => opt.MapFrom(src => src.Tasks.Count(t => !t.IsDeleted)));
        }
    }
}
