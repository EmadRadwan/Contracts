using Application.Catalog.ProductCategories;
using Application.Catalog.ProductPrices;
using Application.Catalog.Products;
using Application.Catalog.ProductSuppliers;
using Application.Order.CustomerRequests;
using Application.Parties.Parties;
using Application.ProductCategories;
using Application.ProductFacilities;
using Application.RoleTypes;
using AutoMapper;
using Domain;

namespace Application.Core;

public class MappingProfiles : Profile
{
    public MappingProfiles()
    {
        CreateMap<Party, Party>();
        CreateMap<Party, PartyDto>();
        CreateMap<Party, PartyFromPartyIdDto>()
            .ForMember(d => d.FromPartyId, o => o.MapFrom(s => s.PartyId))
            .ForMember(d => d.FromPartyName, o => o.MapFrom(s => s.Description));


        CreateMap<RoleType, RoleTypeDto>();
        CreateMap<ProductCategoryMember, ProductCategoryMemberDto>()
            .ForMember(d => d.Description, o => o.MapFrom(s => s.ProductCategory.Description))
            .ForMember(d => d.FromDate,
                o => o.MapFrom(
                    s => DateTime.SpecifyKind(s.FromDate.Truncate(TimeSpan.FromSeconds(1)), DateTimeKind.Utc)));
        CreateMap<ProductFacility, ProductFacilityDto>()
            .ForMember(d => d.FacilityName, o => o.MapFrom(s => s.Facility.FacilityName));


        CreateMap<ProductCategory, ProductCategoryDto>();
        CreateMap<Product, Product>();
        CreateMap<Product, ProductDto>();
        CreateMap<Product, ProductRecord>()
            .ForMember(d => d.ProductTypeDescription, o => o.MapFrom(s => s.ProductType!.Description))
            .ForMember(d => d.PrimaryProductCategoryDescription,
                o => o.MapFrom(s => s.PrimaryProductCategory!.Description))
            //.ForMember(dest => dest.GoodIdentificationsExist, opt => opt.MapFrom(src => src.GoodIdentifications.Any()))
            //.ForMember(dest => dest.ProductPricesExist, opt => opt.MapFrom(src => src.ProductPrices.Any()))
            //.ForMember(dest => dest.QuoteItemsExist, opt => opt.MapFrom(src => src.QuoteItems.Any()))
            //.ForMember(dest => dest.OrderItemsExist, opt => opt.MapFrom(src => src.OrderItems.Any()))
            //.ForMember(dest => dest.ProductAssocProductsExist, opt => opt.MapFrom(src => src.ProductAssocProducts.Any()))
            .ForMember(dest => dest.ProductFacilitiesExist, opt => opt.MapFrom(src => src.ProductFacilities.Any()));

        CreateMap<ProductFacility, ProductFacility>();
        CreateMap<Facility, Facility>();


        CreateMap<ProductPrice, ProductPrice>();
        CreateMap<ProductPrice, ProductPriceDto>()
            .ForMember(d => d.ProductPriceTypeDescription, o => o.MapFrom(s => s.ProductPriceType!.Description))
            .ForMember(d => d.CurrencyUomDescription, o => o.MapFrom(s => s.CurrencyUom!.Description));
        /*.ForMember(d => d.FromDate,
            o => o.MapFrom(
                s => DateTime.SpecifyKind(s.FromDate.Truncate(TimeSpan.FromSeconds(1)), DateTimeKind.Utc)));
                */

        /*.ForMember(dest => dest.ThruDate, opt => {
        opt.PreCondition(src => (src.ThruDate != null));
        opt.MapFrom(src => {
            // Expensive resolution process that can be avoided with a PreCondition
            DateTime.SpecifyKind(src.ThruDate, DateTimeKind.Utc);
        });
    });*/


        CreateMap<CustRequestItem, CustRequestItemDto>()
            .ForMember(d => d.ProductName, o => o.MapFrom(s => s.Product!.ProductName));


        CreateMap<SupplierProduct, SupplierProduct>();
        CreateMap<SupplierProduct, SupplierProductDto>()
            .ForMember(d => d.CurrencyUomDescription, o => o.MapFrom(s => s.CurrencyUom.Description))
            .ForMember(d => d.QuantityUomDescription, o => o.MapFrom(s => s.QuantityUom!.Description))
            .ForMember(d => d.PartyName, o => o.MapFrom(s => s.Party.Description))
            .ForMember(d => d.AvailableFromDate,
                o => o.MapFrom(
                    s => DateTime.SpecifyKind(s.AvailableFromDate.Truncate(TimeSpan.FromSeconds(1)),
                        DateTimeKind.Utc)));
    }
}