using AutoMapper;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Persistence;

namespace Application.Catalog.ProductCategories;

public class ListHierarchicalCategoriesRawMaterials
{
    public class Query : IRequest<Result<List<ProductCategoryParentChildDto>>>
    {
        public string Language { get; set; } = "en";
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
            var language = (request.Language ?? "en").ToLower();

            // Load all categories
            var categories = await _context.ProductCategories
                .ToListAsync(cancellationToken);

            if (!categories.Any())
                return Result<List<ProductCategoryParentChildDto>>.Success(new List<ProductCategoryParentChildDto>());

            // Dictionary for fast lookup
            var categoryDict = categories.ToDictionary(c => c.ProductCategoryId);

            var result = new List<ProductCategoryParentChildDto>();

            void BuildTree(string categoryId, string? parentId, ProductCategoryParentChildDto? parentDto)
            {
                if (!categoryDict.TryGetValue(categoryId, out var cat))
                    return;

                var dto = new ProductCategoryParentChildDto
                {
                    ParentProductCategoryId = parentId,
                    ProductCategoryId = categoryId,
                    Description = language == "ar" 
                        ? cat.DescriptionArabic ?? cat.Description ?? categoryId 
                        : cat.Description ?? categoryId,
                    Text = language == "ar" 
                        ? cat.DescriptionArabic ?? cat.Description ?? categoryId 
                        : cat.Description ?? categoryId,
                    Items = new List<ProductCategoryParentChildDto>()
                };

                // Get direct children
                var children = categories
                    .Where(c => c.PrimaryParentCategoryId == categoryId)
                    .ToList();

                // Build children recursively
                foreach (var child in children)
                {
                    BuildTree(child.ProductCategoryId, categoryId, dto);
                }

                // Add to parent or root
                if (parentDto != null)
                {
                    parentDto.Items.Add(dto);
                }
                else
                {
                    result.Add(dto); // Root level (RAW_MATERIALS)
                }
            }

            // Start building from RAW_MATERIALS
            BuildTree("RAW_MATERIALS", null, null);

            // Optional: Add debug info (you can remove later)
            if (result.Any())
            {
                var root = result.First();
                Console.WriteLine($"[Debug] RAW_MATERIALS found. Children count: {root.Items.Count}");
            }
            else
            {
                Console.WriteLine("[Debug] RAW_MATERIALS category not found in database!");
            }

            return Result<List<ProductCategoryParentChildDto>>.Success(result);
        }
    }
}