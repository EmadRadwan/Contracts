using Application.Accounting.Payments;
using Application.Accounting.Services;
using Application.Accounting.Services.Models;
using Application.Catalog.ProductStores;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Persistence;
using Domain;

namespace Application.Order.SalesRequests;

public class ApproveSalesRequest
{
    public class Command : IRequest<Result<CreateSalesRequest.SalesRequestResponseDto>>
    {
        public string SalesRequestId { get; set; } = null!;
    }

    public class Handler : IRequestHandler<Command, Result<CreateSalesRequest.SalesRequestResponseDto>>
    {
        private readonly DataContext _context;
        private readonly IProductStoreService _productStoreService;
        private readonly IPaymentHelperService _paymentHelperService;
        private readonly IAcctgTransService _acctgTransService;


        public Handler(
            DataContext context,
            IProductStoreService productStoreService,
            IPaymentHelperService paymentHelperService, IAcctgTransService acctgTransService)
        {
            _context = context;
            _productStoreService = productStoreService;
            _paymentHelperService = paymentHelperService;
            _acctgTransService = acctgTransService;
        }

        public async Task<Result<CreateSalesRequest.SalesRequestResponseDto>> Handle(Command request,
            CancellationToken ct)
        {
            var salesRequestId = request.SalesRequestId;

            await using var transaction = await _context.Database.BeginTransactionAsync(ct);

            try
            {
                // 1. Load SalesRequest with required data
                var sr = await _context.SalesRequests
                    .FirstOrDefaultAsync(x => x.SalesRequestId == salesRequestId, ct);

                if (sr == null)
                    return Result<CreateSalesRequest.SalesRequestResponseDto>.Failure("Sales request not found");

                if (sr.StatusId == "SALES_REQUEST_APPROVED")
                    return Result<CreateSalesRequest.SalesRequestResponseDto>.Failure(
                        "Sales request is already approved");

                // 2. Update status
                sr.StatusId = "SALES_REQUEST_APPROVED";
                sr.LastUpdatedStamp = DateTime.UtcNow;

                // 3. Get company PayTo PartyId (cached or from service)
                var companyPartyId = await _productStoreService.GetProductStorePayToPartId();

                // 4. Generate Payments
                var paymentsToCreate = new List<CreatePaymentParam>();

                var totalPrice = sr.TotalPrice ?? 0m;
                var advance = sr.AdvancePayment ?? 0m;
                var remaining = totalPrice - advance;

                // Always create Advance Payment (even if full payment)
                paymentsToCreate.Add(new CreatePaymentParam
                {
                    PartyIdFrom = sr.FromPartyId!, // Customer pays
                    PartyIdTo = companyPartyId, // Company receives
                    Amount = advance,
                    EffectiveDate = sr.SaleDate ?? DateTime.UtcNow.Date,
                    PaymentTypeId = "RECEIPT_ADVANCE_PAYMENT",
                    StatusId = "PMNT_NOT_PAID", // or "PMNT_PAID" if paid on spot?
                    Comments = "Advance payment - Sales Request Approved",
                    PaymentMethodId = null,
                    PaymentMethodTypeId = null
                });

                // Only generate installments if partial payment
                if (advance < totalPrice &&
                    sr.NumberOfInstallments > 0 &&
                    sr.DateOfFirstInstallment.HasValue &&
                    sr.MonthsBetweenInstallments > 0)
                {
                    var installmentAmount = remaining / sr.NumberOfInstallments;
                    var currentDueDate = sr.DateOfFirstInstallment.Value.Date;

                    for (int i = 1; i <= sr.NumberOfInstallments; i++)
                    {
                        paymentsToCreate.Add(new CreatePaymentParam
                        {
                            PartyIdFrom = sr.FromPartyId!,
                            PartyIdTo = companyPartyId,
                            Amount = installmentAmount,
                            EffectiveDate = currentDueDate,
                            PaymentTypeId = "RECEIPT_DUE_INSTALLMENT",
                            StatusId = "PMNT_NOT_PAID",
                            Comments = $"Installment {i} of {sr.NumberOfInstallments} - SR {sr.SalesRequestId}",
                            PaymentMethodId = null,
                            PaymentMethodTypeId = null
                        });

                        currentDueDate = currentDueDate.AddMonths((int)sr.MonthsBetweenInstallments);
                    }
                }

                // 5. Create all payments
                foreach (var param in paymentsToCreate)
                {
                    // Link payment to SalesRequest via Comments or add a SalesRequestId field later
                    var payment = await _paymentHelperService.CreatePayment(param);
                    if (payment == null)
                    {
                        await transaction.RollbackAsync(ct);
                        return Result<CreateSalesRequest.SalesRequestResponseDto>.Failure(
                            "Failed to create one or more payments");
                    }
                }

                var apartment = await CreateSalesRequest.Handler.GetApartmentLovProjection(_context, sr.ProductId!, ct)
                                ?? new CreateSalesRequest.ApartmentLovProjection
                                    { ApartmentId = sr.ProductId!, ApartmentName = "Unknown Apartment" };

                // 6. Create single accounting transaction for the full apartment sale amount
                // Following your exact OFBiz-style pattern (no balance check, manual seq, etc.)

                // Reuse the same service you already inject/instantiate elsewhere
                // Assuming you have IAcctgTransService available — if not, add it to ctor
                var acctgTransParams = new CreateAcctgTransParams
                {
                    AcctgTransTypeId = "APARTMENT_SALE", // or "APARTMENT_SALE" if you add it later
                    TransactionDate = sr.SaleDate ?? DateTime.UtcNow.Date,
                    IsPosted = "Y",
                    Description = $"Apartment Sale - SR {sr.SalesRequestId} - {apartment.ApartmentName}",
                    GlFiscalTypeId = "ACTUAL"
                };

                var acctgTransId = await _acctgTransService.CreateAcctgTrans(acctgTransParams);

                var stamp = DateTime.UtcNow;
                var seq = 0;

                // Debit: Accounts Receivable - Customer owes full amount
                var debitEntry = new AcctgTransEntry
                {
                    AcctgTransId = acctgTransId,
                    AcctgTransEntrySeqId = (++seq).ToString().PadLeft(3, '0'), // "001"
                    GlAccountId = "121100", // AR - Customers
                    DebitCreditFlag = "D",
                    AcctgTransEntryTypeId = "_NA_",
                    Amount = totalPrice,
                    ReconcileStatusId = "AES_NOT_RECONCILED",
                    Description = $"Apartment sale receivable - {apartment.ApartmentName}",
                    OrganizationPartyId = companyPartyId,
                    CreatedStamp = stamp,
                    LastUpdatedStamp = stamp
                };
                await _acctgTransService.CreateAcctgTransEntry(debitEntry);

                // Credit: Revenue from Apartment Sales
                var creditEntry = new AcctgTransEntry
                {
                    AcctgTransId = acctgTransId,
                    AcctgTransEntrySeqId = (++seq).ToString().PadLeft(3, '0'), // "002"
                    GlAccountId = "250120", // Revenue - Apartment Sales
                    DebitCreditFlag = "C",
                    AcctgTransEntryTypeId = "_NA_",
                    Amount = totalPrice,
                    ReconcileStatusId = "AES_NOT_RECONCILED",
                    Description = $"Apartment sale revenue - {apartment.ApartmentName}",
                    OrganizationPartyId = companyPartyId,
                    CreatedStamp = stamp,
                    LastUpdatedStamp = stamp
                };
                await _acctgTransService.CreateAcctgTransEntry(creditEntry);


                // 7. Save approval
                var saved = await _context.SaveChangesAsync(ct) > 0;
                if (!saved)
                {
                    await transaction.RollbackAsync(ct);
                    return Result<CreateSalesRequest.SalesRequestResponseDto>.Failure(
                        "Failed to approve sales request");
                }

                await transaction.CommitAsync(ct);

                // 8. Return full DTO (same as create)
                var response = await BuildResponseDto(sr, ct);
                return Result<CreateSalesRequest.SalesRequestResponseDto>.Success(response);
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync(ct);
                return Result<CreateSalesRequest.SalesRequestResponseDto>.Failure(
                    $"Failed to approve sales request: {ex.Message}");
            }
        }

