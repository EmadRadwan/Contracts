using MediatR;
using Persistence;
using AutoMapper;
using Application.Accounting.OrganizationGlSettings;
using Application.Accounting.Services;




namespace Application.Shipments.OrganizationGlSettings
{
    public class GetInventoryValuationReport
    {
        public class Query : IRequest<Result<InventoryValuationContext>>
        {
            public string? OrganizationPartyId { get; set; }
            public string? FacilityId { get; set; }
            public string? ProductId { get; set; }
            public DateTime? ThruDate { get; set; }
        }

        public class Handler : IRequestHandler<Query, Result<InventoryValuationContext>>
        {
            private readonly IAcctgReportsService _acctgReportsService;

            public Handler(IAcctgReportsService acctgReportsService)
            {
                _acctgReportsService = acctgReportsService;
            }

            public async Task<Result<InventoryValuationContext>> Handle(Query request, CancellationToken cancellationToken)
            {
                try
                {
                    // Calls the service method you will create to fetch and compute 
                    // the Inventory Valuation Report based on the InventoryItemDetailForSum view.
                    var context = await _acctgReportsService.ComputeInventoryValuation(
                        request.OrganizationPartyId,
                        request.FacilityId,
                        request.ProductId,
                        request.ThruDate
                    );

                    return Result<InventoryValuationContext>.Success(context);
                }
                catch (Exception ex)
                {
                    return Result<InventoryValuationContext>.Failure(ex.Message);
                }
            }
        }
    }
}
