using Application.Catalog.ProductStores;
using Application.Errors;
using Application.Order.Orders;
using Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Persistence;

namespace Application.Core;

public interface IUtilityService
{
    Task<string> GetNextSequence(string sequenceName);
    Task<string> GetNextOrderSequence(string orderTypeId);
    Task<OrderRole> GetOrderRole(string orderId, string roleTypeId);
    OrderRole CreateOrderRole(string orderId, string roleTypeId, string partyId);
    OrderRole UpdateOrderRole(string orderId, string roleTypeId, string partyId);
    OrderStatus CreateOrderStatus(string orderId, string statusTypeId);
    InvoiceStatus CreateInvoiceStatus(string invoiceId, string statusTypeId);
    void DeleteOrderStatus(string orderId, string statusTypeId);
    Task DeleteAllOrderItemStatusAsync(OrderItemDto2 orderItem);
    Task UpdateAllOrderItemStatusAsync(OrderItemDto2 orderItem);
    OrderStatus CreateOrderItemStatus(OrderItem orderItem, string statusTypeId);
    InvoiceRole CreateInvoiceRole(string invoiceId, string roleTypeId, string partyId);
    Task<string> GetPaymentParentType(string paymentTypeId);
    Task<string?> GetBaseCurrencyForLoggedInUser();
    Task<TEntity> FindLocalOrDatabaseAsync<TEntity>(params object[] keyValues) where TEntity : class;
    
    Task<List<TEntity>> FindLocalOrDatabaseListAsync<TEntity>(
        Func<IQueryable<TEntity>, IQueryable<TEntity>> additionalConditions = null, 
        params object[] keyValues) where TEntity : class;
    
    Task<List<TEntity>> RetrieveLocalOrDatabaseListAsync<TEntity>(
        Func<IQueryable<TEntity>, IQueryable<TEntity>> additionalConditions = null,
        params object[] keyValues) where TEntity : class;
    
}

public class UtilityService : IUtilityService
{
    private readonly DataContext _context;
    private readonly ILogger<UtilityService> _logger;
    private readonly IProductStoreService _productStoreService;


    // Allow only 1 concurrent access
    private readonly SemaphoreSlim _semaphore = new(1, 1);


    public UtilityService(DataContext context, IProductStoreService productStoreService,
        ILogger<UtilityService> logger)
    {
        _context = context;
        _productStoreService = productStoreService;
        _logger = logger;
    }

    public async Task<string?> GetBaseCurrencyForLoggedInUser()
    {
        // get productStore
        var productStore = await _productStoreService.GetProductStoreForLoggedInUser();

        // get partyAcctgPreference for partyIdTo based on product store pay to company
        var partyAcctgPreference = await _context.PartyAcctgPreferences.SingleOrDefaultAsync(x =>
            x.PartyId == productStore.PayToPartyId);

        return partyAcctgPreference!.BaseCurrencyUomId!;
    }

    public async Task<string> GetNextSequence(string sequenceName)
    {
        await _semaphore.WaitAsync(); // Acquire the semaphore

        try
        {
            var sequenceRecord = await _context.SequenceValueItems
                .Where(x => x.SeqName == sequenceName).SingleOrDefaultAsync();

            if (sequenceRecord == null) throw new Exception("Sequence record not found.");

            var newSequence = sequenceRecord.SeqId + 1;
            sequenceRecord.SeqId = newSequence;
            // log the sequenceRecord after updating it with all possible details that might prevent it from being updated
            _logger.LogInformation(
                $"Sequence record updated. Sequence name: {sequenceName}. Sequence ID: {newSequence}. Last updated stamp: {sequenceRecord.LastUpdatedStamp}.");


            sequenceRecord.LastUpdatedStamp = DateTime.UtcNow;

            return newSequence.ToString();
        }
        catch (Exception ex)
        {
            // Wrap the original exception in a GetNextSequenceException and re-throw it
            throw new GetNextSequenceException($"Failed to get next sequence. Sequence name: {sequenceName}.", ex);
        }

        finally
        {
            _semaphore.Release(); // Ensure to release the semaphore, even if an exception was thrown
        }
    }


    public async Task<OrderRole> GetOrderRole(string orderId, string roleTypeId)
    {
        try
        {
            var orderRole = await _context.OrderRoles
                .Where(x => x.OrderId == orderId && x.RoleTypeId == roleTypeId).SingleOrDefaultAsync();

            if (orderRole == null) throw new Exception("Order role not found.");

            return orderRole;
        }
        catch (Exception ex)
        {
            // Wrap the original exception in a GetOrderRoleException and re-throw it
            throw new GetOrderRoleException(
                $"Failed to get order role. Order ID: {orderId}. Role type ID: {roleTypeId}.", ex);
        }
    }