        // Extracted for clarity — same as your existing logic
        private async Task<CreateSalesRequest.SalesRequestResponseDto> BuildResponseDto(SalesRequest sr,
            CancellationToken ct)
        {
            var fromParty = await _context.Parties
                .Where(p => p.PartyId == sr.FromPartyId)
                .Select(p => new { p.PartyId, p.Description, Phone = string.Empty })
                .FirstOrDefaultAsync(ct);
            
            var employee = await _context.Parties
                .Where(p => p.PartyId == sr.EmployeePartyId)
                .Select(p => new { p.PartyId, p.Description })
                .FirstOrDefaultAsync(ct);


            var apartment = await CreateSalesRequest.Handler.GetApartmentLovProjection(_context, sr.ProductId!, ct)
                            ?? new CreateSalesRequest.ApartmentLovProjection { ApartmentId = sr.ProductId! };

            var statusDesc = await _context.StatusItems
                .Where(s => s.StatusId == sr.StatusId)
                .Select(s => s.Description ?? s.StatusId)
                .FirstOrDefaultAsync(ct) ?? "Approved";

            return new CreateSalesRequest.SalesRequestResponseDto
            {
                SalesRequestId = sr.SalesRequestId,
                FromPartyId = fromParty?.PartyId ?? sr.FromPartyId!,
                FromPartyName = fromParty?.Description ?? string.Empty,
                FromPartyPhone = fromParty?.Phone ?? string.Empty,
                EmployeePartyId = employee?.PartyId ?? sr.EmployeePartyId ?? string.Empty,
                EmployeeName = employee?.Description ?? string.Empty,
                ApartmentId = apartment.ApartmentId,
                ApartmentName = apartment.ApartmentName,
                ProjectName = apartment.ProjectName,
                FloorNumber = apartment.FloorNumber,
                ApartmentSpaceM2 = apartment.ApartmentSpaceM2,
                GardenSpaceM2 = apartment.GardenSpaceM2,
                ApartmentPricePerM2 = apartment.ApartmentPricePerM2,
                GardenPricePerM2 = apartment.GardenPricePerM2 ?? sr.GardenPricePerM2,
                TotalPrice = sr.TotalPrice ?? 0m,
                Discount = sr.Discount,
                AdvancePayment = sr.AdvancePayment ?? 0m,
                NumberOfInstallments = (int)sr.NumberOfInstallments,
                DateOfFirstInstallment = sr.DateOfFirstInstallment,
                MonthsBetweenInstallments = (int)sr.MonthsBetweenInstallments,
                MaintenanceDeposit = sr.MaintenanceDeposit,
                SaleDate = sr.SaleDate ?? DateTime.UtcNow,
                Comments = sr.Comments,
                StatusId = sr.StatusId,
                StatusDescription = statusDesc,
                CreatedStamp = (DateTime)sr.CreatedStamp,
                LastUpdatedStamp = (DateTime)sr.LastUpdatedStamp
            };
        }
    }
}