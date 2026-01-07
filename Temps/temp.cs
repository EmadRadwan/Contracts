decimal transferAmount = financialSummary.TotalSalesInvoice
                         - financialSummary.TotalPurchaseInvoice
                         - financialSummary.TotalPaymentsIn
                         + financialSummary.TotalPaymentsOut
                         + openingBalanceImpact
                         + rentalPropertyImpact;