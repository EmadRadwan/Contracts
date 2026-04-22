public async Task<IQueryable<PaymentRecord>> Handle(Query request, CancellationToken cancellationToken)
{
    var language = request.Language?.ToLower() ?? "en";
    var isArabic = language == "ar";

    var query = (from pyt in _context.Payments
            join ptt in _context.PaymentTypes on pyt.PaymentTypeId equals ptt.PaymentTypeId
            join sts in _context.StatusItems on pyt.StatusId equals sts.StatusId
            join pty in _context.Parties on pyt.PartyIdFrom equals pty.PartyId
            
            // Existing Join
            join pmt in _context.PaymentMethodTypes on pyt.PaymentMethodTypeId equals pmt.PaymentMethodTypeId
                into pmtJoin from pmt in pmtJoin.DefaultIfEmpty()

            // 1. New Left Join: PaymentMethods
            join pm in _context.PaymentMethods on pyt.PaymentMethodId equals pm.PaymentMethodId 
                into pmJoin from pm in pmJoin.DefaultIfEmpty()

            // 2. New Left Join: GlAccounts (using OverrideGlAccountId)
            join gl in _context.GlAccounts on pyt.OverrideGlAccountId equals gl.GlAccountId 
                into glJoin from gl in glJoin.DefaultIfEmpty()

            // ... (other existing joins) ...
            join ptyto in _context.Parties on pyt.PartyIdTo equals ptyto.PartyId into ptytoJoin
            from ptyto in ptytoJoin.DefaultIfEmpty()
            join opp in _context.OrderPaymentPreferences on pyt.PaymentPreferenceId equals opp.OrderPaymentPreferenceId into oppJoin
            from opp in oppJoin.DefaultIfEmpty()
            join ord in _context.OrderHeaders on opp.OrderId equals ord.OrderId into ordJoin
            from ord in ordJoin.DefaultIfEmpty()
            // ... (rest of joins) ...

            select new PaymentRecord
            {
                PaymentId = pyt.PaymentId,
                PaymentTypeId = pyt.PaymentTypeId,
                PaymentTypeDescription = isArabic ? ptt.DescriptionArabic : ptt.Description,
                
                // Using the new joins:
                PaymentMethodId = pyt.PaymentMethodId,
                PaymentMethodDescription = pm != null ? pm.Description : null, // From PaymentMethod
                
                OverrideGlAccountId = pyt.OverrideGlAccountId,
                AccountNameArabic = gl != null ? gl.AccountNameArabic : null, // From GlAccount

                PaymentMethodTypeId = pyt.PaymentMethodTypeId,
                PaymentMethodTypeDescription = pmt != null
                    ? (isArabic ? pmt.DescriptionArabic : pmt.Description)
                    : null,

                // ... (rest of the existing mapping) ...
                PartyIdFrom = pyt.PartyIdFrom,
                // ... (keep existing fields) ...
                Amount = pyt.Amount,
                CurrencyUomId = pyt.CurrencyUomId ?? "EGP"
            })
        .AsQueryable();

    // ... (rest of your logic for OData and post-processing) ...
}