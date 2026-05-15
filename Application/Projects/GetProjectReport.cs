using Application.Accounting.Payments;
using Application.Core;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Persistence;

namespace Application.Projects
{
    public class GetProjectReport
    {
        public class Query : IRequest<ProjectReportDto>
        {
            public string ProjectId { get; set; } = null!;
            public DateTime? ExpensesStartDate { get; set; }
            public DateTime? ExpensesEndDate { get; set; }
            public bool ExpensesAllData { get; set; }
            public DateTime? RevenuesStartDate { get; set; }
            public DateTime? RevenuesEndDate { get; set; }
            public bool RevenuesAllData { get; set; }
        }

        public class Handler : IRequestHandler<Query, ProjectReportDto>
        {
            private readonly DataContext _context;

            public Handler(DataContext context)
            {
                _context = context;
            }

            public async Task<ProjectReportDto> Handle(Query request, CancellationToken cancellationToken)
            {
                var expenses = await GetExpenses(request, cancellationToken);
                var revenues = await GetRevenues(request, cancellationToken);
                var directPayments = await GetDirectPayments(request, cancellationToken);
                var operatingExpenses = await GetOperatingExpenses(request, cancellationToken);
                var accountingTransactions = await GetAccountingTransactions(request, cancellationToken);

                // Filter out duplicates across expense sections
                var expensePaymentIds = expenses
                    .Where(e => !string.IsNullOrEmpty(e.PaymentId))
                    .Select(e => e.PaymentId!)
                    .ToHashSet();

                var filteredDirectPayments = directPayments
                    .Where(dp => !expensePaymentIds.Contains(dp.PaymentId))
                    .ToList();

                var reportedPaymentIds = expensePaymentIds;
                foreach (var dp in filteredDirectPayments)
                {
                    reportedPaymentIds.Add(dp.PaymentId);
                }

                var filteredOperatingExpenses = operatingExpenses
                    .Where(oe => !reportedPaymentIds.Contains(oe.PaymentId))
                    .ToList();

                return new ProjectReportDto
                {
                    Expenses = expenses,
                    Revenues = revenues,
                    DirectPayments = filteredDirectPayments,
                    OperatingExpenses = filteredOperatingExpenses,
                    AccountingTransactions = accountingTransactions
                };
            }

