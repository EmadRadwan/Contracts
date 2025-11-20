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
                            // Update logic (same as yours)
                            existingItem.ProductId = item.ProductId;
                            existingItem.Description = item.Description;
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
                            existingItem.DeductionDescription = item.DeductionDescription;
                            existingItem.AchievementPercent = item.AchievementPercentage ?? 0;
                            existingItem.Notes = item.Notes;
                            existingItem.ProcurementDate = item.ProcurementDate;
                            existingItem.TransportationExpenses = item.TransportationExpenses ?? 0;
                            existingItem.Gratuities = item.Gratuities ?? 0;
                            existingItem.LastUpdatedStamp = stamp;
                        }
                        else
                        {
                            var newId = await _utilityService.GetNextSequence("WorkEffort");
                            var newItem = new WorkEffort
                            {
                                /* same as your create logic */
                            };
                            newItem.WorkEffortId = newId;
                            newItem.WorkEffortParentId = certificate.WorkEffortId;
                            newItem.WorkEffortTypeId = "CERTIFICATE_ITEM";
                            // ... copy all fields
                            newItem.CreatedDate = stamp;
                            newItem.LastUpdatedStamp = stamp;
                            newItem.CurrentStatusId = "WEPR_CREATED";
                            _context.WorkEfforts.Add(newItem);
                        }
                    }

                    // REFACTOR: Critical — Handle PO delete + recreate
                    string? newOrderId = null;
                    string? newRelatedOrderId = null;


                    // Step 2: Re-create PO using exact same logic as CreateProjectCertificate
                    if (category != "COMPANY_SUPPLY_SALE_CERTIFICATE")
                    {
                        string? oldOrderId = workEffortQuery.RelatedOrderId;
                        if (!string.IsNullOrEmpty(oldOrderId))
                        {
                            // 1. Clear FK + save
                            workEffortQuery.RelatedOrderId = null;
                            await _context.SaveChangesAsync(cancellationToken);

                            // 2. Delete old PO completely
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
                            await _context.OrderAdjustments.Where(x => x.OrderId == oldOrderId)
                                .ExecuteDeleteAsync(cancellationToken);
                            await _context.OrderStatuses.Where(x => x.OrderId == oldOrderId)
                                .ExecuteDeleteAsync(cancellationToken);
                            await _context.OrderRoles.Where(x => x.OrderId == oldOrderId)
                                .ExecuteDeleteAsync(cancellationToken);

                            var prefIds = await _context.OrderPaymentPreferences
                                .Where(p => p.OrderId == oldOrderId)
                                .Select(p => p.OrderPaymentPreferenceId)
                                .ToListAsync(cancellationToken);

                            if (prefIds.Any())
                                await _context.Payments.Where(p => prefIds.Contains(p.PaymentPreferenceId))
                                    .ExecuteDeleteAsync(cancellationToken);

                            await _context.OrderPaymentPreferences.Where(p => p.OrderId == oldOrderId)
                                .ExecuteDeleteAsync(cancellationToken);
                            await _context.OrderHeaders.Where(h => h.OrderId == oldOrderId)
                                .ExecuteDeleteAsync(cancellationToken);

                        }

                        // 3. NOW create new PO — change tracker is clean!
                        var orderItems = new List<OrderItemDto2>();
                        int seq = 1;

                        string fromPartyId = category switch
                        {
                            "SUPPLY_PROCUREMENT_CERTIFICATE" => certificate.PartyIdSupplier ??
                                                                throw new InvalidOperationException(
                                                                    "PartyIdSupplier required"),
                            "WORKMANSHIP_CONTRACTING_CERTIFICATE" => certificate.PartyIdContractor ??
                                                                     throw new InvalidOperationException(
                                                                         "PartyIdContractor required"),
                            _ => throw new InvalidOperationException("Unsupported category")
                        };

                        foreach (var item in certificate.CertificateItems!)
                        {
                            decimal netAmount = item.Net;

                            orderItems.Add(new OrderItemDto2
                            {
                                OrderItemSeqId = seq.ToString("D4"),
                                ProductId = item.ProductId,
                                ProductName = item.ProductName,
                                Quantity = 1,
                                UnitPrice = netAmount - item.Insurance - item.AdditionalInsurance,
                                SubTotal = netAmount - item.Insurance - item.AdditionalInsurance,
                                UomId = item.UomId,
                                FacilityId = certificate.FacilityId,
                                ItemDescription = item.Description,
                                OrderItemTypeId = "PROJECT_CERTIFICATE_ITEM",
                                StatusId = "ITEM_CREATED"
                            });
                            seq++;

                            if (category == "WORKMANSHIP_CONTRACTING_CERTIFICATE")
                            {
                                if (item.Insurance.GetValueOrDefault() != 0)
                                {
                                    orderItems.Add(new OrderItemDto2
                                    {
                                        OrderItemSeqId = seq++.ToString("D4"),
                                        ProductId = item.ProductId,
                                        ProductName = $"تأمين - {item.ProductName}",
                                        Quantity = 1,
                                        UnitPrice = Math.Abs(item.Insurance!.Value),
                                        SubTotal = Math.Abs(item.Insurance.Value),
                                        UomId = item.UomId,
                                        FacilityId = certificate.FacilityId,
                                        ItemDescription = "تأمين مستحق",
                                        OrderItemTypeId = "PROJECT_INSURANCE",
                                        StatusId = "ITEM_CREATED"
                                    });
                                }

                                if (item.AdditionalInsurance.GetValueOrDefault() != 0)
                                {
                                    orderItems.Add(new OrderItemDto2
                                    {
                                        OrderItemSeqId = seq++.ToString("D4"),
                                        ProductId = item.ProductId,
                                        ProductName = $"تأمين إضافي - {item.ProductName}",
                                        Quantity = 1,
                                        UnitPrice = Math.Abs(item.AdditionalInsurance!.Value),
                                        SubTotal = Math.Abs(item.AdditionalInsurance.Value),
                                        UomId = item.UomId,
                                        FacilityId = certificate.FacilityId,
                                        ItemDescription = "تأمين إضافي",
                                        OrderItemTypeId = "PROJECT_ADDITIONAL_INSURANCE",
                                        StatusId = "ITEM_CREATED"
                                    });
                                }
                            }
                        }

                        var orderDto = new OrderDto
                        {
                            OrderTypeId = "PURCHASE_ORDER",
                            FromPartyId = fromPartyId,
                            CurrencyUomId = await _productStoreService.GetProductStoreDefaultCurrencyId(),
                            OrderDate = stamp,
                            StatusId = "ORDER_CREATED",
                            InternalRemarks =
                                $"Auto-generated from Certificate {workEffortQuery.CertificateNumber} (Updated {stamp:yyyy-MM-dd HH:mm})",
                            GrandTotal = orderItems.Sum(x => x.SubTotal),
                            OrderItems = orderItems,
                            OrderAdjustments = new()
                        };

                        var poResult = await _orderService.CreatePurchaseOrder(orderDto);
                        
                        newOrderId = poResult.OrderId; 
                        workEffortQuery.RelatedOrderId = newOrderId;
                    
                        await _context.SaveChangesAsync(cancellationToken);
                        
                        if (poResult == null)
                        {
                            await transaction.RollbackAsync(cancellationToken);
                            return Result<ProjectCertificateDto>.Failure("Failed to create purchase order");
                        }

                        newOrderId = poResult.OrderId;

                        // APPROVE
                        var approveDto = new OrderDto
                        {
                            OrderId = newOrderId,
                            FromPartyId = fromPartyId,
                            GrandTotal = poResult.GrandTotal,
                            CurrencyUomId = await _productStoreService.GetProductStoreDefaultCurrencyId(),
                            OrderDate = stamp,
                            StatusId = "ORDER_APPROVED", // or whatever your approval logic expects
                            InternalRemarks =
                                $"Auto-approved after certificate update - {workEffortQuery.CertificateNumber}",

                            OrderItems = await _context.OrderItems
                                .Where(oi => oi.OrderId == newOrderId)
                                .Select(oi => new OrderItemDto2
                                {
                                    OrderId = oi.OrderId,
                                    OrderItemSeqId = oi.OrderItemSeqId, // CRITICAL — WAS MISSING!
                                    ProductId = oi.ProductId,
                                    Quantity = oi.Quantity,
                                    UnitPrice = oi.UnitPrice,
                                    ItemDescription = oi.ItemDescription,
                                    OrderItemTypeId = oi.OrderItemTypeId,
                                    CreatedStamp = oi.CreatedStamp,
                                    LastUpdatedStamp = oi.LastUpdatedStamp
                                })
                                .ToListAsync(cancellationToken),

                            OrderAdjustments = await _context.OrderAdjustments
                                .Where(oa => oa.OrderId == newOrderId)
                                .Select(oa => new OrderAdjustmentDto2
                                {
                                    OrderAdjustmentId = oa.OrderAdjustmentId,
                                    OrderAdjustmentTypeId = oa.OrderAdjustmentTypeId,
                                    OrderId = oa.OrderId,
                                    OrderItemSeqId = oa.OrderItemSeqId,
                                    Amount = oa.Amount,
                                    CorrespondingProductId = oa.CorrespondingProductId,
                                    IsManual = oa.IsManual,
                                    CreatedDate = oa.CreatedDate,
                                    SourcePercentage = oa.SourcePercentage,
                                    IsAdjustmentDeleted = false
                                })
                                .ToListAsync(cancellationToken)
                        };

                        await _orderService.UpdateOrApprovePurchaseOrder(approveDto, "APPROVE");
                    }

                    
                    await _context.SaveChangesAsync(cancellationToken);   // ← ADD THIS


                    // Final save
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
                        CertificateItems = certificate.CertificateItems
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