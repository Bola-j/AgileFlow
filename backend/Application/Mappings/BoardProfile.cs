using AgileFlow.Domain.Entities;
using Application.DTOs.Board;
using AutoMapper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Mappings
{
    public class BoardProfile : Profile
    {
        public BoardProfile()
        {
            CreateMap<Board, CreateBoardResponse>()
                .ForMember(dest => dest.BoardId, opt => opt.MapFrom(src => src.Id))
                .ForMember(dest => dest.ProjectId, opt => opt.MapFrom(src => src.ProjectId))
                .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.Name));

            CreateMap<Board, GetBoardDetailsResponse>()
                .ForMember(dest => dest.BoardId, opt => opt.MapFrom(src => src.Id))
                .ForMember(dest => dest.ProjectId, opt => opt.MapFrom(src => src.ProjectId))
                .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.Name))
                .ForMember(dest => dest.Columns, opt => opt.MapFrom(src =>
                    src.BoardColumns.Where(c => !c.IsDeleted).OrderBy(c => c.Position)));

            CreateMap<BoardColumn, ColumnResponse>()
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id))
                .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.Name))
                .ForMember(dest => dest.Position, opt => opt.MapFrom(src => src.Position))
                .ForMember(dest => dest.Tasks, opt => opt.MapFrom(src =>
                                    src.Tasks.Where(t => !t.IsDeleted)));

            CreateMap<Board, BoardSummaryResponse>()
                .ForMember(dest => dest.BoardId, opt => opt.MapFrom(src => src.Id))
                .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.Name));
        }
    }
}
