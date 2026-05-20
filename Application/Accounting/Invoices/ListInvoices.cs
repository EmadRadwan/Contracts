using Application.Core;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Persistence;
using Domain;                    // Assuming InvoiceView is in Domain
using Application.Interfaces;
using Microsoft.AspNetCore.OData.Query; // For IUserAccessor

namespace Application.Accounting.Invoices
{
    public class ListInvoices
    {
        public class Query : IRequest<IQueryable<InvoiceView>>
        {
            public ODataQueryOptions<InvoiceView> Options { get; set; } = null!;
        }

        public class Handler : IRequestHandler<Query, IQueryable<InvoiceView>>
        {
            private readonly DataContext _context;
            private readonly IUserAccessor _userAccessor;

            public Handler(DataContext context, IUserAccessor userAccessor)
            {
                _context = context;
                _userAccessor = userAccessor;
            }

            public async Task<IQueryable<InvoiceView>> Handle(Query request, CancellationToken cancellationToken)
            {
                var query = _context.InvoiceRecords.AsQueryable();

                // Get current username
                var username = _userAccessor.GetUsername();

                // If no user (should not happen in authorized endpoint), hide payroll invoices by default
                if (string.IsNullOrEmpty(username))
                {
                    query = query.Where(i => i.InvoiceTypeId != "PAYROL_INVOICE");
                    return query;
                }

                // Check if user has the special role
                var hasViewPayrollRole = await _context.UserRoles
                    .Join(_context.Roles, ur => ur.RoleId, r => r.Id, (ur, r) => new { ur.UserId, RoleName = r.Name })
                    .Join(_context.Users, ur => ur.UserId, u => u.Id, (ur, u) => new { ur.RoleName, u.UserName })
                    .AnyAsync(x => x.UserName == username && x.RoleName == "viewPayrollInvoices", cancellationToken);

                if (!hasViewPayrollRole)
                {
                    query = query.Where(i => i.InvoiceTypeId != "PAYROL_INVOICE");
                }

                return query;
            }
        }
    }
}