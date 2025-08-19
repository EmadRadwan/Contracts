using Application.Catalog.ProductStores;
using Application.Shipments.Accounting;
using AutoMapper;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Persistence;

namespace Application.Accounting.OrganizationGlSettings;

public class GetGlAccountOrganizationAndClass
{
    public class Query : IRequest<Result<List<GlAccountOrganizationAndClassDto>>>
    {
        public string CompanyId { get; set; }
    }

    public class Handler : IRequestHandler<Query, Result<List<GlAccountOrganizationAndClassDto>>>
    {
        private readonly DataContext _context;

        public Handler(DataContext context)
        {
            _context = context;
        }

        public async Task<Result<List<GlAccountOrganizationAndClassDto>>> Handle(Query request,
            CancellationToken cancellationToken)
        {
            try
            {
                var glAccounts = await _context.GlAccountOrganizations
                    .Include(x => x.GlAccount)
                    .Include(x => x.OrganizationParty)
                    .Where(x => x.OrganizationPartyId == request.CompanyId)
                    .OrderBy(x => x.GlAccount.GlAccountClassId)
                    .Select(x => new GlAccountOrganizationAndClassDto
                    {
                        GlAccountId = x.GlAccountId,
                        OrganizationPartyId = x.OrganizationPartyId,
                        GlAccountTypeId = x.GlAccount.GlAccountTypeId,
                        GlAccountTypeName = x.GlAccount.GlAccountType!.Description,
                        GlAccountClassId = x.GlAccount.GlAccountClassId,
                        AccountCode = x.GlAccount.AccountCode,
                        AccountName = x.GlAccount.AccountName,
                        ClassName = x.GlAccount.GlAccountClass!.Description
                    }).ToListAsync(cancellationToken);


                return Result<List<GlAccountOrganizationAndClassDto>>.Success(glAccounts);
            }
            catch (Exception ex)
            {
                // Handle exception and return an error result
                return Result<List<GlAccountOrganizationAndClassDto>>.Failure(ex.Message);
            }
        }
    }
}