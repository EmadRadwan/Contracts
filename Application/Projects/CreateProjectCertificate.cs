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
                RuleFor(x => x.Certificate!.PartyId).NotEmpty().WithMessage("Party ID is required");
                // REFACTOR: Validate CertificateItems for CONTRACTING_CERTIFICATE
                // Purpose: Ensure items have required fields and valid values
                // Context: Prevents zeroed-out items unless intentional
                RuleFor(x => x.Certificate!.CertificateItems)
                    .Must(items => items != null && items.Any())
                    .WithMessage("At least one certificate item is required");
                RuleForEach(x => x.Certificate!.CertificateItems)
                    .Must(item => item.Quantity > 0)
                    .WithMessage("Quantity must be greater than 0")
                    .When(x => x.Certificate!.CertificateCategory == "CONTRACTING_CERTIFICATE");
                RuleForEach(x => x.Certificate!.CertificateItems)
                    .Must(item => item.UnitPrice > 0)
                    .WithMessage("Unit price must be greater than 0")
                    .When(x => x.Certificate!.CertificateCategory == "CONTRACTING_CERTIFICATE");
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

                    if (certificate.CertificateCategory == "CONTRACTING_CERTIFICATE")
                    {
                        // REFACTOR: Optimize party query
                        // Purpose: Reuse party entity to avoid redundant queries
                        // Context: Improves performance and clarity
                        var party = await _context.Parties
                            .FirstOrDefaultAsync(p => p.PartyId == certificate.PartyId, cancellationToken);
                        if (party == null)
                        {
                            await transaction.RollbackAsync(cancellationToken);
                            return Result<ProjectCertificateDto>.Failure("Party not found");
                        }

                        partyCode = party.PartyId;
                        var certificateCount = await _context.WorkEfforts
                            .CountAsync(
                                we => we.PartyId == certificate.PartyId &&
                                      we.CertificateCategory == "CONTRACTING_CERTIFICATE", cancellationToken);
                        newProjectCertificateSerial = $"{partyCode}-{certificateCount + 1:D4}";
                    }
                    else
                    {
                        newProjectCertificateSerial = await _utilityService.GetNextSequence("WorkEffort");
                    }

                    // Create certificate header
                    var workEffort = new WorkEffort
                    {
                        WorkEffortId = newWorkEffortSerial,
                        CertificateNumber = newProjectCertificateSerial,
                        WorkEffortTypeId = "PROJECT_CERTIFICATE",
                        CertificateCategory = certificate.CertificateCategory,
                        PartyId = certificate.PartyId,
                        ProjectId = certificate.ProjectId,
                        Description = certificate.Description,
                        EstimatedStartDate = certificate.EstimatedStartDate,
                        EstimatedCompletionDate = certificate.EstimatedCompletionDate,
                        CurrentStatusId = "WEPR_IN_PROGRESS",
                        CreatedDate = stamp,
                        LastUpdatedStamp = stamp
                    };
                    _context.WorkEfforts.Add(workEffort);

                    // Create certificate items
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
                            FacilityId = item.FacilityId,
                            IsContractorPurchased = item.IsContractorPurchased,
                            CreatedDate = stamp,
                            LastUpdatedStamp = stamp,
                            CurrentStatusId = "WEPR_IN_PROGRESS"
                        };
                        _context.WorkEfforts.Add(itemWorkEffort);
                    }

                    // REFACTOR: Create Purchase Order for PROCUREMENT_CERTIFICATE with discount adjustments
                    // Purpose: Generate a PO with OrderAdjustments for non-zero discounts in certificate items
                    // Context: Maps discounts to OrderAdjustmentDto2, aligning with frontend PO data structure
                    if (certificate.CertificateCategory == "PROCUREMENT_CERTIFICATE" ||
                        certificate.CertificateCategory == "CONTRACTING_CERTIFICATE")
                    {
                        // Fetch service product IDs
                        var serviceProductIds = await _context.Products
                            .Where(p => p.ProductTypeId == "SERVICE")
                            .Select(p => p.ProductId)
                            .ToListAsync(cancellationToken);

                        // Filter items for PO: include all for PROCUREMENT, only services or contractor-purchased goods for CONTRACTING
                        var poItems = certificate.CertificateCategory == "PROCUREMENT_CERTIFICATE"
                            ? certificate.CertificateItems
                            : certificate.CertificateItems
                                .Where(item => item.IsContractorPurchased || // Contractor-purchased goods
                                               serviceProductIds.Contains(item.ProductId)) // Services
                                .ToList();

                        if (poItems.Any())
                        {
                            var orderAdjustments = poItems
                                .Select((item, index) => new { Item = item, Index = index })
                                .Where(x => x.Item.Discount.HasValue && x.Item.Discount > 0)
                                .Select(x => new OrderAdjustmentDto2
                                {
                                    OrderAdjustmentId = Guid.NewGuid().ToString(),
                                    OrderAdjustmentTypeId = "DISCOUNT_ADJUSTMENT",
                                    OrderAdjustmentTypeDescription = "خصم",
                                    OrderId = null, // Will be set in CreatePurchaseOrder
                                    OrderItemSeqId = (x.Index + 1).ToString("D4"),
                                    Amount = -x.Item.Discount.Value, // Negative for discount
                                    CorrespondingProductId = x.Item.ProductId,
                                    CorrespondingProductName = x.Item.ProductName,
                                    IsManual = "Y",
                                    CreatedDate = stamp,
                                    IsAdjustmentDeleted = false,
                                    SourcePercentage = x.Item.TotalAmount > 0
                                        ? (x.Item.Discount.Value / x.Item.TotalAmount) * 100
                                        : 0
                                })
                                .ToList();

                            var orderDto = new OrderDto
                            {
                                OrderTypeId = "PURCHASE_ORDER",
                                FromPartyId = certificate.PartyId,
                                CurrencyUomId = await _productStoreService.GetProductStoreDefaultCurrencyId(),
                                OrderDate = stamp,
                                StatusId = "ORDER_CREATED",
                                StatusDescription = "Created",
                                InternalRemarks = $"Auto-generated from Certificate {newProjectCertificateSerial}",
                                GrandTotal = poItems.Sum(i => i.TotalAmount - (i.Discount ?? 0)),
                                OrderItems = poItems.Select((item, index) => new OrderItemDto2
                                {
                                    OrderItemSeqId = (index + 1).ToString("D4"),
                                    ProductId = item.ProductId,
                                    ProductName = item.ProductName,
                                    Quantity = item.Quantity,
                                    UnitPrice = item.UnitPrice,
                                    SubTotal = item.TotalAmount -
                                               (item.Discount ??
                                                0), // REFACTOR: Fixed typo (i.Discount to item.Discount)
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

                    // Construct response DTO
                    var resultDto = new ProjectCertificateDto
                    {
                        WorkEffortId = workEffort.WorkEffortId,
                        CertificateNumber = workEffort.CertificateNumber,
                        WorkEffortTypeId = workEffort.WorkEffortTypeId,
                        ProjectId = workEffort.ProjectId,
                        PartyId = workEffort.PartyId,
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