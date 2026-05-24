using AutoMapper;
using PharmacyManagement.Models;
using PharmacyManagement.DTO;
namespace PharmacyManagement.Mapping
{
    public class InventoryProfile : Profile
    {
        public InventoryProfile()
        {
            CreateMap<AddInventoryDto, Inventory>().ReverseMap();
            CreateMap<Inventory, ReadInventoryDto>()
                .ForMember(dest => dest.DrugName, opt => opt.MapFrom(src => src.Drug != null ? src.Drug.Name : string.Empty))
                .ForMember(dest => dest.SupplierName, opt => opt.MapFrom(src => src.Supplier != null ? src.Supplier.Name : string.Empty))
                .ForMember(dest => dest.DrugStorageInstructions, opt => opt.MapFrom(src => src.Drug != null ? src.Drug.StorageInstructions : null));
            CreateMap<UpdateInventoryQuantityDto, Inventory>(); 
        }
        
    }
}