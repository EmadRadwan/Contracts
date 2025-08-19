using Application.Accounting.FinAccounts;
using Application.Accounting.Payments;
using Application.Core;
using Application.Shipments.Payments;
using Domain;
using Microsoft.EntityFrameworkCore;
using Persistence;

namespace Application.Accounting.Services;

public interface IFinAccountService
{
    Task UpdateFinAccountTrans(CreateFinAccountTransParam input);
    Task<string> CreateFinAccountTrans(CreateFinAccountTransParam input);
    Task<Result<string>> CreateFinAccountTrans(CreateFinAccountTransRequest request);

    Task<Result<DepositWithdrawResultDto>> DepositOrWithdrawPayments(
        DepositWithdrawPaymentsDto request);

    
}

public class FinAccountService : IFinAccountService
{
    private readonly DataContext _context;
    private readonly IPaymentHelperService _paymentHelperService;
    private readonly IUtilityService _utilityService;
    private readonly IAcctgMiscService _acctgMiscService;
    private readonly IGeneralLedgerService _generalLedgerService;


    public FinAccountService(DataContext context, IUtilityService utilityService,
        IPaymentHelperService paymentHelperService, IAcctgMiscService acctgMiscService,
        IGeneralLedgerService generalLedgerService)

    {
        _context = context;
        _utilityService = utilityService;
        _paymentHelperService = paymentHelperService;
        _acctgMiscService = acctgMiscService;
        _generalLedgerService = generalLedgerService;
    }


    public async Task UpdateFinAccountTrans(CreateFinAccountTransParam input)
    {
        var stamp = DateTime.UtcNow;
        var finAccountTrans = await _context.FinAccountTrans.FindAsync(input.FinAccountTransId);
        // update the financial account transaction
        if (finAccountTrans != null)
        {
            finAccountTrans.FinAccountId = input.FinAccountId;
            finAccountTrans.PartyId = input.PartyId;
            finAccountTrans.Amount = input.Amount;
            finAccountTrans.TransactionDate = input.EffectiveDate;
            finAccountTrans.Comments = input.Comments;
            finAccountTrans.FinAccountTransTypeId = input.FinAccountTransTypeId;
            finAccountTrans.LastUpdatedStamp = stamp;
        }
    }

    public async Task<string> CreateFinAccountTrans(CreateFinAccountTransParam param)
    {
        var stamp = DateTime.UtcNow;
        // get the next sequence number for the financial account transaction
        var nextSeqId = await _utilityService.GetNextSequence("FinAccountTrans");
        var newFinAccountTrans = new FinAccountTran
        {
            FinAccountTransId = nextSeqId,
            FinAccountId = param.FinAccountId,
            PaymentId = param.PaymentId,
            StatusId = param.StatusId,
            PartyId = param.PartyId,
            Amount = param.Amount,
            TransactionDate = param.EffectiveDate,
            Comments = param.Comments,
            FinAccountTransTypeId = param.FinAccountTransTypeId,
            CreatedStamp = stamp,
            LastUpdatedStamp = stamp
        };

        _context.FinAccountTrans.Add(newFinAccountTrans);

        return newFinAccountTrans.FinAccountTransId;
    }

