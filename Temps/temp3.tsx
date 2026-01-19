if (t.isSalesInvoice) {
    dain = t.grossAmount;           // دائن = customer owes more
    periodChange = +t.grossAmount;  // balance increases (he owes us)
}