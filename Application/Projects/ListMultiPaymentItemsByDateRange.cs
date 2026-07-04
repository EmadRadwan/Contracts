using MediatR;
using Microsoft.EntityFrameworkCore;
using Persistence;
using Application.Core;

namespace Application.Projects
{
    public class ListMultiPaymentItemsByDateRange
    {
        public class Query : IRequest<Result<List<MultiPaymentItemReportRecord>>>
        {
            public DateTime StartDate { get; set; }
            public DateTime EndDate { get; set; }
            public string GlAccountId { get; set; }
        }

        public class Handler : IRequestHandler<Query, Result<List<MultiPaymentItemReportRecord>>>
        {
            private readonly DataContext _context;

            public Handler(DataContext context)
            {
                _context = context;
            }

            public async Task<Result<List<MultiPaymentItemReportRecord>>> Handle(Query request, CancellationToken cancellationToken)
            {
                var query = from item in _context.WorkEfforts.AsNoTracking()
                            join parent in _context.WorkEfforts.AsNoTracking()
                                on item.WorkEffortParentId equals parent.WorkEffortId
                            
                            join project in _context.WorkEfforts.AsNoTracking()
                                on item.ProjectId equals project.WorkEffortId into projects
                            from project in projects.DefaultIfEmpty()

                            join subProject in _context.WorkEfforts.AsNoTracking()
                                on item.SubProjectId equals subProject.WorkEffortId into subProjects
                            from subProject in subProjects.DefaultIfEmpty()

                            join service in _context.Products.AsNoTracking()
                                on item.ServiceId equals service.ProductId into services
                            from service in services.DefaultIfEmpty()

                            join product in _context.Products.AsNoTracking()
                                on item.ProductId equals product.ProductId into products
                            from product in products.DefaultIfEmpty()

                            join supplier in _context.Parties.AsNoTracking()
                                on item.PartyIdSupplier equals supplier.PartyId into suppliers
                            from supplier in suppliers.DefaultIfEmpty()

                            join contractor in _context.Parties.AsNoTracking()
                                on item.PartyIdContractor equals contractor.PartyId into contractors
                            from contractor in contractors.DefaultIfEmpty()

                            join costCenter in _context.CostCenters.AsNoTracking()
                                on item.CostCenterId equals costCenter.CostCenterId into costCenters
                            from costCenter in costCenters.DefaultIfEmpty()

                            join glAccount in _context.GlAccounts.AsNoTracking()
                                on item.GlAccountId equals glAccount.GlAccountId into glAccounts
                            from glAccount in glAccounts.DefaultIfEmpty()

                            where item.WorkEffortTypeId == "PAYMENT_CERTIFICATE_ITEM"
                                  && parent.WorkEffortTypeId == "PAYMENT_CERTIFICATE"
                                  && parent.EstimatedStartDate >= request.StartDate
                                  && parent.EstimatedStartDate <= request.EndDate
                                  && (string.IsNullOrEmpty(request.GlAccountId) || parent.GlAccountId == request.GlAccountId)

                            select new MultiPaymentItemReportRecord
                            {
                                WorkEffortId = item.WorkEffortId,
                                GlAccountId = item.GlAccountId,
                                GlAccountName = glAccount != null ? glAccount.AccountNameArabic : null,
                                ItemType = item.CostType,
                                ServiceId = item.ServiceId,
                                ServiceName = service != null ? service.ProductName : "",
                                ProductId = item.ProductId,
                                ProductName = product != null ? product.ProductName : "",
                                Description = item.Description,
                                Amount = (decimal?)item.Amount,
                                Discount = (decimal?)item.Discount,
                                TransportationExpenses = (decimal?)item.TransportationExpenses,
                                Gratuities = (decimal?)item.Gratuities,
                                Total = (decimal?)item.TotalAmount,
                                EstimatedStartDate = item.EstimatedStartDate,
                                PartyIdSupplier = item.PartyIdSupplier,
                                PartyIdSupplierName = supplier != null ? supplier.Description : "",
                                PartyIdContractor = item.PartyIdContractor,
                                PartyIdContractorName = contractor != null ? contractor.Description : "",
                                CostCenterId = item.CostCenterId,
                                CostCenterName = costCenter != null ? costCenter.Description : "",
                                ProjectId = item.ProjectId,
                                ProjectName = project != null ? project.ProjectName : "",
                                SubProjectId = item.SubProjectId,
                                SubProjectName = subProject != null ? subProject.SubProjectName : "",
                                ParentWorkEffortId = parent.WorkEffortId,
                                ParentDescription = parent.Description,
                                CertificateDate = parent.EstimatedStartDate
                            };

                var results = await query.ToListAsync(cancellationToken);

                var itemTypeDescriptions = new Dictionary<string, string>
                {
                    { "MATERIALS", "المواد" },
                    { "LABOR", "العمالة" },
                    { "EQUIPMENT", "المعدات" },
                    { "EXPENSES", "المصروفات" }
                };

                foreach (var item in results)
                {
                    item.ItemTypeDescription = itemTypeDescriptions.GetValueOrDefault(item.ItemType ?? "", "");
                }

                return Result<List<MultiPaymentItemReportRecord>>.Success(results);
            }
        }
    }
}
