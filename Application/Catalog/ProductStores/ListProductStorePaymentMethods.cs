using Application.Shipments.PaymentMethodTypes;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Persistence;

namespace Application.Catalog.ProductStores;

public class ListProductStorePaymentMethods
{
    public class Query : IRequest<Result<List<PaymentMethodTypeDto>>>
    {
        public string Language { get; set; }
    }


    public class Handler : IRequestHandler<Query, Result<List<PaymentMethodTypeDto>>>
    {
        private readonly DataContext _context;

        public Handler(DataContext context)
        {
            _context = context;
        }

        public async Task<Result<List<PaymentMethodTypeDto>>> Handle(Query request, CancellationToken cancellationToken)
        {
            var language = request.Language;
            var paymentMethodTypes = await (from ps in _context.ProductStorePaymentSettings
                join pt in _context.PaymentMethodTypes on ps.PaymentMethodTypeId equals pt.PaymentMethodTypeId
                select new PaymentMethodTypeDto
                {
                    Value = ps.PaymentMethodTypeId,
                    Label = language == "ar" ? pt.DescriptionArabic : pt.Description
                }).ToListAsync(cancellationToken);


            return Result<List<PaymentMethodTypeDto>>.Success(paymentMethodTypes);
        }
    }
}