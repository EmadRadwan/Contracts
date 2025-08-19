using Domain;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Persistence;
using Application.Accounting.OrganizationGlSettings;
using Application.Accounting.Services;

namespace Application.Accounting.FinAccounts;

public class CreateFinAccountTrans
{
    public class Command : IRequest<Result<string>>
    {
        public CreateFinAccountTransRequest FinAccountTrans { get; set; }
    }

    public class Handler : IRequestHandler<Command, Result<string>>
    {
        private readonly DataContext _context;
        private readonly IFinAccountService _finAccountService;

        public Handler(DataContext context, IFinAccountService finAccountService)
        {
            _context = context;
            _finAccountService = finAccountService;
        }

        public async Task<Result<string>> Handle(Command request, CancellationToken cancellationToken)
        {
            await using var transaction = await _context.Database.BeginTransactionAsync(cancellationToken);

            try 
            {
                var result = await _finAccountService.CreateFinAccountTrans(request.FinAccountTrans);
                
                await _context.SaveChangesAsync(cancellationToken);
                await transaction.CommitAsync(cancellationToken);

                return result;
            } 
            catch (Exception ex)
            {
                await transaction.RollbackAsync(cancellationToken);

                return Result<string>.Failure($"Error creating Fin Account Transaction: {ex.Message}");
            }   
        }
    }
}