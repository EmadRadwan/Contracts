using Application.Order.Orders;
using MediatR;
using Microsoft.AspNetCore.OData.Query;
using Microsoft.EntityFrameworkCore;
using Persistence;
using Application.Core;        // ← Important: for DateHelper

namespace Application.Accounting.Payments;

public class ListPaymentsWithDueStatus
{
    public class Query : IRequest<IQueryable<PaymentRecord>>
    {
        public ODataQueryOptions<PaymentRecord> Options { get; set; } = null!;
        public string Language { get; set; } = "en";
    }

    public class Handler : IRequestHandler<Query, IQueryable<PaymentRecord>>
    {
        private readonly DataContext _context;

        public Handler(DataContext context)
        {
            _context = context;
        }

        public async Task<IQueryable<PaymentRecord>> Handle(Query request, CancellationToken cancellationToken)
        {
            var language = request.Language?.ToLower() ?? "en";
            var isArabic = language == "ar";

            var query = (
                from pyt in _context.Payments

                // Required joins
                join ptt in _context.PaymentTypes on pyt.PaymentTypeId equals ptt.PaymentTypeId
                join sts in _context.StatusItems on pyt.StatusId equals sts.StatusId
                join pty in _context.Parties on pyt.PartyIdFrom equals pty.PartyId

                // LEFT JOINs
                join pmt in _context.PaymentMethodTypes 
                    on pyt.PaymentMethodTypeId equals pmt.PaymentMethodTypeId into pmtJoin
                from pmt in pmtJoin.DefaultIfEmpty()

                join pm in _context.PaymentMethods 
                    on pyt.PaymentMethodId equals pm.PaymentMethodId into pmJoin
                from pm in pmJoin.DefaultIfEmpty()

                join ptyto in _context.Parties 
                    on pyt.PartyIdTo equals ptyto.PartyId into ptytoJoin
                from ptyto in ptytoJoin.DefaultIfEmpty()

                join opp in _context.OrderPaymentPreferences 
                    on pyt.PaymentPreferenceId equals opp.OrderPaymentPreferenceId into oppJoin
                from opp in oppJoin.DefaultIfEmpty()

                join ord in _context.OrderHeaders 
                    on opp.OrderId equals ord.OrderId into ordJoin
                from ord in ordJoin.DefaultIfEmpty()

                join we in _context.WorkEfforts 
                    on ord.OrderId equals we.RelatedOrderId into weJoin
                from we in weJoin.DefaultIfEmpty()

                join cc in _context.CostCenters 
                    on pyt.CostCenterId equals cc.CostCenterId into ccJoin
                from cc in ccJoin.DefaultIfEmpty()

                join proj in _context.WorkEfforts 
                    on pyt.WorkEffortId equals proj.WorkEffortId into projJoin
                from proj in projJoin.DefaultIfEmpty()

                join sr in _context.SalesRequests 
                    on pyt.SalesRequestId equals sr.SalesRequestId into srJoin
                from sr in srJoin.DefaultIfEmpty()

                join prod in _context.Products 
                    on sr.ProductId equals prod.ProductId into prodJoin
                from prod in prodJoin.DefaultIfEmpty()

                select new PaymentRecord
                {
                    PaymentId = pyt.PaymentId,
                    PaymentTypeId = pyt.PaymentTypeId,
                    PaymentTypeDescription = isArabic ? ptt.DescriptionArabic : ptt.Description,

                    PaymentMethodId = pyt.PaymentMethodId,
                    BankName = pm != null ? pm.Description : null,
                    PaymentMethodTypeId = pyt.PaymentMethodTypeId,
                    PaymentMethodTypeDescription = pmt != null
                        ? (isArabic ? pmt.DescriptionArabic : pmt.Description)
                        : null,

                    PartyIdFrom = pyt.PartyIdFrom,
                    PartyIdFromName = pty.Description ?? string.Empty,

                    PartyIdTo = pyt.PartyIdTo,
                    PartyIdToName = ptyto != null
                        ? ptyto.Description
                        : (pyt.PartyIdTo == "Company" ? "Company" : pyt.PartyIdTo ?? "Unknown"),

                    StatusId = pyt.StatusId,
                    StatusDescription = isArabic ? sts.DescriptionArabic : sts.Description,
                    StatusDescriptionEnglish = sts.Description,

                    EffectiveDate = pyt.EffectiveDate,           // DateOnly?
                    Comments = pyt.Comments,
                    PaymentRefNum = pyt.PaymentRefNum,
                    PaymentPreferenceId = pyt.PaymentPreferenceId,

                    Amount = pyt.Amount,
                    ActualCurrencyAmount = pyt.ActualCurrencyAmount ?? pyt.Amount,
                    CurrencyUomId = pyt.CurrencyUomId ?? "EGP",

                    OverrideGlAccountId = pyt.OverrideGlAccountId,

                    FromPartyId = new OrderPartyDto
                    {
                        FromPartyId = pty.PartyId,
                        FromPartyName = pty.Description ?? string.Empty
                    },

                    IsDisbursement = ptt.ParentTypeId == "DISBURSEMENT",
                    OrganizationPartyId = ptt.ParentTypeId == "DISBURSEMENT" 
                        ? pyt.PartyIdFrom 
                        : pyt.PartyIdTo,

                    OrderId = ord != null ? ord.OrderId : null,
                    CertificateNumber = we != null ? we.CertificateNumber : null,

                    ChequeNumber = pyt.ChequeNumber,
                    ChequeDate = pyt.ChequeDate,
                    ProjectId = pyt.WorkEffortId,
                    ProjectName = proj != null ? proj.ProjectName : null,
                    CostCenterId = pyt.CostCenterId,
                    CostCenterDescription = cc != null ? cc.Description : null,
                    ProductId = prod != null ? prod.ProductId : null,
                    BuildingNumber = prod != null ? prod.BuildingNumber : null,
                    CreatedStamp = (DateTime)pyt.CreatedStamp,

                    // Fixed: Use DayNumber for DateOnly compatibility
                    DaysUntilDue = pyt.EffectiveDate.HasValue 
                        ? (pyt.EffectiveDate.Value.DayNumber - DateHelper.Today.DayNumber)
                        : 0
                }
            ).AsQueryable();

            // 1. Intercept OData $filter/$orderby for computed fields that have no SQL mapping.
            // dueStatusArabic is computed in-memory after materialization (see below), so pushing a
            // filter or sort on it into the EF query builds an expression that ToListAsync cannot
            // translate ("... DueStatusArabic ... could not be translated"), producing a 500. The base
            // OData controller re-applies the full $filter/$orderby in-memory on the materialized list
            // (where DueStatusArabic is populated), so skipping the EF-side apply loses nothing.
            var filterString = request.Options?.Filter?.RawValue;
            var orderByString = request.Options?.OrderBy?.RawValue;
            var filterUsesComputed = filterString != null && filterString.Contains("dueStatusArabic");
            var orderByUsesComputed = orderByString != null && orderByString.Contains("dueStatusArabic");

            // Apply OData $filter to the EF query only when it references no computed/unmapped field.
            if (request.Options?.Filter != null && !filterUsesComputed)
            {
                try
                {
                    query = request.Options.Filter.ApplyTo(query, new ODataQuerySettings()) as IQueryable<PaymentRecord>;
                }
                catch (Exception)
                {
                    // Fallback: If it failed (likely due to type mismatch in DateOnly fields),
                    // we'll handle filtering in-memory later.
                }
            }

            if (request.Options?.OrderBy != null && !orderByUsesComputed)
                query = request.Options.OrderBy.ApplyTo(query, new ODataQuerySettings()) as IQueryable<PaymentRecord>;

            // Materialize the query
            var finalList = await query.ToListAsync(cancellationToken);

            // Post-processing for DueStatusArabic
            foreach (var record in finalList)
            {
                var effectiveDate = record.EffectiveDate ?? DateHelper.Today;

                var quarterNum = (effectiveDate.Month - 1) / 3 + 1;
                var quarterArabic = quarterNum switch
                {
                    1 => "الأول",
                    2 => "الثاني",
                    3 => "الثالث",
                    4 => "الرابع",
                    _ => string.Empty
                };
                var quarterAndYear = $"(الربع {quarterArabic} {effectiveDate.Year})";

                if (record.StatusId == "PMNT_NOT_PAID")
                {
                    var type = record.IsDisbursement ? "دفعة" : "مستحق";
                    var typePaid = record.IsDisbursement ? "دفعة مستحقة" : "مستحق";

                    if (record.DaysUntilDue < 0)
                    {
                        var daysOverdue = Math.Abs(record.DaysUntilDue);
                        record.DueStatusArabic = daysOverdue <= 30
                            ? $"{type} متأخرة منذ {daysOverdue} يوم"
                            : $"{type} متأخرة جداً {quarterAndYear}";
                    }
                    else if (record.DaysUntilDue == 0)
                        record.DueStatusArabic = $"{typePaid} اليوم";
                    else if (record.DaysUntilDue == 1)
                        record.DueStatusArabic = $"{typePaid} غداً";
                    else if (record.DaysUntilDue <= 3)
                        record.DueStatusArabic = $"{typePaid} بعد {record.DaysUntilDue} أيام";
                    else if (record.DaysUntilDue <= 7)
                        record.DueStatusArabic = $"{typePaid} هذا الأسبوع";
                    else if (record.DaysUntilDue <= 30)
                        record.DueStatusArabic = $"{typePaid} خلال الشهر";
                    else if (record.DaysUntilDue <= 90)
                        record.DueStatusArabic = $"{typePaid} خلال 3 أشهر {quarterAndYear}";
                    else
                        record.DueStatusArabic = $"{typePaid} لاحقاً {quarterAndYear}";
                }
                else
                {
                    record.DueStatusArabic = record.StatusDescription;
                }
            }

            if (!string.IsNullOrEmpty(filterString))
            {
                var filterParts = filterString.Split("and", StringSplitOptions.TrimEntries);
                foreach (var part in filterParts)
                {
                    if (part.Contains("effectiveDate") || part.Contains("chequeDate"))
                    {
                        var match = System.Text.RegularExpressions.Regex.Match(part, @"(effectiveDate|chequeDate)\s+(ge|le|eq|gt|lt)\s+(')?(\d{4}-\d{2}-\d{2})(T\d{2}:\d{2}:\d{2}(?:\.\d+)?Z)?(')?");
                        if (match.Success)
                        {
                            var field = match.Groups[1].Value;
                            var op = match.Groups[2].Value;
                            var dateStr = match.Groups[4].Value;
                            if (DateOnly.TryParse(dateStr, out var filterDate))
                            {
                                if (field == "effectiveDate")
                                {
                                    finalList = op switch
                                    {
                                        "ge" => finalList.Where(r => r.EffectiveDate >= filterDate).ToList(),
                                        "le" => finalList.Where(r => r.EffectiveDate <= filterDate).ToList(),
                                        "eq" => finalList.Where(r => r.EffectiveDate == filterDate).ToList(),
                                        "gt" => finalList.Where(r => r.EffectiveDate > filterDate).ToList(),
                                        "lt" => finalList.Where(r => r.EffectiveDate < filterDate).ToList(),
                                        _ => finalList
                                    };
                                }
                                else if (field == "chequeDate")
                                {
                                    finalList = op switch
                                    {
                                        "ge" => finalList.Where(r => r.ChequeDate >= filterDate).ToList(),
                                        "le" => finalList.Where(r => r.ChequeDate <= filterDate).ToList(),
                                        "eq" => finalList.Where(r => r.ChequeDate == filterDate).ToList(),
                                        "gt" => finalList.Where(r => r.ChequeDate > filterDate).ToList(),
                                        "lt" => finalList.Where(r => r.ChequeDate < filterDate).ToList(),
                                        _ => finalList
                                    };
                                }
                            }
                        }
                    }
                }
            }

            return finalList.AsQueryable();
        }
    }
}