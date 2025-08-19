using Application.Accounting.Services;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Persistence;

namespace Application.Accounting.BillingAccounts;

public class ListBillingAccountInvoices
{
    public class Query : IRequest<Result<ListBillingAccountInvoicesResult>>
    {
        public ListBillingAccountInvoicesRequest Parameters { get; set; }

        public Query(ListBillingAccountInvoicesRequest parameters)
        {
            Parameters = parameters;
        }
    }

    public class ListBillingAccountInvoicesHandler : IRequestHandler<ListBillingAccountInvoices.Query, Result<ListBillingAccountInvoicesResult>>
    {
        private readonly DataContext _context;
        private readonly IInvoiceUtilityService _invoiceUtilityService;

        public ListBillingAccountInvoicesHandler(DataContext context, IInvoiceUtilityService invoiceUtilityService)
        {
            _context = context;
            _invoiceUtilityService = invoiceUtilityService;
        }

        public async Task<Result<ListBillingAccountInvoicesResult>> Handle(ListBillingAccountInvoices.Query request, CancellationToken cancellationToken)
        {
            try
            {
                var parameters = request.Parameters;

                // Step 1: Retrieve Invoices based on BillingAccountId and optional StatusId
                var invoiceQuery = _context.Invoices
                    .Where(i => i.BillingAccountId == parameters.BillingAccountId);

                if (!string.IsNullOrEmpty(parameters.StatusId))
                {
                    invoiceQuery = invoiceQuery.Where(i => i.StatusId == parameters.StatusId);
                }

                // Include Party entities to get descriptions
                var invoices = await invoiceQuery
                    .ToListAsync(cancellationToken);

                var billingAccountInvoices = new List<BillingAccountInvoiceDto>();

                // Step 2: Process each invoice
                foreach (var invoice in invoices)
                {
                    // Step 2.1: Get Party Descriptions from Party.Description
                    var descriptionFrom = invoice.PartyIdFromNavigation?.Description ?? string.Empty;
                    var descriptionTo = invoice.Party?.Description ?? string.Empty;

                    // Step 2.2: Compute PaidInvoice, AmountToApply, and Total
                    var paidInvoice = await _invoiceUtilityService.GetInvoiceNotApplied(invoice.InvoiceId) == 0;

                    var invoiceNotApplied = await _invoiceUtilityService.GetInvoiceNotApplied(invoice.InvoiceId);
                    var amountToApply = invoiceNotApplied;

                    var invoiceTotal = await _invoiceUtilityService.GetInvoiceTotal(invoice.InvoiceId, true);
                    var total = invoiceTotal;
                    var invoiceType = await _context.InvoiceTypes.FindAsync(invoice.InvoiceTypeId);
                    var status = await _context.StatusItems.FindAsync(invoice.StatusId);

                    // Step 2.3: Create DTO
                    var dto = new BillingAccountInvoiceDto
                    {
                        BillingAccountId = invoice.BillingAccountId,
                        InvoiceId = invoice.InvoiceId,
                        InvoiceTypeId = invoice.InvoiceTypeId,
                        InvoiceTypeDescription = invoiceType.Description,
                        InvoiceDate = (DateTime)invoice.InvoiceDate,
                        StatusId = invoice.StatusId,
                        StatusDescription = status.Description,
                        Description = invoice.Description,

                        DescriptionFrom = descriptionFrom,
                        DescriptionTo = descriptionTo,

                        PaidInvoice = paidInvoice,
                        AmountToApply = amountToApply,
                        Total = total
                    };

                    billingAccountInvoices.Add(dto);
                }

                var result = new ListBillingAccountInvoicesResult
                {
                    BillingAccountInvoices = billingAccountInvoices
                };

                return Result<ListBillingAccountInvoicesResult>.Success(result);
            }
            catch (Exception ex)
            {
                return Result<ListBillingAccountInvoicesResult>.Failure($"Error listing billing account invoices: {ex.Message}");
            }
        }
    }
}