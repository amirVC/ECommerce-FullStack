using AutoMapper;
using ECommerceAPI.DTOs;
using ECommerceAPI.Extensions;
using ECommerceAPI.Models;

namespace ECommerceAPI.Profiles;

public class MappingProfile : Profile
{
    public MappingProfile()
    {
        CreateMap<Product, ProductDto>()
            .ForMember(
                dest => dest.FinalPrice,
                opt => opt.MapFrom(src => src.EffectivePrice())
            )
            .ForMember(
                dest => dest.EffectivePrice,
                opt => opt.MapFrom(src => src.EffectivePrice())
            )
            .ForMember(
                dest => dest.TotalDiscountAmount,
                opt => opt.MapFrom(src =>
                    src.Price - src.EffectivePrice())
            )
            .ForMember(
                dest => dest.IsOnSale,
                opt => opt.MapFrom(src => src.IsOnSale())
            );

        CreateMap<ProductImage, ProductImageDto>();
        CreateMap<Address, AddressDto>();
        CreateMap<CreateAddressDto, Address>();
        CreateMap<UpdateAddressDto, Address>();

        CreateMap<DeliveryMethod, DeliveryMethodDto>();
        CreateMap<CreateDeliveryMethodDto, DeliveryMethod>();
        CreateMap<UpdateDeliveryMethodDto, DeliveryMethod>();

        CreateMap<Shipment, ShipmentDto>()
            .ForMember(d => d.Status, o => o.MapFrom(s => s.Status.ToString()))
            .ForMember(d => d.History, o => o.MapFrom(s => s.StatusHistory));
        CreateMap<ShipmentStatusHistory, ShipmentStatusHistoryDto>()
            .ForMember(d => d.Status, o => o.MapFrom(s => s.Status.ToString()));
    }
}