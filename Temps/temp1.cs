var query = from ate in _context.AcctgTransEntries
            join act in _context.AcctgTrans 
                on ate.AcctgTransId equals act.AcctgTransId
            join att in _context.AcctgTransTypes 
                on act.AcctgTransTypeId equals att.AcctgTransTypeId 
                into transTypes 
            from att in transTypes.DefaultIfEmpty()
            
            join p in _context.Parties 
                on act.PartyId equals p.PartyId 
                into parties 
            from p in parties.DefaultIfEmpty()
            
            join prod in _context.Products 
                on ate.ProductId equals prod.ProductId 
                into products 
            from prod in products.DefaultIfEmpty()
            
            join we in _context.WorkEfforts 
                on act.WorkEffortId equals we.WorkEffortId 
                into workEfforts 
            from we in workEfforts.DefaultIfEmpty()
            
            // ── Added left outer join to get the Project ──
            join project in _context.WorkEfforts 
                on we.ProjectId equals project.WorkEffortId 
                into projects 
            from project in projects.DefaultIfEmpty()
            
            where ate.OrganizationPartyId == request.OrganizationPartyId
               && ate.GlAccountId == request.GlAccountId
               && act.IsPosted == "Y"
               && act.GlFiscalTypeId == "ACTUAL"
            
            select new TransactionEntryDto
            {
                AcctgTransId          = ate.AcctgTransId,
                AcctgTransEntrySeqId  = ate.AcctgTransEntrySeqId,
                TransactionDate       = (DateTime)act.TransactionDate,
                AcctgTransTypeId      = act.AcctgTransTypeId ?? "Unknown",
                AcctgTransTypeDescription = att != null ? att.Description : (act.AcctgTransTypeId ?? "Unknown"),
                GlFiscalTypeId        = act.GlFiscalTypeId,
                InvoiceId             = act.InvoiceId,
                PaymentId             = act.PaymentId,
                WorkEffortId          = act.WorkEffortId,
                ShipmentId            = act.ShipmentId,
                PartyId               = act.PartyId,
                PartyName             = p != null ? p.Description : null,
                ProductId             = ate.ProductId,
                ProductName           = prod != null ? prod.ProductName : null,
                IsPosted              = act.IsPosted,
                PostedDate            = act.PostedDate,
                DebitCreditFlag       = ate.DebitCreditFlag,
                Amount                = (decimal)ate.Amount,
                Description           = act.Description,
                CurrencyUomId         = ate.CurrencyUomId,
                CertificateNumber     = we != null ? we.CertificateNumber : null,
                
                // New field – coming from the parent/project WorkEffort
                ProjectName           = project != null ? project.WorkEffortName : null   // ← or .Description, .Name, etc.
            };