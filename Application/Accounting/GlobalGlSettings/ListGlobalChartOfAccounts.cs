using Application.Interfaces;
using AutoMapper;
using Domain;
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
                    GlXbrlClassId = account.GlXbrlClassId,
                    GlAccountClassDescription = language == "en" ? account.GlAccountClass.Description : account.GlAccountClass.DescriptionArabic,
                    ParentGlAccountId = account.ParentGlAccountId,
                    Description = account.Description,
                    AccountCode = account.AccountCode,
                    AccountName = account.AccountNameArabic ?? account.AccountName,
                    ParentAccountName = _context.GlAccounts
                        .Where(a => a.GlAccountId == account.ParentGlAccountId)
                        .Select(a => a.AccountName) 
                        .FirstOrDefault(),
                    ProductId = account.ProductId,
                    ExternalId = account.ExternalId,
                    GlReportId = account.GlReportId,
                    GlReportDescription = language == "en" ? account.GlReport.Description : account.GlReport.DescriptionArabic,
                    GlClassCourseId = account.GlClassCourseId,
                    GlClassCourseDescription = language == "en" ? account.GlClassCourse.Description : account.GlClassCourse.DescriptionArabic,
                    GlSubClassId = account.GlSubClassId,
                    GlSubClassDescription = language == "en" ? account.GlSubClass.Description : account.GlSubClass.DescriptionArabic,
                    GlSubClass2Id = account.GlSubClass2Id,
                    GlSubClass2Description = language == "en" ? account.GlSubClass2.Description : account.GlSubClass2.DescriptionArabic,
                    GlAccountCourseLabelId = account.GlAccountCourseLabelId,
                    GlAccountCourseLabelDescription = language == "en" ? account.GlAccountCourseLabel.Description : account.GlAccountCourseLabel.DescriptionArabic,
                    Expanded = false
                });
            
            var entityType = _context.Model.FindEntityType(typeof(GlAccount));

            var foreignKeysPointingHere =
                _context.Model.GetEntityTypes()
                    .SelectMany(et => et.GetForeignKeys())
                    .Where(fk => fk.PrincipalEntityType == entityType)
                    .ToList();


            return accountsQuery;
        }
    }
}