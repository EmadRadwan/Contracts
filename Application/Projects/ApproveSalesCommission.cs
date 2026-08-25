using Application.Accounting.Payments;
using Application.Accounting.Services;
using Application.Catalog.ProductStores;
using Application.Core;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Persistence;

namespace Application.Projects;

public class ApproveSalesCommission
{
    public class Command : IRequest<Result<SalesCommissionDto>>
    {
        public string SalesCommissionId { get; set; } = null!;
    }

    public class Handler : IRequestHandler<Command, Result<SalesCommissionDto>>
    {
        private readonly DataContext _context;
        private readonly ILogger<Handler> _logger;
        private readonly IPaymentHelperService _paymentHelperService;
        private readonly IProductStoreService _productStoreService;

        public Handler(
            DataContext context,
            ILogger<Handler> logger,
            IPaymentHelperService paymentHelperService,
            IProductStoreService productStoreService)
        {
            _context = context;
            _logger = logger;
            _paymentHelperService = paymentHelperService;
            _productStoreService = productStoreService;
        }

        public async Task<Result<SalesCommissionDto>> Handle(Command request, CancellationToken cancellationToken)
        {
            var commission = await _context.SalesCommissions
                .FirstOrDefaultAsync(x => x.SalesCommissionId == request.SalesCommissionId, cancellationToken);

            if (commission == null)
                return Result<SalesCommissionDto>.Failure("Commission record not found");

            if (commission.StatusId != "COMMISSION_PENDING")
                return Result<SalesCommissionDto>.Failure("Only pending commissions can be approved");

            var companyPartyId = await _productStoreService.GetProductStorePayToPartId();
            var effectiveDate = DateOnly.FromDateTime(commission.CommissionDate ?? DateTime.UtcNow);

            await using var transaction = await _context.Database.BeginTransactionAsync(cancellationToken);
            try
            {
                // Approval pays each party their full percentage-based amount — the collection ratio
                // that scaled the amounts at create/update time no longer applies once approved.
                var isIndirect = commission.SaleTypeId == "COMM_SALE_INDIRECT";
                var fullAmounts = SalesCommissionCalculator.CalculateFullAmounts(commission, isIndirect);

                commission.SalesRepAmount = fullAmounts.SalesRepAmount;
                commission.SalesRepNetAmount = fullAmounts.SalesRepAmount;
                commission.ManagerAmount = fullAmounts.ManagerAmount;
                commission.ManagerNetAmount = fullAmounts.ManagerAmount;
                commission.SalesRep2Amount = fullAmounts.SalesRep2Amount;
                commission.SalesRep2NetAmount = fullAmounts.SalesRep2Amount;
                commission.Manager2Amount = fullAmounts.Manager2Amount;
                commission.Manager2NetAmount = fullAmounts.Manager2Amount;
                commission.ExternalCompanyGrossAmount = fullAmounts.ExternalCompanyGrossAmount;
                commission.ExternalCompanyNetAmount = fullAmounts.ExternalCompanyNetAmount;
                commission.ExternalSalesRepAmount = fullAmounts.ExternalSalesRepAmount;
                commission.ExternalSalesRepNetAmount = fullAmounts.ExternalSalesRepNetAmount;
                commission.ExternalManagerAmount = fullAmounts.ExternalManagerAmount;
                commission.ExternalManagerNetAmount = fullAmounts.ExternalManagerNetAmount;

                commission.StatusId = "COMMISSION_APPROVED";
                commission.LastUpdatedStamp = DateTime.UtcNow;
                await _context.SaveChangesAsync(cancellationToken);

                // Party names + project/apartment context for the payment comment
                var partyIds = new[]
                {
                    commission.SalesRepPartyId, commission.ManagerPartyId, commission.SalesRep2PartyId, commission.Manager2PartyId,
                    commission.ExternalCompanyPartyId, commission.ExternalSalesRepPartyId, commission.ExternalManagerPartyId
                }.Where(id => id != null).Distinct().ToList();

                var partyNames = await _context.Parties
                    .Where(p => partyIds.Contains(p.PartyId))
                    .ToDictionaryAsync(p => p.PartyId, p => p.Description, cancellationToken);

                string NameOf(string? partyId) =>
                    (partyId != null && partyNames.TryGetValue(partyId, out var name) ? name : null) ?? "-";

                var apartmentName = await _context.SalesRequests
                    .Where(x => x.SalesRequestId == commission.SalesRequestId)
                    .Join(_context.Products, s => s.ProductId, p => p.ProductId, (s, p) => p.ProductName)
                    .FirstOrDefaultAsync(cancellationToken) ?? "-";

                var projectName = (commission.ProjectId != null
                    ? await _context.WorkEfforts
                        .Where(w => w.WorkEffortId == commission.ProjectId)
                        .Select(w => w.ProjectName)
                        .FirstOrDefaultAsync(cancellationToken)
                    : null) ?? "-";

                // One outgoing payment per party that has a payable amount (e.g. an optional party
                // left at 0%/unassigned gets no payment).
                var payments = new List<(string partyId, decimal amount, string role, decimal percent)>();

                if (!string.IsNullOrEmpty(commission.SalesRepPartyId) && (commission.SalesRepNetAmount ?? commission.SalesRepAmount) > 0)
                    payments.Add((commission.SalesRepPartyId, commission.SalesRepNetAmount ?? commission.SalesRepAmount ?? 0, "مندوب", commission.SalesRepPercent));

                if (!string.IsNullOrEmpty(commission.ManagerPartyId) && (commission.ManagerNetAmount ?? commission.ManagerAmount) > 0)
                    payments.Add((commission.ManagerPartyId, commission.ManagerNetAmount ?? commission.ManagerAmount ?? 0, "مدير", commission.ManagerPercent));

                if (!string.IsNullOrEmpty(commission.SalesRep2PartyId) && (commission.SalesRep2NetAmount ?? commission.SalesRep2Amount) > 0)
                    payments.Add((commission.SalesRep2PartyId, commission.SalesRep2NetAmount ?? commission.SalesRep2Amount ?? 0, "مندوب ثانٍ", commission.SalesRep2Percent ?? 0));

                if (!string.IsNullOrEmpty(commission.Manager2PartyId) && (commission.Manager2NetAmount ?? commission.Manager2Amount) > 0)
                    payments.Add((commission.Manager2PartyId, commission.Manager2NetAmount ?? commission.Manager2Amount ?? 0, "مدير ثانٍ", commission.Manager2Percent ?? 0));

                if (!string.IsNullOrEmpty(commission.ExternalCompanyPartyId) && (commission.ExternalCompanyNetAmount ?? commission.ExternalCompanyGrossAmount) > 0)
                    payments.Add((commission.ExternalCompanyPartyId, commission.ExternalCompanyNetAmount ?? commission.ExternalCompanyGrossAmount ?? 0, "شركة وسيط", commission.ExternalCompanyPercent ?? 0));

                if (!string.IsNullOrEmpty(commission.ExternalSalesRepPartyId) && (commission.ExternalSalesRepNetAmount ?? commission.ExternalSalesRepAmount) > 0)
                    payments.Add((commission.ExternalSalesRepPartyId, commission.ExternalSalesRepNetAmount ?? commission.ExternalSalesRepAmount ?? 0, "مندوب وسيط", commission.ExternalSalesRepPercent ?? 0));

                if (!string.IsNullOrEmpty(commission.ExternalManagerPartyId) && (commission.ExternalManagerNetAmount ?? commission.ExternalManagerAmount) > 0)
                    payments.Add((commission.ExternalManagerPartyId, commission.ExternalManagerNetAmount ?? commission.ExternalManagerAmount ?? 0, "مدير وسيط", commission.ExternalManagerPercent ?? 0));

                // Party slots are optional at create/update time, but approving a commission is what
                // generates the payments — approving one where every slot is still unassigned would
                // produce nothing and silently move the record to APPROVED.
                if (payments.Count == 0)
                {
                    await transaction.RollbackAsync(cancellationToken);
                    return Result<SalesCommissionDto>.Failure(
                        "لا يوجد أي طرف مستحق للعمولة — حدد طرفاً واحداً على الأقل بنسبته قبل الاعتماد");
                }

                foreach (var (partyId, amount, role, percent) in payments)
                {
                    // Commission payments are paid as whole amounts — round to 0 decimals.
                    var roundedAmount = Math.Round(amount, 0, MidpointRounding.AwayFromZero);

                    var param = new CreatePaymentParam
                    {
                        PaymentTypeId = "COMMISSION_PAYMENT",
                        StatusId = "PMNT_NOT_PAID",
                        PartyIdFrom = companyPartyId,
                        PartyIdTo = partyId,
                        Amount = roundedAmount,
                        EffectiveDate = effectiveDate,
                        SalesRequestId = commission.SalesRequestId,
                        ProjectId = commission.ProjectId,
                        Comments = $"عمولة {role} — {NameOf(partyId)} ({percent:0.####}%) — مشروع {projectName} — شقة {apartmentName} — طلب بيع {commission.SalesRequestId} — عمولة {commission.SalesCommissionId}",
                    };

                    var payment = await _paymentHelperService.CreatePayment(param);
                    if (payment == null)
                    {
                        await transaction.RollbackAsync(cancellationToken);
                        return Result<SalesCommissionDto>.Failure($"فشل إنشاء دفعة العمولة للطرف {partyId}");
                    }
                    // save changes
                    await _context.SaveChangesAsync(cancellationToken); 
                }

                await transaction.CommitAsync(cancellationToken);

                return Result<SalesCommissionDto>.Success(new SalesCommissionDto
                {
                    SalesCommissionId = commission.SalesCommissionId,
                    SalesRequestId = commission.SalesRequestId,
                    StatusId = commission.StatusId
                });
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync(cancellationToken);
                _logger.LogError(ex, "Failed to approve commission {Id}", request.SalesCommissionId);
                return Result<SalesCommissionDto>.Failure("Failed to approve commission");
            }
        }
    }
}
