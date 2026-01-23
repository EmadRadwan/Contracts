using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Persistence;

namespace Application.Projects;

public class ListMultiPaymentItems
{
    public class Query : IRequest<Result<List<MultiPaymentItemDto>>>
    {
        public string WorkEffortId { get; set; }
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
                return Result<List<MultiPaymentItemDto>>.Failure(string.Join("; ", validationResult.Errors.Select(e => e.ErrorMessage)));

            try
            {
                var multiPaymentItems = await _context.WorkEfforts
                    .Where(item => item.WorkEffortParentId == request.WorkEffortId 
                        && item.WorkEffortTypeId == "PAYMENT_CERTIFICATE_ITEM")
                    .GroupJoin(_context.WorkEfforts.Where(p => p.WorkEffortTypeId == "PROJECT"),
                        item => item.ProjectId,
                        project => project.WorkEffortId,
                        (item, projects) => new { item, projects })
                    .SelectMany(x => x.projects.DefaultIfEmpty(), (x, project) => new { x.item, project })
                    .GroupJoin(_context.WorkEfforts.Where(sp => sp.WorkEffortTypeId == "SUB_PROJECT"),
                        x => x.item.SubProjectId,
                        subProject => subProject.WorkEffortId,
                        (x, subProjects) => new { x.item, x.project, subProjects })
                    .SelectMany(x => x.subProjects.DefaultIfEmpty(), (x, subProject) => new { x.item, x.project, subProject })
                    .GroupJoin(_context.Products,
                        x => x.item.ServiceId,
                        service => service.ProductId,
                        (x, services) => new { x.item, x.project, x.subProject, services })
                    .SelectMany(x => x.services.DefaultIfEmpty(), (x, service) => new { x.item, x.project, x.subProject, service })
                    .GroupJoin(_context.Products,
                        x => x.item.ProductId,
                        product => product.ProductId,
                        (x, products) => new { x.item, x.project, x.subProject, x.service, products })
                    .SelectMany(x => x.products.DefaultIfEmpty(), (x, product) => new { x.item, x.project, x.subProject, x.service, product })
                    .GroupJoin(_context.Parties,
                        x => x.item.PartyIdSupplier,
                        supplier => supplier.PartyId,
                        (x, suppliers) => new { x.item, x.project, x.subProject, x.service, x.product, suppliers })
                    .SelectMany(x => x.suppliers.DefaultIfEmpty(), (x, supplier) => new { x.item, x.project, x.subProject, x.service, x.product, supplier })
                    .GroupJoin(_context.Parties,
                        x => x.item.PartyIdContractor,
                        contractor => contractor.PartyId,
                        (x, contractors) => new { x.item, x.project, x.subProject, x.service, x.product, x.supplier, contractors })
                    .SelectMany(x => x.contractors.DefaultIfEmpty(), (x, contractor) => new MultiPaymentItemDto
                    {
                        WorkEffortId = x.item.WorkEffortId,
                        GlAccountId = x.item.GlAccountId,
                        ItemType = x.item.CostType,
                        ServiceId = x.item.ServiceId,
                        ServiceName = x.service != null ? x.service.ProductName : "",
                        ProductId = x.item.ProductId,
                        ProductName = x.product != null ? x.product.ProductName : "",
                        Description = x.item.Description,
                        Amount = (decimal?)x.item.Amount, // Adjust if Amount is a separate DB field
                        Discount = (decimal?)x.item.Discount,
                        TransportationExpenses = (decimal?)x.item.TransportationExpenses,
                        Gratuities = (decimal?)x.item.Gratuities,
                        Total = (decimal?)x.item.TotalAmount,
                        PartyIdSupplier = x.item.PartyIdSupplier,
                        PartyIdSupplierName = x.supplier != null ? x.supplier.Description : "",
                        PartyIdContractor = x.item.PartyIdContractor,
                        PartyIdContractorName = contractor != null ? contractor.Description : ""
                    })
                    .ToListAsync(cancellationToken);

                var itemTypeDescriptions = new Dictionary<string, string>
                {
                    { "MATERIALS", "المواد" },
                    { "LABOR", "العمالة" },
                    { "EQUIPMENT", "المعدات" },
                    { "EXPENSES", "المصروفات" }
                };

                foreach (var item in multiPaymentItems)
                {
                    item.ItemTypeDescription = itemTypeDescriptions.ContainsKey(item.ItemType ?? "")
                        ? itemTypeDescriptions[item.ItemType]
                        : "";
                    
                    item.DiscountMode = item.Discount > 0 ? "value" : "percentage";
                }

                if (!multiPaymentItems.Any())
                    return Result<List<MultiPaymentItemDto>>.Success(new List<MultiPaymentItemDto>());

                return Result<List<MultiPaymentItemDto>>.Success(multiPaymentItems);
            }
            catch (Exception ex)
            {
                return Result<List<MultiPaymentItemDto>>.Failure($"Failed to retrieve multi-payment items: {ex.Message}");
            }
        }
    }
}