using Application.Errors;
using Domain;
using Persistence;

namespace Application.Core;

public interface IStatusService
{
    Task CreateShipmentStatus(string statusId, string shipmentId);
}

public class StatusService : IStatusService
{
    private readonly DataContext _context;

    public StatusService(DataContext context)
    {
        _context = context;
    }

    public Task CreateShipmentStatus(string statusId, string shipmentId)
    {
        try
        {
            var stamp = DateTime.UtcNow;
            var shipmentStatus = new ShipmentStatus
            {
                StatusId = statusId,
                ShipmentId = shipmentId,
                StatusDate = stamp,
                LastUpdatedStamp = stamp,
                CreatedStamp = stamp
            };
            _context.ShipmentStatuses.Add(shipmentStatus);
        }
        catch (Exception ex)
        {
            throw new CreateStatusException("Failed to create shipment status.", ex);
        }

        return Task.CompletedTask;
    }
}