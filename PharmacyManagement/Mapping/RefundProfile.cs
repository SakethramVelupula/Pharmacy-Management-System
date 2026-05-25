using AutoMapper;
using PharmacyManagement.DTO;
using PharmacyManagement.Models;

namespace PharmacyManagement.Mapping
{
    public class RefundProfile : Profile
    {
        public RefundProfile()
        {
            CreateMap<Refund, RefundDto>();
            CreateMap<RequestRefundDto, Refund>()
                .ForMember(dest => dest.RefundId, opt => opt.Ignore())
                .ForMember(dest => dest.Status, opt => opt.Ignore())
                .ForMember(dest => dest.StripeRefundId, opt => opt.Ignore())
                .ForMember(dest => dest.StripeRefundStatus, opt => opt.Ignore())
                .ForMember(dest => dest.AdminNotes, opt => opt.Ignore())
                .ForMember(dest => dest.RequestedAt, opt => opt.Ignore())
                .ForMember(dest => dest.ResolvedAt, opt => opt.Ignore())
                .ForMember(dest => dest.Amount, opt => opt.Ignore())
                .ForMember(dest => dest.RequestedById, opt => opt.Ignore());
        }
    }
}
