using AutoMapper;
using PharmacyManagement.Models;
using PharmacyManagement.DTO;
namespace PharmacyManagement.Mapping
{
    public class DrugProfile : Profile
    {
        public DrugProfile()
        {
            CreateMap<Drug, DrugDto>();
            CreateMap<DrugDto, Drug>();
            
            CreateMap<CreateDrugDto, Drug>();
            CreateMap<UpdateDrugDto, Drug>();

        }
        
    }
}