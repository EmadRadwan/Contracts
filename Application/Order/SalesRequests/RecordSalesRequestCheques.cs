using Application.Accounting.Payments;
using Application.Accounting.Services;
using Application.Accounting.Services.Models;
using Application.Catalog.ProductStores;
using Domain;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Persistence;

namespace Application.Order.SalesRequests;

// Attaches physically-received cheque details (bank, cheque number, cheque date, amount)
// onto existing not-yet-collected installment/maintenance Payments for a Sales Request,
// and posts the reclass entry for the cheques actually handed over:
//   Debit  124410 Cheques Under Collection (one line per cheque)
//   Credit Customer receivable account (one line, sum of the cheques recorded)
// This replaces the old lump-sum "IsChequesDelivered" posting that used to fire at
// ApproveSalesRequest.cs time against the full contract price - that assumption was wrong
// per the client's cheque-collection design, since not every installment ends up cheque-paid.
public class RecordSalesRequestCheques
{
    public class ChequeEntry
    {
        public string PaymentId { get; set; } = null!;
        public string PaymentMethodId { get; set; } = null!;
        public string ChequeNumber { get; set; } = null!;
        public DateOnly ChequeDate { get; set; }
        public decimal Amount { get; set; }
    }

    public class Command : IRequest<Result<List<ListChequeableSalesRequestPayments.ChequeablePaymentDto>>>
    {
        public string SalesRequestId { get; set; } = null!;
        // Bank the cheques were drawn on (the customer's own bank, not ours) - free text,
        // entered once for the whole batch, and echoed into the reclass transaction descriptions.
        public string CustomerBankName { get; set; } = null!;
        public List<ChequeEntry> Cheques { get; set; } = new();
    }

    public class CommandValidator : AbstractValidator<Command>
    {
        public CommandValidator()
        {
            RuleFor(x => x.SalesRequestId).NotEmpty();
            RuleFor(x => x.CustomerBankName).NotEmpty();
            RuleFor(x => x.Cheques).NotEmpty();
            RuleForEach(x => x.Cheques).ChildRules(cheque =>
            {
                cheque.RuleFor(c => c.PaymentId).NotEmpty();
                cheque.RuleFor(c => c.PaymentMethodId).NotEmpty();
                cheque.RuleFor(c => c.ChequeNumber).NotEmpty();
                cheque.RuleFor(c => c.ChequeDate).NotEmpty();
                cheque.RuleFor(c => c.Amount).GreaterThan(0);
            });
        }
    }

    public class Handler : IRequestHandler<Command, Result<List<ListChequeableSalesRequestPayments.ChequeablePaymentDto>>>
    {
        private readonly DataContext _context;
        private readonly IPaymentHelperService _paymentHelperService;
        private readonly IAcctgTransService _acctgTransService;
        private readonly IProductStoreService _productStoreService;

        public Handler(
            DataContext context,
            IPaymentHelperService paymentHelperService,
            IAcctgTransService acctgTransService,
            IProductStoreService productStoreService)
        {
            _context = context;
            _paymentHelperService = paymentHelperService;
            _acctgTransService = acctgTransService;
            _productStoreService = productStoreService;
        }

        // Mirrors ApproveSalesRequest.Handler.GetReceivableGlAccountId
        private async Task<string> GetReceivableGlAccountId(
            string organizationPartyId,
            string customerPartyId,
            CancellationToken ct)
        {
            var partyGlAccount = await _context.PartyGlAccounts
                .Where(pga =>
                    pga.OrganizationPartyId == organizationPartyId &&
                    pga.PartyId == customerPartyId &&
                    pga.RoleTypeId == "BILL_TO_CUSTOMER" &&
                    pga.GlAccountTypeId == "ACCOUNTS_RECEIVABLE")
                .Select(pga => pga.GlAccountId)
                .FirstOrDefaultAsync(ct);

            return partyGlAccount ?? "121100";
        }

