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

                var query = from we in _context.WorkEfforts.AsNoTracking()
                            join si in _context.StatusItems on we.CurrentStatusId equals si.StatusId into statusGroup
                            join p in _context.Parties on we.PartyIdEmployee equals p.PartyId into partyGroup
                            from p in partyGroup.DefaultIfEmpty()
                            from si in statusGroup.DefaultIfEmpty()
                            where we.WorkEffortTypeId == "PAYMENT_CERTIFICATE"
                            select new MultiPaymentCertificateRecord
                            {
                                WorkEffortId = we.WorkEffortId,
                                Code = we.CertificateNumber,
                                Date = (DateTime)we.EstimatedStartDate,
                                Description = we.Description,
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
                                PartyIdEmployee = p != null ? p.PartyId : null,
                                PartyEmployeeName = p != null ? p.Description : null
                            };

                return query;
            }
        }
    }
}