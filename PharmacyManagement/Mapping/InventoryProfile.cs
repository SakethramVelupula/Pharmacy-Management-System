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
            CreateMap<ReadInventoryDto, Inventory>().ReverseMap();
            CreateMap<UpdateInventoryQuantityDto, Inventory>(); 
        }
        
    }
}