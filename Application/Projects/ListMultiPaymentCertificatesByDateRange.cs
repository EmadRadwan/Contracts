using MediatR;
using Microsoft.EntityFrameworkCore;
using Persistence;
using Application.Core;

namespace Application.Projects
{
    public class ListMultiPaymentCertificatesByDateRange
    {
        public class Query : IRequest<Result<List<MultiPaymentCertificateRecord>>>
        {
            public DateTime StartDate { get; set; }
            public DateTime EndDate { get; set; }
            public string Language { get; set; }
        }

        public class Handler : IRequestHandler<Query, Result<List<MultiPaymentCertificateRecord>>>
        {
            private readonly DataContext _context;

            public Handler(DataContext context)
            {
                _context = context;
            }

            public async Task<Result<List<MultiPaymentCertificateRecord>>> Handle(Query request, CancellationToken cancellationToken)
            {
                var language = request.Language ?? "en";

                var query = from we in _context.WorkEfforts.AsNoTracking()
                            join gl in _context.GlAccounts
                                on we.GlAccountId equals gl.GlAccountId into glGroup
                            from gl in glGroup.DefaultIfEmpty()

                            join party in _context.Parties
                                on we.PartyIdEmployee equals party.PartyId into pGroup
                            from party in pGroup.DefaultIfEmpty()

                            join trans in _context.AcctgTrans
                                on we.WorkEffortId equals trans.WorkEffortId into transGroup
                            from trans in transGroup
                                .Where(t => t.AcctgTransTypeId == "DISBURSEMENT")
                                .DefaultIfEmpty()

                            where we.WorkEffortTypeId == "PAYMENT_CERTIFICATE"
                                  && we.EstimatedStartDate >= request.StartDate
                                  && we.EstimatedStartDate <= request.EndDate

                            select new MultiPaymentCertificateRecord
                            {
                                WorkEffortId = we.WorkEffortId,
                                Code = we.CertificateNumber,
                                Date = (DateTime)we.EstimatedStartDate,
                                Description = we.Description,
                                Notes = we.Notes,
                                PartyIdEmployee = we.PartyIdEmployee,
                                PartyName = party != null ? party.Description : we.PartyIdEmployee,
                                CurrentStatusId = we.CurrentStatusId,
                                StatusDescription = language == "ar"
                                    ? (we.CurrentStatusId == "WEPR_CREATED" ? "تم الإنشاء"
                                        : we.CurrentStatusId == "WEPR_APPROVED" ? "تمت الموافقة"
                                        : we.CurrentStatusId == "WEPR_COMPLETE" ? "مكتمل"
                                        : "غير معروف")
                                    : (we.CurrentStatusId == "WEPR_CREATED" ? "Created"
                                        : we.CurrentStatusId == "WEPR_APPROVED" ? "Approved"
                                        : we.CurrentStatusId == "WEPR_COMPLETE" ? "Complete"
                                        : "Unknown"),
                                GlAccountId = we.GlAccountId,
                                AccountName = gl != null ? gl.AccountNameArabic : null,
                                LastUpdatedStamp = we.LastUpdatedStamp,
                                Amount = _context.WorkEfforts
                                    .Where(item => item.WorkEffortParentId == we.WorkEffortId && item.WorkEffortTypeId == "PAYMENT_CERTIFICATE_ITEM")
                                    .Sum(item => (decimal?)item.Amount) ?? 0,
                                AcctgTransId = trans != null ? trans.AcctgTransId : null
                            };

                var result = await query.ToListAsync(cancellationToken);
                return Result<List<MultiPaymentCertificateRecord>>.Success(result);
            }
        }
    }
}
