
using Domain;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Persistence;

namespace Application.Shipments.OrganizationGlSettings;

public class GetCreditCardTypes
{
    public class Query: IRequest<Result<List<CreditCardTypeDto>>>
    {

    }

    public class Handler: IRequestHandler<Query, Result<List<CreditCardTypeDto>>>
    {
        private readonly DataContext _context;

        public Handler(DataContext context)
        {
            _context = context;
        }

        public async Task<Result<List<CreditCardTypeDto>>> Handle(Query request, CancellationToken cancellationToken)
        {
            try
            {
                var creditCardTypes = await (from enm in _context.Enumerations
                where enm.EnumTypeId == "CREDIT_CARD_TYPE"
                select new CreditCardTypeDto
                {
                    CardType = enm.EnumId,
                    Description = enm.Description               
                }
                ).ToListAsync(cancellationToken);

                return Result<List<CreditCardTypeDto>>.Success(creditCardTypes);
            }
            catch (Exception ex)
            {
                // Handle exception and return an error result
                return Result<List<CreditCardTypeDto>>.Failure(ex.Message);
            }
        }
    }
}