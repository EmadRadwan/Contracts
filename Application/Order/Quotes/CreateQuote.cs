using Application.Interfaces;
using Application.order.Quotes;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Persistence;

namespace Application.Order.Quotes;

public class CreateQuote
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
            ILogger<CreateQuote> logger)
        {
            _userAccessor = userAccessor;
            _context = context;
            _quoteService = quoteService;
            _logger = logger;
        }

        public async Task<Result<QuoteDto>> Handle(Command request, CancellationToken cancellationToken)
        {
            var transaction = await _context.Database.BeginTransactionAsync(cancellationToken);

            var user = await _context.Users.FirstOrDefaultAsync(x =>
                x.UserName == _userAccessor.GetUsername(), cancellationToken);

            // create quote
            _logger.LogDebug("Starting _quoteService.CreateQuote");
            var newQuote = await _quoteService.CreateQuote(request.QuoteDto);
            _logger.LogDebug("Finished _quoteService.CreateQuote {newQuote}", newQuote);


            var result = await _context.SaveChangesAsync(cancellationToken) > 0;

            if (!result)
            {
                await transaction.RollbackAsync(cancellationToken);
                return Result<QuoteDto>.Failure("Failed to create quote");
            }

            await transaction.CommitAsync(cancellationToken);

            var quoteToReturn = new QuoteDto
            {
                QuoteId = newQuote.QuoteId,
                FromPartyId = request.QuoteDto.FromPartyId,
                StatusDescription = "Created",
                VehicleId = request.QuoteDto.VehicleId,
                CustomerRemarks = request.QuoteDto.CustomerRemarks,
                InternalRemarks = request.QuoteDto.InternalRemarks,
                CurrentMileage = request.QuoteDto.CurrentMileage,
                CurrencyUomId = request.QuoteDto.CurrencyUomId,
                AgreementId = request.QuoteDto.AgreementId
            };


            return Result<QuoteDto>.Success(quoteToReturn);
        }
    }
}