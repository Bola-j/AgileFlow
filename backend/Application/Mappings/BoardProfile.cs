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

            CreateMap<Board, GetBoardDetailsResponse>()
                .ForMember(dest => dest.Columns, opt => opt.MapFrom(src =>
                    src.BoardColumns
                        .Where(c => !c.IsDeleted)
                        .OrderBy(c => c.Position)));

            CreateMap<BoardColumn, ColumnResponse>()
                .ForMember(dest => dest.Tasks, opt => opt.MapFrom(src =>
                    src.Tasks.Where(t => !t.IsDeleted)));
        }
    }
}
