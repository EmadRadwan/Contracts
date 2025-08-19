using Application.Catalog.Products;
using Moq;
using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore;
using Persistence;
using Application.Catalog.ProductStores;
using Application.Core;
using Application.Shipments;
using Domain;
using Application.order.Orders;
using Application.Order.Orders;

public class CreateSalesOrderTests
{
    private readonly DataContext _context;
    private readonly Mock<IUtilityService> _utilityServiceMock;
    private readonly Mock<IProductStoreService> _productStoreServiceMock;
    private readonly Mock<IProductService> _productServiceMock;
    private readonly Mock<IShipmentService> _shipmentServiceMock;
    private readonly Mock<ILogger<OrderService>> _loggerMock;
    private readonly Mock<IOrderHelperService> _orderHelperService;

    private readonly OrderService _orderService;

    public CreateSalesOrderTests()
    {
        // MySQL connection string for testing
        var connectionString = "server=localhost;user=root;password=baba1934;database=erp;";

        // Configure the DbContext to use MySQL
        var options = new DbContextOptionsBuilder<DataContext>()
            .UseMySql(connectionString, new MySqlServerVersion(new Version(8, 2, 0)))
            .Options; // Fix: Call .Options to build the DbContextOptions object

        _context = new DataContext(options); // Use the MySQL context

        // Mock the other services
        _utilityServiceMock = new Mock<IUtilityService>();
        _productStoreServiceMock = new Mock<IProductStoreService>();
        _productServiceMock = new Mock<IProductService>();
        _shipmentServiceMock = new Mock<IShipmentService>();
        _loggerMock = new Mock<ILogger<OrderService>>();
        _orderHelperService = new Mock<IOrderHelperService>();

        // Instantiate the OrderService using the mocks and MySQL context
        _orderService = new OrderService(
            _context,
            _utilityServiceMock.Object,
            _productStoreServiceMock.Object,
            _loggerMock.Object,
            null, // Facility service mock (if needed)
            null, // Quote service mock (if needed)
            _shipmentServiceMock.Object,
            _productServiceMock.Object, // Product service mock (if needed)
            null, // Inventory service mock (if needed)
            null
        );
    }

    [Fact]
    public async Task CreateSalesOrder_ValidOrder_ReturnsCorrectOrderId()
    {
        // Arrange
        var orderDto = new OrderDto
        {
            FromPartyId = "b22c01f6-e61f-4904-954f-40419915bf94",
            FromPartyName = "Astrid Rice",
            GrandTotal = 648,
            OrderItems = new List<OrderItemDto2>
            {
                new OrderItemDto2
                {
                    OrderItemSeqId = "01",
                    ProductId = "001a2b3c-4d5e-6f7a-8b9c-d0e1f2a3b4c1",
                    ProductName = "Aspirin",
                    Quantity = 2,
                    UnitPrice = 300,
                    SubTotal = 600,
                    CollectTax = true,
                    FacilityId = "b6705327-bb0b-421f-9a1e-e94bbf7a68d2"
                }
            },
            OrderAdjustments = new List<OrderAdjustmentDto2>
            {
                new OrderAdjustmentDto2
                {
                    OrderAdjustmentId = Guid.NewGuid().ToString(),
                    OrderAdjustmentTypeId = "SALES_TAX",
                    OrderAdjustmentTypeDescription = "Sales Tax",
                    OrderItemSeqId = "01",
                    Amount = 48,
                    IsAdjustmentDeleted = false
                },
                new OrderAdjustmentDto2
                {
                    OrderAdjustmentId = Guid.NewGuid().ToString(),
                    OrderAdjustmentTypeId = "DISCOUNT_ADJUSTMENT",
                    OrderAdjustmentTypeDescription = "Discount",
                    OrderItemSeqId = "01",
                    Amount = -6,
                    IsAdjustmentDeleted = false
                }
            }
        };

        var expectedOrderId = "SO10677"; // Expected order ID for the test
        var defaultCurrency = "EGY";
        var productStore = new ProductStore { ProductStoreId = "9000", ReserveOrderEnumId = "RESERVE_INVENTORY" };

        // Mocking utility and product store service behavior
        _utilityServiceMock.Setup(s => s.GetNextOrderSequence("SALES_ORDER")).ReturnsAsync(expectedOrderId);
        _productStoreServiceMock.Setup(s => s.GetProductStoreDefaultCurrencyId()).ReturnsAsync(defaultCurrency);
        _productStoreServiceMock.Setup(s => s.GetProductStoreForLoggedInUser()).ReturnsAsync(productStore);
        _shipmentServiceMock.Setup(s => s.CreateOrderItemShipGroup(expectedOrderId))
            .Returns(new OrderItemShipGroup { ShipGroupSeqId = "SG001" });

        // Act
        var result = await _orderService.CreateSalesOrder(orderDto);

        // Assert
        Assert.NotNull(result); // Ensure the result is not null
        Assert.Equal(expectedOrderId, result.OrderId); // Check if the OrderId matches the expected one
        var savedOrder = await _context.OrderHeaders.FindAsync(result.OrderId); // Verify the order was saved
        Assert.NotNull(savedOrder); // Ensure that the order was saved in the MySQL database

        _loggerMock.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Create Sales order header. create sales order")),
                It.IsAny<Exception>(),
                It.Is<Func<It.IsAnyType, Exception, string>>((v, t) => true)), Times.AtLeastOnce);
    }
}