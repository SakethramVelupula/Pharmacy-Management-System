using AutoMapper;
using PharmacyManagement.DTO;
using PharmacyManagement.Models;

namespace PharmacyManagement.Mapping
{
    public class SupplierProfile : Profile
    {
        public SupplierProfile()
        {
            CreateMap<Supplier, SupplierDto>();
            CreateMap<CreateSupplierDto, Supplier>();
        }
    }
}