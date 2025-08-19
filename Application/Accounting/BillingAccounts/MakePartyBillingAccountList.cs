using Application.Order.Orders;
using AutoMapper;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Persistence;

namespace Application.Shipments.BillingAccounts;

public class MakePartyBillingAccountList
{
    public class Query : IRequest<Result<List<BillingAccountModel>>>
    {
        public string PartyId { get; set; }
    }

    public class Handler : IRequestHandler<Query, Result<List<BillingAccountModel>>>
    {
        private readonly DataContext _context;
        private readonly IOrderHelperService _orderHelperService;

        public Handler(DataContext context, IOrderHelperService orderHelperService)
        {
            _orderHelperService = orderHelperService;
            _context = context;
        }

        public async Task<Result<List<BillingAccountModel>>> Handle(Query request, CancellationToken cancellationToken)
        {
            var partyBillingAccounts = await _orderHelperService.MakePartyBillingAccountList(request.PartyId, null);

            return Result<List<BillingAccountModel>>.Success(partyBillingAccounts);
        }
    }
}