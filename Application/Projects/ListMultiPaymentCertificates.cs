using MediatR;
using Microsoft.AspNetCore.OData.Query;
using Microsoft.EntityFrameworkCore;
using Persistence;

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
            var language = request.Language ?? "en"; // fallback

            var query = from we in _context.WorkEfforts.AsNoTracking()
                        join gl in _context.GlAccounts 
                            on we.GlAccountId equals gl.GlAccountId into glGroup
                        from gl in glGroup.DefaultIfEmpty()

                        join party in _context.Parties 
                            on we.PartyIdEmployee equals party.PartyId into pGroup
                        from party in pGroup.DefaultIfEmpty()

                        // ─── Left join to AcctgTrans ────────────────────────────────
                        join trans in _context.AcctgTrans
                            on we.WorkEffortId equals trans.WorkEffortId into transGroup
                        from trans in transGroup
                            .Where(t => t.AcctgTransTypeId == "DISBURSEMENT")
                            .DefaultIfEmpty()   // keeps rows even when no transaction exists

                        where we.WorkEffortTypeId == "PAYMENT_CERTIFICATE"

                        select new MultiPaymentCertificateRecord
                        {
                            WorkEffortId          = we.WorkEffortId,
                            Code                  = we.CertificateNumber,
                            Date                  = (DateTime)we.EstimatedStartDate,
                            Description           = we.Description,
                            Notes                 = we.Notes,
                            PartyIdEmployee       = we.PartyIdEmployee,
                            PartyName             = party != null ? party.Description : we.PartyIdEmployee,

                            CurrentStatusId       = we.CurrentStatusId,

                            StatusDescription = language == "ar"
                                ? (we.CurrentStatusId == "WEPR_CREATED" ? "تم الإنشاء"
                                    : we.CurrentStatusId == "WEPR_APPROVED" ? "تمت الموافقة"
                                    : we.CurrentStatusId == "WEPR_COMPLETE" ? "مكتمل"
                                    : "غير معروف")
                                : (we.CurrentStatusId == "WEPR_CREATED" ? "Created"
                                    : we.CurrentStatusId == "WEPR_APPROVED" ? "Approved"
                                    : we.CurrentStatusId == "WEPR_COMPLETE" ? "Complete"
                                    : "Unknown"),

                            GlAccountId           = we.GlAccountId,
                            AccountName           = gl != null ? gl.AccountNameArabic : null,
                            LastUpdatedStamp      = we.LastUpdatedStamp,

                            Amount = _context.WorkEfforts
                                .Where(item => item.WorkEffortParentId == we.WorkEffortId && item.WorkEffortTypeId == "PAYMENT_CERTIFICATE_ITEM")
                                .Sum(item => (decimal?)item.Amount) ?? 0,

                            // ─── New field(s) ───────────────────────────────────────
                            AcctgTransId = trans != null ? trans.AcctgTransId : null
                        };

            return query;
        }
    }
}
}