        public async Task<Result<List<ListChequeableSalesRequestPayments.ChequeablePaymentDto>>> Handle(
            Command request, CancellationToken ct)
        {
            var sr = await _context.SalesRequests
                .FirstOrDefaultAsync(x => x.SalesRequestId == request.SalesRequestId, ct);

            if (sr == null)
                return Result<List<ListChequeableSalesRequestPayments.ChequeablePaymentDto>>.Failure(
                    "Sales request not found");

            var paymentIds = request.Cheques.Select(c => c.PaymentId).ToList();

            var payments = await _context.Payments
                .Where(p => paymentIds.Contains(p.PaymentId))
                .ToListAsync(ct);

            foreach (var cheque in request.Cheques)
            {
                var payment = payments.FirstOrDefault(p => p.PaymentId == cheque.PaymentId);

                if (payment == null)
                    return Result<List<ListChequeableSalesRequestPayments.ChequeablePaymentDto>>.Failure(
                        $"Payment {cheque.PaymentId} not found");

                if (payment.SalesRequestId != request.SalesRequestId)
                    return Result<List<ListChequeableSalesRequestPayments.ChequeablePaymentDto>>.Failure(
                        $"Payment {cheque.PaymentId} does not belong to sales request {request.SalesRequestId}");

                if (payment.StatusId != "PMNT_NOT_PAID")
                    return Result<List<ListChequeableSalesRequestPayments.ChequeablePaymentDto>>.Failure(
                        $"Payment {cheque.PaymentId} is not in a state that can receive a cheque");

                if (!string.IsNullOrEmpty(payment.ChequeNumber))
                    return Result<List<ListChequeableSalesRequestPayments.ChequeablePaymentDto>>.Failure(
                        $"Payment {cheque.PaymentId} already has a cheque attached");
            }

            var paymentMethodIds = request.Cheques.Select(c => c.PaymentMethodId).Distinct().ToList();

            var existingPaymentMethodIds = await _context.PaymentMethods
                .Where(pm => paymentMethodIds.Contains(pm.PaymentMethodId))
                .Select(pm => pm.PaymentMethodId)
                .ToListAsync(ct);

            var missingPaymentMethodId = paymentMethodIds.FirstOrDefault(id => !existingPaymentMethodIds.Contains(id));
            if (missingPaymentMethodId != null)
                return Result<List<ListChequeableSalesRequestPayments.ChequeablePaymentDto>>.Failure(
                    $"Bank/payment method {missingPaymentMethodId} not found");

            // Prevent the same cheque (bank + number) from being recorded twice, either within
            // this request or against an already-recorded cheque elsewhere in the system.
            var duplicateInRequest = request.Cheques
                .GroupBy(c => (c.PaymentMethodId, c.ChequeNumber))
                .FirstOrDefault(g => g.Count() > 1);

            if (duplicateInRequest != null)
                return Result<List<ListChequeableSalesRequestPayments.ChequeablePaymentDto>>.Failure(
                    $"Cheque {duplicateInRequest.Key.ChequeNumber} is listed more than once in this request");

            var chequeNumbers = request.Cheques.Select(c => c.ChequeNumber).ToList();

            var existingChequeNumbers = await _context.Payments
                .Where(p => chequeNumbers.Contains(p.ChequeNumber))
                .Select(p => new { p.ChequeNumber, p.PaymentMethodId })
                .ToListAsync(ct);

            foreach (var cheque in request.Cheques)
            {
                if (existingChequeNumbers.Any(e =>
                        e.ChequeNumber == cheque.ChequeNumber && e.PaymentMethodId == cheque.PaymentMethodId))
                {
                    return Result<List<ListChequeableSalesRequestPayments.ChequeablePaymentDto>>.Failure(
                        $"Cheque {cheque.ChequeNumber} is already recorded against that bank");
                }
            }

            await using var transaction = await _context.Database.BeginTransactionAsync(ct);

            try
            {
                foreach (var cheque in request.Cheques)
                {
                    await _paymentHelperService.UpdatePayment(new CreatePaymentParam
                    {
                        PaymentId = cheque.PaymentId,
                        PaymentMethodId = cheque.PaymentMethodId,
                        ChequeNumber = cheque.ChequeNumber,
                        ChequeDate = cheque.ChequeDate,
                        Amount = cheque.Amount
                    });
                }

                // Post the reclass entry for each cheque physically received:
                // Debit 124410 Cheques Under Collection / Credit the customer's receivable
                // account, using the cheque's actual amount. One AcctgTrans per cheque (with
                // PaymentId set) rather than one shared batch transaction, so that a later
                // switch away from this specific cheque (see UpdatePayment.cs) can find and
                // reverse exactly this posting without touching the other cheques in the batch.
                var companyPartyId = await _productStoreService.GetProductStorePayToPartId();
                var receivableGlAccountId = await GetReceivableGlAccountId(companyPartyId, sr.FromPartyId!, ct);
                var stamp = DateTime.UtcNow;

                foreach (var cheque in request.Cheques)
                {
                    var receivedDescription =
                        $"استلام شيك رقم {cheque.ChequeNumber} من بنك العميل {request.CustomerBankName} - طلب مبيعات {sr.SalesRequestId}";
                    var reclassDescription =
                        $"استلام شيك رقم {cheque.ChequeNumber} من بنك العميل {request.CustomerBankName}، وإعادة تصنيفه من الذمم المدينة - طلب مبيعات {sr.SalesRequestId}";

                    var acctgTransId = await _acctgTransService.CreateAcctgTrans(new CreateAcctgTransParams
                    {
                        AcctgTransTypeId = "APARTMENT_SALE_CHEQUE",
                        TransactionDate = DateOnly.FromDateTime(DateTime.UtcNow),
                        IsPosted = "Y",
                        Description = receivedDescription,
                        GlFiscalTypeId = "ACTUAL",
                        SalesRequestId = sr.SalesRequestId,
                        PaymentId = cheque.PaymentId,
                        PartyId = sr.FromPartyId
                    });

                    await _acctgTransService.CreateAcctgTransEntry(new AcctgTransEntry
                    {
                        AcctgTransId = acctgTransId,
                        AcctgTransEntrySeqId = "001",
                        GlAccountId = "124410",
                        DebitCreditFlag = "D",
                        AcctgTransEntryTypeId = "_NA_",
                        Amount = cheque.Amount,
                        ReconcileStatusId = "AES_NOT_RECONCILED",
                        Description = receivedDescription,
                        OrganizationPartyId = companyPartyId,
                        ProductId = sr.ProductId,
                        PartyId = sr.FromPartyId,
                        CreatedStamp = stamp,
                        LastUpdatedStamp = stamp
                    });

                    await _acctgTransService.CreateAcctgTransEntry(new AcctgTransEntry
                    {
                        AcctgTransId = acctgTransId,
                        AcctgTransEntrySeqId = "002",
                        GlAccountId = receivableGlAccountId,
                        DebitCreditFlag = "C",
                        AcctgTransEntryTypeId = "_NA_",
                        Amount = cheque.Amount,
                        ReconcileStatusId = "AES_NOT_RECONCILED",
                        Description = reclassDescription,
                        OrganizationPartyId = companyPartyId,
                        ProductId = sr.ProductId,
                        PartyId = sr.FromPartyId,
                        CreatedStamp = stamp,
                        LastUpdatedStamp = stamp
                    });
                }

                await _context.SaveChangesAsync(ct);
                await transaction.CommitAsync(ct);
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync(ct);
                return Result<List<ListChequeableSalesRequestPayments.ChequeablePaymentDto>>.Failure(
                    $"Failed to record cheques: {ex.Message}");
            }

            var updated = payments
                .Where(p => paymentIds.Contains(p.PaymentId))
                .Select(p => new ListChequeableSalesRequestPayments.ChequeablePaymentDto
                {
                    PaymentId = p.PaymentId,
                    PaymentTypeId = p.PaymentTypeId,
                    DueDate = p.EffectiveDate,
                    Amount = p.Amount,
                    Comments = p.Comments
                })
                .ToList();

            return Result<List<ListChequeableSalesRequestPayments.ChequeablePaymentDto>>.Success(updated);
        }
    }
}
