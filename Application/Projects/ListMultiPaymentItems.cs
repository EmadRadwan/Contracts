using Application.Common;
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
                // REFACTOR: Modified query to filter by WorkEffortParentId instead of WorkEffortId
                // and added WorkEffortTypeId filter for PAYMENT_CERTIFICATE_ITEM.
                // Joined with Products table to retrieve ProductId and ProductName.
                // This ensures we fetch child work efforts of the specified parent
                // and only include payment certificate items, with accurate product details.
                var multiPaymentItems = await _context.WorkEfforts
                    .Where(item => item.WorkEffortParentId == request.WorkEffortId 
                        && item.WorkEffortTypeId == "PAYMENT_CERTIFICATE_ITEM")
                    .Join(_context.Products,
                        workEffort => workEffort.ProductId,
                        product => product.ProductId,
                        (workEffort, product) => new MultiPaymentItemDto
                        {
                            WorkEffortId = workEffort.WorkEffortId,
                            ProjectId = workEffort.ProjectId,
                            ProjectName = workEffort.ProjectName,
                            SubProjectId = workEffort.SubProjectId,
                            SubProjectName = workEffort.SubProjectName,
                            ItemType = workEffort.CostType,
                            ProductId = product.ProductId,
                            ProductName = product.ProductName,
                            Description = workEffort.Description,
                            TotalAmount = (decimal)workEffort.TotalAmount,
                            Discount = (decimal)workEffort.Discount,
                            TransportationExpenses = (decimal)workEffort.TransportationExpenses,
                            Gratuities = (decimal)workEffort.Gratuities
                        })
                    .ToListAsync(cancellationToken);

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

public class MultiPaymentItemDto
{
    public string WorkEffortId { get; set; }
    public string ProjectId { get; set; }
    public string ProjectName { get; set; }
    public string SubProjectId { get; set; }
    public string SubProjectName { get; set; }
    public string ItemType { get; set; }
    public string ProductId { get; set; }
    public string ProductName { get; set; }
    public string Description { get; set; }
    public decimal TotalAmount { get; set; }
    public decimal Discount { get; set; }
    public decimal TransportationExpenses { get; set; }
    public decimal Gratuities { get; set; }
}