    public async Task<Result<DepositWithdrawResultDto>> DepositOrWithdrawPayments(
        DepositWithdrawPaymentsDto request)
    {
        var resultDto = new DepositWithdrawResultDto();

        // 1) Validate inputs
        if (request.PaymentIds == null || !request.PaymentIds.Any())
        {
            return Result<DepositWithdrawResultDto>.Failure("No payments provided.");
        }

        // 2) Fetch FinAccount
        var finAccount = await _context.FinAccounts
            .FirstOrDefaultAsync(f => f.FinAccountId == request.FinAccountId);
        if (finAccount == null)
        {
            return Result<DepositWithdrawResultDto>.Failure("FinAccount not found.");
        }

        // 3) Validate FinAccount status
        if (finAccount.StatusId == "FNACT_MANFROZEN" || finAccount.StatusId == "FNACT_CANCELLED")
        {
            return Result<DepositWithdrawResultDto>.Failure("Cannot process a Manually Frozen or Canceled FinAccount.");
        }

        // 4) Retrieve and validate Payments
        var payments = await _context.Payments
            .Where(p => request.PaymentIds.Contains(p.PaymentId))
            .ToListAsync();

        if (payments.Count != request.PaymentIds.Count)
        {
            return Result<DepositWithdrawResultDto>.Failure("One or more Payment IDs not found.");
        }

        decimal paymentRunningTotal = 0m;

        foreach (var payment in payments)
        {
            paymentRunningTotal += payment.Amount;

            if (!string.IsNullOrEmpty(payment.FinAccountTransId))
            {
                return Result<DepositWithdrawResultDto>.Failure(
                    $"Payment {payment.PaymentId} is already linked to a FinAcctTrans.");
            }

            if (payment.StatusId != "PMNT_SENT" && payment.StatusId != "PMNT_RECEIVED")
            {
                return Result<DepositWithdrawResultDto>.Failure(
                    $"Payment {payment.PaymentId} must have status PMNT_SENT or PMNT_RECEIVED.");
            }
        }

        // 5) Process Payments
        if ("Y".Equals(request.GroupInOneTransaction, StringComparison.OrdinalIgnoreCase))
        {
            foreach (var payment in payments)
            {
                if (payment.StatusId != "PMNT_RECEIVED")
                {
                    return Result<DepositWithdrawResultDto>.Failure(
                        $"Payment [{payment.PaymentId}] cannot be included: must be a receipt.");
                }
            }

            var createTransParam = new CreateFinAccountTransParam
            {
                FinAccountId = request.FinAccountId,
                FinAccountTransTypeId = "DEPOSIT",
                PartyId = finAccount.OwnerPartyId,
                Amount = paymentRunningTotal,
                StatusId = "FINACT_TRNS_CREATED",
                EffectiveDate = DateTime.UtcNow
            };

            var finAccountTransId = await CreateFinAccountTrans(createTransParam);
            resultDto.FinAccountTransId = finAccountTransId;

            foreach (var payment in payments)
            {
                var updateParam = new CreatePaymentParam
                {
                    PaymentId = payment.PaymentId,
                    StatusId = "UPDATED_STATUS", // Replace with the appropriate status if applicable
                    Amount = payment.Amount,
                    PaymentMethodId = payment.PaymentMethodId,
                    PaymentMethodTypeId = payment.PaymentMethodTypeId,
                    PaymentPreferenceId = payment.PaymentPreferenceId,
                    EffectiveDate = DateTime.UtcNow,
                    PartyIdFrom = payment.PartyIdFrom,
                    PartyIdTo = payment.PartyIdTo,
                    PaymentTypeId = payment.PaymentTypeId,
                };

                var updatedPayment = await _paymentHelperService.UpdatePayment(updateParam);
                if (updatedPayment == null)
                {
                    return Result<DepositWithdrawResultDto>.Failure($"Failed to update Payment {payment.PaymentId}.");
                }
            }

            var paymentGroupId = await _paymentHelperService.CheckAndCreateBatchForValidPayments(request.PaymentIds);

            if (string.IsNullOrEmpty(paymentGroupId))
            {
                return Result<DepositWithdrawResultDto>.Failure("Failed to create a valid payment group.");
            }

            resultDto.PaymentGroupId = paymentGroupId;
        }
        else
        {
            foreach (var payment in payments)
            {
                var finAccountTransTypeId = payment.StatusId == "PMNT_RECEIVED" ? "DEPOSIT" : "WITHDRAWAL";

                var createTransParam = new CreateFinAccountTransParam
                {
                    FinAccountId = request.FinAccountId,
                    FinAccountTransTypeId = finAccountTransTypeId,
                    PartyId = finAccount.OwnerPartyId,
                    Amount = payment.Amount,
                    StatusId = "FINACT_TRNS_CREATED",
                    PaymentId = payment.PaymentId,
                    EffectiveDate = DateTime.UtcNow
                };

                var finAccountTransId = await CreateFinAccountTrans(createTransParam);

                var updateParam = new CreatePaymentParam
                {
                    PaymentId = payment.PaymentId,
                    StatusId = "UPDATED_STATUS", // Replace with the appropriate status if applicable
                    Amount = payment.Amount,
                    PaymentMethodId = payment.PaymentMethodId,
                    PaymentMethodTypeId = payment.PaymentMethodTypeId,
                    PaymentPreferenceId = payment.PaymentPreferenceId,
                    EffectiveDate = DateTime.UtcNow,
                    PartyIdFrom = payment.PartyIdFrom,
                    PartyIdTo = payment.PartyIdTo,
                    PaymentTypeId = payment.PaymentTypeId,
                };

                var updatedPayment = await _paymentHelperService.UpdatePayment(updateParam);
                if (updatedPayment == null)
                {
                    return Result<DepositWithdrawResultDto>.Failure($"Failed to update Payment {payment.PaymentId}.");
                }
            }
        }

        return Result<DepositWithdrawResultDto>.Success(resultDto);
    }