            private async Task<List<ProjectExpenseRecord>> GetExpenses(Query request, CancellationToken ct)
            {
                var query = from header in _context.WorkEfforts.AsNoTracking()
                    join item in _context.WorkEfforts.AsNoTracking()
                        on header.WorkEffortId equals item.WorkEffortParentId
                    let partyId = header.PartyIdSupplier ?? header.PartyIdContractor ?? header.PartyIdEmployee
                    join p in _context.Parties.AsNoTracking() on partyId equals p.PartyId into pJoin
                    from p in pJoin.DefaultIfEmpty()
                    let prodId = item.ProductId ?? item.ServiceId
                    join prod in _context.Products.AsNoTracking() on prodId equals prod.ProductId into prodJoin
                    from prod in prodJoin.DefaultIfEmpty()
                    join opp in _context.OrderPaymentPreferences.AsNoTracking() on header.RelatedOrderId equals
                        opp.OrderId into oppJoin
                    from opp in oppJoin.DefaultIfEmpty()
                    join pyt in _context.Payments.AsNoTracking() on opp.OrderPaymentPreferenceId equals
                        pyt.PaymentPreferenceId into pytJoin
                    from pyt in pytJoin.DefaultIfEmpty()
                    where header.CurrentStatusId == "WEPR_APPROVED"
                          && header.CertificateCategory != "COMPANY_SUPPLY_SALE_CERTIFICATE"
                          && (item.WorkEffortTypeId == "CERTIFICATE_ITEM" ||
                              item.WorkEffortTypeId == "PAYMENT_CERTIFICATE_ITEM")
                    let projId = header.ProjectId ?? item.ProjectId
                    where projId == request.ProjectId
                    where !(header.WorkEffortTypeId == "PAYMENT_CERTIFICATE" && projId == null)
                    select new { header, item, projId, partyId, p, prodId, prod, paymentId = pyt.PaymentId };

                if (!request.ExpensesAllData)
                {
                    if (request.ExpensesStartDate.HasValue)
                        query = query.Where(x =>
                            (x.item.ProcurementDate ?? x.item.EstimatedStartDate ??
                                x.header.EstimatedStartDate ?? x.header.CreatedDate) >= request.ExpensesStartDate.Value);
                    if (request.ExpensesEndDate.HasValue)
                        query = query.Where(x =>
                            (x.item.ProcurementDate ?? x.item.EstimatedStartDate ??
                                x.header.EstimatedStartDate ?? x.header.CreatedDate) <= request.ExpensesEndDate.Value);
                }

                var results = await query.Select(x => new ProjectExpenseRecord
                {
                    ExpenseItemKey = x.item.WorkEffortId,
                    CertificateKey = x.header.WorkEffortId,
                    CertificateNumber = x.header.WorkEffortTypeId == "PROJECT_CERTIFICATE"
                        ? x.header.CertificateNumber
                        : null,
                    PaymentId =
                        x.header.WorkEffortTypeId == "PAYMENT_CERTIFICATE" ? x.header.WorkEffortId : x.paymentId,
                    ProjectId = x.projId,
                    PartyId = x.partyId,
                    PartyName = x.p.Description,
                    PartyRole = x.header.PartyIdSupplier != null ? "Supplier" :
                        x.header.PartyIdContractor != null ? "Contractor" :
                        x.header.PartyIdEmployee != null ? "Employee" : "Unknown",
                    ProductId = x.prodId,
                    ProductName = x.prod.ProductName,
                    ExpenseDate = x.item.ProcurementDate ??
                                  x.item.EstimatedStartDate ?? x.header.EstimatedStartDate ?? x.header.CreatedDate,
                    RecordType =
                        x.header.WorkEffortTypeId == "PROJECT_CERTIFICATE" &&
                        (x.header.CertificateCategory == "SUPPLY_PROCUREMENT_CERTIFICATE" ||
                         x.header.CertificateCategory == "WORKMANSHIP_CONTRACTING_CERTIFICATE") ? "ProjectCertificate" :
                        x.header.WorkEffortTypeId == "PAYMENT_CERTIFICATE" ? "MultiPaymentCertificate" : "Other",
                    CertificateType = x.header.WorkEffortTypeId == "PROJECT_CERTIFICATE"
                        ? (x.header.CertificateCategory == "SUPPLY_PROCUREMENT_CERTIFICATE"
                            ? "Supply Procurement"
                            : x.header.CertificateCategory == "WORKMANSHIP_CONTRACTING_CERTIFICATE"
                                ? "Workmanship Contracting"
                                : "Project Certificate")
                        : x.header.WorkEffortTypeId == "PAYMENT_CERTIFICATE"
                            ? "Multi-Payment / Direct Expense"
                            : "Unknown",
                    CertificateTypeArabic = x.header.WorkEffortTypeId == "PROJECT_CERTIFICATE"
                        ? (x.header.CertificateCategory == "SUPPLY_PROCUREMENT_CERTIFICATE" ? "توريد مواد" :
                            x.header.CertificateCategory == "WORKMANSHIP_CONTRACTING_CERTIFICATE" ? "مقاولات مصنعيات" :
                            x.header.CertificateCategory == "COMPANY_SUPPLY_SALE_CERTIFICATE" ? "بيع توريدات الشركة" :
                            "مستخلص مشروع")
                        : x.header.WorkEffortTypeId == "PAYMENT_CERTIFICATE"
                            ? "مستخلص دفعات متعددة / مصاريف مباشرة"
                            : "غير معروف",
                    CertificateCategoryCode = x.header.CertificateCategory,
                    CertificateDescription = x.header.Description,
                    ItemDescription = x.item.Description,
                    RelatedPurchaseOrderId = x.header.RelatedOrderId,
                    IsSupplyProcurement = x.header.CertificateCategory == "SUPPLY_PROCUREMENT_CERTIFICATE",
                    IsWorkmanship = x.header.CertificateCategory == "WORKMANSHIP_CONTRACTING_CERTIFICATE",
                    IsMultiPaymentCertificate = x.header.WorkEffortTypeId == "PAYMENT_CERTIFICATE",
                    Quantity = x.item.Quantity ?? 1m,
                    UnitRate = x.item.Rate,
                    GrossAmount = x.item.TotalAmount ?? x.item.Amount ?? 0m,
                    DiscountAmount = x.item.Discount ?? 0m,
                    DeductionsAmount = x.item.Deductions ?? 0m,
                    InsuranceAmount = x.item.Insurance ?? 0m,
                    TransportationExpensesAmount = x.item.TransportationExpenses ?? 0m,
                    GratuitiesAmount = x.item.Gratuities ?? 0m,
                    AchievementPercentage = x.header.CertificateCategory == "WORKMANSHIP_CONTRACTING_CERTIFICATE"
                        ? (x.item.AchievementPercent ?? 0m)
                        : (x.header.CertificateCategory == "SUPPLY_PROCUREMENT_CERTIFICATE" ||
                           x.header.WorkEffortTypeId == "PAYMENT_CERTIFICATE"
                            ? 100m
                            : (x.item.AchievementPercent ?? 0m))
                }).ToListAsync(ct);

                foreach (var r in results)
                {
                    r.NetCertifiedAmount = r.GrossAmount - r.DiscountAmount - r.DeductionsAmount - r.InsuranceAmount
                                           + r.TransportationExpensesAmount + r.GratuitiesAmount;
                }

                return results;
            }

