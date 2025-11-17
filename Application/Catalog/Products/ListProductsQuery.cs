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
                join pt in _context.ProductTypes on product.ProductTypeId equals pt.ProductTypeId
                join pc in _context.ProductCategories on product.PrimaryProductCategoryId equals pc.ProductCategoryId
                    into pcGroup
                from pc in pcGroup.DefaultIfEmpty() // LEFT JOIN for category
                join proj in _context.WorkEfforts on product.ProjectId equals proj.WorkEffortId into projGroup
                from proj in projGroup.DefaultIfEmpty() // LEFT JOIN for project
                select new ProductRecord
                {
                    ProductId = product.ProductId,
                    ProductName = product.ProductName,
                    ProductTypeId = product.ProductTypeId,
                    Comments = product.Comments,
                    PrimaryProductCategoryId = product.PrimaryProductCategoryId,
                    Description = product.Description,
                    OriginalImageUrl = product.OriginalImageUrl,

                    // Localized descriptions
                    ProductTypeDescription = language == "ar"
                        ? pt.DescriptionArabic
                        : pt.Description,
                    PrimaryProductCategoryDescription = pc != null
                        ? (language == "ar" ? pc.DescriptionArabic : pc.Description)
                        : null,

                    // New apartment-specific fields
                    ProjectId = product.ProjectId,
                    ProjectName = proj != null ? proj.ProjectName : null,
                    FloorNumber = product.FloorNumber,
                    ApartmentSpaceM2 = product.ApartmentSpaceM2,
                    GardenSpaceM2 = product.GardenSpaceM2,
                    ApartmentPricePerM2 = product.ApartmentPricePerM2,
                    GardenPricePerM2 = product.GardenPricePerM2,
                    ApartmentStatusId = product.ApartmentStatusId,
                    ApartmentNumber = product.ApartmentNumber
                };

            return query.AsQueryable();
        }
    }
}