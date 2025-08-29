using Application.Catalog.Products;
using Application.Catalog.ProductStores;
using AutoMapper;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Persistence;
using System.Threading;
using System.Threading.Tasks;
using System.Linq;
using System.Collections.Generic;

namespace Application.Project;

// REFACTOR: Define DTO for certificate items
// Purpose: Map database fields to CertificateItem
// Context: Matches OrderItemDto2 structure
public class CertificateItemDto
{
    public string WorkEffortId { get; set; }
    public string WorkEffortParentId { get; set; }
    public string Description { get; set; }
    public decimal Quantity { get; set; }
    public decimal TotalAmount { get; set; }
    public int CompletionPercentage { get; set; }
    public ProductLovDto ProductId { get; set; }
    public string ProductName { get; set; }
    public string Notes { get; set; }
    public bool IsDeleted { get; set; }
    public string QuantityUom { get; set; }
    public string UomDescription { get; set; }
    public decimal? Rate { get; set; }
}

// REFACTOR: Define query and handler
// Purpose: Fetch certificate items for a workEffortId
// Context: Modeled after ListPurchaseOrderItems
public class ListCertificateItems
{
    public class Query : IRequest<Result<List<CertificateItemDto>>>
    {
        public string WorkEffortId { get; set; }
        public string Language { get; set; }
    }

    public class Handler : IRequestHandler<Query, Result<List<CertificateItemDto>>>
    {
        private readonly DataContext _context;

        public Handler(DataContext context)
        {
            _context = context;
        }

        public async Task<Result<List<CertificateItemDto>>> Handle(Query request, CancellationToken cancellationToken)
        {
            // REFACTOR: Validate workEffortId
            // Purpose: Prevent null reference exceptions
            if (string.IsNullOrEmpty(request.WorkEffortId))
            {
                return Result<List<CertificateItemDto>>.Failure("WorkEffortId cannot be null or empty.");
            }

            // REFACTOR: Default to English for language
            // Purpose: Ensure consistent behavior
            var language = request.Language ?? "en";

            // REFACTOR: Fetch certificate items
            // Purpose: Query workEffort table for items
            // Context: Joins with Products and Uoms, no shipment receipts
            var certificateItems = (from we in _context.WorkEfforts
                join prd in _context.Products on we.ProductId equals prd.ProductId into prdGroup
                from prd in prdGroup.DefaultIfEmpty()
                where we.WorkEffortParentId == request.WorkEffortId && we.WorkEffortTypeId == "CERTIFICATE_ITEM"
                select new CertificateItemDto
                {
                    WorkEffortId = we.WorkEffortId,
                    WorkEffortParentId = we.WorkEffortParentId,
                    Description = we.Description,
                    Quantity = we.Quantity ?? 0,
                    Rate = we.Rate ?? 0,
                    TotalAmount = (we.Quantity ?? 0) * (we.Rate ?? 0),
                    CompletionPercentage = (int)(we.CompletionPercentage ?? 0),
                    ProductId = prd != null
                        ? new ProductLovDto
                        {
                            ProductId = prd.ProductId,
                            ProductName = prd.ProductName
                        }
                        : null,
                    ProductName = prd != null ? prd.ProductName : null,
                    Notes = we.Notes,
                    IsDeleted = false,
                    QuantityUom = null, // Set in product query
                    UomDescription = null, // Set in product query
                }).ToList();


            return Result<List<CertificateItemDto>>.Success(certificateItems);
        }
    }
}