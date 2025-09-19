using Application.Core;
using Application.Order.Orders;
using FluentValidation;
using MediatR;
using Persistence;
using Domain;
using Application.Interfaces;
using Microsoft.EntityFrameworkCore;
using Application.Catalog.ProductStores;
using System;
using Application.order.Orders;

namespace Application.Projects
{
    public class CreateProjectCertificate
    {
        public class Command : IRequest<Result<ProjectCertificateDto>>
        {
            public ProjectCertificateDto? Certificate { get; set; }
        }

        public class CommandValidator : AbstractValidator<Command>
        {
            public CommandValidator()
            {
                RuleFor(x => x.Certificate!.CertificateItems)
                    .Must(items => items != null && items.Any())
                    .WithMessage("At least one certificate item is required");
            }
        }

        public class Handler : IRequestHandler<Command, Result<ProjectCertificateDto>>
        {
            private readonly DataContext _context;
            private readonly IUserAccessor _userAccessor;
            private readonly IUtilityService _utilityService;
            private readonly IOrderService _orderService;
            private readonly IProductStoreService _productStoreService;

            public Handler(DataContext context, IUserAccessor userAccessor, IUtilityService utilityService,
                IOrderService orderService, IProductStoreService productStoreService)
            {
                _context = context;
                _userAccessor = userAccessor;
                _utilityService = utilityService;
                _orderService = orderService;
                _productStoreService = productStoreService;
            }

