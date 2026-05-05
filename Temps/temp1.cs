public async Task<PaymentsDailyResponse> Handle(Query request, CancellationToken ct)
{
    var isOutgoing = request.PaymentType?.ToLower() == "outgoing";

    var fromDateTime = request.FromDate.ToDateTime(new TimeOnly(0, 0));
    var toDateTime = request.ToDate.ToDateTime(new TimeOnly(0, 0)).AddDays(1);

    var query = from pyt in _context.Payments
                join ptt in _context.PaymentTypes on pyt.PaymentTypeId equals ptt.PaymentTypeId
                join sts in _context.StatusItems on pyt.StatusId equals sts.StatusId
                join ptyFrom in _context.Parties on pyt.PartyIdFrom equals ptyFrom.PartyId
                join ptyTo in _context.Parties on pyt.PartyIdTo equals ptyTo.PartyId
                join pmt in _context.PaymentMethodTypes 
                    on pyt.PaymentMethodTypeId equals pmt.PaymentMethodTypeId into pmtGroup
                from pmt in pmtGroup.DefaultIfEmpty()

                join proj in _context.WorkEfforts 
                    on pyt.WorkEffortId equals proj.WorkEffortId into projJoin
                from proj in projJoin.DefaultIfEmpty()

                join cc in _context.CostCenters 
                    on pyt.CostCenterId equals cc.CostCenterId into ccJoin
                from cc in ccJoin.DefaultIfEmpty()

                join sr in _context.SalesRequests 
                    on pyt.SalesRequestId equals sr.SalesRequestId into srJoin
                from sr in srJoin.DefaultIfEmpty()

                join prod in _context.Products 
                    on sr.ProductId equals prod.ProductId into prodJoin
                from prod in prodJoin.DefaultIfEmpty()

                where 
                    // Created OR Last Updated within the date range
                    (
                        (pyt.CreatedStamp >= fromDateTime && pyt.CreatedStamp < toDateTime) ||
                        (pyt.LastUpdatedStamp >= fromDateTime && pyt.LastUpdatedStamp < toDateTime)
                    )
                    && (isOutgoing 
                        ? ptt.ParentTypeId == "DISBURSEMENT" 
                        : ptt.ParentTypeId != "DISBURSEMENT")

                select new PaymentRecordDto
                {
                    PaymentId = pyt.PaymentId,
                    PaymentTypeId = pyt.PaymentTypeId,
                    PaymentTypeDescription = request.Language == "ar" 
                        ? ptt.DescriptionArabic : ptt.Description,

                    PaymentMethodId = pyt.PaymentMethodId,
                    PaymentMethodTypeId = pyt.PaymentMethodTypeId,
                    PaymentMethodTypeDescription = pmt != null
                        ? (request.Language == "ar" ? pmt.DescriptionArabic : pmt.Description)
                        : null,

                    PartyIdFrom = pyt.PartyIdFrom,
                    PartyIdFromName = ptyFrom.Description ?? string.Empty,
                    PartyIdTo = pyt.PartyIdTo,
                    PartyIdToName = ptyTo.Description ?? string.Empty,

                    StatusId = pyt.StatusId,
                    StatusDescription = request.Language == "ar" 
                        ? sts.DescriptionArabic : sts.Description,
                    StatusDescriptionEnglish = sts.Description,

                    EffectiveDate = pyt.EffectiveDate,
                    Comments = pyt.Comments,
                    PaymentRefNum = pyt.PaymentRefNum,
                    PaymentPreferenceId = pyt.PaymentPreferenceId,
                    ActualCurrencyAmount = pyt.ActualCurrencyAmount ?? pyt.Amount,
                    OverrideGlAccountId = pyt.OverrideGlAccountId,
                    OrganizationPartyId = ptt.ParentTypeId == "DISBURSEMENT" 
                        ? pyt.PartyIdFrom : pyt.PartyIdTo,

                    Amount = pyt.Amount,
                    CurrencyUomId = pyt.CurrencyUomId ?? "EGP",
                    IsDisbursement = ptt.ParentTypeId == "DISBURSEMENT",

                    ChequeNumber = pyt.ChequeNumber,
                    ChequeDate = pyt.ChequeDate,
                    CertificateNumber = null,
                    ProjectName = proj != null ? proj.ProjectName : null,
                    CostCenterDescription = cc != null ? cc.Description : null,
                    ProductId = prod != null ? prod.ProductId : null,
                    BuildingNumber = prod != null ? prod.BuildingNumber : null,
                };

    var data = await query.ToListAsync(ct);

    // === Post-processing: Calculate DaysUntilDue and DueStatusArabic ===
    foreach (var record in data)
    {
        var effectiveDate = record.EffectiveDate ?? DateHelper.Today;
        record.DaysUntilDue = effectiveDate.DayNumber - DateHelper.Today.DayNumber;

        if (record.StatusId != "PMNT_NOT_PAID")
        {
            record.DueStatusArabic = record.StatusDescription;
            continue;
        }

        var type = record.IsDisbursement ? "دفعة" : "مستحق";
        var typePaid = record.IsDisbursement ? "دفعة مستحقة" : "مستحق";

        var quarterText = GetQuarterArabic(effectiveDate);

        if (record.DaysUntilDue < 0)
        {
            var daysOverdue = Math.Abs(record.DaysUntilDue);
            record.DueStatusArabic = daysOverdue <= 30
                ? $"{type} متأخرة منذ {daysOverdue} يوم"
                : $"{type} متأخرة جداً {quarterText}";
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

    return new PaymentsDailyResponse
    {
        Data = data,
        Total = data.Count
    };
}