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

        private async Task<string> GetReceivableGlAccountId(
            string organizationPartyId,
            string customerPartyId,
            CancellationToken ct)
        {
            // Try to find party-specific override
            var partyGlAccount = await _context.PartyGlAccounts
                .Where(pga =>
                    pga.OrganizationPartyId == organizationPartyId &&
                    pga.PartyId == customerPartyId &&
                    pga.RoleTypeId == "BILL_TO_CUSTOMER" && // most common for sales
                    pga.GlAccountTypeId == "ACCOUNTS_RECEIVABLE")
                .Select(pga => pga.GlAccountId)
                .FirstOrDefaultAsync(ct);

            // Fall back to default parent AR account
            return partyGlAccount ?? "121100";
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
                    .Include(s => s.Installments) // Critical: Load custom installments
                    .FirstOrDefaultAsync(x => x.SalesRequestId == salesRequestId, ct);

                if (sr == null)
                    return Result<CreateSalesRequest.SalesRequestResponseDto>.Failure("Sales request not found");

                if (sr.StatusId == "SALES_REQUEST_APPROVED")
                    return Result<CreateSalesRequest.SalesRequestResponseDto>.Failure(
                        "Sales request is already approved");

                if (sr.Installments == null || !sr.Installments.Any())
                {
                    return Result<CreateSalesRequest.SalesRequestResponseDto>.Failure(
                        "Cannot approve: No payment plan defined. Please create and apply a custom payment plan.");
                }

                // 2. Update status
                sr.StatusId = "SALES_REQUEST_APPROVED";
                sr.LastUpdatedStamp = DateTime.UtcNow;

                // get Apartment record from Product table
                var product = await _context.Products
                    .FirstOrDefaultAsync(p => p.ProductId == sr.ProductId, ct);

                // mark apartment as sold
                if (product != null)
                {
                    product.ApartmentStatusId = "APARTMENT_SOLD";
                }

                // 3. Get company PayTo PartyId (cached or from service)
                var companyPartyId = await _productStoreService.GetProductStorePayToPartId();

                // 4. Generate Payments
                var paymentsToCreate = new List<CreatePaymentParam>();

                var totalPrice = sr.TotalPrice ?? 0m;

                // Sort by due date (and installment number as fallback) to ensure consistent order
                var orderedInstallments = sr.Installments
                    .OrderBy(i => i.DueDate)
                    .ThenBy(i => i.InstallmentNumber)
                    .ToList();

                foreach (var inst in orderedInstallments)
                {
                    var paymentTypeId = inst.IsAdvance
                        ? "RECEIPT_ADVANCE_PAYMENT"
                        : "RECEIPT_DUE_INSTALLMENT";

                    var comments = inst.IsAdvance
                        ? $"Advance payment #{inst.InstallmentNumber} – Apt {sr.ProductId} – SR {sr.SalesRequestId}"
                        : $"Installment #{inst.InstallmentNumber} – Due {inst.DueDate:yyyy-MM-dd} – Apt {sr.ProductId} – SR {sr.SalesRequestId}";
                    
                    paymentsToCreate.Add(new CreatePaymentParam
                    {
                        PartyIdFrom = sr.FromPartyId!, // Customer
                        PartyIdTo = companyPartyId, // Company
                        Amount = inst.Amount,
                        EffectiveDate = inst.DueDate, // Use exact due date from plan
                        PaymentTypeId = paymentTypeId,
                        StatusId = "PMNT_NOT_PAID", // Unpaid by default
                        Comments = comments,
                        SalesRequestId = sr.SalesRequestId,
                        PaymentMethodId = null,
                        PaymentMethodTypeId = null
                    });
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

                if (sr.MaintenanceDeposit > 0)
                {
                    var maintenanceDueDate = (sr.SaleDate ?? DateTime.UtcNow.Date).AddYears(2);

                    var maintenancePaymentParam = new CreatePaymentParam
                    {
                        PartyIdFrom = sr.FromPartyId!, // Customer
                        PartyIdTo = companyPartyId, // Company
                        Amount = sr.MaintenanceDeposit.Value, // It's nullable, but we checked > 0
                        EffectiveDate = maintenanceDueDate,
                        PaymentTypeId = "RECEIPT_MAINTENANCE_AMOUNT",
                        StatusId = "PMNT_NOT_PAID",
                        Comments =
                            $"Maintenance deposit - Due {maintenanceDueDate:yyyy-MM-dd} - SR {sr.SalesRequestId}",
                        SalesRequestId = sr.SalesRequestId,
                        PaymentMethodId = null,
                        PaymentMethodTypeId = null
                    };

                    var maintenancePayment = await _paymentHelperService.CreatePayment(maintenancePaymentParam);
                    if (maintenancePayment == null)
                    {
                        await transaction.RollbackAsync(ct);
                        return Result<CreateSalesRequest.SalesRequestResponseDto>.Failure(
                            "Failed to create maintenance deposit payment");
                    }
                }

                var apartment = await CreateSalesRequest.Handler.GetApartmentLovProjection(_context, sr.ProductId!, ct)
                                ?? new CreateSalesRequest.ApartmentLovProjection
                                    { ApartmentId = sr.ProductId!, ApartmentName = "Unknown Apartment" };


                // 6. Create single accounting transaction for the full apartment sale amount
                // Following your exact OFBiz-style pattern (no balance check, manual seq, etc.)

                var receivableGlAccountId = await GetReceivableGlAccountId(
                    companyPartyId,
                    sr.FromPartyId!,
                    ct);

                var acctgTransParams = new CreateAcctgTransParams
                {
                    AcctgTransTypeId = "APARTMENT_SALE_INSTALLMENTS", // or "APARTMENT_SALE" if you add it later
                    TransactionDate = sr.SaleDate ?? DateTime.UtcNow.Date,
                    IsPosted = "Y",
                    Description = $"Apartment Sale - SR {sr.SalesRequestId} - {apartment.ApartmentName}",
                    GlFiscalTypeId = "ACTUAL",
                    SalesRequestId = sr.SalesRequestId,
                    PartyId = sr.FromPartyId
                };

                var acctgTransId = await _acctgTransService.CreateAcctgTrans(acctgTransParams);

                var stamp = DateTime.UtcNow;
                var seq = 0;

                // Debit: Accounts Receivable - Customer owes full amount
                var debitEntry = new AcctgTransEntry
                {
                    AcctgTransId = acctgTransId,
                    AcctgTransEntrySeqId = (++seq).ToString().PadLeft(3, '0'), // "001"
                    GlAccountId = receivableGlAccountId,
                    DebitCreditFlag = "D",
                    AcctgTransEntryTypeId = "_NA_",
                    Amount = totalPrice,
                    ReconcileStatusId = "AES_NOT_RECONCILED",
                    Description = $"Receivable for apartment {sr.ProductId} ({apartment.ApartmentName}) - SR {sr.SalesRequestId}",
                    OrganizationPartyId = companyPartyId,
                    ProductId = sr.ProductId,
                    PartyId = sr.FromPartyId,
                    CreatedStamp = stamp,
                    LastUpdatedStamp = stamp
                };
                await _acctgTransService.CreateAcctgTransEntry(debitEntry);

                // Credit: Revenue from Apartment Sales
                var creditEntry = new AcctgTransEntry
                {
                    AcctgTransId = acctgTransId,
                    AcctgTransEntrySeqId = (++seq).ToString().PadLeft(3, '0'), // "002"
                    GlAccountId = "250120",
                    DebitCreditFlag = "C",
                    AcctgTransEntryTypeId = "_NA_",
                    Amount = totalPrice,
                    ReconcileStatusId = "AES_NOT_RECONCILED",
                    Description = $"Revenue - apartment {sr.ProductId} ({apartment.ApartmentName}) - SR {sr.SalesRequestId}",
                    OrganizationPartyId = companyPartyId,
                    PartyId = sr.FromPartyId,
                    ProductId = sr.ProductId,
                    CreatedStamp = stamp,
                    LastUpdatedStamp = stamp
                };
                await _acctgTransService.CreateAcctgTransEntry(creditEntry);

                if (sr.IsChequesDelivered == true)
                {
                    acctgTransParams = new CreateAcctgTransParams
                    {
                        AcctgTransTypeId = "APARTMENT_SALE_CHEQUE", // or "APARTMENT_SALE" if you add it later
                        TransactionDate = sr.SaleDate ?? DateTime.UtcNow.Date,
                        IsPosted = "Y",
                        Description = $"Apartment Sale - SR {sr.SalesRequestId} - {apartment.ApartmentName}",
                        GlFiscalTypeId = "ACTUAL",
                        SalesRequestId = sr.SalesRequestId,
                        PartyId = sr.FromPartyId
                    };

                    acctgTransId = await _acctgTransService.CreateAcctgTrans(acctgTransParams);

                    seq = 0;

                    // Debit: Accounts Receivable - Customer owes full amount
                    debitEntry = new AcctgTransEntry
                    {
                        AcctgTransId = acctgTransId,
                        AcctgTransEntrySeqId = (++seq).ToString().PadLeft(3, '0'), // "001"
                        GlAccountId = "124410",
                        DebitCreditFlag = "D",
                        AcctgTransEntryTypeId = "_NA_",
                        Amount = totalPrice,
                        ReconcileStatusId = "AES_NOT_RECONCILED",
                        Description = $"Apartment sale receivable - {apartment.ApartmentName}",
                        OrganizationPartyId = companyPartyId,
                        ProductId = sr.ProductId,
                        PartyId = sr.FromPartyId,
                        CreatedStamp = stamp,
                        LastUpdatedStamp = stamp
                    };
                    await _acctgTransService.CreateAcctgTransEntry(debitEntry);

                    // Credit: Revenue from Apartment Sales
                    creditEntry = new AcctgTransEntry
                    {
                        AcctgTransId = acctgTransId,
                        AcctgTransEntrySeqId = (++seq).ToString().PadLeft(3, '0'), // "002"
                        GlAccountId = receivableGlAccountId,
                        DebitCreditFlag = "C",
                        AcctgTransEntryTypeId = "_NA_",
                        Amount = totalPrice,
                        ReconcileStatusId = "AES_NOT_RECONCILED",
                        Description = $"Apartment sale revenue - {apartment.ApartmentName}",
                        OrganizationPartyId = companyPartyId,
                        PartyId = sr.FromPartyId,
                        ProductId = sr.ProductId,
                        CreatedStamp = stamp,
                        LastUpdatedStamp = stamp
                    };
                    await _acctgTransService.CreateAcctgTransEntry(creditEntry);
                }


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
                IsChequesDelivered = sr.IsChequesDelivered,
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