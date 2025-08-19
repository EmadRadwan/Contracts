using Application.Interfaces;
using AutoMapper;
using MediatR;
using Microsoft.AspNetCore.OData.Query;
using Microsoft.Extensions.Logging;
using Persistence;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Application.Accounting.Payments;

public class ListPaymentGroups
{
    public class Query : IRequest<IQueryable<PaymentGroupRecord>>
    {
        public ODataQueryOptions<PaymentGroupRecord> Options { get; set; }
        public string Language { get; set; }
    }
    

    public class Handler : IRequestHandler<Query, IQueryable<PaymentGroupRecord>>
    {
        private readonly DataContext _context;
        private readonly ILogger<Handler> _logger;

        public Handler(DataContext context, ILogger<Handler> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<IQueryable<PaymentGroupRecord>> Handle(Query request, CancellationToken cancellationToken)
        {
            var language = request.Language ?? "en"; // Default to English if language not specified

            var query = from pg in _context.PaymentGroups
                        join pgt in _context.PaymentGroupTypes on pg.PaymentGroupTypeId equals pgt.PaymentGroupTypeId
                        join pgm in _context.PaymentGroupMembers on pg.PaymentGroupId equals pgm.PaymentGroupId into paymentGroupMembers
                        from pgm in paymentGroupMembers.DefaultIfEmpty() // Left join for PaymentGroupMembers
                        join py in _context.Payments on pgm != null ? pgm.PaymentId : null equals py.PaymentId into payments
                        from py in payments.DefaultIfEmpty() // Left join for Payments
                        join fat in _context.FinAccountTrans on py != null ? py.FinAccountTransId : null equals fat.FinAccountTransId into finAccountTrans
                        from fat in finAccountTrans.DefaultIfEmpty() // Left join for FinAccountTrans
                        join fa in _context.FinAccounts on fat != null ? fat.FinAccountId : null equals fa.FinAccountId into finAccounts
                        from fa in finAccounts.DefaultIfEmpty() // Left join for FinAccount
                        select new PaymentGroupRecord
                        {
                            PaymentGroupId = pg.PaymentGroupId,
                            PaymentGroupTypeId = pg.PaymentGroupTypeId,
                            PaymentGroupTypeDescription = language == "ar" ? pgt.DescriptionArabic : pgt.Description,
                            PaymentGroupName = pg.PaymentGroupName,
                            FinAccountName = fa != null ? fa.FinAccountName : null,
                            OwnerPartyId = fa != null ? fa.OwnerPartyId : null,
                            CanGenerateDepositSlip = pg.PaymentGroupTypeId == "BATCH_PAYMENT" && pgm != null,
                            CanPrintCheck = pg.PaymentGroupTypeId == "CHECK_RUN" && pgm != null,
                            CanCancel = pg.PaymentGroupTypeId == "BATCH_PAYMENT" && pgm != null && fat != null && fat.StatusId != "FINACT_TRNS_APPROVED"
                        };

            _logger.LogInformation("Executing OData query for PaymentGroups with language: {Language}", language);

            return query.AsQueryable();
        }
    }
}