            private async Task<List<ProjectRevenueRecord>> GetRevenues(Query request, CancellationToken ct)
            {
                var today = DateTime.Today;

                var query = from p in _context.Payments.AsNoTracking()
                    join pf in _context.Parties.AsNoTracking() on p.PartyIdFrom equals pf.PartyId into pfJoin
                    from pf in pfJoin.DefaultIfEmpty()
                    join pt_type in _context.PaymentTypes.AsNoTracking() on p.PaymentTypeId equals pt_type.PaymentTypeId
                        into ptJoin
                    from pt_type in ptJoin.DefaultIfEmpty()
                    join sr in _context.SalesRequests.AsNoTracking() on p.SalesRequestId equals sr.SalesRequestId into
                        srJoin
                    from sr in srJoin.DefaultIfEmpty()
                    join apt in _context.Products.AsNoTracking() on sr.ProductId equals apt.ProductId into aptJoin
                    from apt in aptJoin.DefaultIfEmpty()
                    join proj in _context.WorkEfforts.AsNoTracking() on (apt.ProjectId ?? p.WorkEffortId) equals
                        proj.WorkEffortId into projJoin
                    from proj in projJoin.DefaultIfEmpty()
                    where p.PartyIdTo == "Company"
                          && p.SalesRequestId != null
                          && (p.PaymentTypeId == "RECEIPT_ADVANCE_PAYMENT"
                              || p.PaymentTypeId == "RECEIPT_DUE_INSTALLMENT"
                              || p.PaymentTypeId == "RECEIPT_MAINTENANCE_AMOUNT")
                          && p.Amount > 0m
                    let projId = apt.ProjectId ?? p.WorkEffortId
                    where projId == request.ProjectId
                    select new { p, pf, pt_type, sr, apt, proj, projId };

                if (!request.RevenuesAllData)
                {
                    if (request.RevenuesStartDate.HasValue)
                        query = query.Where(x => x.p.EffectiveDate >= DateOnly.FromDateTime(request.RevenuesStartDate.Value));
                    if (request.RevenuesEndDate.HasValue)
                        query = query.Where(x => x.p.EffectiveDate <= DateOnly.FromDateTime(request.RevenuesEndDate.Value));
                }

                var results = await query.Select(x => new ProjectRevenueRecord
                {
                    PaymentId = x.p.PaymentId,
                    SalesRequestId = x.p.SalesRequestId,
                    ApartmentId = x.sr.ProductId,
                    BuildingNumber = x.apt.BuildingNumber,
                    ProjectId = x.projId,
                    ProjectName = x.proj.ProjectName,
                    CustomerPartyId = x.p.PartyIdFrom,
                    CustomerName = x.pf.Description,
                    PaymentTypeId = x.p.PaymentTypeId,
                    PaymentTypeArabic = x.pt_type.DescriptionArabic,
                    RevenueCategory = x.p.PaymentTypeId == "RECEIPT_ADVANCE_PAYMENT" ? "Advance Payment" :
                        x.p.PaymentTypeId == "RECEIPT_DUE_INSTALLMENT" ? "Installment" :
                        x.p.PaymentTypeId == "RECEIPT_MAINTENANCE_AMOUNT" ? "Maintenance Deposit" : "Other",
                    ScheduledAmount = x.p.Amount,
                    CollectedAmount = x.p.StatusId == "PMNT_RECEIVED" ? x.p.Amount : 0m,
                    OutstandingAmount = x.p.StatusId != "PMNT_RECEIVED" ? x.p.Amount : 0m,
                    LateAmount = (x.p.StatusId != "PMNT_RECEIVED" && x.p.EffectiveDate < DateOnly.FromDateTime(today))
                        ? x.p.Amount
                        : 0m,
                    FutureAmount =
                        (x.p.StatusId != "PMNT_RECEIVED" && x.p.EffectiveDate >= DateOnly.FromDateTime(today))
                            ? x.p.Amount
                            : 0m,
                    PaymentStatus = x.p.StatusId == "PMNT_RECEIVED"
                        ? "Received"
                        : (x.p.EffectiveDate < DateOnly.FromDateTime(today) ? "Late" : "Upcoming"),
                    StatusId = x.p.StatusId,
                    StatusDescription = x.p.StatusId == "PMNT_RECEIVED"
                        ? "تم التحصيل"
                        : (x.p.StatusId == "PMNT_NOT_PAID" ? "غير محصل" : x.p.StatusId),
                    DueDate = x.p.EffectiveDate != null ? x.p.EffectiveDate.Value.ToDateTime(TimeOnly.MinValue) : null,
                    CreatedDate = x.p.CreatedStamp,
                    Comments = x.p.Comments,
                    ChequeNumber = x.p.ChequeNumber,
                    DueStatusArabic = null, // Will be calculated in post-processing
                    Year = x.p.EffectiveDate != null ? x.p.EffectiveDate.Value.Year : (int?)null,
                    Quarter = null
                }).ToListAsync(ct);

                // Post-processing for DueStatusArabic and OverdueBucket (Consistent with ListPayments)
                foreach (var r in results)
                {
                    // Calculate Quarter from DueDate (populated from EffectiveDate)
                    if (r.DueDate.HasValue)
                    {
                        var month = r.DueDate.Value.Month;
                        r.Quarter = "Q" + ((month - 1) / 3 + 1);
                    }
                    
                    // Calculate due status using consistent logic
                    r.DueStatusArabic = CalculateDueStatusArabic(r.DueDate, false, r.StatusId, r.StatusDescription);

                    if (r.PaymentStatus == "Received")
                    {
                        r.OverdueBucket = "Received";
                    }
                    else
                    {
                        if (r.DueDate >= today)
                            r.OverdueBucket = "Upcoming";
                        else if (r.DueDate.HasValue)
                        {
                            var diff = (today - r.DueDate.Value).Days;
                            r.DaysOverdue = diff;
                            r.OverdueBucket = diff switch
                            {
                                <= 30 => "Late (1-30 Days)",
                                <= 90 => "Late (31-90 Days)",
                                _ => "Late (Over 90 Days)"
                            };
                        }
                    }
                }

                return results;
            }

