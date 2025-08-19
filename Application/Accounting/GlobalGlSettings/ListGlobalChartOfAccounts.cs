using Application.Interfaces;
using AutoMapper;
using MediatR;
using Microsoft.AspNetCore.OData.Query;
using Microsoft.Extensions.Logging;
using Persistence;

namespace Application.Shipments.GlobalGlSettings;

public class ListGlobalChartOfAccounts
{
    public class Query : IRequest<IQueryable<GlAccountRecord>>
    {
        public ODataQueryOptions<GlAccountRecord> Options { get; set; }
        public string Language { get; set; }  // Add Language property

    }

    public class Handler : IRequestHandler<Query, IQueryable<GlAccountRecord>>
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

        public async Task<IQueryable<GlAccountRecord>> Handle(Query request, CancellationToken cancellationToken)
        {
            var language = request.Language;  // Access the language from the request

            var accountsQuery = _context.GlAccounts
                .Select(account => new GlAccountRecord
                {
                    GlAccountId = account.GlAccountId,
                    GlAccountTypeId = account.GlAccountTypeId,
                    GlAccountTypeDescription = language == "en" ? account.GlAccountType.Description : account.GlAccountType.DescriptionArabic,
                    GlAccountClassId = account.GlAccountClassId,
                    GlResourceTypeId = account.GlResourceTypeId,
                    GlResourceTypeDescription = language == "en" ? account.GlAccountClass.Description : account.GlAccountClass.DescriptionArabic,
                    ParentGlAccountId = account.ParentGlAccountId,
                    AccountCode = account.AccountCode,
                    AccountName = language == "en" ? account.AccountName : account.AccountNameArabic,
                    ParentAccountName = _context.GlAccounts
                        .Where(a => a.GlAccountId == account.ParentGlAccountId)
                        .Select(a => language == "en" ? a.AccountName : a.AccountNameArabic) 
                        .FirstOrDefault(),
                    Expanded = false
                });

            return accountsQuery;
        }
    }
}