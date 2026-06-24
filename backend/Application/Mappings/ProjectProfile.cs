using AgileFlow.Domain.Entities;
using Application.DTOs.Project;
using AutoMapper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Mappings
{
    public class ProjectProfile : Profile
    {
        public ProjectProfile()
        {
            // Project → ProjectResponse
            CreateMap<Project, ProjectResponse>()
                .ForMember(dest => dest.Status,
                    opt => opt.MapFrom(src => src.Status.ToString()));
        }
    }
}
