// REFACTOR: Change to LEFT JOIN for PaymentMethodTypes
// Purpose: Most payments have NULL PaymentMethodTypeId (cash, advance, etc.)
// Improvement: Prevents legitimate payments from being excluded entirely
var query = from pyt in _context.Payments
    join ptt in _context.PaymentTypes on pyt.PaymentTypeId equals ptt.PaymentTypeId
    join sts in _context.StatusItems on pyt.StatusId equals sts.StatusId
    join ptyFrom in _context.Parties on pyt.PartyIdFrom equals ptyFrom.PartyId
    join ptyTo in _context.Parties on pyt.PartyIdTo equals ptyTo.PartyId
    join pmt in _context.PaymentMethodTypes 
        on pyt.PaymentMethodTypeId equals pmt.PaymentMethodTypeId into pmtGroup
    from pmt in pmtGroup.DefaultIfEmpty()   // ← LEFT JOIN
    where pyt.CreatedStamp >= startOfDayEgypt
          && pyt.CreatedStamp < endOfDayEgypt
          && (isOutgoing ? ptt.ParentTypeId == "DISBURSEMENT" : ptt.ParentTypeId != "DISBURSEMENT")
    select new PaymentRecordDto
    {
        // ... other fields
        PaymentMethodTypeId = pyt.PaymentMethodTypeId ?? "",
        PaymentMethodTypeDescription = pmt != null 
            ? (request.Language == "ar" ? pmt.DescriptionArabic : pmt.Description)
            : "N/A",  // or "" or "Cash" or whatever makes sense
        // ...
    };