using Domain;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Persistence;

namespace Application.Accounting.FinAccounts
{
    public class GetDepositWithdrawPayments
    {
        public class Query : IRequest<Result<List<PaymentPartyNameDto>>>
        {
            // Removed NoConditionFind since it's no longer needed
            public string PaymentMethodTypeId { get; set; }
            public DateTime? FromDate { get; set; }
            public DateTime? ThruDate { get; set; }
            public string PartyIdFrom { get; set; }
            public bool CheckFinAccountTransNull { get; set; } // Now defaults to false if not provided
            public List<string> PartyIdSetFromFinAccountRole { get; set; }
        }

        public class Handler : IRequestHandler<Query, Result<List<PaymentPartyNameDto>>>
        {
            private readonly DataContext _context;

            public Handler(DataContext context)
            {
                _context = context;
            }

            public async Task<Result<List<PaymentPartyNameDto>>> Handle(Query request, CancellationToken cancellationToken)
            {
                // Build the base query with necessary left joins
                var baseQuery =
                    from pay in _context.Payments

                    // Left join PaymentType
                    join payType in _context.PaymentTypes
                        on pay.PaymentTypeId equals payType.PaymentTypeId into payTypeJoin
                    from payTypeLeft in payTypeJoin.DefaultIfEmpty()

                    // Left join PaymentMethodType
                    join pmType in _context.PaymentMethodTypes
                        on pay.PaymentMethodTypeId equals pmType.PaymentMethodTypeId into pmTypeJoin
                    from pmTypeLeft in pmTypeJoin.DefaultIfEmpty()

                    // Left join StatusItem
                    join stItem in _context.StatusItems
                        on pay.StatusId equals stItem.StatusId into stItemJoin
                    from stItemLeft in stItemJoin.DefaultIfEmpty()

                    // Left join fromParty
                    join fromParty in _context.Parties
                        on pay.PartyIdFrom equals fromParty.PartyId into fromPartyJoin
                    from fromPartyLeft in fromPartyJoin.DefaultIfEmpty()

                    // Left join fromParty->Person
                    join fromPerson in _context.Persons
                        on fromPartyLeft.PartyId equals fromPerson.PartyId into fromPersonJoin
                    from fromPersonLeft in fromPersonJoin.DefaultIfEmpty()

                    // Left join fromParty->PartyGroup
                    join fromGroup in _context.PartyGroups
                        on fromPartyLeft.PartyId equals fromGroup.PartyId into fromGroupJoin
                    from fromGroupLeft in fromGroupJoin.DefaultIfEmpty()

                    // Left join toParty
                    join toParty in _context.Parties
                        on pay.PartyIdTo equals toParty.PartyId into toPartyJoin
                    from toPartyLeft in toPartyJoin.DefaultIfEmpty()

                    // Left join toParty->Person
                    join toPerson in _context.Persons
                        on toPartyLeft.PartyId equals toPerson.PartyId into toPersonJoin
                    from toPersonLeft in toPersonJoin.DefaultIfEmpty()

                    // Left join toParty->PartyGroup
                    join toGroup in _context.PartyGroups
                        on toPartyLeft.PartyId equals toGroup.PartyId into toGroupJoin
                    from toGroupLeft in toGroupJoin.DefaultIfEmpty()
                    select new
                    {
                        Payment = pay,
                        PaymentType = payTypeLeft,
                        PaymentMethodType = pmTypeLeft,
                        StatusItem = stItemLeft,
                        FromParty = fromPartyLeft,
                        FromPerson = fromPersonLeft,
                        FromGroup = fromGroupLeft,
                        ToParty = toPartyLeft,
                        ToPerson = toPersonLeft,
                        ToGroup = toGroupLeft
                    };

                // Apply filtering based on the optional parameters

                // Filter by party IDs if provided
                if (request.PartyIdSetFromFinAccountRole != null && request.PartyIdSetFromFinAccountRole.Any())
                {
                    baseQuery = baseQuery.Where(x =>
                        request.PartyIdSetFromFinAccountRole.Contains(x.Payment.PartyIdTo) ||
                        request.PartyIdSetFromFinAccountRole.Contains(x.Payment.PartyIdFrom));
                }

                // Filter by payment method type if provided
                if (!string.IsNullOrEmpty(request.PaymentMethodTypeId))
                {
                    baseQuery = baseQuery.Where(x => x.Payment.PaymentMethodTypeId == request.PaymentMethodTypeId);
                }

                // Filter by effective date range if provided
                if (request.FromDate.HasValue)
                {
                    baseQuery = baseQuery.Where(x => x.Payment.EffectiveDate >= request.FromDate.Value);
                }

                if (request.ThruDate.HasValue)
                {
                    baseQuery = baseQuery.Where(x => x.Payment.EffectiveDate <= request.ThruDate.Value);
                }

                // Filter by partyIdFrom if provided
                if (!string.IsNullOrEmpty(request.PartyIdFrom))
                {
                    baseQuery = baseQuery.Where(x => x.Payment.PartyIdFrom == request.PartyIdFrom);
                }

                // Filter by FinAccountTransId null condition if explicitly requested
                if (request.CheckFinAccountTransNull)
                {
                    baseQuery = baseQuery.Where(x => x.Payment.FinAccountTransId == null);
                }

                // Execute the query
                var joinedData = await baseQuery.ToListAsync(cancellationToken);

                // Map the joined data to PaymentPartyNameDto
                var resultList = joinedData.Select(x =>
                {
                    var fromFirstName = x.FromPerson?.FirstName;
                    var fromLastName = x.FromPerson?.LastName;
                    var fromGroupName = x.FromGroup?.GroupName;

                    var toFirstName = x.ToPerson?.FirstName;
                    var toLastName = x.ToPerson?.LastName;
                    var toGroupName = x.ToGroup?.GroupName;

                    return new PaymentPartyNameDto
                    {
                        PaymentId = x.Payment.PaymentId,
                        PartyIdFrom = x.Payment.PartyIdFrom,
                        PartyIdTo = x.Payment.PartyIdTo,
                        PaymentMethodTypeId = x.Payment.PaymentMethodTypeId,
                        PaymentTypeId = x.Payment.PaymentTypeId,
                        StatusId = x.Payment.StatusId,
                        EffectiveDate = x.Payment.EffectiveDate,
                        FinAccountTransId = x.Payment.FinAccountTransId,
                        PaymentMethodTypeDesc = x.PaymentMethodType?.Description,
                        PaymentTypeDesc = x.PaymentType?.Description,
                        ParentPaymentTypeId = x.PaymentType?.ParentTypeId,
                        StatusDesc = x.StatusItem?.Description,
                        PartyFromFirstName = fromFirstName,
                        PartyFromLastName = fromLastName,
                        PartyFromGroupName = fromGroupName,
                        PartyToFirstName = toFirstName,
                        PartyToLastName = toLastName,
                        PartyToGroupName = toGroupName
                    };
                }).ToList();

                return Result<List<PaymentPartyNameDto>>.Success(resultList);
            }
        }
    }
}
