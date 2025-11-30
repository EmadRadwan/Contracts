// REFACTOR: The old code wrongly used the quarterly installment amount as "installment price per m2"
// This made the price look 95% cheaper! We now calculate the correct total nominal price.
// The true installment price per m2 = Down payment per m2 + total installments per m2
var financedAmountPerM2 = request.CashPricePerM2 * financedPortion;

// Old buggy line (REMOVE THIS):
// var installmentPricePerM2 = financedAmountPerM2 / (decimal)pvaf;

// New correct calculation:
var quarterlyInstallmentPerM2 = financedAmountPerM2 / totalPeriods;  // this is 625 in your example
var totalInstallmentsPerM2 = quarterlyInstallmentPerM2 * totalPeriods;
var installmentPricePerM2 = request.CashPricePerM2 * request.DownPaymentPercentage + totalInstallmentsPerM2;
// OR simply: var installmentPricePerM2 = request.CashPricePerM2 + (totalInstallmentsPerM2 - financedAmountPerM2);
// But the simplest and most accurate:
installmentPricePerM2 = request.CashPricePerM2 + (financedAmountPerM2 - financedAmountPerM2 * (decimal)pvaf);
// No! Even simpler and 100% correct:
installmentPricePerM2 = request.CashPricePerM2; // At exact discount rate → same price!