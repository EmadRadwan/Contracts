using AutoMapper;
using MediatR;
using Persistence;
using Microsoft.EntityFrameworkCore;

namespace Application.Accounting.PaymentGroup;

public class GetPaymentGroupMembers
{
    public class Query : IRequest<Result<List<PaymentGroupMemberDto>>>
    {
        public string PaymentGroupId { get; set; }
    }

    public class Handler : IRequestHandler<Query, Result<List<PaymentGroupMemberDto>>>
    {
        private readonly DataContext _context;
        private readonly IMapper _mapper;

        public Handler(DataContext context, IMapper mapper)
        {
            _mapper = mapper;
            _context = context;
        }

        public async Task<Result<List<PaymentGroupMemberDto>>> Handle(Query request, CancellationToken cancellationToken)
        {

                var paymentGroupMembers = await (from pmg in _context.PaymentGroupMembers
                    where pmg.PaymentGroupId == request.PaymentGroupId
                    select new PaymentGroupMemberDto
                    {
                        PaymentGroupId = pmg.PaymentGroupId,
                        PaymentId = pmg.PaymentId,
                        FromDate = pmg.FromDate,
                        ThruDate = pmg.ThruDate,
                    }
                ).ToListAsync(cancellationToken);

            return Result<List<PaymentGroupMemberDto>>.Success(paymentGroupMembers);
        }
    }
}
