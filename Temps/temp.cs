using Application.Interfaces;
using MediatR;
using Microsoft.AspNetCore.OData.Query;
using Microsoft.EntityFrameworkCore;
using Persistence;

namespace Application.Order.Orders.Orders;

public class ListOrders
{
    public class Query : IRequest<IQueryable<OrderRecord>>
    {
        public ODataQueryOptions<OrderRecord> Options { get; set; }
        public string Language { get; set; }
    }

    public class Handler : IRequestHandler<Query, IQueryable<OrderRecord>>
    {
        private readonly DataContext _context;

        public Handler(DataContext context)
        {
            _context = context;
        }

        public async Task<IQueryable<OrderRecord>> Handle(Query request, CancellationToken cancellationToken)
        {
            var language = request.Language;

            // REFACTOR: Added left join with WorkEffort using RelatedOrderId to include certificate_number
            // Using left join ensures orders without a related WorkEffort are included with null certificate_number
            // This simplifies the query by directly joining OrderView with WorkEffort, removing the need for OrderHeaderWorkEfforts
            var query = from ov in _context.OrderView.AsNoTracking()
                        join we in _context.WorkEfforts on ov.OrderId equals we.RelatedOrderId into weGroup
                        from we in weGroup.DefaultIfEmpty()
                        select new OrderRecord
                        {
                            OrderId = ov.OrderId,
                            FromPartyName = ov.FromPartyName + " ( " + ov.FromPartyContactNumber + " )",
                            OrderDate = ov.OrderDate,
                            StatusId = ov.OrderStatus,
                            StatusDescription = language == "ar" ? ov.StatusDescriptionArabic :
                                               language == "tr" ? ov.StatusDescriptionTurkish :
                                               ov.StatusDescription,
                            BillingAccountId = ov.BillingAccountId,
                            PaymentMethodId = ov.PaymentMethodId,
                            PaymentMethodTypeId = ov.PaymentMethodTypeId,
                            AgreementId = ov.AgreementId,
                            PaymentId = ov.PaymentId,
                            InvoiceId = ov.InvoiceId,
                            GrandTotal = ov.GrandTotal,
                            OrderTypeId = ov.OrderTypeId,
                            OrderTypeDescription = language == "ar" ? ov.OrderTypeDescriptionArabic :
                                                  language == "tr" ? ov.OrderTypeDescriptionTurkish :
                                                  ov.OrderTypeDescription,
                            CurrencyUomId = ov.CurrencyUomId,
                            CurrencyUomDescription = language == "ar" ? ov.CurrencyUomDescriptionArabic :
                                                    language == "tr" ? ov.CurrencyUomDescriptionTurkish :
                                                    ov.CurrencyUomDescription,
                            FromPartyId = new OrderPartyDto
                            {
                                FromPartyId = ov.FromPartyId,
                                FromPartyName = ov.FromPartyNameDescription
                            },
                            // REFACTOR: Added CertificateNumber from WorkEffort
                            // This field will be null if no WorkEffort is associated with the order via RelatedOrderId
                            CertificateNumber = we != null ? we.CertificateNumber : null
                        };

            return query;
        }
    }
}