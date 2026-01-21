decimal transferAmount = financialSummary.TotalSalesInvoice
                         - financialSummary.TotalPurchaseInvoice
                         - financialSummary.TotalPaymentsIn
                         + financialSummary.TotalPaymentsOut
                         + rentalPropertyDebitImpact
                         - partnerAccrualCreditImpact
                         + apartmentSaleImpact
                         + chequeIssuedImpact;               // ← add here