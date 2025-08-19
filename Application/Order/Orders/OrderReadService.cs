using Application._Base;
using Application.Shipments.Payments;



using Application.Catalog.ProductStores;
using Application.Common;
using Application.Core;
using Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Persistence;

namespace Application.Order.Orders;

public interface IOrderReadService
{
}

public class OrderReadService : IOrderReadService
{
    private readonly DataContext _context;
    private readonly ILogger<OrderReadService> _logger;

    public OrderReadService(DataContext context,
        ILogger<OrderReadService> logger)
    {
        _context = context;
        _logger = logger;
    }
}