            /// <summary>
            /// Consistent DueStatusArabic calculation for revenues (incoming payments)
            /// isDisbursement = false for all revenues here
            /// </summary>
            private static string CalculateDueStatusArabic(DateTime? dueDate, bool isDisbursement, string? statusId,
                string? statusDescription)
            {
                if (statusId != "PMNT_NOT_PAID" && !string.IsNullOrEmpty(statusId))
                    return statusDescription ?? statusId;

                if (dueDate == null)
                    return "غير محدد";

                var effectiveDateOnly = DateOnly.FromDateTime(dueDate.Value);
                var daysUntilDue = effectiveDateOnly.DayNumber - DateHelper.Today.DayNumber;

                var type = isDisbursement ? "دفعة" : "مستحق";
                var typePaid = isDisbursement ? "دفعة مستحقة" : "مستحق";
                var quarterText = GetQuarterArabic(effectiveDateOnly);

                if (daysUntilDue < 0)
                {
                    var daysOverdue = Math.Abs(daysUntilDue);
                    return daysOverdue switch
                    {
                        1 => $"{type} متأخرة بيوم واحد",
                        2 => $"{type} متأخرة بيومين",
                        _ => daysOverdue <= 30
                            ? $"{type} متأخرة منذ {daysOverdue} يوم"
                            : $"{type} متأخرة جداً {quarterText}"
                    };
                }
                else if (daysUntilDue == 0)
                    return $"{typePaid} اليوم";
                else if (daysUntilDue == 1)
                    return $"{typePaid} غداً";
                else if (daysUntilDue <= 3)
                    return $"{typePaid} بعد {daysUntilDue} أيام";
                else if (daysUntilDue <= 7)
                    return $"{typePaid} هذا الأسبوع";
                else if (daysUntilDue <= 30)
                    return $"{typePaid} خلال الشهر";
                else if (daysUntilDue <= 90)
                    return $"{typePaid} خلال 3 أشهر {quarterText}";
                else
                    return $"{typePaid} لاحقاً {quarterText}";
            }

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

