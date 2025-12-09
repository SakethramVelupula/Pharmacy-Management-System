using AutoMapper;
using PharmacyManagement.Models;
using PharmacyManagement.DTO;
namespace PharmacyManagement.Mapping
{
    public class UserProfile : Profile
    {
        public UserProfile()
        {
            CreateMap<LoginDto, User>();
            
            CreateMap<RegisterDto, User>()
                .ForMember(dest => dest.Email, opt => opt.MapFrom(src => src.Email))
                .ForMember(dest => dest.UserName, opt => opt.MapFrom(src => src.Name))
                .ForMember(dest => dest.Role, opt => opt.Ignore());

        }
        
    }
}