using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.AspNetCore.OData.Query;
using Microsoft.EntityFrameworkCore;
using Persistence;
using Application.Accounting.Payments;   // assuming PaymentRecord & OrderPartyDto live here
using Application.Order.Orders;           // if needed for OrderPartyDto or similar

namespace Application.Accounting.Payments;

public class ListPayments
{
    public class Query : IRequest<IQueryable<PaymentRecord>>
    {
        public ODataQueryOptions<PaymentRecord> Options { get; set; } = null!;
        public string Language { get; set; } = "en";
        public string? PaymentType { get; set; } // "incoming" or "outgoing"
    }

    public class Handler : IRequestHandler<Query, IQueryable<PaymentRecord>>
    {
        private readonly DataContext _context;

        public Handler(DataContext context)
        {
            _context = context;
        }

        public async Task<IQueryable<PaymentRecord>> Handle(Query request, CancellationToken cancellationToken)
        {
            var language = request.Language?.ToLower() ?? "en";
            var isArabic = language == "ar";

            // 1. Base query with all joins – intermediate flat shape
            var baseQuery = from pyt in _context.Payments
                            join ptt in _context.PaymentTypes on pyt.PaymentTypeId equals ptt.PaymentTypeId
                            join sts in _context.StatusItems on pyt.StatusId equals sts.StatusId
                            join pty in _context.Parties on pyt.PartyIdFrom equals pty.PartyId

                            join pmt in _context.PaymentMethodTypes on pyt.PaymentMethodTypeId equals pmt.PaymentMethodTypeId into pmtJoin
                            from pmt in pmtJoin.DefaultIfEmpty()

                            join ptyto in _context.Parties on pyt.PartyIdTo equals ptyto.PartyId into ptytoJoin
                            from ptyto in ptytoJoin.DefaultIfEmpty()

                            join opp in _context.OrderPaymentPreferences on pyt.PaymentPreferenceId equals opp.OrderPaymentPreferenceId into oppJoin
                            from opp in oppJoin.DefaultIfEmpty()

                            join ord in _context.OrderHeaders on opp.OrderId equals ord.OrderId into ordJoin
                            from ord in ordJoin.DefaultIfEmpty()

                            join we in _context.WorkEfforts on ord.OrderId equals we.RelatedOrderId into weJoin
                            from we in weJoin.DefaultIfEmpty()

                            join cc in _context.CostCenters on pyt.CostCenterId equals cc.CostCenterId into ccJoin
                            from cc in ccJoin.DefaultIfEmpty()

                            join proj in _context.WorkEfforts on pyt.WorkEffortId equals proj.WorkEffortId into projJoin
                            from proj in projJoin.DefaultIfEmpty()

                            // NEW: SalesRequest → Product left joins
                            join sr in _context.SalesRequests on pyt.SalesRequestId equals sr.SalesRequestId into srJoin
                            from sr in srJoin.DefaultIfEmpty()

                            join prod in _context.Products on sr.ProductId equals prod.ProductId into prodJoin
                            from prod in prodJoin.DefaultIfEmpty()

                            select new
                            {
                                Payment = pyt,
                                PaymentType = ptt,
                                Status = sts,
                                PartyFrom = pty,
                                PartyTo = ptyto,
                                PaymentMethod = pmt,
                                OrderPref = opp,
                                Order = ord,
                                WorkEffortRelated = we,
                                CostCenter = cc,
                                Project = proj,
                                SalesRequest = sr,
                                Product = prod
                            };

            // 2. Apply outgoing/incoming filter
            if (!string.IsNullOrEmpty(request.PaymentType))
            {
                var isOutgoing = request.PaymentType.ToLower() == "outgoing";
                baseQuery = baseQuery.Where(x => x.PaymentType.ParentTypeId == "DISBURSEMENT" == isOutgoing);
            }

            // 3. Apply OData $filter and $orderby on the intermediate shape (much more translatable)
            IQueryable<object> filteredAndOrdered = baseQuery;

            if (request.Options.Filter != null)
            {
                filteredAndOrdered = request.Options.Filter.ApplyTo(filteredAndOrdered, new ODataQuerySettings());
            }

            if (request.Options.OrderBy != null)
            {
                filteredAndOrdered = request.Options.OrderBy.ApplyTo(filteredAndOrdered, new ODataQuerySettings());
            }

            // 4. Final projection to PaymentRecord (heavy logic happens after ordering/filtering)
            var finalQuery = filteredAndOrdered.Select(x => new PaymentRecord
            {
                PaymentId = x.Payment.PaymentId,
                PaymentTypeId = x.Payment.PaymentTypeId,
                PaymentTypeDescription = isArabic ? x.PaymentType.DescriptionArabic : x.PaymentType.Description,

                PaymentMethodId = x.Payment.PaymentMethodId,
                PaymentMethodTypeId = x.Payment.PaymentMethodTypeId,
                PaymentMethodTypeDescription = x.PaymentMethod != null
                    ? (isArabic ? x.PaymentMethod.DescriptionArabic : x.PaymentMethod.Description)
                    : null,

                PartyIdFrom = x.Payment.PartyIdFrom,
                PartyIdFromName = x.PartyFrom.Description ?? string.Empty,

                PartyIdTo = x.Payment.PartyIdTo,
                PartyIdToName = x.PartyTo != null
                    ? x.PartyTo.Description
                    : (x.Payment.PartyIdTo == "Company" ? "Company" : x.Payment.PartyIdTo ?? "Unknown"),

                StatusId = x.Payment.StatusId,
                StatusDescription = isArabic ? x.Status.DescriptionArabic : x.Status.Description,
                StatusDescriptionEnglish = x.Status.Description,

                EffectiveDate = (DateTime)x.Payment.EffectiveDate,
                Comments = x.Payment.Comments,
                PaymentRefNum = x.Payment.PaymentRefNum,
                PaymentPreferenceId = x.Payment.PaymentPreferenceId,
                IsBankTransfer = x.Payment.IsBankTransfer,
                Amount = x.Payment.Amount,
                ActualCurrencyAmount = x.Payment.ActualCurrencyAmount ?? x.Payment.Amount,
                CurrencyUomId = x.Payment.CurrencyUomId ?? "EGP",

                FinAccountTransId = x.Payment.FinAccountTransId,
                OverrideGlAccountId = x.Payment.OverrideGlAccountId,

                FromPartyId = new OrderPartyDto
                {
                    FromPartyId = x.PartyFrom.PartyId,
                    FromPartyName = x.PartyFrom.Description ?? string.Empty
                },

                IsDisbursement = x.PaymentType.ParentTypeId == "DISBURSEMENT",
                OrganizationPartyId = x.PaymentType.ParentTypeId == "DISBURSEMENT"
                    ? x.Payment.PartyIdFrom
                    : x.Payment.PartyIdTo,

                OrderId = x.Order != null ? x.Order.OrderId : null,
                CertificateNumber = x.WorkEffortRelated != null ? x.WorkEffortRelated.CertificateNumber : null,

                ChequeNumber = x.Payment.ChequeNumber,
                ChequeDate = x.Payment.ChequeDate,
                ProjectId = x.Payment.WorkEffortId,
                ProjectName = x.Project != null ? x.Project.ProjectName : null,
                CostCenterId = x.Payment.CostCenterId,
                SalesRequestId = x.Payment.SalesRequestId,
                CostCenterDescription = x.CostCenter != null ? x.CostCenter.Description : null,

                // NEW fields from Product
                ProductId = x.Product != null ? x.Product.ProductId : null,
                BuildingNumber = x.Product != null ? x.Product.BuildingNumber : null,

                CreatedStamp = (DateTime)x.Payment.CreatedStamp
            }).AsQueryable();

            return await Task.FromResult(finalQuery);
        }
    }
}