            private async Task<List<PaymentRecord>> GetDirectPayments(Query request, CancellationToken ct)
            {
                var project = await _context.WorkEfforts.AsNoTracking()
                    .FirstOrDefaultAsync(w => w.WorkEffortId == request.ProjectId, ct);

                if (project == null) return new List<PaymentRecord>();

                var glAccountId = project.GlAccountId;

                var paymentsQuery = from pyt in _context.Payments.AsNoTracking()
                    join ptt in _context.PaymentTypes.AsNoTracking() on pyt.PaymentTypeId equals ptt.PaymentTypeId
                    join sts in _context.StatusItems.AsNoTracking() on pyt.StatusId equals sts.StatusId
                    join pty in _context.Parties.AsNoTracking() on pyt.PartyIdFrom equals pty.PartyId
                    join pmt in _context.PaymentMethodTypes.AsNoTracking() on pyt.PaymentMethodTypeId equals pmt
                        .PaymentMethodTypeId into pmtJoin
                    from pmt in pmtJoin.DefaultIfEmpty()
                    join ptyto in _context.Parties.AsNoTracking() on pyt.PartyIdTo equals ptyto.PartyId into ptytoJoin
                    from ptyto in ptytoJoin.DefaultIfEmpty()
                    join cc in _context.CostCenters.AsNoTracking() on pyt.CostCenterId equals cc.CostCenterId into
                        ccJoin
                    from cc in ccJoin.DefaultIfEmpty()
                    join proj in _context.WorkEfforts.AsNoTracking() on pyt.WorkEffortId equals proj.WorkEffortId into
                        projJoin
                    from proj in projJoin.DefaultIfEmpty()
                    join sr in _context.SalesRequests.AsNoTracking() on pyt.SalesRequestId equals sr.SalesRequestId into
                        srJoin
                    from sr in srJoin.DefaultIfEmpty()
                    join prod in _context.Products.AsNoTracking() on sr.ProductId equals prod.ProductId into prodJoin
                    from prod in prodJoin.DefaultIfEmpty()
                    join approvedBy in _context.Parties.AsNoTracking() on pyt.ApprovedByPartyId equals approvedBy
                        .PartyId into approvedByJoin
                    from approvedBy in approvedByJoin.DefaultIfEmpty()
                    join createdBy in _context.Parties.AsNoTracking() on pyt.CreatedByPartyId equals createdBy.PartyId
                        into createdByJoin
                    from createdBy in createdByJoin.DefaultIfEmpty()
                    where (
                              (glAccountId != null && pyt.OverrideGlAccountId == glAccountId))
                          && pyt.StatusId == "PMNT_SENT"
                          && ptt.ParentTypeId == "DISBURSEMENT"
                    select new PaymentRecord
                    {
                        PaymentId = pyt.PaymentId,
                        PaymentTypeId = pyt.PaymentTypeId,
                        PaymentTypeDescription = ptt.DescriptionArabic,
                        PaymentMethodId = pyt.PaymentMethodId,
                        PaymentMethodTypeId = pyt.PaymentMethodTypeId,
                        PaymentMethodTypeDescription = pmt != null ? pmt.DescriptionArabic : null,
                        PartyIdFrom = pyt.PartyIdFrom,
                        PartyIdFromName = pty.Description ?? string.Empty,
                        PartyIdTo = pyt.PartyIdTo,
                        PartyIdToName = ptyto != null
                            ? ptyto.Description
                            : (pyt.PartyIdTo == "Company" ? "Golden Land" : pyt.PartyIdTo ?? "Unknown"),
                        StatusId = pyt.StatusId,
                        StatusDescription = sts.DescriptionArabic,
                        StatusDescriptionEnglish = sts.Description,
                        EffectiveDate = pyt.EffectiveDate,
                        CreatedStamp = pyt.CreatedStamp ?? DateTime.MinValue,
                        Comments = pyt.Comments,
                        PaymentRefNum = pyt.PaymentRefNum,
                        PaymentPreferenceId = pyt.PaymentPreferenceId,
                        IsBankTransfer = pyt.IsBankTransfer,
                        Amount = pyt.Amount,
                        ActualCurrencyAmount = pyt.ActualCurrencyAmount ?? pyt.Amount,
                        CurrencyUomId = pyt.CurrencyUomId ?? "EGP",
                        IsDisbursement = true,
                        OrganizationPartyId = pyt.PartyIdFrom,
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
                        CreatedByPartyName = createdBy != null ? createdBy.Description : null,
                        ChequeNumber = pyt.ChequeNumber,
                        ChequeDate = pyt.ChequeDate,
                        DueStatusArabic = CalculateDueStatusArabic(
                            pyt.EffectiveDate != null ? pyt.EffectiveDate.Value.ToDateTime(TimeOnly.MinValue) : null,
                            true,
                            pyt.StatusId,
                            sts.DescriptionArabic)
                    };

                if (!request.ExpensesAllData)
                {
                    if (request.ExpensesStartDate.HasValue)
                    {
                        var start = DateOnly.FromDateTime(request.ExpensesStartDate.Value);
                        paymentsQuery = paymentsQuery.Where(p => p.EffectiveDate >= start);
                    }
                    if (request.ExpensesEndDate.HasValue)
                    {
                        var end = DateOnly.FromDateTime(request.ExpensesEndDate.Value);
                        paymentsQuery = paymentsQuery.Where(p => p.EffectiveDate <= end);
                    }
                }

                var payments = await paymentsQuery.ToListAsync(ct);

                return payments.OrderByDescending(x => x.EffectiveDate).ThenByDescending(x => x.CreatedStamp).ToList();
            }

