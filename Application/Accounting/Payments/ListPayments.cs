using Application.Interfaces;
using Application.Order.Orders;
using AutoMapper;
using MediatR;
using Microsoft.AspNetCore.OData.Query;
using Persistence;

namespace Application.Accounting.Payments;

public class ListPayments
{
    public class Query : IRequest<IQueryable<PaymentRecord>>
    {
        public ODataQueryOptions<PaymentRecord> Options { get; set; }
        public string Language { get; set; }
        public string? PaymentType { get; set; }
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
            var language = request.Language;
            var query = (from pyt in _context.Payments
                join ptt in _context.PaymentTypes on pyt.PaymentTypeId equals ptt.PaymentTypeId
                join sts in _context.StatusItems on pyt.StatusId equals sts.StatusId
                join pty in _context.Parties on pyt.PartyIdFrom equals pty.PartyId
                join ptyto in _context.Parties on pyt.PartyIdTo equals ptyto.PartyId
                join pmt in _context.PaymentMethodTypes on pyt.PaymentMethodTypeId equals pmt.PaymentMethodTypeId
                join cc in _context.CreditCards on pyt.PaymentMethodId equals cc.PaymentMethodId into creditCardJoin
                from cc in creditCardJoin.DefaultIfEmpty()
                join opp in _context.OrderPaymentPreferences on pyt.PaymentPreferenceId equals opp.OrderPaymentPreferenceId into orderPaymentJoin
                from opp in orderPaymentJoin.DefaultIfEmpty()
                join ord in _context.OrderHeaders on opp.OrderId equals ord.OrderId into orderJoin
                from ord in orderJoin.DefaultIfEmpty()
                // REFACTOR: Added left outer join with WorkEfforts to retrieve certificateNumber
                // This join uses RelatedOrderId to link with OrderId, ensuring optional matching
                // Left outer join ensures payments without associated WorkEfforts are still included
                join we in _context.WorkEfforts on ord.OrderId equals we.RelatedOrderId into workEffortJoin
                from we in workEffortJoin.DefaultIfEmpty()
                select new PaymentRecord
                {
                    PaymentId = pyt.PaymentId,
                    PaymentTypeId = pyt.PaymentTypeId,
                    PaymentTypeDescription = language == "ar" ? ptt.DescriptionArabic : ptt.Description,
                    PaymentMethodId = pyt.PaymentMethodId,
                    PaymentMethodTypeId = pyt.PaymentMethodTypeId,
                    PaymentMethodTypeDescription = language == "ar" ? pmt.DescriptionArabic : pmt.Description,
                    PartyIdFrom = pyt.PartyIdFrom,
                    PartyIdFromName = pty.Description,
                    PartyIdTo = pyt.PartyIdTo,
                    PartyIdToName = ptyto.Description,
                    StatusId = pyt.StatusId,
                    StatusDescription = language == "ar" ? sts.DescriptionArabic : sts.Description,
                    StatusDescriptionEnglish = sts.Description,
                    EffectiveDate = (DateTime)pyt.EffectiveDate,
                    Comments = pyt.Comments,
                    PaymentRefNum = pyt.PaymentRefNum,
                    PaymentPreferenceId = pyt.PaymentPreferenceId,
                    ActualCurrencyAmount = pyt.ActualCurrencyAmount,
                    OverrideGlAccountId = pyt.OverrideGlAccountId,
                    OrganizationPartyId = ptt.ParentTypeId == "DISBURSEMENT" ? pyt.PartyIdFrom : pyt.PartyIdTo,
                    Amount = pyt.Amount,
                    CurrencyUomId = pyt.CurrencyUomId,
                    FinAccountTransId = pyt.FinAccountTransId,
                    CreditCardNumber = cc != null ? cc.CardNumber : null,
                    CreditCardExpiryDate = cc != null ? cc.ExpireDate : null,
                    FromPartyId = new OrderPartyDto
                    {
                        FromPartyId = pty.PartyId,
                        FromPartyName = pty.Description ?? string.Empty
                    },
                    IsDisbursement = ptt.ParentTypeId == "DISBURSEMENT",
                    OrderId = ord != null ? ord.OrderId : null,
                    // REFACTOR: Added CertificateNumber from WorkEfforts
                    // This field is included to provide the certificate number associated with the order, if available
                    CertificateNumber = we != null ? we.CertificateNumber : null
                }).AsQueryable();

            // REFACTOR: Filter query based on PaymentType (incoming or outgoing)
            if (!string.IsNullOrEmpty(request.PaymentType))
            {
                bool isDisbursement = request.PaymentType.ToLower() == "outgoing";
                query = query.Where(p => p.IsDisbursement == isDisbursement);
            }

            return await Task.FromResult(query);
        }
    }
}