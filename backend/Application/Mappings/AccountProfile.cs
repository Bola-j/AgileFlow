using AgileFlow.Domain.Entities;
using Application.DTOs.Account;
using AutoMapper;

namespace Application.Mappings
{
    public class AccountProfile : Profile
    {
        public AccountProfile()
        {
            CreateMap<AppUser, AccountResponse>()
                .ForMember(dest => dest.UserId,
                    opt => opt.MapFrom(src => src.Id))
                .ForMember(dest => dest.FirstName,
                    opt => opt.MapFrom(src => src.First_Name))
                .ForMember(dest => dest.LastName,
                    opt => opt.MapFrom(src => src.Last_Name))
                .ForMember(dest => dest.ProfilePicture,
                    opt => opt.MapFrom(src => src.Profile_Picture))
                .ForMember(dest => dest.Dob,
                    opt => opt.MapFrom(src => src.DOB))
                .ForMember(dest => dest.GithubUsername,
                    opt => opt.MapFrom(src => src.Github_Username));
        }
    }
}
