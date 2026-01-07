using MediatR;
using Microsoft.EntityFrameworkCore;
using Persistence;

namespace Application.Reports;

public class GetPaymentForReport
{
    public record Query(string PaymentId, string Language = "ar") : IRequest<PaymentReportDto>;

    public class Handler : IRequestHandler<Query, PaymentReportDto>
    {
        private readonly DataContext _context;

        public Handler(DataContext context)
        {
            _context = context;
        }

        public async Task<PaymentReportDto> Handle(Query request, CancellationToken ct)
        {
            var isArabic = request.Language.Equals("ar", StringComparison.OrdinalIgnoreCase);

            var query = from pyt in _context.Payments
                        where pyt.PaymentId == request.PaymentId

                        join ptt in _context.PaymentTypes on pyt.PaymentTypeId equals ptt.PaymentTypeId
                        join sts in _context.StatusItems on pyt.StatusId equals sts.StatusId
                        join pty in _context.Parties on pyt.PartyIdFrom equals pty.PartyId

                        // LEFT JOINs (same as your ListPayments)
                        join pmt in _context.PaymentMethods 
                            on pyt.PaymentMethodId equals pmt.PaymentMethodId into pmtJoin
                        from pmt in pmtJoin.DefaultIfEmpty()

                        join ptyto in _context.Parties 
                            on pyt.PartyIdTo equals ptyto.PartyId into ptytoJoin
                        from ptyto in ptytoJoin.DefaultIfEmpty()

                        join cc in _context.CostCenters 
                            on pyt.CostCenterId equals cc.CostCenterId into ccJoin
                        from cc in ccJoin.DefaultIfEmpty()

                        join proj in _context.WorkEfforts 
                            on pyt.WorkEffortId equals proj.WorkEffortId into projJoin
                        from proj in projJoin.DefaultIfEmpty()

                        select new PaymentReportDto
                        {
                            PaymentId = pyt.PaymentId,
                            PaymentTypeDescription = isArabic ? ptt.DescriptionArabic : ptt.Description,
                            PaymentParentTypeDescription = ptt.ParentTypeId,
                            FromPartyName = pty.Description ?? string.Empty,
                            ToPartyName = ptyto != null 
                                ? ptyto.Description 
                                : (pyt.PartyIdTo == "Company" ? "Company" : pyt.PartyIdTo ?? "Unknown"),
                            StatusDescription = isArabic ? sts.DescriptionArabic : sts.Description,
                            EffectiveDate = (DateTime)pyt.EffectiveDate,
                            Amount = pyt.Amount,
                            CurrencyUomId = pyt.CurrencyUomId ?? "EGP",
                            PaymentMethodId = pyt.PaymentMethodId,
                            PaymentMethodDescription = pmt.Description,
                            ChequeNumber = pyt.ChequeNumber,
                            ChequeDate = pyt.ChequeDate,
                            ProjectName = proj != null ? proj.ProjectName : null,
                            CostCenterDescription = cc != null ? cc.Description : "غير محدد",
                            Comments = pyt.Comments
                        };

            var result = await query.FirstOrDefaultAsync(ct);

            if (result == null)
                throw new Exception($"Payment with ID {request.PaymentId} not found.");

            return result;
        }
    }

}

public class PaymentReportDto
{
    public string PaymentId { get; set; } = string.Empty;
    public string PaymentTypeDescription { get; set; } = string.Empty;
    public string PaymentParentTypeDescription { get; set; } = string.Empty;
    public string FromPartyName { get; set; } = string.Empty;
    public string ToPartyName { get; set; } = string.Empty;
    public string StatusDescription { get; set; } = string.Empty;
    public DateTime EffectiveDate { get; set; }
    public decimal Amount { get; set; }
    public string CurrencyUomId { get; set; } = "EGP";
    public string PaymentMethodId { get; set; } = string.Empty;
    public string PaymentMethodDescription { get; set; } = string.Empty;
    public string? ChequeNumber { get; set; }
    public DateTime? ChequeDate { get; set; }
    public string? ProjectName { get; set; }
    public string? CostCenterDescription { get; set; }
    public string? Comments { get; set; }
}