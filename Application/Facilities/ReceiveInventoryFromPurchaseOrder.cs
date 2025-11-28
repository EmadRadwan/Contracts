using Application.Order.Orders;
using Application.Projects;
using Application.Shipments;
using Domain;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.Logging;
using Persistence;

namespace Application.Facilities;

public class ReceiveInventoryFromPurchaseOrder
{
    public class Command : IRequest<Result<ReceiveInventoryResult>>
    {
        public string OrderId { get; set; }
        public string FacilityId { get; set; }
        public bool DeliverToSite { get; set; } = false;
    }

    public class Handler : IRequestHandler<Command, Result<ReceiveInventoryResult>>
    {
        private readonly DataContext _context;
        private readonly ILogger<Handler> _logger;
        private readonly IMediator _mediator;
        private readonly IShipmentService _shipmentService;

        public Handler(DataContext context, IMediator mediator, ILogger<Handler> logger,
            IShipmentService shipmentService)
        {
            _context = context;
            _mediator = mediator;
            _logger = logger;
            _shipmentService = shipmentService;
        }

        public async Task<Result<ReceiveInventoryResult>> Handle(Command request, CancellationToken cancellationToken)
        {
            // this prevents nested transaction errors when called within an outer transaction (e.g., from another handler),
            // allowing seamless integration while maintaining atomicity. Improves robustness by checking CurrentTransaction.
            IDbContextTransaction? transaction = null;
            var ownsTransaction = false;
            try
            {
                if (_context.Database.CurrentTransaction == null)
                {
                    transaction = await _context.Database.BeginTransactionAsync(cancellationToken);
                    ownsTransaction = true;
                    _logger.LogDebug("Started new transaction for ReceiveInventoryFromPurchaseOrder.");
                }
                else
                {
                    _logger.LogDebug("Using existing transaction for ReceiveInventoryFromPurchaseOrder.");
                }

                // this aligns with ListPurchaseOrderItemsForReceive logic and prevents processing invalid orders.
                var purchaseOrder = await _context.OrderHeaders
                    .FirstOrDefaultAsync(po => po.OrderId == request.OrderId, cancellationToken);
                if (purchaseOrder == null)
                {
                    _logger.LogWarning("Purchase Order with ID {PurchaseOrderId} not found.", request.OrderId);
                    return Result<ReceiveInventoryResult>.Failure(
                        $"Purchase Order with ID {request.OrderId} not found.");
                }

                if (!string.Equals(purchaseOrder.OrderTypeId, "PURCHASE_ORDER", StringComparison.OrdinalIgnoreCase))
                {
                    _logger.LogWarning("OrderTypeId for Purchase Order ID {PurchaseOrderId} is not 'PURCHASE_ORDER'.",
                        request.OrderId);
                    return Result<ReceiveInventoryResult>.Failure(
                        $"OrderTypeId for Purchase Order ID {request.OrderId} is invalid.");
                }

                // this ensures a shipment exists before attempting to receive inventory, addressing the failure due to missing shipment artifacts.
                // Improves code by consolidating the workflow in the backend, bypassing frontend dependencies.
                var orderShipments = await _context.OrderShipments
                    .Where(os => os.OrderId == request.OrderId)
                    .ToListAsync(cancellationToken);

                if (!orderShipments.Any())
                {
                    var quickReceiveResult =
                        await _shipmentService.QuickReceivePurchaseOrder(request.OrderId, request.FacilityId);
                    if (quickReceiveResult == null)
                    {
                        _logger.LogWarning("Failed to create shipment for OrderId: {OrderId}", request.OrderId);
                        if (ownsTransaction) await transaction.RollbackAsync(cancellationToken);
                        return Result<ReceiveInventoryResult>.Failure("Failed to create shipment for purchase order.");
                    }

                    // : Added SaveChanges to persist shipment-related entities before proceeding to inventory receipt;
                    // this ensures the shipment is available for ReceiveInventoryProducts, maintaining transaction integrity.
                    var shipmentSaveResult = await _context.SaveChangesAsync(cancellationToken);
                    if (shipmentSaveResult <= 0)
                    {
                        _logger.LogWarning("Failed to persist shipment for OrderId: {OrderId}", request.OrderId);
                        if (ownsTransaction) await transaction.RollbackAsync(cancellationToken);
                        return Result<ReceiveInventoryResult>.Failure("Failed to persist shipment for purchase order.");
                    }
                }

                //  Extracted order item fetching into a separate method for clarity and reusability.
                // This improves maintainability by isolating data retrieval logic and makes it easier to test.
                var orderItems = await GetOrderItemsForPurchaseOrder(request.OrderId, cancellationToken);
                if (!orderItems.Any())
                {
                    _logger.LogWarning("No order items found for OrderId: {OrderId}", request.OrderId);
                    if (ownsTransaction) await transaction.RollbackAsync(cancellationToken);
                    return Result<ReceiveInventoryResult>.Failure("No order items found for the provided OrderId.");
                }

                //  Moved DTO construction to a separate method to encapsulate mapping logic.
                // This reduces code duplication and makes the handler more focused on orchestration.
                var receivedItemsDto = await ConstructReceiveInventoryItemsDto(request.OrderId, request.FacilityId,
                    orderItems, cancellationToken);

                //  Reuse existing ReceiveInventoryProducts logic to avoid duplicating transaction and error-handling code.
                // This ensures consistency with existing inventory receiving functionality. Note: Inner handler will detect existing transaction and skip its own.
                var result =
                    await _mediator.Send(new ReceiveInventoryProducts.Command { ReceivedItems = receivedItemsDto },
                        cancellationToken);
                if (!result.IsSuccess)
                {
                    _logger.LogError("Failed to receive inventory for OrderId: {OrderId}. Error: {Error}",
                        request.OrderId, result.Error);
                    if (ownsTransaction) await transaction.RollbackAsync(cancellationToken);
                    return result;
                }

                var workEffort = await _context.WorkEfforts
                    .FirstOrDefaultAsync(
                        we => we.RelatedOrderId == request.OrderId && we.WorkEffortTypeId == "PROJECT_CERTIFICATE",
                        cancellationToken);

                if (workEffort != null)
                {
                    workEffort.CurrentStatusId = "WEPR_APPROVED";
                    workEffort.LastStatusUpdate = DateTime.UtcNow; // Update timestamp for auditability
                    _context.WorkEfforts.Update(workEffort);
                    _logger.LogInformation(
                        "Updated WorkEffort {WorkEffortId} status to WEPR_APPROVED for OrderId: {OrderId}",
                        workEffort.WorkEffortId, request.OrderId);
                }
                else
                {
                    _logger.LogWarning("No PROJECT_CERTIFICATE found for OrderId: {OrderId}", request.OrderId);
                    // Note: Not failing the transaction as certificate update is secondary to inventory receipt
                }

                // REFACTOR: Added SaveChanges to persist WorkEffort update before committing the transaction;
                // this ensures the certificate status is saved within the same transaction as inventory receipt.
                var workEffortSaveResult = await _context.SaveChangesAsync(cancellationToken);
                if (workEffortSaveResult > 0)
                {
                    _logger.LogDebug("Persisted WorkEffort status update for OrderId: {OrderId}", request.OrderId);
                }

                if (request.DeliverToSite && workEffort != null &&
                    workEffort.CertificateCategory == "SUPPLY_PROCUREMENT_CERTIFICATE")
                {
                    try
                    {
                        _logger.LogInformation(
                            "DeliverToSite is true — attempting to auto-create COMPANY_SUPPLY_SALE_CERTIFICATE for OrderId: {OrderId}",
                            request.OrderId);

                        // Fetch the original procurement certificate + its items
                        var procurementCertificate = await _context.WorkEfforts
                            .Where(we =>
                                we.WorkEffortId == workEffort.WorkEffortId &&
                                we.WorkEffortTypeId == "PROJECT_CERTIFICATE")
                            .Select(we => new
                            {
                                we.ProjectId,
                                we.PartyIdSupplier,
                                we.FacilityId,
                                we.Description,
                                we.EstimatedStartDate,
                                we.EstimatedCompletionDate,
                                Items = _context.WorkEfforts
                                    .Where(item =>
                                        item.WorkEffortParentId == we.WorkEffortId &&
                                        item.WorkEffortTypeId == "CERTIFICATE_ITEM")
                                    .Select(item => new CertificateItemDto
                                    {
                                        ProductId = item.ProductId,
                                        ProductName = item.Product != null ? item.Product.ProductName : "Unknown",
                                        UomId = item.QuantityUomId,
                                        UomName = item.QuantityUomId, // You may want to map from Uom table
                                        Quantity = (decimal)item.Quantity,
                                        UnitPrice = item.Rate ?? 0,
                                        TotalAmount = item.TotalAmount ?? 0,
                                        Description = item.Description,
                                        ProcurementDate = item.ProcurementDate,
                                        TransportationExpenses = item.TransportationExpenses,
                                        Gratuities = item.Gratuities,
                                        Discount = item.Discount,
                                        // Note: We do NOT copy insurance/deductions — those belong to procurement
                                        Insurance = 0,
                                        AdditionalInsurance = 0,
                                        Deductions = 0,
                                        DeductionDescription = "",
                                        AchievementPercentage = 100, // Fully issued to site
                                        Net = item.TotalAmount ?? 0,
                                        Deserved = item.TotalAmount ?? 0,
                                    })
                                    .ToList()
                            })
                            .FirstOrDefaultAsync(cancellationToken);

                        var adjustedItems = new List<CertificateItemDto>();

                        foreach (var item in procurementCertificate.Items)
                        {
                            // Guard against divide-by-zero
                            var quantity = item.Quantity > 0 ? item.Quantity : 1;

                            // REFACTOR: Calculate total landed cost and redistribute into unit price
                            // Formula: (base amount - discount + transport + gratuities) / quantity
                            var baseAmount = item.UnitPrice * quantity;
                            var discount = item.Discount ?? 0;
                            var transport = item.TransportationExpenses ?? 0;
                            var gratuities = item.Gratuities ?? 0;

                            var totalLandedCost = baseAmount - discount + transport + gratuities;
                            var newUnitPrice = Math.Round(totalLandedCost / quantity, 4); // Adjust precision as needed

                            // Create clean item for site issuance
                            var cleanItem = new CertificateItemDto
                            {
                                ProductId = item.ProductId,
                                ProductName = item.ProductName,
                                UomId = item.UomId,
                                UomName = item.UomName,
                                Quantity = item.Quantity,
                                UnitPrice = newUnitPrice,
                                TotalAmount = Math.Round(newUnitPrice * quantity, 2),
                                Description = item.Description ?? $"إصدار إلى الموقع - {item.ProductName}",
                                ProcurementDate = DateTime.UtcNow,

                                // REFACTOR: Zero out all adjustments — they are now baked into UnitPrice
                                Discount = 0,
                                TransportationExpenses = 0,
                                Gratuities = 0,

                                // Clean slate for contractor billing
                                Insurance = 0,
                                AdditionalInsurance = 0,
                                Deductions = 0,
                                DeductionDescription = "",
                                AchievementPercentage = 100,
                                Net = Math.Round(newUnitPrice * quantity, 2),
                                Deserved = Math.Round(newUnitPrice * quantity, 2),
                            };

                            adjustedItems.Add(cleanItem);
                        }

                        // REFACTOR: Use adjusted items with capitalized cost
                        var saleCertificate = new ProjectCertificateDto
                        {
                            CertificateCategory = "COMPANY_SUPPLY_SALE_CERTIFICATE",
                            ProjectId = procurementCertificate.ProjectId,
                            PartyIdContractor = "SITE",
                            PartyIdSupplier = null,
                            FacilityId = procurementCertificate.FacilityId,
                            Description =
                                $"إصدار مواد إلى الموقع (تكلفة مُدمجة) - مرجع: {workEffort.CertificateNumber}",
                            EstimatedStartDate = procurementCertificate.EstimatedStartDate,
                            EstimatedCompletionDate = DateTime.UtcNow,

                            // REFACTOR: Use repriced, clean items
                            CertificateItems = adjustedItems
                        };

                        var createSaleResult = await _mediator.Send(
                            new CreateProjectCertificate.Command { Certificate = saleCertificate },
                            cancellationToken);

                        if (createSaleResult.IsSuccess)
                        {
                            _logger.LogInformation(
                                "Successfully created COMPANY_SUPPLY_SALE_CERTIFICATE {CertificateNumber} for site delivery",
                                createSaleResult.Value.CertificateNumber);
                            
                            var issueResult = await _mediator.Send(
                                new IssueMaterialsForCertificate.Command
                                {
                                    WorkEffortId = createSaleResult.Value.WorkEffortId  // ← This is the new certificate ID
                                },
                                cancellationToken);

                            if (issueResult.IsSuccess)
                            {
                                _logger.LogInformation(
                                    "Materials successfully issued from inventory for certificate {CertificateNumber}",
                                    createSaleResult.Value.CertificateNumber);
                            }
                        }
                        else
                        {
                            // Do NOT fail the entire receipt — issuance is secondary
                            _logger.LogError(
                                "Failed to auto-create COMPANY_SUPPLY_SALE_CERTIFICATE: {Error}",
                                createSaleResult.Error);
                        }
                    }
                    catch (Exception ex)
                    {
                        // Critical: Do NOT crash inventory receipt if issuance fails
                        _logger.LogError(ex,
                            "Exception occurred while trying to auto-create COMPANY_SUPPLY_SALE_CERTIFICATE for OrderId {OrderId}. Continuing with receipt.",
                            request.OrderId);
                    }
                }


                // REFACTOR: Commit only if this handler owns the transaction; defers to outer transaction otherwise.
                // Improves integration in nested scenarios without premature commits.
                if (ownsTransaction)
                {
                    await transaction.CommitAsync(cancellationToken);
                    _logger.LogDebug("Committed transaction for ReceiveInventoryFromPurchaseOrder.");
                }

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,
                    "An error occurred while processing ReceiveInventoryFromPurchaseOrder for OrderId: {OrderId}.",
                    request.OrderId);
                if (ownsTransaction) await transaction.RollbackAsync(cancellationToken);
                return Result<ReceiveInventoryResult>.Failure(
                    $"An error occurred while receiving inventory: {ex.Message}");
            }
            finally
            {
                // REFACTOR: Dispose transaction only if owned to avoid interfering with outer transactions.
                if (ownsTransaction && transaction != null) await transaction.DisposeAsync();
            }
        }

