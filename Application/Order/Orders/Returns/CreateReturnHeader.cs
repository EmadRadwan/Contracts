using MediatR;
using Microsoft.EntityFrameworkCore;
using Application.Parties.Parties;
using Persistence;
using Serilog;

namespace Application.Order.Orders.Returns;

public class CreateReturnHeader
{
    public class Command : IRequest<Result<ReturnDto>>
    {
        public ReturnDto ReturnDto { get; set; }
    }

    public class Handler : IRequestHandler<Command, Result<ReturnDto>>
    {
        private readonly IReturnService _returnService;
        private readonly DataContext _context;

        public Handler( IReturnService returnService, DataContext context)
        {
            _returnService = returnService;
            _context = context;
        }

        public async Task<Result<ReturnDto>> Handle(Command request, CancellationToken cancellationToken)
        {
            await using var transaction = await _context.Database.BeginTransactionAsync(cancellationToken);
            // var loggerForTransaction = Log.ForContext("Transaction", "create return header");
            var returnHeaderParameters = new CreateReturnHeaderParameters
            {
                DestinationFacilityId = request.ReturnDto.DestinationFacilityId,
                FromPartyId = request.ReturnDto.FromPartyId,
                ReturnHeaderTypeId = request.ReturnDto.ReturnHeaderTypeId,
                PaymentMethodId = request.ReturnDto.PaymentMethodId,
                ToPartyId = request.ReturnDto.ToPartyId,
                CurrencyUomId = request.ReturnDto.CurrencyUomId
            };

            try
            {
                var newReturnHeader = await _returnService.CreateReturnHeader(returnHeaderParameters);
                await _context.SaveChangesAsync(cancellationToken);
                await transaction.CommitAsync(cancellationToken);

                var returnHeaderToReturn = new ReturnDto
                {
                    ReturnId = newReturnHeader.ReturnId,
                    FromPartyId = request.ReturnDto.FromPartyId,
                    FromPartyName = request.ReturnDto.FromPartyName,
                    ToPartyId = request.ReturnDto.ToPartyId,
                    ToPartyName = request.ReturnDto.ToPartyName,
                    StatusId = newReturnHeader.StatusId,
                    EntryDate = request.ReturnDto.EntryDate,
                    StatusDescription = newReturnHeader.StatusId == "RETURN_REQUESTED" ? "Return Requested" : "Supplier Return Requested",
                    CurrencyUomId = request.ReturnDto.CurrencyUomId,
                    DestinationFacilityId = request.ReturnDto.DestinationFacilityId,
                };

                return Result<ReturnDto>.Success(returnHeaderToReturn);
            }
            
            catch (Exception ex)
            {
                // _logger.LogError(ex, "An unexpected error occurred.");
                // await transaction.RollbackAsync(cancellationToken);
                return Result<ReturnDto>.Failure("An unexpected error occurred while creating the return.");
            }
        }
    }
}