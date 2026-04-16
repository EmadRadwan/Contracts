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
            public DateTime? StartDate { get; set; }
            public DateTime? EndDate { get; set; }
            public bool AllData { get; set; }
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

                return new ProjectReportDto
                {
                    Expenses = expenses,
                    Revenues = revenues
                };
            }

            private async Task<List<ProjectExpenseRecord>> GetExpenses(Query request, CancellationToken ct)
            {
                var query = from header in _context.WorkEfforts.AsNoTracking()
                            join item in _context.WorkEfforts.AsNoTracking()
                                on header.WorkEffortId equals item.WorkEffortParentId
                            
                            let partyId = header.PartyIdSupplier ?? header.PartyIdContractor ?? header.PartyIdEmployee
                            join p in _context.Parties.AsNoTracking() on partyId equals p.PartyId into pJoin from p in pJoin.DefaultIfEmpty()
                            
                            let prodId = item.ProductId ?? item.ServiceId
                            join prod in _context.Products.AsNoTracking() on prodId equals prod.ProductId into prodJoin from prod in prodJoin.DefaultIfEmpty()

                            where header.CurrentStatusId == "WEPR_APPROVED"
                               && header.CertificateCategory != "COMPANY_SUPPLY_SALE_CERTIFICATE"
                               && (item.WorkEffortTypeId == "CERTIFICATE_ITEM" || item.WorkEffortTypeId == "PAYMENT_CERTIFICATE_ITEM")
                            
                            let projId = header.ProjectId ?? item.ProjectId
                            where projId == request.ProjectId

                            // Multi-payment exclusion logic from SQL: 
                            // AND NOT (header.WORK_EFFORT_TYPE_ID = 'PAYMENT_CERTIFICATE' AND COALESCE(header.PROJECT_ID, item.PROJECT_ID) IS NULL)
                            where !(header.WorkEffortTypeId == "PAYMENT_CERTIFICATE" && projId == null)

                            select new { header, item, projId, partyId, p, prodId, prod };

                if (!request.AllData)
                {
                    if (request.StartDate.HasValue)
                        query = query.Where(x => (x.item.ProcurementDate ?? x.item.EstimatedStartDate ?? x.header.EstimatedStartDate ?? x.header.CreatedDate) >= request.StartDate.Value);
                    if (request.EndDate.HasValue)
                        query = query.Where(x => (x.item.ProcurementDate ?? x.item.EstimatedStartDate ?? x.header.EstimatedStartDate ?? x.header.CreatedDate) <= request.EndDate.Value);
                }

                var results = await query.Select(x => new ProjectExpenseRecord
                {
                    ExpenseItemKey = x.item.WorkEffortId,
                    CertificateKey = x.header.WorkEffortId,
                    CertificateNumber = x.header.WorkEffortTypeId == "PROJECT_CERTIFICATE" ? x.header.CertificateNumber : null,
                    ProjectId = x.projId,
                    PartyId = x.partyId,
                    PartyName = x.p.Description,
                    PartyRole = x.header.PartyIdSupplier != null ? "Supplier" :
                                x.header.PartyIdContractor != null ? "Contractor" :
                                x.header.PartyIdEmployee != null ? "Employee" : "Unknown",
                    ProductId = x.prodId,
                    ProductName = x.prod.ProductName,
                    ExpenseDate = x.item.ProcurementDate ?? x.item.EstimatedStartDate ?? x.header.EstimatedStartDate ?? x.header.CreatedDate,
                    RecordType = x.header.WorkEffortTypeId == "PROJECT_CERTIFICATE" && (x.header.CertificateCategory == "SUPPLY_PROCUREMENT_CERTIFICATE" || x.header.CertificateCategory == "WORKMANSHIP_CONTRACTING_CERTIFICATE") ? "ProjectCertificate" :
                                 x.header.WorkEffortTypeId == "PAYMENT_CERTIFICATE" ? "MultiPaymentCertificate" : "Other",
                    CertificateType = x.header.WorkEffortTypeId == "PROJECT_CERTIFICATE" ? 
                                        (x.header.CertificateCategory == "SUPPLY_PROCUREMENT_CERTIFICATE" ? "Supply Procurement" :
                                         x.header.CertificateCategory == "WORKMANSHIP_CONTRACTING_CERTIFICATE" ? "Workmanship Contracting" : "Project Certificate") :
                                      x.header.WorkEffortTypeId == "PAYMENT_CERTIFICATE" ? "Multi-Payment / Direct Expense" : "Unknown",
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
                    AchievementPercentage = x.header.CertificateCategory == "WORKMANSHIP_CONTRACTING_CERTIFICATE" ? (x.item.AchievementPercent ?? 0m) :
                                            (x.header.CertificateCategory == "SUPPLY_PROCUREMENT_CERTIFICATE" || x.header.WorkEffortTypeId == "PAYMENT_CERTIFICATE" ? 100m : (x.item.AchievementPercent ?? 0m))
                }).ToListAsync(ct);

                foreach (var r in results)
                {
                    r.NetCertifiedAmount = r.GrossAmount - r.DiscountAmount - r.DeductionsAmount - r.InsuranceAmount + r.TransportationExpensesAmount + r.GratuitiesAmount;
                }

                return results;
            }

            private async Task<List<ProjectRevenueRecord>> GetRevenues(Query request, CancellationToken ct)
            {
                var today = DateTime.Today;

                var query = from p in _context.Payments.AsNoTracking()
                            join pf in _context.Parties.AsNoTracking() on p.PartyIdFrom equals pf.PartyId into pfJoin from pf in pfJoin.DefaultIfEmpty()
                            join pt_type in _context.PaymentTypes.AsNoTracking() on p.PaymentTypeId equals pt_type.PaymentTypeId into ptJoin from pt_type in ptJoin.DefaultIfEmpty()
                            join sr in _context.SalesRequests.AsNoTracking() on p.SalesRequestId equals sr.SalesRequestId into srJoin from sr in srJoin.DefaultIfEmpty()
                            join apt in _context.Products.AsNoTracking() on sr.ProductId equals apt.ProductId into aptJoin from apt in aptJoin.DefaultIfEmpty()
                            join proj in _context.WorkEfforts.AsNoTracking() on (apt.ProjectId ?? p.WorkEffortId) equals proj.WorkEffortId into projJoin from proj in projJoin.DefaultIfEmpty()

                            where p.PartyIdTo == "Company" && p.SalesRequestId != null
                               && (p.PaymentTypeId == "RECEIPT_ADVANCE_PAYMENT" || p.PaymentTypeId == "RECEIPT_DUE_INSTALLMENT" || p.PaymentTypeId == "RECEIPT_MAINTENANCE_AMOUNT")
                               && (p.Amount) > 0m
                            
                            let projId = apt.ProjectId ?? p.WorkEffortId
                            where projId == request.ProjectId

                            select new { p, pf, pt_type, sr, apt, proj, projId };

                if (!request.AllData)
                {
                    if (request.StartDate.HasValue)
                        query = query.Where(x => x.p.EffectiveDate >= DateOnly.FromDateTime(request.StartDate.Value));
                    if (request.EndDate.HasValue)
                        query = query.Where(x => x.p.EffectiveDate <= DateOnly.FromDateTime(request.EndDate.Value));
                }

                var results = await query.Select(x => new ProjectRevenueRecord
                {
                    PaymentId = x.p.PaymentId,
                    SalesRequestId = x.p.SalesRequestId,
                    ApartmentId = x.sr.ProductId,
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
                    CollectedAmount = x.p.StatusId == "PMNT_RECEIVED" ? (x.p.Amount) : 0m,
                    OutstandingAmount = x.p.StatusId != "PMNT_RECEIVED" ? (x.p.Amount ) : 0m,
                    LateAmount = (x.p.StatusId != "PMNT_RECEIVED" && x.p.EffectiveDate < DateOnly.FromDateTime(today)) ? (x.p.Amount) : 0m,
                    FutureAmount = (x.p.StatusId != "PMNT_RECEIVED" && x.p.EffectiveDate >= DateOnly.FromDateTime(today)) ? (x.p.Amount) : 0m,
                    PaymentStatus = x.p.StatusId == "PMNT_RECEIVED" ? "Received" :
                                    (x.p.EffectiveDate < DateOnly.FromDateTime(today) ? "Late" : "Upcoming"),
                    DueDate = x.p.EffectiveDate != null ? x.p.EffectiveDate.Value.ToDateTime(TimeOnly.MinValue) : null,
                    CreatedDate = x.p.CreatedStamp,
                    Comments = x.p.Comments,
                    ChequeNumber = x.p.ChequeNumber
                }).ToListAsync(ct);

                foreach (var r in results)
                {
                    if (r.PaymentStatus == "Received") r.OverdueBucket = "Received";
                    else if (r.DueDate >= today) r.OverdueBucket = "Upcoming";
                    else if (r.DueDate.HasValue)
                    {
                        var diff = (today - r.DueDate.Value).Days;
                        r.DaysOverdue = diff;
                        if (diff <= 30) r.OverdueBucket = "Late (1-30 Days)";
                        else if (diff <= 90) r.OverdueBucket = "Late (31-90 Days)";
                        else r.OverdueBucket = "Late (Over 90 Days)";
                    }
                }

                return results;
            }
        }
    }
}
