// Application/Accounting/Invoices/ListAbsenceData.cs
using Application.Core;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Persistence;

namespace Application.Accounting.Invoices;

public class ListAbsenceData
{
    public class Query : IRequest<Result<List<AbsenceDataDto>>>
    {
        public DateOnly? FromDate { get; set; }
        public DateOnly? ToDate { get; set; }
        public string OrganizationPartyId { get; set; }
    }

    public class AbsenceDataDto
    {
        public int Serial { get; set; }
        public string EmployeeId { get; set; }
        public string EmployeeName { get; set; }
        public string JobTitle { get; set; }
        public decimal AbsenceDays { get; set; }
        public string FingerPrintAttendanceId { get; set; }
        public string Notes { get; set; } = "";
    }

    public class Handler : IRequestHandler<Query, Result<List<AbsenceDataDto>>>
    {
        private readonly DataContext _context;

        public Handler(DataContext context) => _context = context;

        public async Task<Result<List<AbsenceDataDto>>> Handle(Query request, CancellationToken cancellationToken)
        {
            var invoices = await (
                from inv in _context.Invoices
                where inv.InvoiceTypeId == "PAYROL_INVOICE"
                   && inv.InvoiceDate >= request.FromDate
                   && inv.InvoiceDate <= request.ToDate
                   && inv.PartyId == request.OrganizationPartyId

                join emp in _context.Parties on inv.PartyIdFrom equals emp.PartyId
                join posFul in _context.EmplPositionFulfillments.Where(f => f.ThruDate == null)
                    on emp.PartyId equals posFul.PartyId into pf
                from posFul in pf.DefaultIfEmpty()
                join pos in _context.EmplPositions on posFul.EmplPositionId equals pos.EmplPositionId into p
                from pos in p.DefaultIfEmpty()
                join posType in _context.EmplPositionTypes on pos.EmplPositionTypeId equals posType.EmplPositionTypeId into pt
                from posType in pt.DefaultIfEmpty()

                select new
                {
                    inv.InvoiceId,
                    EmployeeId = emp.PartyId,
                    EmployeeName = emp.Description,
                    JobTitle = posType != null ? posType.Description : "",
                    FingerPrintAttendanceId = emp.FingerPrintAttendanceId
                }).AsNoTracking().ToListAsync(cancellationToken);

            var invoiceIds = invoices.Select(i => i.InvoiceId).ToList();

            var items = await _context.InvoiceItems
                .Where(i => invoiceIds.Contains(i.InvoiceId) && i.InvoiceItemTypeId == "PAYROL_DD_ABSENCE")
                .AsNoTracking()
                .ToListAsync(cancellationToken);

            var itemsByInvoice = items.GroupBy(i => i.InvoiceId)
                .ToDictionary(g => g.Key, g => g.ToList());

            var result = new List<AbsenceDataDto>();

            foreach (var inv in invoices)
            {
                var invItems = itemsByInvoice.GetValueOrDefault(inv.InvoiceId) ?? new List<Domain.InvoiceItem>();
                var absenceDays = invItems.Sum(i => i.Quantity ?? 0);

                result.Add(new AbsenceDataDto
                {
                    EmployeeId = inv.EmployeeId,
                    EmployeeName = inv.EmployeeName,
                    JobTitle = inv.JobTitle,
                    AbsenceDays = absenceDays,
                    FingerPrintAttendanceId = inv.FingerPrintAttendanceId,
                    Notes = ""
                });
            }

            var sorted = result.OrderBy(x => x.EmployeeName).ToList();
            for (int i = 0; i < sorted.Count; i++)
                sorted[i].Serial = i + 1;

            return Result<List<AbsenceDataDto>>.Success(sorted);
        }
    }
}
