using Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Persistence;

namespace Application.Manufacturing
{
    public interface IBOMTreeService
    {
        Task Initialize(string productId, string bomTypeId, DateTime inDate, int type, decimal quantity);

        Task<string> CreateManufacturingOrders(
            string facilityId, DateTime date, string workEffortName, string description, string routingId,
            string orderId = null, string orderItemSeqId = null, string shipGroupSeqId = null, string shipmentId = null,
            string userLogin = null);
    }

    public class BOMTreeService : IBOMTreeService
    {
        private readonly DataContext _context;
        private readonly IBOMNodeService _bomNodeService;
        private readonly ILogger<BOMTreeService> _logger;
        private BOMTree tree;
        public const int EXPLOSION = 0;
        public const int EXPLOSION_SINGLE_LEVEL = 1;
        public const int EXPLOSION_MANUFACTURING = 2;
        public const int IMPLOSION = 3;


        public BOMTreeService(DataContext context, IBOMNodeService bomNodeService, ILogger<BOMTreeService> logger)
        {
            _context = context;
            _bomNodeService = bomNodeService;
            _logger = logger;
        }


        public async Task Initialize(string productId, string bomTypeId, DateTime inDate, int type, decimal quantity)
        {
            try
            {
                // Initial check to ensure the product and BOM type are provided.
                if (string.IsNullOrEmpty(productId) || string.IsNullOrEmpty(bomTypeId))
                    throw new ArgumentException("ProductId and BomTypeId cannot be null or empty.");

                // If no date is provided, use the current date. Ensures valid date for the operation.
                if (inDate == default) inDate = DateTime.UtcNow;

                // Initialize the BOMTree object that represents the product's BOM structure.
                tree = new BOMTree(productId, bomTypeId, inDate);

                // Retrieve the input product from the database.
                var inputProduct = await _context.Products
                    .AsNoTracking()
                    .FirstOrDefaultAsync(p => p.ProductId == productId);

                // If the product does not exist in the database, terminate the process.
                if (inputProduct == null)
                {
                    throw new InvalidOperationException($"Product with ID {productId} not found.");
                }

                // Initialize the default productId for rules as the input productId.
                string productIdForRules = productId;

                // Load the product's standard features from the database. These are attributes that apply to the product.
                var productFeatures = await _context.ProductFeatureAppls
                    .AsNoTracking()
                    .Where(f => f.ProductId == productId && f.ProductFeatureApplTypeId == "STANDARD_FEATURE")
                    .Select(f => f.ProductFeature)
                    .ToListAsync();

                // Check if the product is manufactured as another product, indicating that the BOM is for a substituted product.
                var manufacturedAsProduct = await ManufacturedAsProduct(productId, inDate);

                // If the product is manufactured as another product, set the productId accordingly.
                var productIdToUse = manufacturedAsProduct != null ? manufacturedAsProduct.ProductIdTo : productId;

                // Retrieve the product (or the substituted one) for further BOM initialization.
                var product = await _context.Products
                    .AsNoTracking()
                    .FirstOrDefaultAsync(p => p.ProductId == productIdToUse);

                // If no valid product is found, stop the process.
                if (product == null)
                {
                    return;
                }

                // Create the original BOMNode for the product and assign it to the BOMTree.
                BOMNode originalNode = await _bomNodeService.CreateBOMNode(inputProduct.ProductId, quantity);
                originalNode.SetTree(tree);

                // Check if the product has a BOM; if not, treat it as a virtual product that may need configuration.
                if (!await HasBom(inputProduct, inDate))
                {
                    var virtualProducts = await _context.ProductAssocs
                        .AsNoTracking()
                        .Where(pa =>
                            pa.ProductAssocTypeId == "PRODUCT_VARIANT" && pa.ProductId == inputProduct.ProductId)
                        .ToListAsync();

                    // Identify the valid virtual product for the current date.
                    var validVirtualProduct = virtualProducts
                        .FirstOrDefault(p => p.FromDate <= inDate && (p.ThruDate == null || p.ThruDate >= inDate));

                    // If a valid virtual product is found, adjust the BOM for that virtual product.
                    if (validVirtualProduct != null)
                    {
                        productIdForRules = validVirtualProduct.ProductIdTo;

                        // Check if the virtual product is also manufactured as a different product.
                        manufacturedAsProduct = await ManufacturedAsProduct(validVirtualProduct.ProductIdTo, inDate);

                        // Determine the correct productId, either the manufactured product or the virtual one.
                        productIdToUse = manufacturedAsProduct != null
                            ? manufacturedAsProduct.ProductIdTo
                            : validVirtualProduct.ProductIdTo;

                        // Retrieve the actual product to use in the BOM setup.
                        product = await _context.Products
                            .AsNoTracking()
                            .FirstOrDefaultAsync(p => p.ProductId == productIdToUse);

                        // If no valid product is found at this stage, terminate.
                        if (product == null)
                        {
                            return;
                        }
                    }
                }

                try
                {
                    // Create and initialize the root BOMNode for the product.
                    tree.Root = await _bomNodeService.CreateBOMNode(inputProduct.ProductId, quantity);
                    tree.Root.Quantity = quantity;
                    tree.Root.SetTree(tree);
                    tree.Root.SetProductForRules(productIdForRules);
                    tree.Root.SetSubstitutedNode(originalNode);

                    // Depending on the BOM type, either load parents (for implosion) or load children (for explosion).
                    if (type == IMPLOSION)
                    {
                        await _bomNodeService.LoadParents(tree.Root, tree.BomTypeId, inDate, productFeatures);
                    }
                    else
                    {
                        await _bomNodeService.LoadChildren(tree.Root, tree.BomTypeId, inDate, productFeatures, type);
                    }
                }
                catch (Exception ex)
                {
                    // In case of any error during the BOM node creation, log and clear the root.
                    Console.WriteLine("Initialize - Error");
                    Console.WriteLine(ex);
                    tree.Root = null;
                }

                // Set the initial quantity and amount for the root node.
                tree.RootQuantity = 1;
                tree.RootAmount = 0;
            }
            catch (Exception e)
            {
                // General error handling to capture any issues in the initialization process.
                Console.WriteLine(e);
                throw;
            }
        }

