using MediatR;
using Microsoft.Extensions.Logging;
using Persistence;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Application.Facilities;

public class UpdatePicklistCommand : IRequest<Result<UpdatePicklistResult>>
{
    public string PicklistId { get; set; }
    public string StatusId { get; set; }
}


public class UpdatePicklistCommandHandler 
    : IRequestHandler<UpdatePicklistCommand, Result<UpdatePicklistResult>>
{
    private readonly IPickListService _picklistService;
    private readonly ILogger<UpdatePicklistCommandHandler> _logger;
    private readonly DataContext _context;

    public UpdatePicklistCommandHandler(
        IPickListService picklistService,
        ILogger<UpdatePicklistCommandHandler> logger,
        DataContext context)
    {
        _picklistService = picklistService;
        _logger = logger;
        _context = context;
    }

    public async Task<Result<UpdatePicklistResult>> Handle(
        UpdatePicklistCommand request, 
        CancellationToken cancellationToken)
    {
        await using var transaction = await _context.Database.BeginTransactionAsync(cancellationToken);

        try
        {
            // Build input for domain logic
            var input = new UpdatePicklistInput
            {
                PicklistId = request.PicklistId,
                StatusId = request.StatusId,
            };

            // 1) Call service
            var resultDto = await _picklistService.UpdatePicklist(input);

            // 2) Save changes
            await _context.SaveChangesAsync(cancellationToken);

            // 3) Commit transaction
            await transaction.CommitAsync(cancellationToken);

            // Return success
            return Result<UpdatePicklistResult>.Success(resultDto);
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync(cancellationToken);

            _logger.LogError(ex, "Error updating picklist with ID = {PicklistId}", request.PicklistId);
            return Result<UpdatePicklistResult>.Failure($"Exception: {ex.Message}");
        }
    }
}
