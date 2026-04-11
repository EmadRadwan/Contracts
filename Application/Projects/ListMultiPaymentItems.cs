using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Persistence;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Application.Projects;

public class ListMultiPaymentItems
{
    public class Query : IRequest<Result<List<MultiPaymentItemDto>>>
    {
        public string WorkEffortId { get; set; } = string.Empty;
    }

    public class QueryValidator : AbstractValidator<Query>
    {
        public QueryValidator()
        {
            RuleFor(x => x.WorkEffortId).NotEmpty().WithMessage("Work Effort ID is required");
        }
    }

    public class Handler : IRequestHandler<Query, Result<List<MultiPaymentItemDto>>>
    {
        private readonly DataContext _context;

        public Handler(DataContext context)
        {
            _context = context;
        }

        public async Task<Result<List<MultiPaymentItemDto>>> Handle(Query request, CancellationToken cancellationToken)
        {
            var validator = new QueryValidator();
            var validationResult = await validator.ValidateAsync(request, cancellationToken);

            if (!validationResult.IsValid)
                return Result<List<MultiPaymentItemDto>>.Failure(
                    string.Join("; ", validationResult.Errors.Select(e => e.ErrorMessage)));

            try
            {
                var multiPaymentItems = await _context.WorkEfforts
                    .Where(item => item.WorkEffortParentId == request.WorkEffortId
                                   && item.WorkEffortTypeId == "PAYMENT_CERTIFICATE_ITEM")

                    // ── Project ──
                    .GroupJoin(_context.WorkEfforts.Where(p => p.WorkEffortTypeId == "PROJECT"),
                        item => item.ProjectId,
                        project => project.WorkEffortId,
                        (item, projects) => new { item, projects })
                    .SelectMany(x => x.projects.DefaultIfEmpty(),
                        (x, project) => new { x.item, project })

                    // ── SubProject ──
                    .GroupJoin(_context.WorkEfforts.Where(sp => sp.WorkEffortTypeId == "SUB_PROJECT"),
                        x => x.item.SubProjectId,
                        subProject => subProject.WorkEffortId,
                        (x, subProjects) => new { x.item, x.project, subProjects })
                    .SelectMany(x => x.subProjects.DefaultIfEmpty(),
                        (x, subProject) => new { x.item, x.project, subProject })

                    // ── Service (Product) ──
                    .GroupJoin(_context.Products,
                        x => x.item.ServiceId,
                        service => service.ProductId,
                        (x, services) => new { x.item, x.project, x.subProject, services })
                    .SelectMany(x => x.services.DefaultIfEmpty(),
                        (x, service) => new { x.item, x.project, x.subProject, service })

                    // ── Product ──
                    .GroupJoin(_context.Products,
                        x => x.item.ProductId,
                        product => product.ProductId,
                        (x, products) => new { x.item, x.project, x.subProject, x.service, products })
                    .SelectMany(x => x.products.DefaultIfEmpty(),
                        (x, product) => new { x.item, x.project, x.subProject, x.service, product })

                    // ── Supplier Party ──
                    .GroupJoin(_context.Parties,
                        x => x.item.PartyIdSupplier,
                        supplier => supplier.PartyId,
                        (x, suppliers) => new { x.item, x.project, x.subProject, x.service, x.product, suppliers })
                    .SelectMany(x => x.suppliers.DefaultIfEmpty(),
                        (x, supplier) => new { x.item, x.project, x.subProject, x.service, x.product, supplier })

                    // ── Contractor Party ──
                    .GroupJoin(_context.Parties,
                        x => x.item.PartyIdContractor,
                        contractor => contractor.PartyId,
                        (x, contractors) => new { x.item, x.project, x.subProject, x.service, x.product, x.supplier, contractors })
                    .SelectMany(x => x.contractors.DefaultIfEmpty(),
                        (x, contractor) => new { x.item, x.project, x.subProject, x.service, x.product, x.supplier, contractor })

                    // ── Cost Center (Separate CostCenters table) ──
                    .GroupJoin(_context.CostCenters,                          // ← Changed here
                        x => x.item.CostCenterId,
                        costCenter => costCenter.CostCenterId,                // adjust property name if different (e.g. Id)
                        (x, costCenters) => new { x.item, x.project, x.subProject, x.service, x.product, x.supplier, x.contractor, costCenters })
                    .SelectMany(y => y.costCenters.DefaultIfEmpty(),
                        (y, costCenter) => new { y.item, y.project, y.subProject, y.service, y.product, y.supplier, y.contractor, costCenter })

                    // ── GlAccount (from GlAccounts table) ──
                    .GroupJoin(_context.GlAccounts,
                        x => x.item.GlAccountId,
                        gl => gl.GlAccountId,
                        (x, glAccounts) => new { x.item, x.project, x.subProject, x.service, x.product, x.supplier, x.contractor, x.costCenter, glAccounts })
                    .SelectMany(z => z.glAccounts.DefaultIfEmpty(),
                        (z, glAccount) => new MultiPaymentItemDto
                        {
                            WorkEffortId = z.item.WorkEffortId,
                            GlAccountId = z.item.GlAccountId,
                            GlAccountName = glAccount != null ? glAccount.AccountNameArabic : null,

                            ItemType = z.item.CostType,
                            ServiceId = z.item.ServiceId,
                            ServiceName = z.service != null ? z.service.ProductName : "",
                            ProductId = z.item.ProductId,
                            ProductName = z.product != null ? z.product.ProductName : "",
                            Description = z.item.Description,

                            Amount = (decimal?)z.item.Amount,
                            Discount = (decimal?)z.item.Discount,
                            TransportationExpenses = (decimal?)z.item.TransportationExpenses,
                            Gratuities = (decimal?)z.item.Gratuities,
                            Total = (decimal?)z.item.TotalAmount,
                            EstimatedStartDate = z.item.EstimatedStartDate,

                            PartyIdSupplier = z.item.PartyIdSupplier,
                            PartyIdSupplierName = z.supplier != null ? z.supplier.Description : "",
                            PartyIdContractor = z.item.PartyIdContractor,
                            PartyIdContractorName = z.contractor != null ? z.contractor.Description : "",

                            // Cost Center from separate table
                            CostCenterId = z.item.CostCenterId,
                            CostCenterName = z.costCenter != null ? z.costCenter.Description : "",   // ← This should now populate

                            ProjectId = z.item.ProjectId,
                            SubProjectId = z.item.SubProjectId,
                            SubProjectName = z.subProject != null ? z.subProject.SubProjectName : ""
                        })
                    .ToListAsync(cancellationToken);

                // Post-processing
                var itemTypeDescriptions = new Dictionary<string, string>
                {
                    { "MATERIALS", "المواد" },
                    { "LABOR", "العمالة" },
                    { "EQUIPMENT", "المعدات" },
                    { "EXPENSES", "المصروفات" }
                };

                foreach (var item in multiPaymentItems)
                {
                    item.ItemTypeDescription = itemTypeDescriptions.GetValueOrDefault(item.ItemType ?? "", "");

                    item.DiscountMode = item.Discount > 0
                        ? "value"
                        : item.Discount < 0
                            ? "percentage"
                            : null;
                }

                return Result<List<MultiPaymentItemDto>>.Success(multiPaymentItems);
            }
            catch (Exception ex)
            {
                return Result<List<MultiPaymentItemDto>>.Failure(
                    $"Failed to retrieve multi-payment items: {ex.Message}");
            }
        }
    }
}