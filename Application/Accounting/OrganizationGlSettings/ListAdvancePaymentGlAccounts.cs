using AutoMapper;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Persistence;

namespace Application.GlAccounts
{
    public class ListAdvancePaymentGlAccounts
    {
        public class Query : IRequest<Result<List<AdvancePaymentGlAccountDto>>>
        {
            public string Language { get; set; }
        }

        public class Handler : IRequestHandler<Query, Result<List<AdvancePaymentGlAccountDto>>>
        {
            private readonly DataContext _context;
            private readonly IMapper _mapper;
            private readonly ILogger<Handler> _logger;

            public Handler(DataContext context, IMapper mapper, ILogger<Handler> logger)
            {
                _context = context;
                _mapper = mapper;
                _logger = logger;
            }

            public async Task<Result<List<AdvancePaymentGlAccountDto>>> Handle(Query request, CancellationToken cancellationToken)
            {
                // REFACTOR: Use queryable with conditional AccountName selection based on Language
                // Purpose: Returns AccountNameArabic when Language is "ar", otherwise AccountName, optimizing database query
                // Context: Assumes GlAccounts table has AccountNameArabic column; uses ternary-like expression in LINQ
                var query = _context.GlAccounts
                    .Where(x => x.ParentGlAccountId == "100070")
                    .Select(x => new AdvancePaymentGlAccountDto
                    {
                        GlAccountIdAdvancedPayment = x.GlAccountId,
                        AccountCode = x.AccountCode,
                        // REFACTOR: Conditionally select AccountNameArabic or AccountName based on Language
                        // Purpose: Supports multilingual output without additional database queries
                        AccountName = request.Language == "ar" ? x.AccountNameArabic : x.AccountName,
                        Description = x.Description,
                        GlAccountTypeId = x.GlAccountTypeId,
                        GlAccountClassId = x.GlAccountClassId
                    })
                    .AsQueryable();

                // REFACTOR: Log query for debugging and monitoring
                // Purpose: Facilitates troubleshooting and performance analysis
                var queryString = query.ToQueryString();
                _logger.LogDebug("Executing query: {QueryString}", queryString);

                // REFACTOR: Use async execution for scalability
                // Purpose: Non-blocking I/O operation to improve application responsiveness
                var result = await query.ToListAsync(cancellationToken);

                return Result<List<AdvancePaymentGlAccountDto>>.Success(result);
            }
        }
    }

    public class AdvancePaymentGlAccountDto
    {
        public string GlAccountIdAdvancedPayment { get; set; }
        public string AccountCode { get; set; }
        public string AccountName { get; set; }
        public string Description { get; set; }
        public string GlAccountTypeId { get; set; }
        public string GlAccountClassId { get; set; }
    }
}