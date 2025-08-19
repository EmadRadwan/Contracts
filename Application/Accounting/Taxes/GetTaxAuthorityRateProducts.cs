using Application.Accounting.Services;
using Domain;
using MediatR;
using Application.Catalog.ProductStores;




namespace Application.Shipments.Taxes;

public class GetTaxAuthorityRateProducts
{
    public class Query : IRequest<Result<List<TaxAuthorityRateProductDto>>>
    {
        public string TaxAuthGeoId { get; set; }
        public string TaxAuthPartyId { get; set; }
    }

    public class Handler : IRequestHandler<Query, Result<List<TaxAuthorityRateProductDto>>>
    {
        private readonly ITaxService _taxService;
        private readonly IProductStoreService _productStoreService;

        public Handler(ITaxService taxService, IProductStoreService productStoreService)
        {
            _taxService = taxService;
            _productStoreService = productStoreService;
        }

        public async Task<Result<List<TaxAuthorityRateProductDto>>> Handle(Query request,
            CancellationToken cancellationToken)
        {
            // Validate input if needed
            if (string.IsNullOrEmpty(request.TaxAuthGeoId) || string.IsNullOrEmpty(request.TaxAuthPartyId))
            {
                return Result<List<TaxAuthorityRateProductDto>>.Failure("Invalid TaxAuthGeoId or TaxAuthPartyId.");
            }
            
            var payToPartyId = await _productStoreService.GetProductStorePayToPartId();

            
            // Create a new PartyTaxAuthInfo object
            var partyTaxAuthInfo = new PartyTaxAuthInfo
            {
                PartyId = payToPartyId,
                TaxAuthGeoId = request.TaxAuthGeoId,
                TaxAuthPartyId = request.TaxAuthPartyId
            };

            // Call the tax service to get the rate products
            var taxAuthorityRateProducts =
                await _taxService.GetTaxAuthorityRateProductsQuery(partyTaxAuthInfo);

            if (taxAuthorityRateProducts == null || !taxAuthorityRateProducts.Any())
            {
                return Result<List<TaxAuthorityRateProductDto>>.Failure("No tax authority rate products found.");
            }

            return Result<List<TaxAuthorityRateProductDto>>.Success(taxAuthorityRateProducts);
        }
    }
}