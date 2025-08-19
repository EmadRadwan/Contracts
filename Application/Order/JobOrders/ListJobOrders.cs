/*using Application.Interfaces;
using Application.Order.Orders;
using Application.Services;
using MediatR;
using Microsoft.AspNetCore.OData.Query;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Persistence;

namespace Application.Order.JobOrders;

public class ListJobOrders
{
    public class Query : IRequest<IQueryable<OrderRecord>>
    {
        public ODataQueryOptions<OrderRecord> Options { get; set; }
    }


    public class Handler : IRequestHandler<Query, IQueryable<OrderRecord>>
    {
        private readonly DataContext _context;
        private readonly ILogger<Handler> _logger;
        private readonly IUserAccessor _userAccessor;

        public Handler(DataContext context, IUserAccessor userAccessor, ILogger<Handler> logger)
        {
            _context = context;
            _userAccessor = userAccessor;
            _logger = logger;
        }


        public async Task<IQueryable<OrderRecord>> Handle(Query request, CancellationToken cancellationToken)
        {
            var query = from ord in _context.OrderHeaders
                join v in _context.Vehicles.AsNoTracking() on ord.VehicleId equals v.VehicleId
                join orole in _context.OrderRoles.AsNoTracking() on ord.OrderId equals orole.OrderId
                join pty in _context.Parties.AsNoTracking() on orole.PartyId equals pty.PartyId
                join ordt in _context.OrderTypes.AsNoTracking() on ord.OrderTypeId equals ordt.OrderTypeId
                join sts in _context.StatusItems.AsNoTracking() on ord.StatusId equals sts.StatusId
                join pcm in _context.PartyContactMeches.AsNoTracking() on pty.PartyId equals pcm.PartyId
                join cm in _context.ContactMeches.AsNoTracking() on pcm.ContactMechId equals cm.ContactMechId
                join tn in _context.TelecomNumbers.AsNoTracking() on cm.ContactMechId equals tn.ContactMechId
                join pcmp in _context.PartyContactMechPurposes.AsNoTracking() on new { pcm.PartyId, pcm.ContactMechId }
                    equals new
                        { pcmp.PartyId, pcmp.ContactMechId }
                join cmpt in _context.ContactMechPurposeTypes.AsNoTracking() on pcmp.ContactMechPurposeTypeId equals
                    cmpt
                        .ContactMechPurposeTypeId
                where pcmp.ContactMechPurposeTypeId == "PRIMARY_PHONE" &&
                      ordt.OrderTypeId == "PURCHASE_ORDER" && orole.RoleTypeId == "BILL_FROM_VENDOR"
                select new OrderRecord
                {
                    OrderId = ord.OrderId,
                    FromPartyName = pty.Description + " ( " + tn.ContactNumber + " )",
                    OrderDate = ord.OrderDate,
                    StatusDescription = sts.Description,
                    GrandTotal = ord.GrandTotal,
                    OrderTypeId = ordt.OrderTypeId,
                    OrderTypeDescription = ordt.Description,
                    FromPartyId = new OrderPartyDto
                    {
                        FromPartyId = pty.PartyId,
                        FromPartyName = pty.Description ?? string.Empty
                    },
                    VehicleId = new VehicleLovDto
                    {
                        VehicleId = v.VehicleId,
                        ChassisNumber = v.ChassisNumber
                    },
                    CustomerRemarks = ord.CustomerRemarks,
                    InternalRemarks = ord.InternalRemarks,
                    CurrentMileage = ord.CurrentMileage
                };

            return query;
        }
    }
}*/