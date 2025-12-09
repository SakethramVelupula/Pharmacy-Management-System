using AutoMapper;
using PharmacyManagement.DTO;
using PharmacyManagement.Models;

namespace PharmaAPI.Mapping
{
    public class SalesProfile : Profile
    {
        public SalesProfile()
        {
            CreateMap<Sales, SaleDto>().ReverseMap();
            CreateMap<CreateSaleDto, Sales>().ForMember(dest => dest.SalesId, opt => opt.Ignore());
        }
    }
}
