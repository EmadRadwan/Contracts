using Domain;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Persistence;
using Application.Accounting.OrganizationGlSettings;

namespace Application.Accounting.FinAccounts;

public class GetFinAccountTransListAndTotals
{
    public class Command : IRequest<Result<FinAccountTransTotalsDto>>
    {
        public FinAccountTransationParamsDto Params { get; set; }
    }

    public class Handler : IRequestHandler<Command, Result<FinAccountTransTotalsDto>>
    {
        private readonly DataContext _context;

        public Handler(DataContext context)
        {
            _context = context;
        }

        public async Task<Result<FinAccountTransTotalsDto>> Handle(Command request, CancellationToken cancellationToken)
        {
            try
            {
                // 1) =============== First, fetch all transactions for finAccountId to iterate for partial totals
                var finAccountTransactions = await _context.FinAccountTrans
                    .Where(t => t.FinAccountId == request.Params.FinAccountId)
                    .ToListAsync(cancellationToken);

                // Initialize aggregator variables
                decimal grandTotal = 0;
                decimal createdGrandTotal = 0;
                long totalCreatedTransactions = 0;
                decimal approvedGrandTotal = 0;
                long totalApprovedTransactions = 0;
                decimal createdApprovedGrandTotal = 0;
                decimal glReconciliationApprovedGrandTotal = 0;

                // 2) =============== Aggregate “pre-iteration” totals
                foreach (var finAccountTransaction in finAccountTransactions)
                {
                    // Determine if this is a WITHDRAWAL
                    var isWithdrawal = finAccountTransaction.FinAccountTransTypeId == "WITHDRAWAL";

                    // Evaluate status
                    if (finAccountTransaction.StatusId == "FINACT_TRNS_CREATED")
                    {
                        totalCreatedTransactions += 1;
                        if (isWithdrawal)
                        {
                            createdGrandTotal -= (decimal)finAccountTransaction.Amount;
                            createdApprovedGrandTotal -= (decimal)finAccountTransaction.Amount;
                        }
                        else
                        {
                            createdGrandTotal += (decimal)finAccountTransaction.Amount;
                            createdApprovedGrandTotal += (decimal)finAccountTransaction.Amount;
                        }
                    }
                    else if (finAccountTransaction.StatusId == "FINACT_TRNS_APPROVED")
                    {
                        totalApprovedTransactions += 1;
                        if (isWithdrawal)
                        {
                            approvedGrandTotal -= (decimal)finAccountTransaction.Amount;
                            createdApprovedGrandTotal -= (decimal)finAccountTransaction.Amount;
                        }
                        else
                        {
                            approvedGrandTotal += (decimal)finAccountTransaction.Amount;
                            createdApprovedGrandTotal += (decimal)finAccountTransaction.Amount;
                        }

                        // If glReconciliationId matches
                        if (!string.IsNullOrEmpty(request.Params.GlReconciliationId) &&
                            request.Params.GlReconciliationId == finAccountTransaction.GlReconciliationId)
                        {
                            if (isWithdrawal)
                                glReconciliationApprovedGrandTotal -= (decimal)finAccountTransaction.Amount;
                            else
                                glReconciliationApprovedGrandTotal += (decimal)finAccountTransaction.Amount;
                        }
                    }
                }

                // 3) =============== Compute the finAccountTransList with filtering logic
                // Distinguish between glReconciliationId == "_NA_" vs other
                List<FinAccountTransListDto> finAccountTransList = null;

                if (request.Params.GlReconciliationId == "_NA_")
                {
                    // Determine the conditional status: if no status is provided or it is not 'FINACT_TRNS_CANCELED',
                    // then we want to exclude transactions with that status.
                    var isConditionalStatus = string.IsNullOrEmpty(request.Params.StatusId) ||
                                              request.Params.StatusId != "FINACT_TRNS_CANCELED";
                    var conditionalStatusId = isConditionalStatus ? "FINACT_TRNS_CANCELED" : null;

                    var query = from fat in _context.FinAccountTrans
                                join fatt in _context.FinAccountTransTypes
                                    on fat.FinAccountTransTypeId equals fatt.FinAccountTransTypeId
                                join sts in _context.StatusItems
                                    on fat.StatusId equals sts.StatusId
                                where fat.FinAccountId == request.Params.FinAccountId
                                      // Optional filter: if a FinAccountTransTypeId was provided, match it.
                                      && (string.IsNullOrEmpty(request.Params.FinAccountTransTypeId) ||
                                          fat.FinAccountTransTypeId == request.Params.FinAccountTransTypeId)
                                      // If a conditional status is determined, ensure the record's status is not that value.
                                      && (string.IsNullOrEmpty(conditionalStatusId) ||
                                          fat.StatusId != conditionalStatusId)
                                      // If an explicit status was provided, use that filter.
                                      && (string.IsNullOrEmpty(request.Params.StatusId) ||
                                          fat.StatusId == request.Params.StatusId)
                                      // Only include transactions that are not yet reconciled.
                                      && fat.GlReconciliationId == null
                                      // Filter on TransactionDate if specified.
                                      && (!request.Params.FromTransactionDate.HasValue ||
                                          fat.TransactionDate >= request.Params.FromTransactionDate.Value)
                                      && (!request.Params.ThruTransactionDate.HasValue ||
                                          fat.TransactionDate <= request.Params.ThruTransactionDate.Value)
                                      // Filter on EntryDate if specified.
                                      && (!request.Params.FromEntryDate.HasValue ||
                                          fat.EntryDate >= request.Params.FromEntryDate.Value)
                                      && (!request.Params.ThruEntryDate.HasValue ||
                                          fat.EntryDate <= request.Params.ThruEntryDate.Value)
                                select new FinAccountTransListDto
                                {
                                    FinAccountTransId = fat.FinAccountTransId,
                                    FinAccountTransTypeId = fat.FinAccountTransTypeId,
                                    FinAccountTransTypeDescription = fatt.Description,
                                    FinAccountId = fat.FinAccountId,
                                    PartyId = fat.PartyId,
                                    GlReconciliationId = fat.GlReconciliationId,
                                    TransactionDate = fat.TransactionDate,
                                    EntryDate = fat.EntryDate,
                                    Amount = fat.Amount,
                                    PaymentId = fat.PaymentId,
                                    OrderId = fat.OrderId,
                                    OrderItemSeqId = fat.OrderItemSeqId,
                                    PerformedByPartyId = fat.PerformedByPartyId,
                                    ReasonEnumId = fat.ReasonEnumId,
                                    Comments = fat.Comments,
                                    StatusId = fat.StatusId,
                                    StatusDescription = sts.Description,
                                    LastUpdatedStamp = fat.LastUpdatedStamp,
                                    LastUpdatedTxStamp = fat.LastUpdatedTxStamp,
                                    CreatedStamp = fat.CreatedStamp,
                                    CreatedTxStamp = fat.CreatedTxStamp
                                };

                    finAccountTransList = await query.ToListAsync(cancellationToken);
                }
                else
                {
                    var query = from fat in _context.FinAccountTrans
                                join fatt in _context.FinAccountTransTypes
                                    on fat.FinAccountTransTypeId equals fatt.FinAccountTransTypeId
                                join sts in _context.StatusItems
                                    on fat.StatusId equals sts.StatusId
                                where fat.FinAccountId == request.Params.FinAccountId
                                      // Optional filter for FinAccountTransTypeId.
                                      && (string.IsNullOrEmpty(request.Params.FinAccountTransTypeId) ||
                                          fat.FinAccountTransTypeId == request.Params.FinAccountTransTypeId)
                                      // Optional filter for StatusId.
                                      && (string.IsNullOrEmpty(request.Params.StatusId) ||
                                          fat.StatusId == request.Params.StatusId)
                                      // Filter for a provided GlReconciliationId.
                                      && (string.IsNullOrEmpty(request.Params.GlReconciliationId) ||
                                          fat.GlReconciliationId == request.Params.GlReconciliationId)
                                      // Filter on TransactionDate if specified.
                                      && (!request.Params.FromTransactionDate.HasValue ||
                                          fat.TransactionDate >= request.Params.FromTransactionDate.Value)
                                      && (!request.Params.ThruTransactionDate.HasValue ||
                                          fat.TransactionDate <= request.Params.ThruTransactionDate.Value)
                                      // Filter on EntryDate if specified.
                                      && (!request.Params.FromEntryDate.HasValue ||
                                          fat.EntryDate >= request.Params.FromEntryDate.Value)
                                      && (!request.Params.ThruEntryDate.HasValue ||
                                          fat.EntryDate <= request.Params.ThruEntryDate.Value)
                                select new FinAccountTransListDto
                                {
                                    FinAccountTransId = fat.FinAccountTransId,
                                    FinAccountTransTypeId = fat.FinAccountTransTypeId,
                                    FinAccountTransTypeDescription = fatt.Description,
                                    FinAccountId = fat.FinAccountId,
                                    PartyId = fat.PartyId,
                                    GlReconciliationId = fat.GlReconciliationId,
                                    TransactionDate = fat.TransactionDate,
                                    EntryDate = fat.EntryDate,
                                    Amount = fat.Amount,
                                    PaymentId = fat.PaymentId,
                                    OrderId = fat.OrderId,
                                    OrderItemSeqId = fat.OrderItemSeqId,
                                    PerformedByPartyId = fat.PerformedByPartyId,
                                    ReasonEnumId = fat.ReasonEnumId,
                                    Comments = fat.Comments,
                                    StatusId = fat.StatusId,
                                    StatusDescription = sts.Description,
                                    LastUpdatedStamp = fat.LastUpdatedStamp,
                                    LastUpdatedTxStamp = fat.LastUpdatedTxStamp,
                                    CreatedStamp = fat.CreatedStamp,
                                    CreatedTxStamp = fat.CreatedTxStamp
                                };

                    finAccountTransList = await query.ToListAsync(cancellationToken);
                }

                // 4) =============== Compute grandTotal by iterating over finAccountTransList
                decimal computedGrandTotal = 0;
                foreach (var finAccountTrans in finAccountTransList)
                {
                    var isWithdrawal = finAccountTrans.FinAccountTransTypeId == "WITHDRAWAL";
                    if (isWithdrawal)
                        computedGrandTotal -= (decimal)finAccountTrans.Amount;
                    else
                        computedGrandTotal += (decimal)finAccountTrans.Amount;
                }

                // 5) =============== Construct final outputs
                // totalCreatedApprovedTransactions
                var totalCreatedApprovedTransactions = totalCreatedTransactions + totalApprovedTransactions;

                // If openingBalance is provided, add it to glReconciliationApprovedGrandTotal
                if (request.Params.OpeningBalance.HasValue)
                    // In the original script: glReconciliationApprovedGrandTotal += parameters.openingBalance
                    glReconciliationApprovedGrandTotal += request.Params.OpeningBalance.Value;

                // Build final DTO
                var dto = new FinAccountTransTotalsDto
                {
                    FinAccountTransList = finAccountTransList,
                    SearchedNumberOfRecords = finAccountTransList.Count,
                    GrandTotal = computedGrandTotal,
                    CreatedGrandTotal = createdGrandTotal,
                    TotalCreatedTransactions = totalCreatedTransactions,
                    ApprovedGrandTotal = approvedGrandTotal,
                    TotalApprovedTransactions = totalApprovedTransactions,
                    CreatedApprovedGrandTotal = createdApprovedGrandTotal,
                    TotalCreatedApprovedTransactions = totalCreatedApprovedTransactions,
                    GlReconciliationApprovedGrandTotal = glReconciliationApprovedGrandTotal
                };

                return Result<FinAccountTransTotalsDto>.Success(dto);
            }
            catch (Exception ex)
            {
                // Ideally, log the exception here
                return Result<FinAccountTransTotalsDto>.Failure(
                    $"Error retrieving fin account trans list and totals: {ex.Message}");
            }
        }
    }
}