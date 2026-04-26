SELECT
    dp.PaymentId,
    dp.ProjectName,
    dp.Amount AS DirectPaymentAmount,
    exp.CertificateNumber,
    exp.NetCertifiedAmount AS ExpenseAmount,
    dp.PartyIdFromName AS Payee,
    dp.EFFECTIVE_DATE
FROM Fact_Project_DirectPayments_2 dp
         INNER JOIN Fact_Project_Expenses exp
                    ON dp.PaymentId = exp.PaymentId
ORDER BY dp.PaymentId;

SELECT
    dp.PaymentId,
    dp.ProjectName,
    dp.Amount AS DirectPaymentAmount,
    exp.CertificateNumber,
    exp.NetCertifiedAmount AS ExpenseAmount,
    dp.PartyIdFromName AS Payee,
    dp.EFFECTIVE_DATE
FROM erp_contracts.Fact_Project_OperatingExpenses_2 dp
         INNER JOIN Fact_Project_Expenses exp
                    ON dp.PaymentId = exp.PaymentId
ORDER BY dp.PaymentId;