using Application.Order.Orders;
using MediatR;
using Microsoft.AspNetCore.OData.Query;
using Persistence;

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


            var query = (
                from pyt in _context.Payments

                // Required joins (inner – these should always exist)
                join ptt in _context.PaymentTypes
                    on pyt.PaymentTypeId equals ptt.PaymentTypeId
                join sts in _context.StatusItems
                    on pyt.StatusId equals sts.StatusId
                join pty in _context.Parties
                    on pyt.PartyIdFrom equals pty.PartyId

                // REFACTOR: LEFT JOIN – PaymentMethodTypeId is often NULL
                join pmt in _context.PaymentMethodTypes
                    on pyt.PaymentMethodTypeId equals pmt.PaymentMethodTypeId into pmtJoin
                from pmt in pmtJoin.DefaultIfEmpty()

                // REFACTOR: LEFT JOIN – PartyIdTo may be "Company" or missing
                join ptyto in _context.Parties
                    on pyt.PartyIdTo equals ptyto.PartyId into ptytoJoin
                from ptyto in ptytoJoin.DefaultIfEmpty()
                join opp in _context.OrderPaymentPreferences
                    on pyt.PaymentPreferenceId equals opp.OrderPaymentPreferenceId into oppJoin
                from opp in oppJoin.DefaultIfEmpty()
                join ord in _context.OrderHeaders
                    on opp.OrderId equals ord.OrderId into ordJoin
                from ord in ordJoin.DefaultIfEmpty()
                join we in _context.WorkEfforts
                    on ord.OrderId equals we.RelatedOrderId into weJoin
                from we in weJoin.DefaultIfEmpty()

                // REFACTOR: LEFT JOIN for CostCenter – assuming you have a CostCenter entity/table
                join cc in _context.CostCenters on pyt.CostCenterId equals cc.CostCenterId into ccJoin
                from cc in ccJoin.DefaultIfEmpty()

                // REFACTOR: LEFT JOIN for Project (WorkEffort) – to get projectId & project name
                join proj in _context.WorkEfforts on pyt.WorkEffortId equals proj.WorkEffortId into projJoin
                from proj in projJoin.DefaultIfEmpty()
                select new PaymentRecord
                {
                    PaymentId = pyt.PaymentId,
                    PaymentTypeId = pyt.PaymentTypeId,
                    PaymentTypeDescription = isArabic ? ptt.DescriptionArabic : ptt.Description,

                    PaymentMethodId = pyt.PaymentMethodId,
                    PaymentMethodTypeId = pyt.PaymentMethodTypeId,
                    PaymentMethodTypeDescription = pmt != null
                        ? (isArabic ? pmt.DescriptionArabic : pmt.Description)
                        : null,

                    PartyIdFrom = pyt.PartyIdFrom,
                    PartyIdFromName = pty.Description ?? string.Empty,

                    PartyIdTo = pyt.PartyIdTo,
                    PartyIdToName = ptyto != null
                        ? ptyto.Description
                        : (pyt.PartyIdTo == "Company" ? "Company" : pyt.PartyIdTo ?? "Unknown"),

                    StatusId = pyt.StatusId,
                    StatusDescription = isArabic ? sts.DescriptionArabic : sts.Description,
                    StatusDescriptionEnglish = sts.Description,

                    EffectiveDate = (DateTime)pyt.EffectiveDate,
                    Comments = pyt.Comments,
                    PaymentRefNum = pyt.PaymentRefNum,
                    PaymentPreferenceId = pyt.PaymentPreferenceId,
                    IsBankTransfer = pyt.IsBankTransfer,
                    Amount = pyt.Amount,
                    ActualCurrencyAmount = pyt.ActualCurrencyAmount ?? pyt.Amount,
                    CurrencyUomId = pyt.CurrencyUomId ?? "EGP",

                    FinAccountTransId = pyt.FinAccountTransId,
                    OverrideGlAccountId = pyt.OverrideGlAccountId,


                    FromPartyId = new OrderPartyDto
                    {
                        FromPartyId = pty.PartyId,
                        FromPartyName = pty.Description ?? string.Empty
                    },

                    IsDisbursement = ptt.ParentTypeId == "DISBURSEMENT",
                    OrganizationPartyId = ptt.ParentTypeId == "DISBURSEMENT" ? pyt.PartyIdFrom : pyt.PartyIdTo,

                    OrderId = ord != null ? ord.OrderId : null,
                    CertificateNumber = we != null ? we.CertificateNumber : null,

                    ChequeNumber = pyt.ChequeNumber,
                    ChequeDate = pyt.ChequeDate,
                    ProjectId = pyt.WorkEffortId,
                    ProjectName = proj != null ? proj.ProjectName : null,
                    CostCenterId = pyt.CostCenterId,
                    SalesRequestId = pyt.SalesRequestId,
                    CostCenterDescription = cc.Description,
                    CreatedStamp = (DateTime)pyt.CreatedStamp,
                }
            ).AsQueryable();

            // REFACTOR: Filter incoming vs outgoing payments
            if (!string.IsNullOrEmpty(request.PaymentType))
            {
                var isOutgoing = request.PaymentType.ToLower() == "outgoing";
                query = query.Where(p => p.IsDisbursement == isOutgoing);
            }

            // Apply OData $filter, $orderby, $skip, $top etc.
            if (request.Options.Filter != null)
                query = request.Options.Filter.ApplyTo(query, new ODataQuerySettings()) as IQueryable<PaymentRecord>;

            if (request.Options.OrderBy != null)
                query = request.Options.OrderBy.ApplyTo(query, new ODataQuerySettings()) as IQueryable<PaymentRecord>;

            // Note: Skip/Take should be applied by the controller if needed
            return await Task.FromResult(query);
        }
    }
}