using Application.Order.Orders;
using MediatR;
using Persistence;

namespace Application.Accounting.BillingAccounts;

public class GetBillingAccountBalance
{
    public class Query : IRequest<Result<BillingAccountBalanceDto>>
    {
        public string BillingAccountId { get; set; }
    }

    public class Handler : IRequestHandler<Query, Result<BillingAccountBalanceDto>>
    {
        private readonly IOrderHelperService _orderHelperService;
        private readonly DataContext _context;

        public Handler(IOrderHelperService orderHelperService, DataContext context)
        {
            _orderHelperService = orderHelperService;
            _context = context;
        }

        public async Task<Result<BillingAccountBalanceDto>> Handle(Query request, CancellationToken cancellationToken)
        {
            var billingAccount = await _context.BillingAccounts.FindAsync(request.BillingAccountId);
            var billingAccountBalance = await _orderHelperService.GetBillingAccountBalance(billingAccount);

            var result = new BillingAccountBalanceDto()
            {
                BillingAccountBalance = billingAccountBalance
            };
            
            return Result<BillingAccountBalanceDto>.Success(result);
        }
    }
}