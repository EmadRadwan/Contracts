using Application.Accounting.Services;
using Application.Shipments.Payments;



using Domain;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Persistence;

namespace Application.Accounting.FinAccounts;

public class DepositWithdrawPayments
{
    public class Command : IRequest<Result<DepositWithdrawResultDto>>
    {
        public List<string> PaymentIds { get; set; }
        public string FinAccountId { get; set; }

        public string GroupInOneTransaction { get; set; } // "Y" or null
        public string PaymentGroupTypeId { get; set; }
        public string PaymentGroupName { get; set; }
    }

    public class Handler : IRequestHandler<Command, Result<DepositWithdrawResultDto>>
    {
        private readonly IFinAccountService _finAcctService;

        public Handler(IFinAccountService finAcctService)
        {
            _finAcctService = finAcctService;
        }

        public async Task<Result<DepositWithdrawResultDto>> Handle(Command request, CancellationToken cancellationToken)
        {
            var dto = new DepositWithdrawPaymentsDto
            {
                PaymentIds = request.PaymentIds,
                FinAccountId = request.FinAccountId,
                GroupInOneTransaction = request.GroupInOneTransaction,
                PaymentGroupTypeId = request.PaymentGroupTypeId,
                PaymentGroupName = request.PaymentGroupName
            };

            return await _finAcctService.DepositOrWithdrawPayments(dto);
        }
    }
}