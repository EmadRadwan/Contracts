using Application.Core;
using FluentValidation;
using MediatR;
using Persistence;
using Domain;
using Application.Interfaces;
using Microsoft.EntityFrameworkCore;
using Application.Catalog.ProductStores;
using Application.order.Orders;
using Application.Order.Orders;

namespace Application.Projects
{
    public class UpdateProjectCertificate
    {
        private static OrderAdjustmentDto2 CreateAdjustment(string seqId, string typeId, decimal amount,
            CertificateItemDto item)
        {
            return new OrderAdjustmentDto2
            {
                OrderAdjustmentId = Guid.NewGuid().ToString(),
                OrderItemSeqId = seqId,
                OrderAdjustmentTypeId = typeId,
                Amount = amount,
                CorrespondingProductId = item.ProductId,
                IsManual = "Y",
                CreatedDate = DateTime.UtcNow
            };
        }

        public class Command : IRequest<Result<ProjectCertificateDto>>
        {
            public ProjectCertificateDto? Certificate { get; set; }
        }

        public class CommandValidator : AbstractValidator<Command>
        {
            public CommandValidator()
            {
                RuleFor(x => x.Certificate!.WorkEffortId).NotEmpty().WithMessage("Work Effort ID is required");
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
            private readonly IOrderService _orderService; // REFACTOR: Added
            private readonly IProductStoreService _productStoreService; // REFACTOR: Added

            public Handler(
                DataContext context,
                IUserAccessor userAccessor,
                IUtilityService utilityService,
                IOrderService orderService,
                IProductStoreService productStoreService)
            {
                _context = context;
                _userAccessor = userAccessor;
                _utilityService = utilityService;
                _orderService = orderService; // REFACTOR: Injected
                _productStoreService = productStoreService; // REFACTOR: Injected
            }

            public async Task<Result<ProjectCertificateDto>> Handle(Command request,
                CancellationToken cancellationToken)
            {
                _context.Database.SetCommandTimeout(300);
                await using var transaction = await _context.Database.BeginTransactionAsync(cancellationToken);
                try
                {
                    var stamp = DateTime.UtcNow;
                    var certificate = request.Certificate!;

                    var workEffortQuery = await _context.WorkEfforts
                        .Include(we => we.CurrentStatus)
                        .FirstOrDefaultAsync(we => we.WorkEffortId == certificate.WorkEffortId, cancellationToken);

                    if (workEffortQuery == null)
                    {
                        await transaction.RollbackAsync(cancellationToken);
                        return Result<ProjectCertificateDto>.Failure("Certificate not found");
                    }

                    // REFACTOR: Update header fields (same as before)
                    workEffortQuery.Description = certificate.Description ?? workEffortQuery.Description;
                    workEffortQuery.EstimatedStartDate =
                        certificate.EstimatedStartDate ?? workEffortQuery.EstimatedStartDate;
                    workEffortQuery.EstimatedCompletionDate = certificate.EstimatedCompletionDate ??
                                                              workEffortQuery.EstimatedCompletionDate;
                    workEffortQuery.PartyIdSupplier = certificate.PartyIdSupplier ?? workEffortQuery.PartyIdSupplier;
                    workEffortQuery.PartyIdContractor =
                        certificate.PartyIdContractor ?? workEffortQuery.PartyIdContractor;
                    workEffortQuery.ProjectId = certificate.ProjectId ?? workEffortQuery.ProjectId;
                    workEffortQuery.FacilityId = certificate.FacilityId ?? workEffortQuery.FacilityId;
                    workEffortQuery.LastUpdatedStamp = stamp;

                    var category = workEffortQuery.CertificateCategory;

                    // REFACTOR: Handle items (add/update/delete) — unchanged from your version
                    var existingItems = await _context.WorkEfforts
                        .Where(we => we.WorkEffortParentId == certificate.WorkEffortId)
                        .ToListAsync(cancellationToken);

                    foreach (var existingItem in existingItems)
                    {
                        if (!certificate.CertificateItems!.Any(item => item.WorkEffortId == existingItem.WorkEffortId))
                            _context.WorkEfforts.Remove(existingItem);
                    }

                    foreach (var item in certificate.CertificateItems!)
                    {
                        var existingItem = existingItems.FirstOrDefault(ei => ei.WorkEffortId == item.WorkEffortId);

                        if (existingItem != null)
                        {
                            // UPDATE existing record
                            existingItem.ProductId = item.ProductId;
                            existingItem.Description = item.Description;
                            existingItem.DeductionDescription = item.DeductionDescription;
                            existingItem.Quantity = item.Quantity;
                            existingItem.Rate = item.UnitPrice;
                            existingItem.TotalAmount = item.TotalAmount;
                            existingItem.Discount = item.Discount ?? 0;
                            existingItem.Insurance = item.Insurance ?? 0;
                            existingItem.AdditionalInsurance = item.AdditionalInsurance;
                            existingItem.MaterialPrice = category == "WORKMANSHIP_CONTRACTING_CERTIFICATE"
                                ? item.MaterialPrice
                                : 0;
                            existingItem.LaborPrice =
                                category == "WORKMANSHIP_CONTRACTING_CERTIFICATE" ? item.LaborPrice : 0;
                            existingItem.QuantityUomId = item.UomId;
                            existingItem.Deductions = item.Deductions ?? 0;
                            existingItem.AchievementPercent = item.AchievementPercentage ?? 0;
                            existingItem.Notes = item.Notes;
                            existingItem.ProcurementDate = item.ProcurementDate;
                            existingItem.TransportationExpenses = item.TransportationExpenses ?? 0;
                            existingItem.Gratuities = item.Gratuities ?? 0;
                            existingItem.LastUpdatedStamp = stamp;
                        }
                        else
                        {
                            // REFACTOR: Create ONLY for new items
                            var newId = await _utilityService.GetNextSequence("WorkEffort");
                            var newItem = new WorkEffort
                            {
                                WorkEffortId = newId,
                                WorkEffortParentId = certificate.WorkEffortId,
                                WorkEffortTypeId = "CERTIFICATE_ITEM",
                                ProductId = item.ProductId,
                                Description = item.Description,
                                DeductionDescription = item.DeductionDescription,
                                Quantity = item.Quantity,
                                Rate = item.UnitPrice,
                                TotalAmount = item.TotalAmount,
                                Discount = item.Discount ?? 0,
                                Insurance = item.Insurance ?? 0,
                                AdditionalInsurance = item.AdditionalInsurance,
                                MaterialPrice = category == "WORKMANSHIP_CONTRACTING_CERTIFICATE"
                                    ? item.MaterialPrice
                                    : 0,
                                LaborPrice = category == "WORKMANSHIP_CONTRACTING_CERTIFICATE" ? item.LaborPrice : 0,
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

                            _context.WorkEfforts.Add(newItem);
                        }
                    }

                    string? newOrderId = null;

                    // REFACTOR: Only recreate PO if not COMPANY_SUPPLY_SALE_CERTIFICATE (same rule as Create)
                    if (category != "COMPANY_SUPPLY_SALE_CERTIFICATE")
                    {
                        // 1. Delete old PO (if exists)
                        string? oldOrderId = workEffortQuery.RelatedOrderId;
                        if (!string.IsNullOrEmpty(oldOrderId))
                        {
                            workEffortQuery.RelatedOrderId = null;
                            await _context.SaveChangesAsync(cancellationToken);

                            // Delete cascade (same as before — kept intact)
                            await _context.Set<OrderItemShipGroupAssoc>().Where(x => x.OrderId == oldOrderId)
                                .ExecuteDeleteAsync(cancellationToken);
                            await _context.Set<OrderItemBilling>().Where(x => x.OrderId == oldOrderId)
                                .ExecuteDeleteAsync(cancellationToken);
                            await _context.Set<OrderItemContactMech>().Where(x => x.OrderId == oldOrderId)
                                .ExecuteDeleteAsync(cancellationToken);
                            await _context.Set<OrderItemPriceInfo>().Where(x => x.OrderId == oldOrderId)
                                .ExecuteDeleteAsync(cancellationToken);
                            await _context.Set<OrderItemAttribute>().Where(x => x.OrderId == oldOrderId)
                                .ExecuteDeleteAsync(cancellationToken);
                            await _context.OrderItemShipGroups.Where(x => x.OrderId == oldOrderId)
                                .ExecuteDeleteAsync(cancellationToken);
                            await _context.OrderItems.Where(x => x.OrderId == oldOrderId)
                                .ExecuteDeleteAsync(cancellationToken);
                            // ORDER_ADJUSTMENT_BILLING has no ORDER_ID column of its own -- it's only
                            // reachable via ORDER_ADJUSTMENT_ID -- so it's invisible to every other
                            // delete above and must be cleared before OrderAdjustments or the FK
                            // (ORDER_ADJBLNG_OA) blocks the delete. Surfaced by certificate 110-0008:
                            // Reset only clears adjustment billings tied to receipt-derived invoice ids,
                            // so an adjustment billed through any other path survives Reset and then
                            // breaks the next Update's PO-recreation cascade.
                            await _context.OrderAdjustmentBillings
                                .Where(oab => _context.OrderAdjustments
                                    .Any(oa => oa.OrderAdjustmentId == oab.OrderAdjustmentId && oa.OrderId == oldOrderId))
                                .ExecuteDeleteAsync(cancellationToken);
                            await _context.OrderAdjustments.Where(x => x.OrderId == oldOrderId)
                                .ExecuteDeleteAsync(cancellationToken);
                            await _context.OrderStatuses.Where(x => x.OrderId == oldOrderId)
                                .ExecuteDeleteAsync(cancellationToken);
                            await _context.OrderRoles.Where(x => x.OrderId == oldOrderId)
                                .ExecuteDeleteAsync(cancellationToken);

                            var prefIds = await _context.OrderPaymentPreferences
                                .Where(p => p.OrderId == oldOrderId).Select(p => p.OrderPaymentPreferenceId)
                                .ToListAsync(cancellationToken);
                            if (prefIds.Any())
                                await _context.Payments.Where(p => prefIds.Contains(p.PaymentPreferenceId))
                                    .ExecuteDeleteAsync(cancellationToken);

                            await _context.OrderPaymentPreferences.Where(p => p.OrderId == oldOrderId)
                                .ExecuteDeleteAsync(cancellationToken);
                            await _context.OrderHeaders.Where(h => h.OrderId == oldOrderId)
                                .ExecuteDeleteAsync(cancellationToken);
                        }

                        // REFACTOR: Rebuild order items & adjustments EXACTLY like Create handler
                        var orderItems = new List<OrderItemDto2>();
                        var orderAdjustments = new List<OrderAdjustmentDto2>();
                        var seq = 1;

                        var fromPartyId = category switch
                        {
                            "SUPPLY_PROCUREMENT_CERTIFICATE" => certificate.PartyIdSupplier ??
                                                                throw new InvalidOperationException(
                                                                    "PartyIdSupplier required"),
                            "WORKMANSHIP_CONTRACTING_CERTIFICATE" => certificate.PartyIdContractor ??
                                                                     throw new InvalidOperationException(
                                                                         "PartyIdContractor required"),
                            _ => throw new InvalidOperationException($"Unsupported category: {category}")
                        };

                        foreach (var item in certificate.CertificateItems!)
                        {
                            var orderItemSeqId = seq.ToString("D4");

                            if (category == "WORKMANSHIP_CONTRACTING_CERTIFICATE")
                            {
                                // Calculate deserved from raw fields — do not trust the frontend-supplied value
                                // which may be 0 when Redux state was stale after a prior create/update response.
                                var deserved = Math.Max(0m,
                                    item.Quantity * (item.MaterialPrice + item.LaborPrice) *
                                    ((item.AchievementPercentage ?? 0m) / 100m) -
                                    (item.Deductions ?? 0m));

                                // STRATEGY 1: Quantity = 1, UnitPrice = deserved (net after deductions but before insurance)
                                orderItems.Add(new OrderItemDto2
                                {
                                    OrderItemSeqId = orderItemSeqId,
                                    ProductId = item.ProductId,
                                    ProductName = item.ProductName,
                                    Quantity = 1m,
                                    UnitPrice = deserved,
                                    SubTotal = deserved,
                                    UomId = item.UomId,
                                    FacilityId = certificate.FacilityId,
                                    ItemDescription = item.Description,
                                    OrderItemTypeId = "PROJECT_CERTIFICATE_ITEM",
                                    StatusId = "ITEM_CREATED",
                                    CreatedStamp = stamp,
                                    LastUpdatedStamp = stamp
                                });

                                // Insurance & Additional Insurance as negative line items
                                if (item.Insurance.GetValueOrDefault() != 0)
                                {
                                    orderItems.Add(new OrderItemDto2
                                    {
                                        OrderItemSeqId = (++seq).ToString("D4"),
                                        ProductId = item.ProductId,
                                        ProductName = $"تأمين - {item.ProductName}",
                                        Quantity = 1,
                                        UnitPrice = Math.Abs(item.Insurance!.Value),
                                        SubTotal = Math.Abs(item.Insurance.Value),
                                        UomId = item.UomId,
                                        FacilityId = certificate.FacilityId,
                                        ItemDescription = "تأمين مستحق",
                                        OrderItemTypeId = "PROJECT_INSURANCE",
                                        StatusId = "ITEM_CREATED",
                                        CreatedStamp = stamp,
                                        LastUpdatedStamp = stamp
                                    });
                                }

                                if (item.AdditionalInsurance.GetValueOrDefault() != 0)
                                {
                                    orderItems.Add(new OrderItemDto2
                                    {
                                        OrderItemSeqId = (++seq).ToString("D4"),
                                        ProductId = item.ProductId,
                                        ProductName = $"تأمين إضافي - {item.ProductName}",
                                        Quantity = 1,
                                        UnitPrice = Math.Abs(item.AdditionalInsurance!.Value),
                                        SubTotal = Math.Abs(item.AdditionalInsurance.Value),
                                        UomId = item.UomId,
                                        FacilityId = certificate.FacilityId,
                                        ItemDescription = "تأمين إضافي",
                                        OrderItemTypeId = "PROJECT_ADDITIONAL_INSURANCE",
                                        StatusId = "ITEM_CREATED",
                                        CreatedStamp = stamp,
                                        LastUpdatedStamp = stamp
                                    });
                                }

                                // NOTE: Deductions are already subtracted once above, inside `deserved` —
                                // do NOT also add a separate PROJECT_DEDUCTION order item here. See the
                                // matching note in CreateProjectCertificate.cs (certificate 82-0006 /
                                // INV1376 double-deduction incident).
                            }
                            else // SUPPLY_PROCUREMENT_CERTIFICATE
                            {
                                var baseAmount = item.Quantity * item.UnitPrice;

                                orderItems.Add(new OrderItemDto2
                                {
                                    OrderItemSeqId = orderItemSeqId,
                                    ProductId = item.ProductId,
                                    ProductName = item.ProductName,
                                    Quantity = item.Quantity,
                                    UnitPrice = item.UnitPrice,
                                    SubTotal = baseAmount,
                                    UomId = item.UomId,
                                    FacilityId = certificate.FacilityId,
                                    ItemDescription = item.Description,
                                    OrderItemTypeId = "PROJECT_CERTIFICATE_SUPPLY_ITEM",
                                    StatusId = "ITEM_CREATED",
                                    CreatedStamp = stamp,
                                    LastUpdatedStamp = stamp
                                });

                                if (item.Discount.GetValueOrDefault() != 0)
                                    orderAdjustments.Add(CreateAdjustment(orderItemSeqId,
                                        "CERTIFICATE_DISCOUNT_ADJUSTMENT", -item.Discount.Value, item));
                                if (item.TransportationExpenses.GetValueOrDefault() != 0)
                                    orderAdjustments.Add(CreateAdjustment(orderItemSeqId,
                                        "CERTIFICATE_SHIPPING_CHARGES", item.TransportationExpenses.Value, item));
                                if (item.Gratuities.GetValueOrDefault() != 0)
                                    orderAdjustments.Add(CreateAdjustment(orderItemSeqId,
                                        "CERTIFICATE_GRATUTIES_CHARGES", item.Gratuities.Value, item));
                            }

                            seq++;
                        }

                        // REFACTOR: GrandTotal now matches Create logic exactly
                        // For WORKMANSHIP: compute Net from raw fields, not the frontend-supplied Net value
                        decimal grandTotal = category switch
                        {
                            "WORKMANSHIP_CONTRACTING_CERTIFICATE" => certificate.CertificateItems!.Sum(item =>
                                Math.Max(0m,
                                    Math.Max(0m,
                                        item.Quantity * (item.MaterialPrice + item.LaborPrice) *
                                        ((item.AchievementPercentage ?? 0m) / 100m) - (item.Deductions ?? 0m)) -
                                    (item.Insurance ?? 0m) - (item.AdditionalInsurance ?? 0m))),
                            "SUPPLY_PROCUREMENT_CERTIFICATE" => (decimal)(orderItems.Sum(i => i.SubTotal) +
                                                                          orderAdjustments.Sum(a => a.Amount)),
                            _ => 0m
                        };

                        var orderDto = new OrderDto
                        {
                            OrderTypeId = "PURCHASE_ORDER",
                            FromPartyId = fromPartyId,
                            CurrencyUomId = await _productStoreService.GetProductStoreDefaultCurrencyId(),
                            OrderDate = stamp,
                            StatusId = "ORDER_CREATED",
                            StatusDescription = "Created",
                            InternalRemarks =
                                $"Auto-generated from Certificate {workEffortQuery.CertificateNumber} (Updated {stamp:yyyy-MM-dd HH:mm})",
                            GrandTotal = grandTotal,
                            OrderItems = orderItems,
                            OrderAdjustments = orderAdjustments
                        };

                        var poResult = await _orderService.CreatePurchaseOrder(orderDto);
                        if (poResult == null)
                        {
                            await transaction.RollbackAsync(cancellationToken);
                            return Result<ProjectCertificateDto>.Failure("Failed to create purchase order");
                        }

                        newOrderId = poResult.OrderId;
                        workEffortQuery.RelatedOrderId = newOrderId;

                        // REFACTOR: Save now so RelatedOrderId is persisted before approval query
                        await _context.SaveChangesAsync(cancellationToken);
                        
                        await _context.SaveChangesAsync(cancellationToken);
                    }

                    await transaction.CommitAsync(cancellationToken);


                    var project = await _context.WorkEfforts
                        .Where(p => p.WorkEffortId == workEffortQuery.ProjectId)
                        .Select(p => new { p.ProjectName })
                        .FirstOrDefaultAsync(cancellationToken);

                    var supplier = workEffortQuery.PartyIdSupplier != null
                        ? await _context.Parties
                            .Where(p => p.PartyId == workEffortQuery.PartyIdSupplier)
                            .Select(p => new { p.Description })
                            .FirstOrDefaultAsync(cancellationToken)
                        : null;

                    var contractor = workEffortQuery.PartyIdContractor != null
                        ? await _context.Parties
                            .Where(p => p.PartyId == workEffortQuery.PartyIdContractor)
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
                        statusDescriptions.ContainsKey(workEffortQuery.CurrentStatusId)
                            ? statusDescriptions[workEffortQuery.CurrentStatusId]
                            : ("Unknown", "غير معروف");

                    var resultItems = await _context.WorkEfforts
                        .Where(we => we.WorkEffortParentId == workEffortQuery.WorkEffortId && we.WorkEffortTypeId == "CERTIFICATE_ITEM")
                        .Select(item => new CertificateItemDto
                        {
                            WorkEffortId = item.WorkEffortId,
                            ProductId = item.ProductId,
                            Description = item.Description,
                            DeductionDescription = item.DeductionDescription,
                            Quantity = (decimal)item.Quantity,
                            UnitPrice = (decimal)item.Rate,
                            TotalAmount = (decimal)item.TotalAmount,
                            Discount = item.Discount,
                            Insurance = item.Insurance,
                            AdditionalInsurance = item.AdditionalInsurance,
                            MaterialPrice = (decimal)item.MaterialPrice,
                            LaborPrice = (decimal)item.LaborPrice,
                            UomId = item.QuantityUomId,
                            Deductions = item.Deductions,
                            AchievementPercentage = item.AchievementPercent,
                            Notes = item.Notes,
                            ProcurementDate = item.ProcurementDate,
                            TransportationExpenses = item.TransportationExpenses,
                            Gratuities = item.Gratuities,
                            Deserved = category == "WORKMANSHIP_CONTRACTING_CERTIFICATE"
                                ? Math.Max(0m,
                                    (item.Quantity ?? 0m) * ((item.MaterialPrice ?? 0m) + (item.LaborPrice ?? 0m)) *
                                    ((decimal)(item.AchievementPercent ?? 0) / 100m) - (item.Deductions ?? 0m))
                                : 0m,
                            Net = category == "WORKMANSHIP_CONTRACTING_CERTIFICATE"
                                ? Math.Max(0m,
                                    Math.Max(0m,
                                        (item.Quantity ?? 0m) * ((item.MaterialPrice ?? 0m) + (item.LaborPrice ?? 0m)) *
                                        ((decimal)(item.AchievementPercent ?? 0) / 100m) - (item.Deductions ?? 0m)) -
                                    (item.Insurance ?? 0m) - (item.AdditionalInsurance ?? 0m))
                                : (item.TotalAmount ?? 0m)
                        }).ToListAsync(cancellationToken);

                    var resultDto = new ProjectCertificateDto
                    {
                        WorkEffortId = workEffortQuery.WorkEffortId,
                        CertificateNumber = workEffortQuery.CertificateNumber,
                        WorkEffortTypeId = workEffortQuery.WorkEffortTypeId,
                        CertificateCategory = workEffortQuery.CertificateCategory,
                        ProjectId = workEffortQuery.ProjectId,
                        ProjectName = project?.ProjectName ?? "",
                        PartyIdSupplier = workEffortQuery.PartyIdSupplier,
                        PartyNameSupplier = supplier?.Description,
                        PartyIdContractor = workEffortQuery.PartyIdContractor,
                        PartyNameContractor = contractor?.Description,
                        Description = workEffortQuery.Description,
                        EstimatedStartDate = workEffortQuery.EstimatedStartDate,
                        EstimatedCompletionDate = workEffortQuery.EstimatedCompletionDate,
                        StatusDescription = statusDescription,
                        StatusDescriptionArabic = statusDescriptionArabic,
                        CurrentStatusId = workEffortQuery.CurrentStatusId,
                        RelatedOrderId = workEffortQuery.RelatedOrderId,
                        FacilityId = workEffortQuery.FacilityId,
                        CertificateItems = resultItems
                    };

                    return Result<ProjectCertificateDto>.Success(resultDto);
                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync(cancellationToken);
                    return Result<ProjectCertificateDto>.Failure($"Failed to update certificate: {ex.Message}");
                }
            }
        }
    }
}