            public async Task<Result<ProjectCertificateDto>> Handle(Command request,
                CancellationToken cancellationToken)
            {
                await using var transaction = await _context.Database.BeginTransactionAsync(cancellationToken);
                try
                {
                    var stamp = DateTime.UtcNow;
                    var certificate = request.Certificate!;
                    var newWorkEffortSerial = await _utilityService.GetNextSequence("WorkEffort");
                    string newProjectCertificateSerial;
                    string? partyCode = null;

                    // Purpose: Ensure consistent serial numbering based on party code; prioritize PartyIdContractor, fallback to PartyIdSupplier
                    // Context: Replaces previous logic that only used party code for WORKMANSHIP_CONTRACTING_CERTIFICATE
                    string? partyId = certificate.PartyIdContractor ?? certificate.PartyIdSupplier;
                    if (string.IsNullOrEmpty(partyId))
                    {
                        await transaction.RollbackAsync(cancellationToken);
                        return Result<ProjectCertificateDto>.Failure(
                            "No valid party ID (Contractor or Supplier) provided");
                    }

                    var certificateCount = await _context.WorkEfforts
                        .CountAsync(
                            we => (we.PartyIdContractor == partyId || we.PartyIdSupplier == partyId) &&
                                  we.CertificateCategory == certificate.CertificateCategory, cancellationToken);

                    newProjectCertificateSerial = string.Format("{0}-{1:D4}", partyId, certificateCount + 1);

                    var workEffort = new WorkEffort
                    {
                        WorkEffortId = newWorkEffortSerial,
                        CertificateNumber = newProjectCertificateSerial,
                        WorkEffortTypeId = "PROJECT_CERTIFICATE",
                        CertificateCategory = certificate.CertificateCategory,
                        PartyIdSupplier = certificate.PartyIdSupplier,
                        PartyIdContractor = certificate.PartyIdContractor,
                        ProjectId = certificate.ProjectId,
                        Description = certificate.Description,
                        EstimatedStartDate = certificate.EstimatedStartDate,
                        EstimatedCompletionDate = certificate.EstimatedCompletionDate,
                        CurrentStatusId = "WEPR_CREATED",
                        FacilityId = certificate.FacilityId,
                        CreatedDate = stamp,
                        LastUpdatedStamp = stamp
                    };

                    _context.WorkEfforts.Add(workEffort);

                    foreach (var item in certificate.CertificateItems!)
                    {
                        var itemWorkEffortSerial = await _utilityService.GetNextSequence("WorkEffort");
                        var itemWorkEffort = new WorkEffort
                        {
                            WorkEffortId = itemWorkEffortSerial,
                            WorkEffortParentId = newWorkEffortSerial,
                            WorkEffortTypeId = "CERTIFICATE_ITEM",
                            ProductId = item.ProductId,
                            Description = item.Description,
                            Quantity = item.Quantity,
                            Rate = item.UnitPrice,
                            TotalAmount = item.TotalAmount,
                            Discount = item.Discount ?? 0,
                            Insurance = item.Insurance ?? 0,
                            AdditionalInsurance = item.AdditionalInsurance,
                            MaterialPrice = certificate.CertificateCategory == "WORKMANSHIP_CONTRACTING_CERTIFICATE"
                                ? item.MaterialPrice
                                : 0,
                            LaborPrice = certificate.CertificateCategory == "WORKMANSHIP_CONTRACTING_CERTIFICATE"
                                ? item.LaborPrice
                                : 0,
                            QuantityUomId = item.UomId,
                            Deductions = item.Deductions ?? 0,
                            AchievementPercent = item.AchievementPercentage ?? 0,
                            Notes = item.Notes,
                            ProcurementDate = item.ProcurementDate,
                            TransportationExpenses = item.TransportationExpenses ?? 0,
                            Gratuities = item.Gratuities ?? 0,
                            CreatedDate = stamp,
                            LastUpdatedStamp = stamp,
                            CurrentStatusId = "WEPR_CREATED"
                        };
                        _context.WorkEfforts.Add(itemWorkEffort);
                    }

                    string? generatedOrderId = null;
                    OrderHeader? poResult = null;
                    if (certificate.CertificateCategory != "COMPANY_SUPPLY_SALE_CERTIFICATE")
                    {
                        var poItems = certificate.CertificateItems.ToList();
                        if (poItems.Any())
                        {
                            string fromPartyId = certificate.CertificateCategory switch
                            {
                                "SUPPLY_PROCUREMENT_CERTIFICATE" => certificate.PartyIdSupplier ??
                                                                    throw new InvalidOperationException(
                                                                        "PartyIdSupplier is required for supply/sale certificates"),
                                "WORKMANSHIP_CONTRACTING_CERTIFICATE" => certificate.PartyIdContractor ??
                                                                         throw new InvalidOperationException(
                                                                             "PartyIdContractor is required for contractor/workmanship certificates"),
                                _ => throw new InvalidOperationException(
                                    $"Unsupported certificate category: {certificate.CertificateCategory}")
                            };

                            var orderAdjustments = new List<OrderAdjustmentDto2>();
                            for (int index = 0; index < poItems.Count; index++)
                            {
                                var item = poItems[index];
                                var orderItemSeqId = (index + 1).ToString("D4");

                                if (item.Discount.HasValue && item.Discount > 0)
                                {
                                    orderAdjustments.Add(new OrderAdjustmentDto2
                                    {
                                        OrderAdjustmentId = Guid.NewGuid().ToString(),
                                        OrderAdjustmentTypeId = "DISCOUNT_ADJUSTMENT",
                                        OrderAdjustmentTypeDescription = "خصم",
                                        OrderId = null,
                                        OrderItemSeqId = orderItemSeqId,
                                        Amount = -item.Discount.Value,
                                        CorrespondingProductId = item.ProductId,
                                        CorrespondingProductName = item.ProductName,
                                        IsManual = "Y",
                                        CreatedDate = stamp,
                                        IsAdjustmentDeleted = false,
                                        SourcePercentage = item.TotalAmount > 0
                                            ? (item.Discount.Value / item.TotalAmount) * 100
                                            : 0
                                    });
                                }

                                if (item.TransportationExpenses.HasValue && item.TransportationExpenses > 0)
                                {
                                    orderAdjustments.Add(new OrderAdjustmentDto2
                                    {
                                        OrderAdjustmentId = Guid.NewGuid().ToString(),
                                        OrderAdjustmentTypeId = "SHIPPING_CHARGES",
                                        OrderAdjustmentTypeDescription = "Transportation Expenses",
                                        OrderId = null,
                                        OrderItemSeqId = orderItemSeqId,
                                        Amount = item.TransportationExpenses.Value,
                                        CorrespondingProductId = item.ProductId,
                                        CorrespondingProductName = item.ProductName,
                                        IsManual = "Y",
                                        CreatedDate = stamp,
                                        IsAdjustmentDeleted = false,
                                        SourcePercentage = null
                                    });
                                }

                                if (item.Gratuities.HasValue && item.Gratuities > 0)
                                {
                                    orderAdjustments.Add(new OrderAdjustmentDto2
                                    {
                                        OrderAdjustmentId = Guid.NewGuid().ToString(),
                                        OrderAdjustmentTypeId = "MISCELLANEOUS_CHARGE",
                                        OrderAdjustmentTypeDescription = "Gratuities",
                                        OrderId = null,
                                        OrderItemSeqId = orderItemSeqId,
                                        Amount = item.Gratuities.Value,
                                        CorrespondingProductId = item.ProductId,
                                        CorrespondingProductName = item.ProductName,
                                        IsManual = "Y",
                                        CreatedDate = stamp,
                                        IsAdjustmentDeleted = false,
                                        SourcePercentage = null
                                    });
                                }
                            }

                            // REFACTOR: Fixed SubTotal calculation for OrderItems
                            // Purpose: Aligns SubTotal with frontend's net for WORKMANSHIP_CONTRACTING_CERTIFICATE
                            // Context: Uses TotalAmount for non-WORKMANSHIP types, net (deserved - insurance - additionalInsurance) for WORKMANSHIP
                            var orderItems = poItems.Select((item, index) => new OrderItemDto2
                            {
                                OrderItemSeqId = (index + 1).ToString("D4"),
                                ProductId = item.ProductId,
                                ProductName = item.ProductName,
                                Quantity = item.Quantity,
                                UnitPrice = item.UnitPrice,
                                SubTotal = certificate.CertificateCategory == "WORKMANSHIP_CONTRACTING_CERTIFICATE"
                                    ? Math.Max(0,
                                        (item.TotalAmount - (item.Deductions ?? 0)) - (item.Insurance ?? 0) -
                                        (item.AdditionalInsurance))
                                    : item.TotalAmount,
                                FacilityId = certificate.FacilityId,
                                ItemDescription = item.Description,
                                OrderItemTypeId = "PRODUCT_ORDER_ITEM",
                                StatusId = "ITEM_CREATED",
                                CreatedStamp = stamp,
                                LastUpdatedStamp = stamp
                            }).ToList();

                            // REFACTOR: Fixed GrandTotal calculation to sum OrderItem SubTotals
                            // Purpose: Ensures GrandTotal reflects the sum of adjusted SubTotals
                            // Context: Avoids redundant calculations and aligns with OrderItem SubTotals
                            var grandTotal = orderItems.Sum(i => i.SubTotal) +
                                             orderAdjustments.Sum(a => a.Amount);

                            var orderDto = new OrderDto
                            {
                                OrderTypeId = "PURCHASE_ORDER",
                                FromPartyId = fromPartyId,
                                CurrencyUomId = await _productStoreService.GetProductStoreDefaultCurrencyId(),
                                OrderDate = stamp,
                                StatusId = "ORDER_CREATED",
                                StatusDescription = "Created",
                                InternalRemarks = $"Auto-generated from Certificate {newProjectCertificateSerial}",
                                GrandTotal = grandTotal,
                                OrderItems = orderItems,
                                OrderAdjustments = orderAdjustments
                            };


                            poResult = await _orderService.CreatePurchaseOrder(orderDto);
                            if (poResult == null)
                            {
                                await transaction.RollbackAsync(cancellationToken);
                                return Result<ProjectCertificateDto>.Failure("Failed to create purchase order");
                            }

                            generatedOrderId = poResult.OrderId;
                        }
                    }

                    if (!string.IsNullOrEmpty(generatedOrderId))
                    {
                        workEffort.RelatedOrderId = generatedOrderId;
                    }


                    // this ensures UpdateOrApprovePurchaseOrder can query and find the order via FirstOrDefaultAsync, as EF Core queries ignore pending changes.
                    // Improves code by allowing seamless approval in the same transaction without separate scopes or context issues.
                    var createResult = await _context.SaveChangesAsync(cancellationToken);
                    if (createResult <= 0)
                    {
                        await transaction.RollbackAsync(cancellationToken);
                        return Result<ProjectCertificateDto>.Failure("Failed to create certificate and items");
                    }

                    // this integrates backend approval directly behind the scenes, reusing the existing UpdateOrApprovePurchaseOrder method 
                    // without needing a frontend call, ensuring the order is immediately approved as per requirements while maintaining transaction integrity.
                    if (generatedOrderId != null && poResult != null)
                    {
                        // queried from the database after SaveChanges; this populates all required fields (e.g., OrderId, GrandTotal, FromPartyId, OrderItems, OrderAdjustments) 
                        // for the UpdateOrApprovePurchaseOrder method without modifying the called service signature, avoiding impacts on other code. 
                        // Improves code by ensuring complete data transfer for approval logic (e.g., item updates, roles) while keeping the approach isolated to this handler.
                        var orderHeader = await _context.OrderHeaders
                            .FirstOrDefaultAsync(oh => oh.OrderId == generatedOrderId, cancellationToken);
                        if (orderHeader == null)
                        {
                            await transaction.RollbackAsync(cancellationToken);
                            return Result<ProjectCertificateDto>.Failure("Created order not found after persistence");
                        }

                        var orderItems = await _context.OrderItems
                            .Where(oi => oi.OrderId == generatedOrderId)
                            .Select(oi => new OrderItemDto2
                            {
                                OrderId = oi.OrderId,
                                OrderItemSeqId = oi.OrderItemSeqId,
                                ProductId = oi.ProductId,
                                Quantity = oi.Quantity,
                                UnitPrice = oi.UnitPrice,
                                ItemDescription = oi.ItemDescription,
                                OrderItemTypeId = oi.OrderItemTypeId,
                                StatusId = oi.StatusId,
                                CreatedStamp = oi.CreatedStamp,
                                LastUpdatedStamp = oi.LastUpdatedStamp
                                // Add other mappings as needed from OrderItem entity to OrderItemDto2
                            }).ToListAsync(cancellationToken);

                        var orderAdjustments = await _context.OrderAdjustments
                            .Where(oa => oa.OrderId == generatedOrderId)
                            .Select(oa => new OrderAdjustmentDto2
                            {
                                OrderAdjustmentId = oa.OrderAdjustmentId,
                                OrderAdjustmentTypeId = oa.OrderAdjustmentTypeId,
                                //OrderAdjustmentTypeDescription = oa.OrderAdjustmentTypeDescription, // Assuming entity has this
                                OrderId = oa.OrderId,
                                OrderItemSeqId = oa.OrderItemSeqId,
                                Amount = oa.Amount,
                                CorrespondingProductId = oa.CorrespondingProductId, // Assuming mappings match DTO
                                //CorrespondingProductName = oa.CorrespondingProductName,
                                IsManual = oa.IsManual,
                                CreatedDate = oa.CreatedDate,
                                IsAdjustmentDeleted = false,
                                SourcePercentage = oa.SourcePercentage
                                // Add other mappings as needed from OrderAdjustment entity to OrderAdjustmentDto2
                            }).ToListAsync(cancellationToken);

                        var approveOrderDto = new OrderDto
                        {
                            OrderId = orderHeader.OrderId,
                            FromPartyId = certificate.PartyIdContractor ?? certificate.PartyIdSupplier,
                            GrandTotal = orderHeader.GrandTotal,
                            OrderDate = orderHeader.OrderDate,
                            CurrencyUomId = await _productStoreService.GetProductStoreDefaultCurrencyId(),
                            StatusId = orderHeader.StatusId,
                            OrderItems = orderItems,
                            OrderAdjustments = orderAdjustments
                            // Other fields can be set if needed, but minimal set suffices for approval
                        };

                        await _orderService.UpdateOrApprovePurchaseOrder(approveOrderDto, "APPROVE");

                        //  Added a second SaveChanges to persist approval changes (e.g., status updates, roles); 
                        // this is necessary after the service method updates entities but doesn't save, allowing batching within the transaction 
                        // and providing a checkpoint for failure handling.


                        var approveResult = await _context.SaveChangesAsync(cancellationToken);
                        if (approveResult <= 0)
                        {
                            await transaction.RollbackAsync(cancellationToken);
                            return Result<ProjectCertificateDto>.Failure("Failed to approve purchase order");
                        }
                    }

                    await transaction.CommitAsync(cancellationToken);

                    var project = await _context.WorkEfforts
                        .Where(p => p.WorkEffortId == certificate.ProjectId)
                        .Select(p => new { p.ProjectName })
                        .FirstOrDefaultAsync(cancellationToken);

                    var supplier = certificate.PartyIdSupplier != null
                        ? await _context.Parties
                            .Where(p => p.PartyId == certificate.PartyIdSupplier)
                            .Select(p => new { p.Description })
                            .FirstOrDefaultAsync(cancellationToken)
                        : null;

                    var contractor = certificate.PartyIdContractor != null
                        ? await _context.Parties
                            .Where(p => p.PartyId == certificate.PartyIdContractor)
                            .Select(p => new { p.Description })
                            .FirstOrDefaultAsync(cancellationToken)
                        : null;

                    var statusDescriptions = new Dictionary<string, (string English, string Arabic)>
                    {
                        { "WEPR_CREATED", ("Created", "تم الإنشاء") },
                        { "WEPR_APPROVED", ("Approved", "تمت الموافقة") },
                        { "WEPR_COMPLETE", ("Complete", "مكتمل") }
                    };

                    var (statusDescription, statusDescriptionArabic) =
                        statusDescriptions.ContainsKey(workEffort.CurrentStatusId)
                            ? statusDescriptions[workEffort.CurrentStatusId]
                            : ("Unknown", "غير معروف");

                    var resultDto = new ProjectCertificateDto
                    {
                        WorkEffortId = workEffort.WorkEffortId,
                        CertificateNumber = workEffort.CertificateNumber,
                        WorkEffortTypeId = workEffort.WorkEffortTypeId,
                        ProjectId = workEffort.ProjectId,
                        ProjectName = project?.ProjectName ?? "",
                        PartyIdSupplier = workEffort.PartyIdSupplier,
                        PartyNameSupplier = supplier?.Description,
                        PartyIdContractor = workEffort.PartyIdContractor,
                        PartyNameContractor = contractor?.Description,
                        Description = workEffort.Description,
                        EstimatedStartDate = workEffort.EstimatedStartDate,
                        EstimatedCompletionDate = workEffort.EstimatedCompletionDate,
                        StatusDescription = statusDescription,
                        StatusDescriptionArabic = statusDescriptionArabic,
                        CurrentStatusId = workEffort.CurrentStatusId,
                        RelatedOrderId = workEffort.RelatedOrderId,
                        CertificateItems = certificate.CertificateItems
                    };


                    return Result<ProjectCertificateDto>.Success(resultDto);
                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync(cancellationToken);
                    return Result<ProjectCertificateDto>.Failure($"Failed to create certificate: {ex.Message}");
                }
            }
        }
    }
}