    public async Task<Result<string>> CreateFinAccountTrans(CreateFinAccountTransRequest request)
    {
        try
        {
            var glSettings = _acctgMiscService.GetGlArithmeticSettingsInline();
            var ledgerDecimals = glSettings.DecimalScale;
            var roundingMode = glSettings.RoundingMode;

            // Fetch FinAccount
            var finAccount =
                await _context.FinAccounts.FirstOrDefaultAsync(f => f.FinAccountId == request.FinAccountId);
            if (finAccount == null)
                return Result<string>.Failure("Financial account not found.");

            // Validate account status
            if (finAccount.StatusId == "FNACT_MANFROZEN")
                return Result<string>.Failure("Financial account is manually frozen.");
            if (finAccount.StatusId == "FNACT_CANCELLED")
                return Result<string>.Failure("Financial account is cancelled.");

            var newFinAcctTransSequence = await _utilityService.GetNextSequence("FinAccountTrans");

            var nowTimestamp = DateTime.UtcNow;

            var amount = _acctgMiscService.CustomRound(
                (decimal)(request.Amount),
                (int)ledgerDecimals,
                roundingMode
            );

            // Create FinAccountTrans entity
            var newEntity = new FinAccountTran
            {
                FinAccountTransId = newFinAcctTransSequence,
                FinAccountId = request.FinAccountId,
                TransactionDate = request.TransactionDate ?? nowTimestamp,
                EntryDate = request.EntryDate ?? nowTimestamp,
                StatusId = request.StatusId ?? "FINACT_TRNS_APPROVED",
                PerformedByPartyId = request.PerformedByPartyId,
                FinAccountTransTypeId = request.FinAccountTransTypeId,
                Amount = amount,
                CreatedStamp = nowTimestamp,
                LastUpdatedStamp = nowTimestamp
            };

            _context.FinAccountTrans.Add(newEntity);

            // Now post to GL (assuming we have a glAccountId in the request)
            // If the user wants to skip posting, they'd pass no GL account or handle differently.
            if (!string.IsNullOrEmpty(request.GlAccountId))
            {
                var postRequest = new PostFinAccountTransToGlRequest
                {
                    FinAccountTransId = newEntity.FinAccountTransId,
                    GlAccountId = request.GlAccountId
                };

                var postResult = await _generalLedgerService.PostFinAccountTransToGl(postRequest);
                if (!postResult.IsSuccess)
                {
                    return Result<string>.Failure(postResult.Error);
                }
            }

            return Result<string>.Success(newEntity.FinAccountTransId);
        }
        catch (Exception ex)
        {
            return Result<string>.Failure($"Error creating financial account transaction: {ex.Message}");
        }
    }
}