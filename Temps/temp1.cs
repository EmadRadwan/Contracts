var monthStart = new DateTime(request.InvoiceDate.Year, request.InvoiceDate.Month, 1);

// First moment of next month minus 1 tick = last possible moment of current month
var monthEnd = monthStart.AddMonths(1).AddTicks(-1);