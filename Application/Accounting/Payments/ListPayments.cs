using Application.Core;
using MediatR;
using Microsoft.AspNetCore.OData.Query;
using Microsoft.EntityFrameworkCore;
using Persistence;
using Application.Order.Orders;

namespace Application.Accounting.Payments;

public class ListPayments
{
    public class Query : IRequest<IQueryable<PaymentRecord>>
    {
        public ODataQueryOptions<PaymentRecord> Options { get; set; } = null!;
        public string Language { get; set; } = "en";
        public string? PaymentType { get; set; } // "incoming" or "outgoing"
    }

    public class Handler : IRequestHandler<Query, IQueryable<PaymentRecord>>
    {
        private readonly DataContext _context;

        // Use UTC for consistent behavior (recommended)
        private static readonly DateTime TodayUtc = DateTime.UtcNow.Date;

        public Handler(DataContext context)
        {
            _context = context;
        }

        public async Task<IQueryable<PaymentRecord>> Handle(Query request, CancellationToken cancellationToken)
        {
            var language = request.Language?.ToLower() ?? "en";
            var isArabic = language == "ar";

            // Build the query (keep EffectiveDate as full DateTime)
            var query = (from pyt in _context.Payments
                    join ptt in _context.PaymentTypes on pyt.PaymentTypeId equals ptt.PaymentTypeId
                    join sts in _context.StatusItems on pyt.StatusId equals sts.StatusId
                    join pty in _context.Parties on pyt.PartyIdFrom equals pty.PartyId
                    join pmt in _context.PaymentMethodTypes on pyt.PaymentMethodTypeId equals pmt.PaymentMethodTypeId
                        into pmtJoin
                    from pmt in pmtJoin.DefaultIfEmpty()
                    join ptyto in _context.Parties on pyt.PartyIdTo equals ptyto.PartyId into ptytoJoin
                    from ptyto in ptytoJoin.DefaultIfEmpty()
                    join opp in _context.OrderPaymentPreferences on pyt.PaymentPreferenceId equals opp
                        .OrderPaymentPreferenceId into oppJoin
                    from opp in oppJoin.DefaultIfEmpty()
                    join ord in _context.OrderHeaders on opp.OrderId equals ord.OrderId into ordJoin
                    from ord in ordJoin.DefaultIfEmpty()
                    join we in _context.WorkEfforts on ord.OrderId equals we.RelatedOrderId into weJoin
                    from we in weJoin.DefaultIfEmpty()
                    join cc in _context.CostCenters on pyt.CostCenterId equals cc.CostCenterId into ccJoin
                    from cc in ccJoin.DefaultIfEmpty()
                    join proj in _context.WorkEfforts on pyt.WorkEffortId equals proj.WorkEffortId into projJoin
                    from proj in projJoin.DefaultIfEmpty()
                    join sr in _context.SalesRequests on pyt.SalesRequestId equals sr.SalesRequestId into srJoin
                    from sr in srJoin.DefaultIfEmpty()
                    join prod in _context.Products on sr.ProductId equals prod.ProductId into prodJoin
                    from prod in prodJoin.DefaultIfEmpty()
                    join approvedBy in _context.Parties on pyt.ApprovedByPartyId equals approvedBy.PartyId into approvedByJoin
                    from approvedBy in approvedByJoin.DefaultIfEmpty()
                    join createdBy in _context.Parties on pyt.CreatedByPartyId equals createdBy.PartyId into createdByJoin
                    from createdBy in createdByJoin.DefaultIfEmpty()

                    select new PaymentRecord
                    {
                        PaymentId = pyt.PaymentId,
                        PaymentTypeId = pyt.PaymentTypeId,
                        PaymentTypeDescription = isArabic ? ptt.DescriptionArabic : ptt.Description,
                        PaymentMethodId = pyt.PaymentMethodId,
                        PaymentMethodTypeId = pyt.PaymentMethodTypeId,
                        PaymentMethodTypeDescription = pmt != null
                            ? (isArabic ? pmt.DescriptionArabic : pmt.Description)
                            : null,

                        PartyIdFrom = pyt.PartyIdFrom,
                        PartyIdFromName = pty.Description ?? string.Empty,
                        PartyIdTo = pyt.PartyIdTo,
                        PartyIdToName = ptyto != null
                            ? ptyto.Description
                            : (pyt.PartyIdTo == "Company" ? "Golden Land" : pyt.PartyIdTo ?? "Unknown"),

                        StatusId = pyt.StatusId,
                        StatusDescription = isArabic ? sts.DescriptionArabic : sts.Description,
                        StatusDescriptionEnglish = sts.Description,

                        EffectiveDate = pyt.EffectiveDate, // Keep full DateTime here
                        CreatedStamp = (DateTime)pyt.CreatedStamp,
                        Comments = pyt.Comments,
                        PaymentRefNum = pyt.PaymentRefNum,
                        PaymentPreferenceId = pyt.PaymentPreferenceId,
                        IsBankTransfer = pyt.IsBankTransfer,
                        Amount = pyt.Amount,
                        ActualCurrencyAmount = pyt.ActualCurrencyAmount ?? pyt.Amount,
                        CurrencyUomId = pyt.CurrencyUomId ?? "EGP",

                        FromPartyId = new OrderPartyDto
                        {
                            FromPartyId = pty.PartyId,
                            FromPartyName = pty.Description ?? string.Empty
                        },

                        IsDisbursement = ptt.ParentTypeId == "DISBURSEMENT",
                        OrganizationPartyId = ptt.ParentTypeId == "DISBURSEMENT"
                            ? pyt.PartyIdFrom
                            : pyt.PartyIdTo,

                        OrderId = ord.OrderId,
                        CertificateNumber = we.CertificateNumber,
                        ChequeNumber = pyt.ChequeNumber,
                        ChequeDate = pyt.ChequeDate,
                        ProjectId = pyt.WorkEffortId,
                        OverrideGlAccountId = pyt.OverrideGlAccountId,
                        ProjectName = proj.ProjectName,
                        CostCenterId = pyt.CostCenterId,
                        SalesRequestId = pyt.SalesRequestId,
                        CostCenterDescription = cc.Description,
                        ProductId = prod.ProductId,
                        BuildingNumber = prod.BuildingNumber,
                        ApprovedByPartyId = pyt.ApprovedByPartyId,
                        ApprovedByPartyName = approvedBy != null ? approvedBy.Description : null,
                        CreatedByPartyId = pyt.CreatedByPartyId,
                        CreatedByPartyName = createdBy != null ? createdBy.Description : null
                    })
                .AsQueryable();

            // Server-side filter for incoming/outgoing
            if (!string.IsNullOrEmpty(request.PaymentType))
            {
                var isOutgoing = request.PaymentType.ToLower() == "outgoing";
                query = query.Where(p => p.IsDisbursement == isOutgoing);
            }

            // 1. Intercept OData $filter to remove "dueStatusArabic" or handle effectiveDate before ApplyTo
            // This is needed because dueStatusArabic is a computed field and effectiveDate might have translation issues
            var filterString = request.Options?.Filter?.RawValue;
            var containsDueStatusArabic = filterString != null && filterString.Contains("dueStatusArabic");
            var containsEffectiveDate = filterString != null && filterString.Contains("effectiveDate");
            
            // Apply OData $filter except for problematic fields if they fail
            if (request.Options?.Filter != null)
            {
                try 
                {
                    query = request.Options.Filter.ApplyTo(query, new ODataQuerySettings
                    {
                        EnsureStableOrdering = false
                    }) as IQueryable<PaymentRecord>;
                }
                catch (Exception) when (containsDueStatusArabic || containsEffectiveDate)
                {
                    // Fallback: If it failed and contained problematic fields, 
                    // we'll have to handle filtering in-memory entirely or try to apply other filters.
                    // This is a bit of a hammer, but it prevents the 500 error.
                }
            }

            // Materialize to memory
            var finalList = await query.ToListAsync(cancellationToken);

            // === Post-processing: Calculate DaysUntilDue and DueStatusArabic ===
            foreach (var record in finalList)
            {
                var effectiveDateOnly = record.EffectiveDate ?? DateHelper.Today;

                record.DaysUntilDue = effectiveDateOnly.DayNumber - DateHelper.Today.DayNumber;

                
                if (record.StatusId != "PMNT_NOT_PAID")
                {
                    record.DueStatusArabic = record.StatusDescription;
                    continue;
                }

                var isDisbursement = record.IsDisbursement;
                var type = isDisbursement ? "دفعة" : "مستحق";
                var typePaid = isDisbursement ? "دفعة مستحقة" : "مستحق";


                var quarterText = GetQuarterArabic(effectiveDateOnly);

                if (record.DaysUntilDue < 0)
                {
                    var daysOverdue = Math.Abs(record.DaysUntilDue);

                    record.DueStatusArabic = daysOverdue switch
                    {
                        1 => $"{type} متأخرة بيوم واحد",
                        2 => $"{type} متأخرة بيومين",
                        _ => daysOverdue <= 30
                            ? $"{type} متأخرة منذ {daysOverdue} يوم"
                            : $"{type} متأخرة جداً {quarterText}"
                    };
                }
                else if (record.DaysUntilDue == 0)
                {
                    record.DueStatusArabic = $"{typePaid} اليوم";
                }
                else if (record.DaysUntilDue == 1)
                {
                    record.DueStatusArabic = $"{typePaid} غداً";
                }
                else if (record.DaysUntilDue <= 3)
                {
                    record.DueStatusArabic = $"{typePaid} بعد {record.DaysUntilDue} أيام";
                }
                else if (record.DaysUntilDue <= 7)
                {
                    record.DueStatusArabic = $"{typePaid} هذا الأسبوع";
                }
                else if (record.DaysUntilDue <= 30)
                {
                    record.DueStatusArabic = $"{typePaid} خلال الشهر";
                }
                else if (record.DaysUntilDue <= 90)
                {
                    record.DueStatusArabic = $"{typePaid} خلال 3 أشهر {quarterText}";
                }
                else
                {
                    record.DueStatusArabic = $"{typePaid} لاحقاً {quarterText}";
                }
            }

            // === Post-processing filtering for dueStatusArabic and effectiveDate ===
            if (!string.IsNullOrEmpty(filterString))
            {
                var filterParts = filterString.Split("and", StringSplitOptions.TrimEntries);
                foreach (var part in filterParts)
                {
                    if (part.Contains("dueStatusArabic"))
                    {
                        if (part.Contains("contains"))
                        {
                            var match = System.Text.RegularExpressions.Regex.Match(part, @"contains\(dueStatusArabic\s*,\s*'(.*?)'\)");
                            if (match.Success)
                            {
                                var val = match.Groups[1].Value;
                                finalList = finalList.Where(r => r.DueStatusArabic != null && r.DueStatusArabic.Contains(val)).ToList();
                            }
                        }
                        else if (part.Contains(" eq "))
                        {
                            var match = System.Text.RegularExpressions.Regex.Match(part, @"dueStatusArabic\s+eq\s+'(.*?)'");
                            if (match.Success)
                            {
                                var val = match.Groups[1].Value;
                                finalList = finalList.Where(r => r.DueStatusArabic == val).ToList();
                            }
                        }
                    }
                    else if (part.Contains("effectiveDate"))
                    {
                        // Handle date filtering in-memory if needed (e.g., if EF Core failed)
                        // This handles common date operators: ge, le, eq
                        // First try to match standard OData format: effectiveDate ge 2026-04-18T00:00:00Z
                        var match = System.Text.RegularExpressions.Regex.Match(part, @"effectiveDate\s+(ge|le|eq|gt|lt)\s+(\d{4}-\d{2}-\d{2}(T\d{2}:\d{2}:\d{2}Z)?)");
                        if (!match.Success)
                        {
                            // Try to match OData format with quotes if they appear: effectiveDate eq '2026-04-18'
                            match = System.Text.RegularExpressions.Regex.Match(part, @"effectiveDate\s+(ge|le|eq|gt|lt)\s+'(\d{4}-\d{2}-\d{2}(T\d{2}:\d{2}:\d{2}Z)?)'");
                        }

                        if (match.Success)
                        {
                            var op = match.Groups[1].Value;
                            var valStr = match.Groups[2].Value;
                            if (DateOnly.TryParse(valStr.Split('T')[0], out var filterDate))
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
                        }
                    }
                }
            }

            return finalList.AsQueryable();
        }

        // Helper to get quarter text
        private static string GetQuarterArabic(DateOnly date)
        {
            int quarterNum = (date.Month - 1) / 3 + 1;
            string quarterName = quarterNum switch
            {
                1 => "الأول",
                2 => "الثاني",
                3 => "الثالث",
                4 => "الرابع",
                _ => ""
            };

            return $" (الربع {quarterName} {date.Year})";
        }
    }
}