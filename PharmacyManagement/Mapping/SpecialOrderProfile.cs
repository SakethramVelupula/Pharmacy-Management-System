using AutoMapper;
using PharmacyManagement.DTO;
using PharmacyManagement.Models;

namespace PharmacyManagement.Mapping
{
    public class SpecialOrderProfile : Profile
    {
        public SpecialOrderProfile()
        {
            CreateMap<SpecialOrder, SpecialOrderDto>().ReverseMap();

            CreateMap<CreateSpecialOrderDto, SpecialOrder>()
                .ForMember(dest => dest.RequestId, opt => opt.Ignore())
                .ForMember(dest => dest.Status, opt => opt.Ignore())
                .ForMember(dest => dest.AdminNotes, opt => opt.Ignore())
                .ForMember(dest => dest.DateResolved, opt => opt.Ignore())
                .ForMember(dest => dest.DateRequested, opt => opt.Ignore());
        }
    }
}