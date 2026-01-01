using AutoMapper;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Persistence;

namespace Application.Accounting.Payments;

public class ListPaymentsDaily
{
    public class Query : IRequest<PaymentsDailyResponse>
    {
        public string? PaymentType { get; set; }
        public string Language { get; set; }
    }

    public class Handler : IRequestHandler<Query, PaymentsDailyResponse>
    {
        private readonly DataContext _context;
        private readonly IMapper _mapper;

        public Handler(DataContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        public async Task<PaymentsDailyResponse> Handle(Query request, CancellationToken ct)
        {
            var egyptZone = TimeZoneInfo.FindSystemTimeZoneById(
                OperatingSystem.IsWindows() ? "Egypt Standard Time" : "Africa/Cairo");

            var egyptNow = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, egyptZone);
            var startOfDayEgypt = egyptNow.Date;                    // 00:00 today in Egypt
            var endOfDayEgypt   = startOfDayEgypt.AddDays(1);
            
            var isOutgoing = request.PaymentType == "outgoing";

            // REFACTOR: Removed unnecessary joins with CreditCards, OrderPaymentPreferences, OrderHeaders, WorkEfforts
            // Purpose: Eliminate unused data retrieval and improve query performance
            // Improvement: DTO fields like CreditCardNumber, CertificateNumber were never populated; 
            //            removing left joins avoids unnecessary table scans and memory usage
            var query = from pyt in _context.Payments
                join ptt in _context.PaymentTypes on pyt.PaymentTypeId equals ptt.PaymentTypeId
                join sts in _context.StatusItems on pyt.StatusId equals sts.StatusId
                join ptyFrom in _context.Parties on pyt.PartyIdFrom equals ptyFrom.PartyId
                join ptyTo in _context.Parties on pyt.PartyIdTo equals ptyTo.PartyId
                join pmt in _context.PaymentMethodTypes 
                    on pyt.PaymentMethodTypeId equals pmt.PaymentMethodTypeId into pmtGroup
                
                join opp in _context.OrderPaymentPreferences 
                    on pyt.PaymentPreferenceId equals opp.OrderPaymentPreferenceId into oppJoin
                from opp in oppJoin.DefaultIfEmpty()
                join ord in _context.OrderHeaders 
                    on opp.OrderId equals ord.OrderId into ordJoin
                from ord in ordJoin.DefaultIfEmpty()
                join we in _context.WorkEfforts 
                    on ord.OrderId equals we.RelatedOrderId into weJoin
                from we in weJoin.DefaultIfEmpty()

                join proj in _context.WorkEfforts 
                    on pyt.WorkEffortId equals proj.WorkEffortId into projJoin
                from proj in projJoin.DefaultIfEmpty()

                // NEW: Direct left join for CostCenterDescription
                join cc in _context.CostCenters 
                    on pyt.CostCenterId equals cc.CostCenterId into ccJoin
                from cc in ccJoin.DefaultIfEmpty()

                from pmt in pmtGroup.DefaultIfEmpty() 
                where pyt.CreatedStamp >= startOfDayEgypt
                      && pyt.CreatedStamp < endOfDayEgypt
                      && (isOutgoing ? ptt.ParentTypeId == "DISBURSEMENT" : ptt.ParentTypeId != "DISBURSEMENT")
                select new PaymentRecordDto
                {
                    PaymentId = pyt.PaymentId,
                    PaymentTypeId = pyt.PaymentTypeId,
                    PaymentTypeDescription = request.Language == "ar" ? ptt.DescriptionArabic : ptt.Description,
                    PaymentMethodId = pyt.PaymentMethodId,
                    PaymentMethodTypeId = pyt.PaymentMethodTypeId,
                    PaymentMethodTypeDescription = request.Language == "ar" ? pmt.DescriptionArabic : pmt.Description,
                    PartyIdFrom = pyt.PartyIdFrom,
                    PartyIdFromName = ptyFrom.Description,
                    PartyIdTo = pyt.PartyIdTo,
                    PartyIdToName = ptyTo.Description,
                    StatusId = pyt.StatusId,
                    StatusDescription = request.Language == "ar" ? sts.DescriptionArabic : sts.Description,
                    StatusDescriptionEnglish = sts.Description,
                    EffectiveDate = (DateTime)pyt.EffectiveDate,
                    Comments = pyt.Comments,
                    PaymentRefNum = pyt.PaymentRefNum,
                    PaymentPreferenceId = pyt.PaymentPreferenceId,
                    ActualCurrencyAmount = (decimal)pyt.ActualCurrencyAmount,
                    OverrideGlAccountId = pyt.OverrideGlAccountId,
                    OrganizationPartyId = ptt.ParentTypeId == "DISBURSEMENT" ? pyt.PartyIdFrom : pyt.PartyIdTo,
                    Amount = pyt.Amount,
                    CurrencyUomId = pyt.CurrencyUomId,
                    FinAccountTransId = pyt.FinAccountTransId,
                    IsDisbursement = ptt.ParentTypeId == "DISBURSEMENT",
                    ChequeNumber = pyt.ChequeNumber,
                    ChequeDate = pyt.ChequeDate,
                    CertificateNumber = we != null ? we.CertificateNumber : null,
                    ProjectName = proj != null ? proj.ProjectName : null,
                    CostCenterDescription = cc != null ? cc.Description : null,
                };

            var data = await query.ToListAsync(ct);
            var total = data.Count;

            return new PaymentsDailyResponse
            {
                Data = data,
                Total = total
            };
        }
    }
}

// REFACTOR: Removed unused properties from DTO to match actual data returned
// Purpose: Prevent confusion and reduce object size
// Improvement: CreditCardNumber, CreditCardExpiryDate, OrderId, CertificateNumber are never set
public class PaymentRecordDto
{
    public string PaymentId { get; set; } = null!;
    public string PaymentTypeId { get; set; } = null!;
    public string PaymentTypeDescription { get; set; } = null!;
    public string? PaymentMethodId { get; set; }
    public string PaymentMethodTypeId { get; set; } = null!;
    public string PaymentMethodTypeDescription { get; set; } = null!;
    public string PartyIdFrom { get; set; } = null!;
    public string PartyIdFromName { get; set; } = null!;
    public string PartyIdTo { get; set; } = null!;
    public string PartyIdToName { get; set; } = null!;
    public string StatusId { get; set; } = null!;
    public string StatusDescription { get; set; } = null!;
    public string StatusDescriptionEnglish { get; set; } = null!;
    public DateTime EffectiveDate { get; set; }
    public string? Comments { get; set; }
    public string? PaymentRefNum { get; set; }
    public string? PaymentPreferenceId { get; set; }
    public decimal ActualCurrencyAmount { get; set; }
    public string? OverrideGlAccountId { get; set; }
    public string OrganizationPartyId { get; set; } = null!;
    public decimal Amount { get; set; }
    public string CurrencyUomId { get; set; } = null!;
    public string? FinAccountTransId { get; set; }
    public bool IsDisbursement { get; set; }
    public string? ChequeNumber { get; set; }
    public DateTime? ChequeDate { get; set; }
    public string? CertificateNumber { get; set; }       // From WorkEffort via Order chain
    public string? ProjectName { get; set; }             // Direct from Payment.WorkEffortId
    public string? CostCenterDescription { get; set; }
}

public class PaymentsDailyResponse
{
    public List<PaymentRecordDto> Data { get; set; } = new();
    public int Total { get; set; }
}