            private async Task<List<PaymentRecord>> GetAccountingTransactions(Query request, CancellationToken ct)
            {
                var project = await _context.WorkEfforts.AsNoTracking()
                    .FirstOrDefaultAsync(w => w.WorkEffortId == request.ProjectId, ct);

                if (project == null) return new List<PaymentRecord>();

                var glAccountId = project.GlAccountId;

                var transQuery = from ate in _context.AcctgTransEntries.AsNoTracking()
                    join at in _context.AcctgTrans.AsNoTracking() on ate.AcctgTransId equals at.AcctgTransId
                    join att in _context.AcctgTransTypes.AsNoTracking() on at.AcctgTransTypeId equals att.AcctgTransTypeId
                    join pty in _context.Parties.AsNoTracking() on at.PartyId equals pty.PartyId into ptyJoin
                    from pty in ptyJoin.DefaultIfEmpty()
                    where (glAccountId != null && ate.GlAccountId == glAccountId)
                          && (at.AcctgTransTypeId == "GENERAL_JOURNAL" || at.AcctgTransTypeId == "OPENING_BALANCE")
                    select new
                    {
                        at.AcctgTransId,
                        ate.AcctgTransEntrySeqId,
                        at.AcctgTransTypeId,
                        TransTypeDescription = att.Description,
                        at.PartyId,
                        PartyName = pty != null ? pty.Description : string.Empty,
                        at.IsPosted,
                        at.TransactionDate,
                        at.CreatedStamp,
                        EntryDescription = ate.Description,
                        TranDescription = at.Description,
                        EntryVoucherRef = ate.VoucherRef,
                        TranVoucherRef = at.VoucherRef,
                        Amount = ate.DebitCreditFlag == "C" ? -(ate.Amount ?? 0m) : (ate.Amount ?? 0m),
                        CurrencyUomId = ate.CurrencyUomId ?? "EGP",
                        ate.GlAccountId
                    };

                if (!request.ExpensesAllData)
                {
                    if (request.ExpensesStartDate.HasValue)
                    {
                        transQuery = transQuery.Where(t => t.TransactionDate >= request.ExpensesStartDate.Value);
                    }
                    if (request.ExpensesEndDate.HasValue)
                    {
                        transQuery = transQuery.Where(t => t.TransactionDate <= request.ExpensesEndDate.Value);
                    }
                }

                var transactionsData = await transQuery.ToListAsync(ct);

                var transactions = transactionsData.Select(t => new PaymentRecord
                {
                    PaymentId = t.AcctgTransId + ":" + t.AcctgTransEntrySeqId,
                    PaymentTypeId = t.AcctgTransTypeId,
                    PaymentTypeDescription = t.AcctgTransTypeId == "GENERAL_JOURNAL" ? "قيد يومية عام" : 
                                             t.AcctgTransTypeId == "OPENING_BALANCE" ? "رصيد افتتاح" : (t.TransTypeDescription ?? t.AcctgTransTypeId),
                    PartyIdFrom = t.PartyId,
                    PartyIdFromName = t.PartyName,
                    StatusId = t.IsPosted == "Y" ? "POSTED" : "NOT_POSTED",
                    StatusDescription = t.IsPosted == "Y" ? "تم الترحيل" : "غير مرحل",
                    StatusDescriptionEnglish = t.IsPosted == "Y" ? "Posted" : "Not Posted",
                    EffectiveDate = t.TransactionDate.HasValue ? DateOnly.FromDateTime(t.TransactionDate.Value) : null,
                    CreatedStamp = t.CreatedStamp ?? DateTime.MinValue,
                    Comments = t.EntryDescription ?? t.TranDescription ?? string.Empty,
                    PaymentRefNum = t.EntryVoucherRef ?? t.TranVoucherRef ?? string.Empty,
                    Amount = t.Amount,
                    ActualCurrencyAmount = t.Amount,
                    CurrencyUomId = t.CurrencyUomId,
                    IsDisbursement = true,
                    ProjectId = request.ProjectId,
                    OverrideGlAccountId = t.GlAccountId,
                    ProjectName = project.ProjectName,
                    DueStatusArabic = t.IsPosted == "Y" ? "تم الترحيل" : "غير مرحل"
                }).ToList();

                return transactions.OrderByDescending(x => x.EffectiveDate).ThenByDescending(x => x.CreatedStamp).ToList();
            }

