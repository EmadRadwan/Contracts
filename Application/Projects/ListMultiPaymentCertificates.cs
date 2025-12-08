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

            public async Task<IQueryable<MultiPaymentCertificateRecord>> Handle(Query request,
                CancellationToken cancellationToken)
            {
                var language = request.Language;

                var query = from we in _context.WorkEfforts.AsNoTracking()
                    join gl in _context.GlAccounts on we.GlAccountId equals gl.GlAccountId into glGroup
                    from gl in glGroup.DefaultIfEmpty()
                    where we.WorkEffortTypeId == "PAYMENT_CERTIFICATE"
                    select new MultiPaymentCertificateRecord
                    {
                        WorkEffortId = we.WorkEffortId,
                        Code = we.CertificateNumber,
                        Date = (DateTime)we.EstimatedStartDate,
                        Description = we.Description,

                        // REFACTOR: Status description logic unchanged – kept Arabic/English fallback based on Language
                        StatusDescription = language == "ar"
                            ? (we.CurrentStatusId == "WEPR_CREATED" ? "تم الإنشاء"
                                : we.CurrentStatusId == "WEPR_APPROVED" ? "تمت الموافقة"
                                : we.CurrentStatusId == "WEPR_COMPLETE" ? "مكتمل"
                                : "غير معروف")
                            : (we.CurrentStatusId == "WEPR_CREATED" ? "Created"
                                : we.CurrentStatusId == "WEPR_APPROVED" ? "Approved"
                                : we.CurrentStatusId == "WEPR_COMPLETE" ? "Complete"
                                : "Unknown"),

                        CurrentStatusId = we.CurrentStatusId,
                        GlAccountId = we.GlAccountId,

                        // REFACTOR: New field – Arabic account name from GLAccounts table
                        AccountName = gl != null ? gl.AccountNameArabic : null
                    };

                return query;
            }
        }
    }
}