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
                commission.StatusId = "COMMISSION_APPROVED";
                commission.LastUpdatedStamp = DateTime.UtcNow;
                await _context.SaveChangesAsync(cancellationToken);

                // One outgoing payment per party that has a payable amount
                var payments = new List<(string partyId, decimal amount, string role)>
                {
                    (commission.SalesRepPartyId!, commission.SalesRepNetAmount ?? commission.SalesRepAmount ?? 0, "مندوب"),
                    (commission.ManagerPartyId!, commission.ManagerNetAmount ?? commission.ManagerAmount ?? 0, "مدير"),
                };

                if (!string.IsNullOrEmpty(commission.SalesRep2PartyId) && (commission.SalesRep2NetAmount ?? commission.SalesRep2Amount) > 0)
                    payments.Add((commission.SalesRep2PartyId, commission.SalesRep2NetAmount ?? commission.SalesRep2Amount ?? 0, "مندوب ثانٍ"));

                if (!string.IsNullOrEmpty(commission.Manager2PartyId) && (commission.Manager2NetAmount ?? commission.Manager2Amount) > 0)
                    payments.Add((commission.Manager2PartyId, commission.Manager2NetAmount ?? commission.Manager2Amount ?? 0, "مدير ثانٍ"));

                if (!string.IsNullOrEmpty(commission.ExternalCompanyPartyId) && (commission.ExternalCompanyNetAmount ?? commission.ExternalCompanyGrossAmount) > 0)
                    payments.Add((commission.ExternalCompanyPartyId, commission.ExternalCompanyNetAmount ?? commission.ExternalCompanyGrossAmount ?? 0, "شركة وسيط"));

                if (!string.IsNullOrEmpty(commission.ExternalSalesRepPartyId) && (commission.ExternalSalesRepNetAmount ?? commission.ExternalSalesRepAmount) > 0)
                    payments.Add((commission.ExternalSalesRepPartyId, commission.ExternalSalesRepNetAmount ?? commission.ExternalSalesRepAmount ?? 0, "مندوب وسيط"));

                if (!string.IsNullOrEmpty(commission.ExternalManagerPartyId) && (commission.ExternalManagerNetAmount ?? commission.ExternalManagerAmount) > 0)
                    payments.Add((commission.ExternalManagerPartyId, commission.ExternalManagerNetAmount ?? commission.ExternalManagerAmount ?? 0, "مدير وسيط"));

                foreach (var (partyId, amount, role) in payments)
                {
                    var param = new CreatePaymentParam
                    {
                        PaymentTypeId = "COMMISSION_PAYMENT",
                        StatusId = "PMNT_NOT_PAID",
                        PartyIdFrom = companyPartyId,
                        PartyIdTo = partyId,
                        Amount = amount,
                        EffectiveDate = effectiveDate,
                        SalesRequestId = commission.SalesRequestId,
                        ProjectId = commission.ProjectId,
                        Comments = $"عمولة {role} — طلب بيع {commission.SalesRequestId} — عمولة {commission.SalesCommissionId}",
                    };

                    var payment = await _paymentHelperService.CreatePayment(param);
                    if (payment == null)
                    {
                        await transaction.RollbackAsync(cancellationToken);
                        return Result<SalesCommissionDto>.Failure($"فشل إنشاء دفعة العمولة للطرف {partyId}");
                    }
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
