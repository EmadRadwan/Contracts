using Application.Accounting.Services;
using Application.Accounting.Taxes;
using Application.Order.Orders;
using Application.Order.Quotes;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Application.Shipments.Taxes;

public class CalculateTaxAdjustmentsForQuote
{
    public class Command : IRequest<Result<QuoteAdjustmentDto2[]>>
    {
        public QuoteItemsToBeTaxedDto QuoteItems { get; set; }
    }


    public class Handler : IRequestHandler<Command, Result<QuoteAdjustmentDto2[]>>
    {
        private readonly ILogger<CalculateTaxAdjustments> _logger;
        private readonly ITaxService _taxService;

        public Handler(ITaxService taxService, ILogger<CalculateTaxAdjustments> logger)
        {
            _logger = logger;
            _taxService = taxService;
        }

        public async Task<Result<QuoteAdjustmentDto2[]>> Handle(Command request, CancellationToken cancellationToken)
        {
            try
            {
                _logger.LogInformation("Calculate Tax Adjustments For Sales Quote Started. {Transaction} for {QuoteId}",
                    "calculate tax adjustments for sales quote", request.QuoteItems.QuoteItems.First().QuoteId);

                var quoteAdjustments = await _taxService.CalculateTaxAdjustmentsForQuote(request.QuoteItems.QuoteItems);
                return Result<QuoteAdjustmentDto2[]>.Success(quoteAdjustments);
            }
            catch (Exception ex)
            {
                _logger.LogDebug(
                    "An error occurred while calculate tax adjustments for sales quote {Transaction}. Stack Trace: {StackTrace}",
                    "calculate tax adjustments for sales quote", ex.StackTrace);
                return Result<QuoteAdjustmentDto2[]>.Failure(
                    "An error occurred while calculate tax adjustments for sales quote");
            }
        }
    }
}