        private async Task<List<OrderItem>> GetOrderItemsForPurchaseOrder(string orderId,
            CancellationToken cancellationToken)
        {
            // REFACTOR: Used Include to eagerly load Product data, reducing database queries.
            // This improves performance by fetching all necessary data in a single query.
            return await _context.OrderItems
                .Where(oi => oi.OrderId == orderId)
                .Include(oi => oi.Product)
                .ToListAsync(cancellationToken);
        }

        private async Task<ReceiveInventoryItemsDto> ConstructReceiveInventoryItemsDto(
            string orderId, string? facilityId, List<OrderItem> orderItems, CancellationToken cancellationToken)
        {
            var items = new List<ReceiveInventoryItemDto>();
            foreach (var orderItem in orderItems)
            {
                // REFACTOR: Simplified DTO mapping with default values to reduce boilerplate code.
                // Defaults like QuantityAccepted = Quantity ensure full acceptance unless overridden.
                var itemDto = new ReceiveInventoryItemDto
                {
                    OrderId = orderItem.OrderId,
                    OrderItemSeqId = orderItem.OrderItemSeqId,
                    ProductId = orderItem.ProductId,
                    UomId = orderItem.UomId,
                    FacilityId = facilityId,
                    Quantity = orderItem.Quantity,
                    QuantityAccepted = orderItem.Quantity,
                    QuantityRejected = 0,
                    UnitPrice = orderItem.UnitPrice,
                    RejectionReasonId = null
                };
                items.Add(itemDto);
            }

            return new ReceiveInventoryItemsDto { OrderItems = items };
        }
    }
}