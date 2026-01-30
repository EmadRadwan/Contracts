// In select new PartyDto { ... }

join pgaLoan in _context.PartyGlAccounts on new { Org = "Company", P = prty.PartyId, R = "EMPLOYEE", T = "LOANS_RECEIVABLE" }
equals new { Org = pgaLoan.OrganizationPartyId, P = pgaLoan.PartyId, R = pgaLoan.RoleTypeId, T = pgaLoan.GlAccountTypeId } into pgaLoanGroup
from pgaLoan in pgaLoanGroup.DefaultIfEmpty()

    join glaLoan in _context.GlAccounts on pgaLoan.GlAccountId equals glaLoan.GlAccountId into glaLoanGroup
    from glaLoan in glaLoanGroup.DefaultIfEmpty()

    join pgaAccrued in _context.PartyGlAccounts on new { Org = "Company", P = prty.PartyId, R = "EMPLOYEE", T = "ACCRUED_EXPENSES" }
        equals new { Org = pgaAccrued.OrganizationPartyId, P = pgaAccrued.PartyId, R = pgaAccrued.RoleTypeId, T = pgaAccrued.GlAccountTypeId } into pgaAccruedGroup
    from pgaAccrued in pgaAccruedGroup.DefaultIfEmpty()

    join glaAccrued in _context.GlAccounts on pgaAccrued.GlAccountId equals glaAccrued.GlAccountId into glaAccruedGroup
    from glaAccrued in glaAccruedGroup.DefaultIfEmpty()

// Then map:
LoanGlAccountId = pgaLoan != null ? pgaLoan.GlAccountId : null,
LoanGlAccountName = glaLoan != null ? glaLoan.AccountName : null,
LoanGlAccountNameArabic = glaLoan != null ? glaLoan.AccountNameArabic : null,

AccruedGlAccountId = pgaAccrued != null ? pgaAccrued.GlAccountId : null,
AccruedGlAccountName = glaAccrued != null ? glaAccrued.AccountName : null,
AccruedGlAccountNameArabic = glaAccrued != null ? glaAccrued.AccountNameArabic : null,