    public InvoiceRole CreateInvoiceRole(string invoiceId, string roleTypeId, string partyId)
    {
        var stamp = DateTime.UtcNow;
        var invoiceRole = new InvoiceRole
        {
            InvoiceId = invoiceId,
            PartyId = partyId,
            RoleTypeId = roleTypeId,
            CreatedStamp = stamp,
            LastUpdatedStamp = stamp
        };
        _context.InvoiceRoles.Add(invoiceRole);
        return invoiceRole;
    }

    public async Task<string> GetNextOrderSequence(string orderTypeId)
    {
        try
        {
            var orderHeaderNewSequence = await GetNextSequence("OrderHeader");
            //  
            string orderHeaderPrefix = null;
            if (orderTypeId == "SALES_ORDER")
                orderHeaderPrefix = await _productStoreService.GetSalesOrderIdPrefix();
            else
                orderHeaderPrefix = await _productStoreService.GetPurchaseOrderIdPrefix();


            var orderHeaderId = orderHeaderPrefix + orderHeaderNewSequence;
            return orderHeaderId;
        }
        catch (Exception ex)
        {
            // Wrap the original exception in a GetNextSequenceException and re-throw it
            throw new GetNextSequenceException($"Failed to get next sequence. Order type ID: {orderTypeId}.", ex);
        }
    }

    public OrderRole CreateOrderRole(string orderId, string roleTypeId, string partyId)
    {
        var stamp = DateTime.UtcNow;
        var orderRole = new OrderRole
        {
            OrderId = orderId,
            PartyId = partyId,
            RoleTypeId = roleTypeId,
            CreatedStamp = stamp,
            LastUpdatedStamp = stamp
        };
        _context.OrderRoles.Add(orderRole);
        return orderRole;
    }

    public OrderRole UpdateOrderRole(string orderId, string roleTypeId, string partyId)
    {
        var orderRole = _context.OrderRoles.SingleOrDefault(x => x.OrderId == orderId && x.RoleTypeId == roleTypeId);

        if (orderRole == null) throw new Exception("Order role not found.");

        orderRole.PartyId = partyId;
        orderRole.LastUpdatedStamp = DateTime.UtcNow;

        return orderRole;
    }

    public OrderStatus CreateOrderStatus(string orderId, string statusTypeId)
    {
        var stamp = DateTime.UtcNow;
        var orderStatus = new OrderStatus
        {
            OrderStatusId = Guid.NewGuid().ToString(),
            StatusId = statusTypeId,
            OrderId = orderId,
            StatusDatetime = stamp,
            LastUpdatedStamp = stamp,
            CreatedStamp = stamp
        };
        _context.OrderStatuses.Add(orderStatus);
        return orderStatus;
    }

    public InvoiceStatus CreateInvoiceStatus(string invoiceId, string statusTypeId)
    {
        var stamp = DateTime.UtcNow;
        var invoiceStatus = new InvoiceStatus
        {
            StatusId = statusTypeId,
            InvoiceId = invoiceId,
            StatusDate = stamp,
            LastUpdatedStamp = stamp,
            CreatedStamp = stamp
        };
        _context.InvoiceStatuses.Add(invoiceStatus);
        return invoiceStatus;
    }

    public OrderStatus CreateOrderItemStatus(OrderItem orderItem, string statusTypeId)
    {
        var stamp = DateTime.UtcNow;
        var orderStatus = new OrderStatus
        {
            OrderStatusId = Guid.NewGuid().ToString(),
            StatusId = statusTypeId,
            OrderId = orderItem.OrderId,
            OrderItemSeqId = orderItem.OrderItemSeqId,
            StatusDatetime = stamp,
            LastUpdatedStamp = stamp,
            CreatedStamp = stamp
        };
        _context.OrderStatuses.Add(orderStatus);
        return orderStatus;
    }

    public void DeleteOrderStatus(string orderId, string statusTypeId)
    {
        var orderStatus = _context.OrderStatuses.Where(x => x.OrderId == orderId && x.StatusId == statusTypeId)
            .ToList();

        if (orderStatus == null) throw new Exception("Order status not found.");

        _context.OrderStatuses.RemoveRange(orderStatus);
    }

    public async Task DeleteAllOrderItemStatusAsync(OrderItemDto2 orderItem)
    {
        var orderStatus = await _context.OrderStatuses
            .Where(x => x.OrderId == orderItem.OrderId && x.OrderItemSeqId == orderItem.OrderItemSeqId).ToListAsync();

        if (!orderStatus.Any()) throw new InvalidOperationException("Order status not found.");

        _context.OrderStatuses.RemoveRange(orderStatus);
    }

    public async Task UpdateAllOrderItemStatusAsync(OrderItemDto2 orderItem)
    {
        var orderItemStatus = await _context.OrderStatuses
            .Where(x => x.OrderId == orderItem.OrderId && x.OrderItemSeqId == orderItem.OrderItemSeqId).ToListAsync();

        foreach (var orderStatus in orderItemStatus) orderStatus.LastUpdatedStamp = DateTime.UtcNow;
    }

