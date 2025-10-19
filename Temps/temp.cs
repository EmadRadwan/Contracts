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

    // REFACTOR: Simplified validation to focus only on WorkEffortId
    // Purpose: Ensure the required WorkEffortId is provided
    // Context: Reduced complexity compared to original, focusing on essential validation
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
            // REFACTOR: Added validation step before query execution
            // Purpose: Prevent invalid queries from hitting the database
            // Context: Matches simplified validation pattern
            var validator = new QueryValidator();
            var validationResult = await validator.ValidateAsync(request, cancellationToken);
            if (!validationResult.IsValid)
                return Result<List<MultiPaymentItemDto>>.Failure(string.Join("; ", validationResult.Errors.Select(e => e.ErrorMessage)));

            try
            {
                // REFACTOR: Simplified query to fetch essential MultiPaymentItem fields
                // Purpose: Reduce complexity by limiting joins and focusing on core data
                // Context: Optimized for performance and readability
                var multiPaymentItems = await _context.MultiPaymentItems
                    .Where(item => item.WorkEffortId == request.WorkEffortId)
                    .Select(item => new MultiPaymentItemDto
                    {
                        ItemId = item.ItemId,
                        WorkEffortId = item.WorkEffortId,
                        ProjectId = item.ProjectId,
                        ProjectName = item.ProjectName,
                        SubProjectId = item.SubProjectId,
                        SubProjectName = item.SubProjectName,
                        ItemType = item.ItemType,
                        ProductId = item.ProductId,
                        ProductName = item.ProductName,
                        UomId = item.UomId,
                        UomName = item.UomName,
                        Description = item.Description,
                        Amount = item.Amount,
                        Discount = item.Discount,
                        DiscountMode = item.DiscountMode,
                        TransportationExpenses = item.TransportationExpenses,
                        Gratuities = item.Gratuities,
                        Total = item.Total
                    })
                    .ToListAsync(cancellationToken);

                // REFACTOR: Added check for empty results
                // Purpose: Provide clear feedback when no items are found
                // Context: Improves user experience with meaningful responses
                if (!multiPaymentItems.Any())
                    return Result<List<MultiPaymentItemDto>>.Success(new List<MultiPaymentItemDto>());

                return Result<List<MultiPaymentItemDto>>.Success(multiPaymentItems);
            }
            catch (Exception ex)
            {
                // REFACTOR: Standardized error handling
                // Purpose: Provide clear error messages for debugging
                // Context: Consistent with simplified error handling approach
                return Result<List<MultiPaymentItemDto>>.Failure($"Failed to retrieve multi-payment items: {ex.Message}");
            }
        }
    }
}

// REFACTOR: Added DTO to match the form's expected structure
// Purpose: Ensure compatibility with frontend MultiPaymentItem model
// Context: Simplified to include only fields used in the form
public class MultiPaymentItemDto
{
    public string ItemId { get; set; }
    public string WorkEffortId { get; set; }
    public string ProjectId { get; set; }
    public string ProjectName { get; set; }
    public string SubProjectId { get; set; }
    public string SubProjectName { get; set; }
    public string ItemType { get; set; }
    public string ProductId { get; set; }
    public string ProductName { get; set; }
    public string UomId { get; set; }
    public string UomName { get; set; }
    public string Description { get; set; }
    public decimal Amount { get; set; }
    public decimal Discount { get; set; }
    public string DiscountMode { get; set; }
    public decimal TransportationExpenses { get; set; }
    public decimal Gratuities { get; set; }
    public decimal Total { get; set; }
}