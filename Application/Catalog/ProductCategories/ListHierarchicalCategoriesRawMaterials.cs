using AutoMapper;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Persistence;

namespace Application.Catalog.ProductCategories;

public class ListHierarchicalCategoriesRawMaterials
{
    public class Query : IRequest<Result<List<ProductCategoryParentChildDto>>>
    {
        public string Language { get; set; }
    }

    public class Handler : IRequestHandler<Query, Result<List<ProductCategoryParentChildDto>>>
    {
        private readonly DataContext _context;

        public Handler(DataContext context)
        {
            _context = context;
        }

        public async Task<Result<List<ProductCategoryParentChildDto>>> Handle(Query request,
            CancellationToken cancellationToken)
        {
            var language = request.Language;
            // Retrieve the data from the ProductCategories table using Entity Framework
            var categories = await _context.ProductCategories.ToListAsync(cancellationToken);

            // Create a list to store the DTOs
            var dtoList = new List<ProductCategoryParentChildDto>();

            // Define a recursive function to build the DTO list
            void BuildDtoList(string categoryId, string parentId)
            {
                // Find the category data for the current ID
                var categoryData = categories.FirstOrDefault(entry => entry.ProductCategoryId == categoryId);

                // Check if the category should be excluded
                if (categoryData != null)
                {
                    var dto = new ProductCategoryParentChildDto
                    {
                        ParentProductCategoryId = parentId,
                        ProductCategoryId = categoryId,
                        Description = language == "ar"
                            ? categoryData.DescriptionArabic
                            : categoryData.Description ?? categoryData.ProductCategoryId,
                        Text = language == "ar"
                            ? categoryData.DescriptionArabic
                            : categoryData.Description ?? categoryData.ProductCategoryId,
                        Items = new List<ProductCategoryParentChildDto>() // Initialize children list
                    };

                    // Find all children of this category and recursively build the DTO list
                    var children = categories.Where(entry => entry.PrimaryParentCategoryId == categoryId);

                    dto.Items = children.Select(child => new ProductCategoryParentChildDto
                    {
                        ParentProductCategoryId = categoryId,
                        ProductCategoryId = child.ProductCategoryId,
                        Description = language == "ar"
                            ? child.DescriptionArabic
                            : child.Description ?? child.ProductCategoryId,
                        Text = language == "ar"
                            ? child.DescriptionArabic
                            : child.Description ?? child.ProductCategoryId,
                        Items = new List<ProductCategoryParentChildDto>() // Initialize children list
                    }).ToList();

                    foreach (var child in children) BuildDtoList(child.ProductCategoryId, categoryId);

                    // Skip adding top-level items to the dtoList but continue with recursive processing
                    if (parentId != null) dtoList.Add(dto); // Add the DTO to the list for non-top-level items
                }
            }

            // Start building the DTO list from the top-level categories
            var topLevelCategories = new[] { "RAW_MATERIALS" };
            var rootDtos = new List<ProductCategoryParentChildDto>();

            foreach (var topCategory in topLevelCategories) BuildDtoList(topCategory, null);

            // Filter out the top-level categories and add the hierarchical structure to rootDtos
            rootDtos.AddRange(dtoList.Where(dto => topLevelCategories.Contains(dto.ParentProductCategoryId)));

            // Return the root DTOs as a successful result
            return Result<List<ProductCategoryParentChildDto>>.Success(rootDtos);
        }
    }
}