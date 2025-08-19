using Application.Interfaces;
using Application.order.Quotes;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Persistence;

namespace Application.Order.Quotes;

public class UpdateOrApproveQuote
{
    public class Command : IRequest<Result<QuoteDto>>
    {
        public QuoteDto QuoteDto { get; set; }
    }

    public class CommandValidator : AbstractValidator<Command>
    {
        public CommandValidator()
        {
            RuleFor(x => x.QuoteDto).SetValidator(new QuoteValidator());
        }
    }

    public class Handler : IRequestHandler<Command, Result<QuoteDto>>
    {
        private readonly DataContext _context;
        private readonly ILogger _logger;
        private readonly IQuoteService _quoteService;
        private readonly IUserAccessor _userAccessor;


        public Handler(DataContext context, IUserAccessor userAccessor, IQuoteService quoteService,
            ILogger<UpdateOrApproveQuote> _logger)
        {
            _userAccessor = userAccessor;
            _context = context;
            _quoteService = quoteService;
            this._logger = _logger;
        }

        public async Task<Result<QuoteDto>> Handle(Command request, CancellationToken cancellationToken)
        {
            var transaction = _context.Database.BeginTransaction();

            var user = await _context.Users.FirstOrDefaultAsync(x =>
                x.UserName == _userAccessor.GetUsername());

            var updateMode = request.QuoteDto.ModificationType == "UPDATE" ? "UPDATE" : "APPROVE";

            if (updateMode == "UPDATE")
            {
                // update quote
                _logger.LogDebug("Starting _quoteService.UpdateQuote");
                var updatedQuote = await _quoteService.UpdateQuote(request.QuoteDto);
                _logger.LogDebug("Finished _quoteService.UpdateQuote {updatedQuote}", updatedQuote);
            }
            else if (updateMode == "APPROVE")
            {
                await _quoteService.ApproveQuote(request.QuoteDto);
            }

            var result = await _context.SaveChangesAsync(cancellationToken) > 0;

            if (!result)
            {
                await transaction.RollbackAsync(cancellationToken);
                return Result<QuoteDto>.Failure($"Failed to {updateMode} Quote");
            }

            await transaction.CommitAsync(cancellationToken);

            var quoteToReturn = new QuoteDto
            {
                QuoteId = request.QuoteDto.QuoteId,
                FromPartyId = request.QuoteDto.FromPartyId,
                StatusDescription = updateMode == "UPDATE" ? "Updated" : "Approved",
                VehicleId = request.QuoteDto.VehicleId,
                CustomerRemarks = request.QuoteDto.CustomerRemarks,
                InternalRemarks = request.QuoteDto.InternalRemarks,
                CurrentMileage = request.QuoteDto.CurrentMileage
            };


            return Result<QuoteDto>.Success(quoteToReturn);
        }
    }
}