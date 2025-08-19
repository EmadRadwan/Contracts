using AutoMapper;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Persistence;

namespace Application.Shipments.OrganizationGlSettings;

public class GetPartyAccountingPreferences
{
    public class Query : IRequest<Result<PartyAcctgPreferenceDto>>
    {
        public string CompanyId { get; set; }
    }

    public class Handler : IRequestHandler<Query, Result<PartyAcctgPreferenceDto>>
    {
        private readonly DataContext _context;
        private readonly IMapper _mapper;

        public Handler(DataContext context, IMapper mapper)
        {
            _mapper = mapper;
            _context = context;
        }

        public async Task<Result<PartyAcctgPreferenceDto>> Handle(Query request, CancellationToken cancellationToken)
        {
            var partyAccountingPreference = await (from pap in _context.PartyAcctgPreferences
                where pap.PartyId == request.CompanyId
                select new PartyAcctgPreferenceDto
                {
                    PartyId = pap.PartyId,
                    FiscalYearStartMonth = pap.FiscalYearStartMonth,
                    FiscalYearStartDay = pap.FiscalYearStartDay,
                    TaxFormId = pap.TaxFormId,
                    CogsMethodId = pap.CogsMethodId,
                    BaseCurrencyUomId = pap.BaseCurrencyUomId,
                    InvoiceSeqCustMethId = pap.InvoiceSeqCustMethId,
                    InvoiceIdPrefix = pap.InvoiceIdPrefix,
                    LastInvoiceNumber = pap.LastInvoiceNumber,
                    LastInvoiceRestartDate = pap.LastInvoiceRestartDate,
                    UseInvoiceIdForReturns = pap.UseInvoiceIdForReturns,
                    QuoteSeqCustMethId = pap.QuoteSeqCustMethId,
                    QuoteIdPrefix = pap.QuoteIdPrefix,
                    LastQuoteNumber = pap.LastQuoteNumber,
                    OrderSeqCustMethId = pap.OrderSeqCustMethId,
                    OrderIdPrefix = pap.OrderIdPrefix,
                    LastOrderNumber = pap.LastOrderNumber,
                    RefundPaymentMethodId = pap.RefundPaymentMethodId,
                    ErrorGlJournalId = pap.ErrorGlJournalId,
                    EnableAccounting = pap.EnableAccounting,
                    InvoiceSequenceEnumId = pap.InvoiceSequenceEnumId,
                    OrderSequenceEnumId = pap.OrderSequenceEnumId,
                    QuoteSequenceEnumId = pap.QuoteSequenceEnumId,
                    LastUpdatedStamp = pap.LastUpdatedStamp,
                    LastUpdatedTxStamp = pap.LastUpdatedTxStamp
                }).FirstOrDefaultAsync(cancellationToken);

            return Result<PartyAcctgPreferenceDto>.Success(partyAccountingPreference!);
        }
    }
}