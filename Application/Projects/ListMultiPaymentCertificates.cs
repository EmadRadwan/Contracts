using Application.Interfaces;
using MediatR;
using Microsoft.AspNetCore.OData.Query;
using Microsoft.EntityFrameworkCore;
using Persistence;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Application.Projects;

namespace Application.MultiPaymentCertificates
{
    public class ListMultiPaymentCertificates
    {
        public class Query : IRequest<IQueryable<MultiPaymentCertificateRecord>>
        {
            public ODataQueryOptions<MultiPaymentCertificateRecord> Options { get; set; }
            public string Language { get; set; }
        }

        public class Handler : IRequestHandler<Query, IQueryable<MultiPaymentCertificateRecord>>
        {
            private readonly DataContext _context;

            public Handler(DataContext context)
            {
                _context = context;
            }

            public async Task<IQueryable<MultiPaymentCertificateRecord>> Handle(Query request, CancellationToken cancellationToken)
            {
                var language = request.Language;

                // REFACTOR: Structured query to fetch MultiPaymentCertificate data with necessary joins
                // Purpose: Aligns with MultiPaymentCertificate interface, ensuring all required fields are included
                // Context: Matches the structure of ProjectCertificateRecords handler but tailored for PAYMENT_CERTIFICATE
                var query = from we in _context.WorkEfforts.AsNoTracking()
                            join si in _context.StatusItems on we.CurrentStatusId equals si.StatusId into statusGroup
                            from si in statusGroup.DefaultIfEmpty()
                            join pm in _context.PaymentMethods on we.PaymentMethodId equals pm.PaymentMethodId into paymentGroup
                            from pm in paymentGroup.DefaultIfEmpty()
                            where we.WorkEffortTypeId == "PAYMENT_CERTIFICATE"
                            select new MultiPaymentCertificateRecord
                            {
                                WorkEffortId = we.WorkEffortId,
                                Code = we.CertificateNumber,
                                Date = (DateTime)we.CreatedDate,
                                Description = we.Description,
                                PaymentMethodId = we.PaymentMethodId,
                                PaymentMethodDescription = pm.Description,
                                StatusDescription = language == "ar" ? si.DescriptionArabic : si.Description,
                                CurrentStatusId = we.CurrentStatusId
                            };

                return query;
            }
        }
    }

}