        public Product GetInputProduct()
        {
            return tree.InputProduct;
        }

        private async Task<ProductAssoc> ManufacturedAsProduct(string productId, DateTime inDate)
        {
            try
            {
                return await _context.ProductAssocs
                    .AsNoTracking()
                    .FirstOrDefaultAsync(pa => pa.ProductId == productId &&
                                               pa.ProductAssocTypeId == "PRODUCT_MANUFACTURED" &&
                                               pa.FromDate <= inDate && (pa.ThruDate == null || pa.ThruDate >= inDate));
            }
            catch (Exception ex)
            {
                //loggerForTransaction.Error(ex, "ManufacturedAsProduct - Failed to retrieve manufactured product for product {ProductId} with inDate {InDate}",
                //productId, inDate);
                throw new InvalidOperationException($"Error fetching manufactured product for {productId}", ex);
            }
        }


        private async Task<bool> HasBom(Product product, DateTime inDate)
        {
            try
            {
                var children = await _context.ProductAssocs
                    .AsNoTracking()
                    .Where(pa => pa.ProductId == product.ProductId && pa.ProductAssocTypeId == tree.BomTypeId)
                    .Where(pa => pa.FromDate <= inDate && (pa.ThruDate == null || pa.ThruDate >= inDate))
                    .ToListAsync();

                return children.Any();
            }
            catch (Exception ex)
            {
                //loggerForTransaction.Error(ex, "HasBom - Error checking if product {ProductId} has BOM for inDate {InDate}", 
                //product.ProductId, inDate);
                throw new InvalidOperationException($"Error checking BOM for product {product.ProductId}", ex);
            }
        }


        public async Task<string> CreateManufacturingOrders(
            string facilityId, DateTime date, string workEffortName, string description, string routingId,
            string orderId = null, string orderItemSeqId = null, string shipGroupSeqId = null, string shipmentId = null,
            string userLogin = null)
        {
            string workEffortId = null;

            try
            {
                if (tree.Root != null)
                {
                    //loggerForTransaction.Information(
                    // "Starting manufacturing order creation for root node product {ProductId}", Root.Product.ProductId);
                    // If facilityId is not provided, derive it from the order or shipment
                    if (string.IsNullOrEmpty(facilityId))
                    {
                        // Check if orderId is provided and derive the facilityId from the product store linked to the order
                        if (!string.IsNullOrEmpty(orderId))
                        {
                            var order = await _context.OrderHeaders.AsNoTracking()
                                .FirstOrDefaultAsync(x => x.OrderId == orderId);

                            if (order != null)
                            {
                                var productStore = await _context.ProductStores.AsNoTracking()
                                    .FirstOrDefaultAsync(x => x.ProductStoreId == order.ProductStoreId);
                                if (productStore != null)
                                {
                                    facilityId = productStore.InventoryFacilityId;
                                }
                                else
                                {
                                    throw new InvalidOperationException(
                                        $"Product store for OrderId {orderId} not found.");
                                }
                            }
                            else
                            {
                                throw new InvalidOperationException($"Order with OrderId {orderId} not found.");
                            }
                        }

                        // If facilityId is still null, try deriving it from the shipment origin facility
                        if (string.IsNullOrEmpty(facilityId) && !string.IsNullOrEmpty(shipmentId))
                        {
                            var shipment = await _context.Shipments.AsNoTracking()
                                .FirstOrDefaultAsync(x => x.ShipmentId == shipmentId);
                            if (shipment != null)
                            {
                                facilityId = shipment.OriginFacilityId;
                            }
                            else
                            {
                                throw new InvalidOperationException(
                                    $"Shipment with ShipmentId {shipmentId} not found.");
                            }
                        }
                    }

                    // Call Root.CreateManufacturingOrder with the correct parameters
                    var createManufacturingOrderResponse = await _bomNodeService.CreateManufacturingOrder(
                        tree.Root,
                        facilityId,
                        date,
                        workEffortName,
                        description,
                        routingId,
                        orderId,
                        orderItemSeqId,
                        shipGroupSeqId,
                        shipmentId,
                        true, // Set useSubstitute to true as per Java version
                        true // Set ignoreSupplierProducts to true as per Java version
                    );

                    // Extract the productionRunId from the response
                    workEffortId = createManufacturingOrderResponse?.ProductionRunId;
                }
                else
                {
                    throw new InvalidOperationException("Root BOMNode is null.");
                }
            }
            catch (DbUpdateException dbEx)
            {
                //loggerForTransaction.Error(dbEx, "Database error occurred while creating manufacturing orders.");
                throw new InvalidOperationException("Database error occurred while creating manufacturing orders.",
                    dbEx);
            }
            catch (InvalidOperationException ex)
            {
                //loggerForTransaction.Error(ex, "Error creating manufacturing orders.");
                throw new InvalidOperationException($"Error creating manufacturing orders: {ex.Message}", ex);
            }
            catch (Exception ex)
            {
                //loggerForTransaction.Error(ex, "Unexpected error during the manufacturing order creation.");
                throw new Exception("An unexpected error occurred while creating manufacturing orders.", ex);
            }

            return workEffortId;
        }
    }
}