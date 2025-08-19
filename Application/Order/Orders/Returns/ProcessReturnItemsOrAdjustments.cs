using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Persistence;

namespace Application.Order.Orders.Returns
{
    public class ProcessReturnItemsOrAdjustments
    {
        public class Command : IRequest<List<ReturnItemOrAdjustmentResult>>
        {
            public List<ReturnItemOrAdjustmentContext> Contexts { get; set; }
        }

        public class Handler : IRequestHandler<Command, List<ReturnItemOrAdjustmentResult>>
        {
            private readonly DataContext _context;
            private readonly IReturnService _returnService;

            public Handler(DataContext context, IReturnService returnService)
            {
                _context = context;
                _returnService = returnService;
            }

            public async Task<List<ReturnItemOrAdjustmentResult>> Handle(Command request, CancellationToken cancellationToken)
            {
                // Validate the input contexts
                if (request.Contexts == null || request.Contexts.Count == 0)
                {
                    return new List<ReturnItemOrAdjustmentResult>
                    {
                        ReturnItemOrAdjustmentResult.Error("No contexts provided.")
                    };
                }

                var results = new List<ReturnItemOrAdjustmentResult>();

                // Define transaction at a higher level
                await using var transaction = await _context.Database.BeginTransactionAsync(cancellationToken);

                try
                {
                    foreach (var context in request.Contexts)
                    {
                        try
                        {
                            // Fetch the return header type ID using ReturnId from context
                            var returnHeaderTypeId = await _context.ReturnHeaders
                                .Where(rh => rh.ReturnId == context.ReturnId)
                                .Select(rh => rh.ReturnHeaderTypeId)
                                .FirstOrDefaultAsync(cancellationToken);

                            if (string.IsNullOrEmpty(returnHeaderTypeId))
                            {
                                results.Add(ReturnItemOrAdjustmentResult.Error($"No ReturnHeaderTypeId found for ReturnId: {context.ReturnId}"));
                                continue;
                            }

                            // Determine the ReturnItemTypeId using ReturnItemMapKey and ReturnHeaderTypeId
                            var returnItemTypeId = await _context.ReturnItemTypeMaps
                                .Where(rtm => rtm.ReturnItemMapKey == context.ReturnItemMapKey && rtm.ReturnHeaderTypeId == returnHeaderTypeId)
                                .Select(rtm => rtm.ReturnItemTypeId)
                                .FirstOrDefaultAsync(cancellationToken);

                            if (string.IsNullOrEmpty(returnItemTypeId))
                            {
                                results.Add(ReturnItemOrAdjustmentResult.Error($"No ReturnItemTypeId found for ReturnId: {context.ReturnId} and ReturnItemMapKey: {context.ReturnItemMapKey}"));
                                continue;
                            }

                            // Update the context with the determined ReturnItemTypeId
                            context.ReturnItemTypeId = returnItemTypeId;

                            // Delegate processing to the return service
                            var result = await _returnService.CreateReturnItemOrAdjustment(context);
                            results.Add(result);
                        }
                        catch (Exception ex)
                        {
                            // Capture exceptions and include them in the results
                            results.Add(ReturnItemOrAdjustmentResult.Error($"Error processing context: {ex.Message}"));
                        }
                    }

                    var save = await _context.SaveChangesAsync(cancellationToken) > 0;

                    // Final commit if committing only once for the batch
                    await transaction.CommitAsync(cancellationToken);
                }
                catch (Exception ex)
                {
                    // Rollback the entire transaction in case of any outer-level error
                    await transaction.RollbackAsync(cancellationToken);
                    throw; // Rethrow to indicate failure
                }

                return results;
            }
        }
    }
}
