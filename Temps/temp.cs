// REFACTOR: Changed the join logic to get the certificate number from the parent PROJECT_CERTIFICATE 
//           only once per order, avoiding duplication when multiple OrderItemBillings exist.
//           Now we group OrderItemBillings by InvoiceId and OrderId first, then join to WorkEffort once.
var invoiceWithOrder = from inv in _context.Invoices
                       join oib in _context.OrderItemBillings 
                           on inv.InvoiceId equals oib.InvoiceId into oibGroup
                       from oib in oibGroup.DefaultIfEmpty()
                       select new { inv, oib };

var groupedOrders = from x in invoiceWithOrder
                    group x by new { x.inv.InvoiceId, OrderId = x.oib != null ? x.oib.OrderId : (string)null } into g
                    select new
                    {
                        Invoice = g.First().inv,
                        OrderId = g.Key.OrderId
                    };

var query = from invData in groupedOrders
            join invt in _context.InvoiceTypes on invData.Invoice.InvoiceTypeId equals invt.InvoiceTypeId
            join fromParty in _context.Parties on invData.Invoice.PartyIdFrom equals fromParty.PartyId
            join toParty in _context.Parties on invData.Invoice.PartyId equals toParty.PartyId
            join sts in _context.StatusItems on invData.Invoice.StatusId equals sts.StatusId
            join bil in _context.BillingAccounts on invData.Invoice.BillingAccountId equals bil.BillingAccountId into billingGroup
            from bil in billingGroup.DefaultIfEmpty()

            // REFACTOR: Join to WorkEffort only once per OrderId to get the certificate number
            join we in _context.WorkEfforts
                on new { OrderId = invData.OrderId, WorkEffortTypeId = "PROJECT_CERTIFICATE" }
                equals new { OrderId = we.RelatedOrderId, we.WorkEffortTypeId } into weGroup
            from we in weGroup.DefaultIfEmpty()

            select new InvoiceRecord
            {
                InvoiceId = invData.Invoice.InvoiceId,
                InvoiceTypeDescription = request.Language == "ar" ? invt.DescriptionArabic : invt.Description,
                InvoiceDate = invData.Invoice.InvoiceDate,
                StatusId = invData.Invoice.StatusId,
                InvoiceTypeId = invData.Invoice.InvoiceTypeId,
                StatusDescription = request.Language == "ar" ? sts.DescriptionArabic : sts.Description,
                Description = invData.Invoice.Description,
                DueDate = invData.Invoice.DueDate,
                PaidDate = invData.Invoice.PaidDate,
                PartyId = new InvoicePartyDto
                {
                    FromPartyId = invData.Invoice.PartyId,
                    FromPartyName = toParty.Description
                },
                ToPartyName = toParty.Description,
                PartyIdFrom = new InvoicePartyDto
                {
                    FromPartyId = invData.Invoice.PartyIdFrom,
                    FromPartyName = fromParty.Description
                },
                FromPartyName = fromParty.Description,
                BillingAccountId = invData.Invoice.BillingAccountId,
                BillingAccountName = bil != null ? bil.Description : null,
                Total = _context.InvoiceItems
                    .Where(ii => ii.InvoiceId == invData.Invoice.InvoiceId)
                    .Sum(ii => ii.Quantity * ii.Amount),
                OutstandingAmount = 0,
                OrderId = invData.OrderId,
                CertificateNumber = we != null ? we.CertificateNumber : null
            };