using AutoMapper;
using PharmacyManagement.DTO;
using PharmacyManagement.Models;

namespace PharmaAPI.Profiles
{
    public class OrderProfileMapper : Profile
    {
        public OrderProfileMapper()
        {
            CreateMap<Order, OrderDto>().ReverseMap();
            CreateMap<CreateOrderDto, Order>();
            CreateMap<CreatePatientOrderDto, Order>();
            CreateMap<UpdateOrderDto, Order>();
        }
    }
}