    public async Task<string> GetPaymentParentType(string paymentTypeId)
    {
        try
        {
            var paymentType = await _context.PaymentTypes.FirstOrDefaultAsync(pt => pt.PaymentTypeId == paymentTypeId);

            if (paymentType == null) return "UNKNOWN"; // or handle error as needed

            return paymentType.ParentTypeId == "DISBURSEMENT" ? "DISBURSEMENT" : "RECEIPT";
        }
        catch (Exception ex)
        {
            // Log or handle exception
            return "UNKNOWN";
        }
    }
    
    
    public async Task<List<TEntity>> FindLocalOrDatabaseListAsync<TEntity>(
        Func<IQueryable<TEntity>, IQueryable<TEntity>> additionalConditions = null,
        params object[] keyValues) where TEntity : class
    {
        try
        {
            // Define a method to get primary key values for in-memory comparison, handling composite keys
            object[] ExtractPrimaryKeyValues<TEntity>(TEntity entity) where TEntity : class
            {
                var keyNames = _context.Model.FindEntityType(typeof(TEntity)).FindPrimaryKey().Properties.Select(x => x.Name).ToArray();
                var keyValues = keyNames.Select(keyName => typeof(TEntity).GetProperty(keyName)?.GetValue(entity, null)).ToArray();
                return keyValues;
            }

            // Fetch local entities
            var localEntities = _context.Set<TEntity>().Local.ToList();

            // Fetch database entities
            var dbEntities = await _context.Set<TEntity>().ToListAsync();

            // Combine local and database entities, ensuring no duplicates based on composite primary key
            var allEntities = localEntities.Concat(dbEntities)
                .GroupBy(e => string.Join(",", ExtractPrimaryKeyValues(e)))
                .Select(g => g.First())
                .ToList();

            // Apply additional conditions if provided
            if (additionalConditions != null)
            {
                allEntities = additionalConditions(allEntities.AsQueryable()).ToList();
            }

            return allEntities;
        }
        catch (Exception ex)
        {
            // Log and rethrow the exception
            _logger.LogError(ex, $"Error fetching entities for {typeof(TEntity).Name}");
            throw new Exception($"Error fetching entities for {typeof(TEntity).Name}", ex);
        }
    }

    private object[] GetPrimaryKeyValues<TEntity>(DbContext context, TEntity entity) where TEntity : class
    {
        var keyNames = context.Model.FindEntityType(typeof(TEntity)).FindPrimaryKey().Properties.Select(x => x.Name).ToArray();
        var keyValues = keyNames.Select(keyName => typeof(TEntity).GetProperty(keyName).GetValue(entity, null)).ToArray();
        return keyValues;
    }

    public async Task<TEntity> FindLocalOrDatabaseAsync<TEntity>(params object[] keyValues) where TEntity : class
    {
        var localEntity = _context.Set<TEntity>().Local.FirstOrDefault(entry =>
            keyValues.SequenceEqual(GetPrimaryKeyValues(_context, entry)));

        if (localEntity != null)
        {
            return localEntity;
        }

        return await _context.Set<TEntity>().FindAsync(keyValues);
    }

    public async Task<List<TEntity>> RetrieveLocalOrDatabaseListAsync<TEntity>(
        Func<IQueryable<TEntity>, IQueryable<TEntity>> additionalConditions = null,
        params object[] keyValues) where TEntity : class
    {
        try
        {
            // Define a method to get primary key values for in-memory comparison, handling composite keys
            object[] ExtractPrimaryKeyValues(TEntity entity)
            {
                var keyNames = _context.Model.FindEntityType(typeof(TEntity)).FindPrimaryKey().Properties
                    .Select(x => x.Name).ToArray();
                var keyValues = keyNames.Select(keyName => typeof(TEntity).GetProperty(keyName)?.GetValue(entity, null))
                    .ToArray();
                return keyValues;
            }

            // Fetch local entities and apply conditions
            var localQuery = _context.Set<TEntity>().Local.AsQueryable();
            if (additionalConditions != null)
            {
                localQuery = additionalConditions(localQuery);
            }

            var localEntities = localQuery.ToList();

            // Fetch database entities and apply conditions
            var dbQuery = _context.Set<TEntity>().AsQueryable();
            if (additionalConditions != null)
            {
                dbQuery = additionalConditions(dbQuery);
            }

            var dbEntities = await dbQuery.ToListAsync();

            // Combine local and database entities, ensuring no duplicates based on composite primary key
            var allEntities = localEntities
                .Concat(dbEntities)
                .GroupBy(e => string.Join(",", ExtractPrimaryKeyValues(e)))
                .Select(g => g.First())
                .ToList();

            return allEntities;
        }
        catch (Exception ex)
        {
            // Log and rethrow the exception
            _logger.LogError(ex, $"Error fetching entities for {typeof(TEntity).Name}");
            throw new Exception($"Error fetching entities for {typeof(TEntity).Name}", ex);
        }
    }
    
   
}