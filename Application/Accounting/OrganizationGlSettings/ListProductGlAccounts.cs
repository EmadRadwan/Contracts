using Application.Interfaces;
using AutoMapper;
using MediatR;
using Microsoft.AspNetCore.OData.Query;
using Microsoft.Extensions.Logging;
using Persistence;

namespace Application.Shipments.OrganizationGlSettings;

public class ListProductGlAccounts
{
    public class Query : IRequest<IQueryable<ProductGlAccountRecord>>
    {
        public ODataQueryOptions<ProductGlAccountRecord> Options { get; set; }
        public string? CompanyId { get; set; }
    }

    public class Handler : IRequestHandler<Query, IQueryable<ProductGlAccountRecord>>
    {
        private readonly DataContext _context;
        private readonly ILogger<Handler> _logger;
        private readonly IMapper _mapper;
        private readonly IUserAccessor _userAccessor;

        public Handler(DataContext context, IMapper mapper, IUserAccessor userAccessor, ILogger<Handler> logger)
        {
            _mapper = mapper;
            _context = context;
            _userAccessor = userAccessor;
            _logger = logger;
        }

        public async Task<IQueryable<ProductGlAccountRecord>> Handle(Query request, CancellationToken cancellationToken)
        {
            var accountsQuery = _context.GlAccounts
                .Join(_context.ProductGlAccounts,
                    account => account.GlAccountId,
                    accountProd => accountProd.GlAccountId,
                    (account, accountProd) => new { account, accountProd })
                .Where(account => account.accountProd.OrganizationPartyId == request.CompanyId)
                .Select(account => new ProductGlAccountRecord
                {
                    GlAccountId = account.accountProd.GlAccountId,
                    GlAccountTypeId = account.account.GlAccountTypeId,
                    GlAccountTypeDescription = account.account.GlAccountType.Description,
                    OrganizationPartyId = account.accountProd.OrganizationPartyId,
                    ProductId = account.accountProd.ProductId,
                    ProductName = account.accountProd.Product.ProductName,
                    AccountName = _context.GlAccounts
                        .Where(a => a.GlAccountId == account.account.ParentGlAccountId)
                        .Select(a => a.AccountName)
                        .FirstOrDefault()
                });

            return accountsQuery;
        }
    }
}