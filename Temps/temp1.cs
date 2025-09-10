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
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

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

        // REFACTOR: Updated ProjectCertificateDto to include projectName, partyNameSupplier, and partyNameContractor
        // Purpose: Provide full object data for frontend FormComboBox components
        // Context: Ensures Redux state can store complete objects for projectId, partyIdSupplier, and partyIdContractor
        public class ProjectCertificateDto
        {
            public string WorkEffortId { get; set; }
            public string CertificateNumber { get; set; }
            public string WorkEffortTypeId { get; set; }
            public string ProjectId { get; set; }
            public string ProjectName { get; set; } // Added for frontend display
            public string? PartyIdSupplier { get; set; }
            public string? PartyNameSupplier { get; set; } // Added for frontend display
            public string? PartyIdContractor { get; set; }
            public string? PartyNameContractor { get; set; } // Added for frontend display
            public string Description { get; set; }
            public DateTime? EstimatedStartDate { get; set; }
            public DateTime? EstimatedCompletionDate { get; set; }
            public string StatusDescription { get; set; }
            public CertificateItemDto[] CertificateItems { get; set; }
        }

        public class CertificateItemDto
        {
            public string ProductId { get; set; }
            public string Description { get; set; }
            public decimal Quantity { get; set; }
            public decimal UnitPrice { get; set; }
            public decimal TotalAmount { get; set; }
            public decimal? Discount { get; set; }
            public decimal? Insurance { get; set; }
            public decimal? Deductions { get; set; }
            public decimal? CompletionPercentage { get; set; }
            public string Notes { get; set; }
            public DateTime? ProcurementDate { get; set; }
            public string? FacilityId { get; set; }
            public decimal? TransportationExpenses { get; set; }
            public decimal? Gratuities { get; set; }
            public string ProductName { get; set; }
        }

        public class Handler : IRequestHandler<Command, Result<ProjectCertificateDto>>
        {
            private readonly DataContext _context;
            private readonly IUserAccessor _userAccessor;
            private readonly IUtilityService _utilityService;
            private readonly IOrderService _orderService;
            private readonly IProductStoreService _productStoreService;

            public Handler(DataContext context, IUserAccessor userAccessor, IUtilityService utilityService, IOrderService orderService, IProductStoreService productStoreService)
            {
                _context = context;
                _userAccessor = userAccessor;
                _utilityService = utilityService;
                _orderService = orderService;
                _productStoreService = productStoreService;
            }

            public async Task<Result<ProjectCertificateDto>> Handle(Command request, CancellationToken cancellationToken)
            {
                await using var transaction = await _context.Database.BeginTransactionAsync(cancellationToken);
                try
                {
                    var stamp = DateTime.UtcNow;
                    var certificate = request.Certificate!;

                    var newWorkEffortSerial = await _utilityService.GetNextSequence("WorkEffort");
                    string newProjectCertificateSerial;
                    string? partyCode = null;

                    // REFACTOR: Unified serial generation for all certificate types using PartyIdContractor or PartyIdSupplier
                    // Purpose: Ensure consistent serial numbering based on party code; prioritize PartyIdContractor, fallback to PartyIdSupplier
                    // Context: Replaces previous logic that only used party code for WORKMANSHIP_CONTRACTING_CERTIFICATE
                    string? partyId = certificate.PartyIdContractor ?? certificate.PartyIdSupplier;
                    if (string.IsNullOrEmpty(partyId))
                    {
                        await transaction.RollbackAsync(cancellationToken);
                        return Result<ProjectCertificateDto>.Failure("No valid party ID (Contractor or Supplier) provided");
                    }

                    var certificateCount = await _context.WorkEfforts
                        .CountAsync(we => (we.PartyIdContractor == partyId || we.PartyIdSupplier == partyId) && we.CertificateCategory == certificate.CertificateCategory, cancellationToken);
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
                        CurrentStatusId = "WEPR_IN_PROGRESS",
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
                            DiscountAmount = item.Discount ?? 0,
                            InsuranceAmount = item.Insurance ?? 0,
                            Deductions = item.Deductions ?? 0,
                            CompletionPercentage = item.CompletionPercentage,
                            Notes = item.Notes,
                            ProcurementDate = item.ProcurementDate,
                            FacilityId = string.IsNullOrWhiteSpace(item.FacilityId) ? null : item.FacilityId,
                            TransportationExpenses = item.TransportationExpenses ?? 0,
                            Gratuities = item.Gratuities ?? 0,
                            CreatedDate = stamp,
                            LastUpdatedStamp = stamp,
                            CurrentStatusId = "WEPR_IN_PROGRESS"
                        };
                        _context.WorkEfforts.Add(itemWorkEffort);
                    }

                    if (certificate.CertificateCategory != "COMPANY_SUPPLY_SALE_CERTIFICATE")
                    {
                        var poItems = certificate.CertificateItems.ToList();
                        if (poItems.Any())
                        {
                            var fromPartyId = certificate.CertificateCategory is "SUPPLY_PROCUREMENT_CERTIFICATE" or "EXTERNAL_SUPPLY_SALE_CERTIFICATE"
                                ? certificate.PartyIdSupplier
                                : certificate.PartyIdContractor;

                            var discountAdjustments = poItems
                                .Select((item, index) => new { Item = item, Index = index })
                                .Where(x => x.Item.Discount.HasValue && x.Item.Discount > 0)
                                .Select(x => new OrderAdjustmentDto2
                                {
                                    OrderAdjustmentId = Guid.NewGuid().ToString(),
                                    OrderAdjustmentTypeId = "DISCOUNT_ADJUSTMENT",
                                    OrderAdjustmentTypeDescription = "خصم",
                                    OrderId = null,
                                    OrderItemSeqId = (x.Index + 1).ToString("D4"),
                                    Amount = -x.Item.Discount.Value,
                                    CorrespondingProductId = x.Item.ProductId,
                                    CorrespondingProductName = x.Item.ProductName,
                                    IsManual = "Y",
                                    CreatedDate = stamp,
                                    IsAdjustmentDeleted = false,
                                    SourcePercentage = x.Item.TotalAmount > 0 ? (x.Item.Discount.Value / x.Item.TotalAmount) * 100 : 0
                                });

                            var shippingAdjustments = poItems
                                .Select((item, index) => new { Item = item, Index = index })
                                .Where(x => x.Item.TransportationExpenses.HasValue && x.Item.TransportationExpenses > 0)
                                .Select(x => new OrderAdjustmentDto2
                                {
                                    OrderAdjustmentId = Guid.NewGuid().ToString(),
                                    OrderAdjustmentTypeId = "SHIPPING_CHARGES",
                                    OrderAdjustmentTypeDescription = "Transportation Expenses",
                                    OrderId = null,
                                    OrderItemSeqId = (x.Index + 1).ToString("D4"),
                                    Amount = x.Item.TransportationExpenses.Value,
                                    CorrespondingProductId = x.Item.ProductId,
                                    CorrespondingProductName = x.Item.ProductName,
                                    IsManual = "Y",
                                    CreatedDate = stamp,
                                    IsAdjustmentDeleted = false,
                                    SourcePercentage = null
                                });

                            var gratuityAdjustments = poItems
                                .Select((item, index) => new { Item = item, Index = index })
                                .Where(x => x.Item.Gratuities.HasValue && x.Item.Gratuities > 0)
                                .Select(x => new OrderAdjustmentDto2
                                {
                                    OrderAdjustmentId = Guid.NewGuid().ToString(),
                                    OrderAdjustmentTypeId = "MISCELLANEOUS_CHARGE",
                                    OrderAdjustmentTypeDescription = "Gratuities",
                                    OrderId = null,
                                    OrderItemSeqId = (x.Index + 1).ToString("D4"),
                                    Amount = x.Item.Gratuities.Value,
                                    CorrespondingProductId = x.Item.ProductId,
                                    CorrespondingProductName = x.Item.ProductName,
                                    IsManual = "Y",
                                    CreatedDate = stamp,
                                    IsAdjustmentDeleted = false,
                                    SourcePercentage = null
                                });

                            var orderAdjustments = discountAdjustments
                                .Concat(shippingAdjustments)
                                .Concat(gratuityAdjustments)
                                .ToList();

                            var orderDto = new OrderDto
                            {
                                OrderTypeId = "PURCHASE_ORDER",
                                FromPartyId = fromPartyId,
                                CurrencyUomId = await _productStoreService.GetProductStoreDefaultCurrencyId(),
                                OrderDate = stamp,
                                StatusId = "ORDER_CREATED",
                                StatusDescription = "Created",
                                InternalRemarks = $"Auto-generated from Certificate {newProjectCertificateSerial}",
                                GrandTotal = poItems.Sum(i => i.TotalAmount + (i.TransportationExpenses ?? 0) + (i.Gratuities ?? 0) - (i.Discount ?? 0)),
                                OrderItems = poItems.Select((item, index) => new OrderItemDto2
                                {
                                    OrderItemSeqId = (index + 1).ToString("D4"),
                                    ProductId = item.ProductId,
                                    ProductName = item.ProductName,
                                    Quantity = item.Quantity,
                                    UnitPrice = item.UnitPrice,
                                    SubTotal = item.TotalAmount,
                                    FacilityId = item.FacilityId,
                                    ItemDescription = item.Description,
                                    OrderItemTypeId = "PRODUCT_ORDER_ITEM",
                                    StatusId = "ITEM_CREATED",
                                    CreatedStamp = stamp,
                                    LastUpdatedStamp = stamp
                                }).ToList(),
                                OrderAdjustments = orderAdjustments
                            };

                            var poResult = await _orderService.CreatePurchaseOrder(orderDto);
                            if (poResult == null)
                            {
                                await transaction.RollbackAsync(cancellationToken);
                                return Result<ProjectCertificateDto>.Failure("Failed to create purchase order");
                            }
                        }
                    }

                    var result = await _context.SaveChangesAsync(cancellationToken) > 0;
                    if (!result)
                    {
                        await transaction.RollbackAsync(cancellationToken);
                        return Result<ProjectCertificateDto>.Failure("Failed to create certificate and items");
                    }

                    await transaction.CommitAsync(cancellationToken);

                    // REFACTOR: Fetch projectName, partyNameSupplier, and partyNameContractor from database
                    // Purpose: Include names in response to support frontend FormComboBox components
                    // Context: Ensures Redux state can be populated with full object structures
                    var project = await _context.Projects
                        .Where(p => p.ProjectId == certificate.ProjectId)
                        .Select(p => new { p.ProjectName })
                        .FirstOrDefaultAsync(cancellationToken);

                    var supplier = certificate.PartyIdSupplier != null
                        ? await _context.Parties
                            .Where(p => p.PartyId == certificate.PartyIdSupplier)
                            .Select(p => new { p.PartyName })
                            .FirstOrDefaultAsync(cancellationToken)
                        : null;

                    var contractor = certificate.PartyIdContractor != null
                        ? await _context.Parties
                            .Where(p => p.PartyId == certificate.PartyIdContractor)
                            .Select(p => new { p.PartyName })
                            .FirstOrDefaultAsync(cancellationToken)
                        : null;

                    var resultDto = new ProjectCertificateDto
                    {
                        WorkEffortId = workEffort.WorkEffortId,
                        CertificateNumber = workEffort.CertificateNumber,
                        WorkEffortTypeId = workEffort.WorkEffortTypeId,
                        ProjectId = workEffort.ProjectId,
                        ProjectName = project?.ProjectName ?? "", // Use fetched project name
                        PartyIdSupplier = workEffort.PartyIdSupplier,
                        PartyNameSupplier = supplier?.PartyName, // Use fetched supplier name
                        PartyIdContractor = workEffort.PartyIdContractor,
                        PartyNameContractor = contractor?.PartyName, // Use fetched contractor name
                        Description = workEffort.Description,
                        EstimatedStartDate = workEffort.EstimatedStartDate,
                        EstimatedCompletionDate = workEffort.EstimatedCompletionDate,
                        StatusDescription = "CREATED",
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