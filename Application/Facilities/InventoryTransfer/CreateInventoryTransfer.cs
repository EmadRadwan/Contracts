using Application.Catalog.Products.Services.Inventory;
using FluentValidation;
using MediatR;
using Persistence;

namespace Application.Facilities.InventoryTransfer;

public class CreateInventoryTransfer
{
    public class Command : IRequest<Result<InventoryTransferDto>>
    {
        public InventoryTransferDto InventoryTransferDto { get; set; }
    }

    public class CommandValidator : AbstractValidator<Command>
    {
        public CommandValidator()
        {
            RuleFor(x => x.InventoryTransferDto).SetValidator(new InventoryTransferValidator());
        }
    }

    public class Handler : IRequestHandler<Command, Result<InventoryTransferDto>>
    {
        private readonly DataContext _context;
        private readonly IInventoryService _inventoryService;


        public Handler(DataContext context, IInventoryService inventoryService)
        {
            _context = context;
            _inventoryService = inventoryService;
        }

        public async Task<Result<InventoryTransferDto>> Handle(Command request, CancellationToken cancellationToken)
        {
            await using var transaction = await _context.Database.BeginTransactionAsync(cancellationToken);
            try
            {
                var inventoryTransfer = await _inventoryService.CreateInventoryTransfer(request.InventoryTransferDto);

                await _context.SaveChangesAsync(cancellationToken);

                await transaction.CommitAsync(cancellationToken);


                var inventoryTransferToReturn = new InventoryTransferDto();
                return Result<InventoryTransferDto>.Success(inventoryTransferToReturn);
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync(cancellationToken);

                return Result<InventoryTransferDto>.Failure("Error creating InventoryTransfer");
            }
        }
    }
}