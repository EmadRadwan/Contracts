                // 6. Create single accounting transaction for the full apartment sale amount
                // Following your exact OFBiz-style pattern (no balance check, manual seq, etc.)
                var companyPartyId = await _productStoreService.GetProductStorePayToPartId();

                // Reuse the same service you already inject/instantiate elsewhere
                // Assuming you have IAcctgTransService available — if not, add it to ctor
                var acctgTransParams = new CreateAcctgTransParams
                {
                    AcctgTransTypeId = "SALES_INVOICE",      // or "APARTMENT_SALE" if you add it later
                    TransactionDate = sr.SaleDate ?? DateTime.UtcNow.Date,
                    IsPosted = "Y",
                    Description = $"Apartment Sale - SR {sr.SalesRequestId} - {apartment.ApartmentName}",
                    GlFiscalTypeId = "ACTUAL"
                };

                var acctgTransId = await _acctgTransService.CreateAcctgTrans(acctgTransParams);

                var stamp = DateTime.UtcNow;
                var seq = 0;

                // Debit: Accounts Receivable - Customer owes full amount
                var debitEntry = new AcctgTransEntry
                {
                    AcctgTransId = acctgTransId,
                    AcctgTransEntrySeqId = (++seq).ToString().PadLeft(3, '0'), // "001"
                    GlAccountId = "121100",               // AR - Customers
                    DebitCreditFlag = "D",
                    AcctgTransEntryTypeId = "_NA_",
                    Amount = totalPrice,
                    ReconcileStatusId = "AES_NOT_RECONCILED",
                    Description = $"Apartment sale receivable - {apartment.ApartmentName}",
                    OrganizationPartyId = companyPartyId,
                    CreatedStamp = stamp,
                    LastUpdatedStamp = stamp
                };
                await _acctgTransService.CreateAcctgTransEntry(debitEntry);

                // Credit: Revenue from Apartment Sales
                var creditEntry = new AcctgTransEntry
                {
                    AcctgTransId = acctgTransId,
                    AcctgTransEntrySeqId = (++seq).ToString().PadLeft(3, '0'), // "002"
                    GlAccountId = "250120",               // Revenue - Apartment Sales
                    DebitCreditFlag = "C",
                    AcctgTransEntryTypeId = "_NA_",
                    Amount = totalPrice,
                    ReconcileStatusId = "AES_NOT_RECONCILED",
                    Description = $"Apartment sale revenue - {apartment.ApartmentName}",
                    OrganizationPartyId = companyPartyId,
                    CreatedStamp = stamp,
                    LastUpdatedStamp = stamp
                };
                await _acctgTransService.CreateAcctgTransEntry(creditEntry);

                private readonly IAcctgTransService _acctgTransService;

                public Handler(
                    DataContext context,
                    IProductStoreService productStoreService,
                    IPaymentHelperService paymentHelperService,
                    IAcctgTransService acctgTransService)   // ← add this
                {
                    _context = context;
                    _productStoreService = productStoreService;
                    _paymentHelperService = paymentHelperService;
                    _acctgTransService = acctgTransService;  // ← add this
                }