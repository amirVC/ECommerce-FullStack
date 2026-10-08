using AutoMapper;
using ECommerceAPI.DTOs;
using ECommerceAPI.Models;

namespace ECommerceAPI.Profiles
{
    public class ShipmentMappingProfile : Profile
    {
        public ShipmentMappingProfile()
        {
            CreateMap<Shipment, ShipmentDto>()
                .ForMember(d => d.Status, o => o.MapFrom(s => s.Status.ToString()))
                .ForMember(d => d.History, o => o.MapFrom(s => s.StatusHistory.OrderBy(h => h.ChangedAt)));

            CreateMap<ShipmentStatusHistory, ShipmentStatusHistoryDto>()
                .ForMember(d => d.Status, o => o.MapFrom(s => s.Status.ToString()));
        }
    }
}