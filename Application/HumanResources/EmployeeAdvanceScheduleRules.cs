using Domain;
using Microsoft.EntityFrameworkCore;
using Persistence;

namespace Application.HumanResources;

/// <summary>
/// Rules shared by Create/UpdateEmployeeAdvance for long-term deduction plans.
/// A pending installment can only be collected by a payroll run that has not happened yet, and runs
/// move forward month by month — so its month must be strictly after the employee's latest payroll
/// invoice, after every already-deducted installment, and not before the advance was disbursed.
/// </summary>
public static class EmployeeAdvanceScheduleRules
{
    /// <summary>
    /// A row already deducted by a payroll run. BatchCreatePayrollInvoices stamps PayrolInvoiceId +
    /// "PAID"; the invoice READY hook (ApplyPayrollDeductionsToAdvances) may stamp "SCHED_PAID".
    /// </summary>
    public static bool IsProcessed(EmployeeAdvanceSchedule s) =>
        s.PayrolInvoiceId != null || s.StatusId is "PAID" or "SCHED_PAID";

    public static DateOnly MonthKey(DateOnly d) => new(d.Year, d.Month, 1);

    /// <summary>
    /// First month (as a month-start date) a pending installment may fall in for this employee.
    /// </summary>
    public static async Task<DateOnly> EarliestSchedulableMonthAsync(DataContext context, string employeeId,
        DateOnly? advanceDate, IEnumerable<EmployeeAdvanceSchedule> processedSchedules, CancellationToken ct)
    {
        // Not before the month the advance was handed out.
        var earliest = advanceDate.HasValue ? MonthKey(advanceDate.Value) : DateOnly.MinValue;

        // Strictly after the employee's latest payroll run: that month and everything before it are
        // already settled, and the run only deducts rows dated in its own month.
        var lastPayrollDate = await context.Invoices
            .Where(i => i.InvoiceTypeId == "PAYROL_INVOICE" && i.PartyIdFrom == employeeId && i.InvoiceDate != null)
            .MaxAsync(i => i.InvoiceDate, ct);
        if (lastPayrollDate.HasValue)
        {
            var afterLastRun = MonthKey(lastPayrollDate.Value).AddMonths(1);
            if (afterLastRun > earliest) earliest = afterLastRun;
        }

        // Strictly after the last deducted installment (normally implied by the run above).
        var lastProcessed = processedSchedules
            .Where(s => s.DueDate.HasValue)
            .Select(s => s.DueDate!.Value)
            .DefaultIfEmpty(DateOnly.MinValue)
            .Max();
        if (lastProcessed != DateOnly.MinValue)
        {
            var afterLastProcessed = MonthKey(lastProcessed).AddMonths(1);
            if (afterLastProcessed > earliest) earliest = afterLastProcessed;
        }

        return earliest;
    }

    public static string MonthNotSchedulableMessage(DateOnly dueDate, DateOnly earliestMonth) =>
        $"لا يمكن جدولة قسط في شهر {dueDate:MM/yyyy}: أقرب شهر يمكن الخصم فيه هو {earliestMonth:MM/yyyy} " +
        "(بعد تاريخ صرف السلفة وبعد آخر شهر تمت فيه معالجة راتب الموظف).";
}