            private async Task<List<PaymentRecord>> GetOperatingExpenses(Query request, CancellationToken ct)
            {
                var project = await _context.WorkEfforts.AsNoTracking()
                    .FirstOrDefaultAsync(w => w.WorkEffortId == request.ProjectId, ct);

                if (project == null || string.IsNullOrEmpty(project.OperatingExpenseGlAccountId))
                    return new List<PaymentRecord>();

                var parentGlAccountId = project.OperatingExpenseGlAccountId;

                // Get all child GL accounts recursively
                var allGlAccountIds = new HashSet<string> { parentGlAccountId };
                var childAccounts = await _context.GlAccounts
                    .AsNoTracking()
                    .Select(g => new { g.GlAccountId, g.ParentGlAccountId })
                    .ToListAsync(ct);

                void AddChildren(string parentId)
                {
                    var children = childAccounts.Where(c => c.ParentGlAccountId == parentId).ToList();
                    foreach (var child in children)
                    {
                        if (allGlAccountIds.Add(child.GlAccountId))
                        {
                            AddChildren(child.GlAccountId);
                        }
                    }
                }

                AddChildren(parentGlAccountId);

                var query = from pyt in _context.Payments.AsNoTracking()
                    join ptt in _context.PaymentTypes.AsNoTracking()
                        on pyt.PaymentTypeId equals ptt.PaymentTypeId
                    join sts in _context.StatusItems.AsNoTracking()
                        on pyt.StatusId equals sts.StatusId into stsJoin
                    from sts in stsJoin.DefaultIfEmpty()
                    join pty in _context.Parties.AsNoTracking()
                        on pyt.PartyIdFrom equals pty.PartyId into ptyJoin
                    from pty in ptyJoin.DefaultIfEmpty()
                    join pmt in _context.PaymentMethodTypes.AsNoTracking()
                        on pyt.PaymentMethodTypeId equals pmt.PaymentMethodTypeId into pmtJoin
                    from pmt in pmtJoin.DefaultIfEmpty()
                    join ptyto in _context.Parties.AsNoTracking()
                        on pyt.PartyIdTo equals ptyto.PartyId into ptytoJoin
                    from ptyto in ptytoJoin.DefaultIfEmpty()
                    join cc in _context.CostCenters.AsNoTracking()
                        on pyt.CostCenterId equals cc.CostCenterId into ccJoin
                    from cc in ccJoin.DefaultIfEmpty()
                    where pyt.OverrideGlAccountId != null
                          && allGlAccountIds.Contains(pyt.OverrideGlAccountId)
                          //&& pyt.StatusId == "PMNT_SENT"
                          && (ptt.ParentTypeId == "DISBURSEMENT" || ptt.PaymentTypeId == "DISBURSEMENT")
                    select new PaymentRecord
                    {
                        PaymentId = pyt.PaymentId,
                        PaymentTypeId = pyt.PaymentTypeId,
                        PaymentTypeDescription = ptt.DescriptionArabic,
                        PaymentMethodId = pyt.PaymentMethodId,
                        PaymentMethodTypeId = pyt.PaymentMethodTypeId,
                        PaymentMethodTypeDescription = pmt != null ? pmt.DescriptionArabic : null,
                        PartyIdFrom = pyt.PartyIdFrom,
                        PartyIdFromName = pty != null ? (pty.Description ?? string.Empty) : string.Empty,
                        PartyIdTo = pyt.PartyIdTo,
                        PartyIdToName = ptyto != null
                            ? ptyto.Description
                            : (pyt.PartyIdTo == "Company" ? "Golden Land" : pyt.PartyIdTo ?? "Unknown"),
                        StatusId = pyt.StatusId,
                        StatusDescription = sts != null ? sts.DescriptionArabic : pyt.StatusId,
                        StatusDescriptionEnglish = sts != null ? sts.Description : pyt.StatusId,
                        EffectiveDate = pyt.EffectiveDate,
                        CreatedStamp = pyt.CreatedStamp ?? DateTime.MinValue,
                        Comments = pyt.Comments,
                        PaymentRefNum = pyt.PaymentRefNum,
                        IsBankTransfer = pyt.IsBankTransfer,
                        Amount = pyt.Amount,
                        ActualCurrencyAmount = pyt.ActualCurrencyAmount ?? pyt.Amount,
                        CurrencyUomId = pyt.CurrencyUomId ?? "EGP",
                        OrganizationPartyId = pyt.PartyIdFrom,
                        ProjectId = pyt.WorkEffortId,
                        OverrideGlAccountId = pyt.OverrideGlAccountId,
                        CostCenterId = pyt.CostCenterId,
                        SalesRequestId = pyt.SalesRequestId,
                        CostCenterDescription = cc != null ? cc.Description : null,
                        ChequeNumber = pyt.ChequeNumber,
                        ChequeDate = pyt.ChequeDate,
                    };

                // Apply date filtering if not ExpensesAllData
                if (!request.ExpensesAllData)
                {
                    if (request.ExpensesStartDate.HasValue)
                        query = query.Where(p => p.EffectiveDate >= DateOnly.FromDateTime(request.ExpensesStartDate.Value));

                    if (request.ExpensesEndDate.HasValue)
                        query = query.Where(p => p.EffectiveDate <= DateOnly.FromDateTime(request.ExpensesEndDate.Value));
                }

                var results = await query.ToListAsync(ct);
                

                return results;
            }
        }
    }
}