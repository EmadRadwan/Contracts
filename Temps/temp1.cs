// Inside Handle method, after ToListAsync()
foreach (var record in finalList)
{
    // No need for time checks anymore - EffectiveDate is always date-only
    var effectiveDateOnly = record.EffectiveDate.Date;   // safe

    record.DaysUntilDue = (effectiveDateOnly - DateHelper.Today).Days;

    if (record.StatusId != "PMNT_NOT_PAID")
    {
        record.DueStatusArabic = record.StatusDescription;
        continue;
    }

    var isDisbursement = record.IsDisbursement;
    var type = isDisbursement ? "دفعة" : "مستحق";
    var typePaid = isDisbursement ? "دفعة مستحقة" : "مستحق";

    if (record.PaymentTypeId == "PERMANENT_CUSTODY")
    {
        type = "عهدة";
        typePaid = "عهدة";
    }

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
    // ... rest of your conditions (week, month, etc.)
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