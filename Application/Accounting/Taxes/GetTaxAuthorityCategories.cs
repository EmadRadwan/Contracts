using Application.Accounting.Services;
using Domain;
using MediatR;
using Application.Catalog.ProductStores;



using Microsoft.EntityFrameworkCore;
using Persistence;

namespace Application.Shipments.Taxes;

public class GetTaxAuthorityCategories
{
    public class Query : IRequest<Result<List<TaxProductCategoryDto>>>
    {
        public string TaxAuthGeoId { get; set; }
        public string TaxAuthPartyId { get; set; }
    }

    public class Handler : IRequestHandler<Query, Result<List<TaxProductCategoryDto>>>
    {
        private readonly ITaxService _taxService;
        private readonly DataContext _context;

        public Handler(ITaxService taxService, DataContext context)
        {
            _taxService = taxService;
            _context = context;
        }

        public async Task<Result<List<TaxProductCategoryDto>>> Handle(Query request,
            CancellationToken cancellationToken)
        {
            var result = await (from taxCategory in _context.TaxAuthorityCategories
                    join category in _context.ProductCategories
                        on taxCategory.ProductCategoryId equals category.ProductCategoryId
                    where taxCategory.TaxAuthGeoId == request.TaxAuthGeoId
                          && taxCategory.TaxAuthPartyId == request.TaxAuthPartyId
                    select new TaxProductCategoryDto
                    {
                        TaxAuthGeoId = taxCategory.TaxAuthGeoId,
                        TaxAuthPartyId = taxCategory.TaxAuthPartyId,
                        ProductCategoryId = taxCategory.ProductCategoryId,
                        ProductCategoryName = category.Description, // Assuming this field exists in ProductCategories
                    })
                .ToListAsync(cancellationToken: cancellationToken);

            return Result<List<TaxProductCategoryDto>>.Success(result);
        }
    }
}