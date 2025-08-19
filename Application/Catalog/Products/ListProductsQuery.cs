
using MediatR;
using Microsoft.AspNetCore.OData.Query;
using Persistence;

namespace Application.Catalog.Products;

public class ListProductsQuery
{
    public class Query : IRequest<IQueryable<ProductRecord>>
    {
        public ODataQueryOptions<ProductRecord> Options { get; set; }
        public string Language { get; set; }
    }

    public class Handler : IRequestHandler<Query, IQueryable<ProductRecord>>
    {
        private readonly DataContext _context;

        public Handler(DataContext context)
        {
            _context = context;
        }

        public async Task<IQueryable<ProductRecord>> Handle(Query request, CancellationToken cancellationToken)
        {
            var language = request.Language;

            var query = from product in _context.Products
                join uom in _context.Uoms on product.QuantityUomId equals uom.UomId into uoms
                from uom in uoms.DefaultIfEmpty()
                join colorAppl in _context.ProductFeatureAppls
                    on new { product.ProductId, FeatureType = "COLOR" } equals
                    new
                    {
                        colorAppl.ProductId, FeatureType = colorAppl.ProductFeature.ProductFeatureTypeId
                    } into colorAppls
                from colorAppl in colorAppls.DefaultIfEmpty()
                join trademarkAppl in _context.ProductFeatureAppls
                    on new { product.ProductId, FeatureType = "TRADEMARK_NAME" } equals
                    new
                    {
                        trademarkAppl.ProductId, FeatureType = trademarkAppl.ProductFeature.ProductFeatureTypeId
                    } into trademarkAppls
                from trademarkAppl in trademarkAppls.DefaultIfEmpty()
                join sizeAppl in _context.ProductFeatureAppls
                    on new { product.ProductId, FeatureType = "SIZE" } equals
                    new { sizeAppl.ProductId, FeatureType = sizeAppl.ProductFeature.ProductFeatureTypeId }
                    into sizeAppls
                from sizeAppl in sizeAppls.DefaultIfEmpty()
                select new ProductRecord
                {
                    ProductId = product.ProductId,
                    ProductName = product.ProductName,
                    ProductTypeId = product.ProductTypeId,
                    Comments = product.Comments,
                    PrimaryProductCategoryId = product.PrimaryProductCategoryId,
                    Description = product.Description,
                    IntroductionDate = product.IntroductionDate,
                    OriginalImageUrl = product.OriginalImageUrl,
                    QuantityUomId = product.QuantityUomId,
                    IsVirtual = product.IsVirtual,
                    IsVariant = product.IsVariant,
                    QuantityIncluded = product.QuantityIncluded,
                    PiecesIncluded = product.PiecesIncluded,
                    QuantityUomDescription = language == "ar" ? uom.DescriptionArabic : uom.Description,
                    ProductTypeDescription = language == "ar"
                        ? product.ProductType.DescriptionArabic
                        : product.ProductType.Description,
                    PrimaryProductCategoryDescription = language == "ar"
                        ? product.PrimaryProductCategory.DescriptionArabic
                        : product.PrimaryProductCategory.Description,
                    ProductColorId =
                        colorAppl != null && (colorAppl.ThruDate == null || colorAppl.ThruDate > DateTime.Now)
                            ? colorAppl.ProductFeatureId
                            : null,
                    ProductColorDescription = colorAppl != null &&
                                              (colorAppl.ThruDate == null || colorAppl.ThruDate > DateTime.Now)
                        ? (language == "ar"
                            ? colorAppl.ProductFeature.DescriptionArabic
                            : colorAppl.ProductFeature.Description)
                        : null,
                    ProductTrademarkId =
                        trademarkAppl != null &&
                        (trademarkAppl.ThruDate == null || trademarkAppl.ThruDate > DateTime.Now)
                            ? trademarkAppl.ProductFeatureId
                            : null,
                    ProductTrademarkDescription = trademarkAppl != null &&
                                                  (trademarkAppl.ThruDate == null ||
                                                   trademarkAppl.ThruDate > DateTime.Now)
                        ? (language == "ar"
                            ? trademarkAppl.ProductFeature.DescriptionArabic
                            : trademarkAppl.ProductFeature.Description)
                        : null,
                    ProductSizeId = sizeAppl != null && (sizeAppl.ThruDate == null || sizeAppl.ThruDate > DateTime.Now)
                        ? sizeAppl.ProductFeatureId
                        : null,
                    ProductSizeDescription = sizeAppl != null && (sizeAppl.ThruDate == null || sizeAppl.ThruDate > DateTime.Now)
                        ? (language == "ar" ? sizeAppl.ProductFeature.DescriptionArabic : sizeAppl.ProductFeature.Description)
                        : null
                };

            return query.AsQueryable();
        }
    }
}