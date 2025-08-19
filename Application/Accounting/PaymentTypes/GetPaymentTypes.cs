using FluentValidation.Resources;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Persistence;

namespace Application.Shipments.PaymentTypes;

public class GetPaymentTypes
{
    public class Query : IRequest<Result<List<PaymentTypeDto>>>
    {
        public string Language { get; set; }
    }


    public class Handler : IRequestHandler<Query, Result<List<PaymentTypeDto>>>
    {
        private readonly DataContext _context;

        public Handler(DataContext context)
        {
            _context = context;
        }

        public async Task<Result<List<PaymentTypeDto>>> Handle(Query request, CancellationToken cancellationToken)
        {
            var language = request.Language;

            var paymentTypes = await (from pmt in _context.PaymentTypes
                select new PaymentTypeDto
                {
                    PaymentTypeId = pmt.PaymentTypeId,
                    ParentTypeId = pmt.ParentTypeId,
                    Description = language == "ar" ? pmt.DescriptionArabic : pmt.Description
                }).ToListAsync(cancellationToken);

            return Result<List<PaymentTypeDto>>.Success(paymentTypes);
        }
    }
}