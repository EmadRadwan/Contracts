using Application.Accounting.Services;
using Application.Accounting.Services.Models;
using Application.Catalog.ProductStores;
using Domain;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Persistence;

namespace Application.Accounting.Payments;

public class RecordInstallmentPaymentAccounting
{
    public class Command : IRequest<Result<Unit>>
    {
        public string PaymentId { get; set; } = null!;
        public string SalesRequestId { get; set; } = null!;
        public decimal Amount { get; set; }
        public DateOnly? EffectiveDate { get; set; }
        public string ChequeNumber { get; set; } = null!;
        public DateOnly? ChequeDate { get; set; }
        public string PartyIdFrom { get; set; } = null!; // Customer
        public string? Comments { get; set; }
    }

    public class Handler : IRequestHandler<Command, Result<Unit>>
    {
        private readonly DataContext _context;
        private readonly IAcctgTransService _acctgTransService;
        private readonly IProductStoreService _productStoreService;

        public Handler(
            DataContext context,
            IAcctgTransService acctgTransService,
            IProductStoreService productStoreService)
        {
            _context = context;
            _acctgTransService = acctgTransService;
            _productStoreService = productStoreService;
        }

        public async Task<Result<Unit>> Handle(Command request, CancellationToken ct)
        {
            await using var transaction = await _context.Database.BeginTransactionAsync(ct);

            try
            {
                var existingAcctgTrans = await _context.AcctgTrans
                    .Include(at => at.AcctgTransEntries)  // Load entries to delete them
                    .FirstOrDefaultAsync(at => at.PaymentId == request.PaymentId, ct);
                
                if (existingAcctgTrans != null)
                {
                    // 1. Delete entries FIRST (FK constraint)
                    _context.AcctgTransEntries.RemoveRange(existingAcctgTrans.AcctgTransEntries);
                    await _context.SaveChangesAsync(ct);  // Commit deletion of entries

                    // 2. Delete header
                    _context.AcctgTrans.Remove(existingAcctgTrans);
                    await _context.SaveChangesAsync(ct);  // Commit deletion of header
                }
                
                var companyPartyId = await _productStoreService.GetProductStorePayToPartId();

                // Load SalesRequest to get ProductId (Apartment) for description
                var salesRequest = await _context.SalesRequests
                    .Include(sr => sr.Product)
                    .FirstOrDefaultAsync(sr => sr.SalesRequestId == request.SalesRequestId, ct);

                if (salesRequest == null)
                    return Result<Unit>.Failure("Sales request not found for accounting entry.");

                var apartmentName = salesRequest.Product?.ProductName ?? "Unknown Apartment";

                var description = $"Customer Installment Payment (Cheque {request.ChequeNumber}) - SR {request.SalesRequestId} - {apartmentName}";

                // 1. Create Accounting Transaction Header
                var acctgTransParams = new CreateAcctgTransParams
                {
                    AcctgTransTypeId = "INCOMING_PAYMENT", 
                    TransactionDate = request.EffectiveDate,
                    IsPosted = "Y",
                    Description = description,
                    GlFiscalTypeId = "ACTUAL",
                    SalesRequestId = request.SalesRequestId,
                    PaymentId = request.PaymentId,
                    PartyId = request.PartyIdFrom
                };

                var acctgTransId = await _acctgTransService.CreateAcctgTrans(acctgTransParams);

                var stamp = DateTime.UtcNow;
                var seq = 0;

                // 2. Debit: Bank Account (Cheque in hand – undeposited or specific bank)
                // Common GL for cheques received: Undeposited Funds or specific Bank Account
                // Adjust this GL code based on your chart of accounts
                var debitEntry = new AcctgTransEntry
                {
                    AcctgTransId = acctgTransId,
                    AcctgTransEntrySeqId = (++seq).ToString().PadLeft(3, '0'), // "001"
                    GlAccountId = "124410", 
                    DebitCreditFlag = "D",
                    AcctgTransEntryTypeId = "_NA_",
                    Amount = request.Amount,
                    ReconcileStatusId = "AES_NOT_RECONCILED",
                    Description = $"Cheque #{request.ChequeNumber} received - Installment - {apartmentName}",
                    OrganizationPartyId = companyPartyId,
                    ProductId = salesRequest.ProductId,
                    PartyId = request.PartyIdFrom,
                    CreatedStamp = stamp,
                    LastUpdatedStamp = stamp
                };
                await _acctgTransService.CreateAcctgTransEntry(debitEntry);

                // 3. Credit: Accounts Receivable (reduce customer's balance)
                var creditEntry = new AcctgTransEntry
                {
                    AcctgTransId = acctgTransId,
                    AcctgTransEntrySeqId = (++seq).ToString().PadLeft(3, '0'), // "002"
                    GlAccountId = "121100", // Accounts Receivable - Customers
                    DebitCreditFlag = "C",
                    AcctgTransEntryTypeId = "_NA_",
                    Amount = request.Amount,
                    ReconcileStatusId = "AES_NOT_RECONCILED",
                    Description = $"Applied to receivable - Cheque #{request.ChequeNumber} - {apartmentName}",
                    OrganizationPartyId = companyPartyId,
                    ProductId = salesRequest.ProductId,
                    PartyId = request.PartyIdFrom,
                    CreatedStamp = stamp,
                    LastUpdatedStamp = stamp
                };
                await _acctgTransService.CreateAcctgTransEntry(creditEntry);
                
                var saved = await _context.SaveChangesAsync(ct) > 0;
                if (!saved)
                {
                    throw new Exception("Failed to save accounting entries for installment payment.");
                }
                
                await transaction.CommitAsync(ct);

                return Result<Unit>.Success(Unit.Value);
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync(ct);
                return Result<Unit>.Failure($"Failed to record accounting for installment payment: {ex.Message}");
            }
        }
    }
}