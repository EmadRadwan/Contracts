using Application.Interfaces;
using MediatR;
using Microsoft.AspNetCore.OData.Query;
using Microsoft.EntityFrameworkCore;
using Persistence;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Application.Projects;

namespace Application.Projects
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

                // REFACTOR: Enhanced query to include all fields inserted in CreateMultiPaymentCertificate
                // Purpose: Ensure query returns all relevant fields (e.g., ChequeNumber, ChequeDate) to match inserted data
                // Why: Aligns with CreateMultiPaymentCertificate's WorkEffort creation for complete data retrieval
                // Context: Added ChequeNumber, ChequeDate, and status translations to match DTO; kept joins for descriptions
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
                                PaymentMethodDescription = pm != null ? pm.Description : null,
                                // REFACTOR: Added status translation logic to match CreateMultiPaymentCertificate
                                // Purpose: Provide consistent status descriptions in English and Arabic
                                // Why: Mirrors the statusDescriptions dictionary used in CreateMultiPaymentCertificate
                                StatusDescription = language == "ar" 
                                    ? (si != null ? si.DescriptionArabic : 
                                        (we.CurrentStatusId == "WEPR_CREATED" ? "تم الإنشاء" : 
                                         we.CurrentStatusId == "WEPR_APPROVED" ? "تمت الموافقة" : 
                                         we.CurrentStatusId == "WEPR_COMPLETE" ? "مكتمل" : "غير معروف"))
                                    : (si != null ? si.Description : 
                                        (we.CurrentStatusId == "WEPR_CREATED" ? "Created" : 
                                         we.CurrentStatusId == "WEPR_APPROVED" ? "Approved" : 
                                         we.CurrentStatusId == "WEPR_COMPLETE" ? "Complete" : "Unknown")),
                                CurrentStatusId = we.CurrentStatusId,
                                // REFACTOR: Added ChequeNumber and ChequeDate to match inserted WorkEffort fields
                                // Purpose: Reflect all fields populated during certificate creation
                                // Why: Ensures query output matches the data structure inserted by CreateMultiPaymentCertificate
                                ChequeNumber = we.ChequeNumber,
                                ChequeDate = we.ChequeDate
                            };

                return query;
            }
        }
    }
}