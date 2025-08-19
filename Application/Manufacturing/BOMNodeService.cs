using Application.Catalog.Products;
using Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Persistence;

namespace Application.Manufacturing;

public interface IBOMNodeService
{
    Task<BOMNode> CreateBOMNode(string productId, decimal quantity = 0m);
    Task LoadParents(BOMNode node, string bomTypeId, DateTime inDate, List<ProductFeature> productFeatures);

    Task LoadChildren(BOMNode node, string bomTypeId, DateTime inDate,
        List<ProductFeature> productFeatures, int type);

    Task<CreateManufacturingOrderResponse> CreateManufacturingOrder(
        BOMNode node, // Pass node directly
        string facilityId, DateTime date, string workEffortName, string description,
        string routingId, string orderId, string orderItemSeqId, string shipGroupSeqId,
        string shipmentId, bool useSubstitute, bool ignoreSupplierProducts);
}

public class BOMNodeService : IBOMNodeService
{
    private readonly DataContext _context;
    private readonly IProductHelperService _productHelperService;
    private readonly IProductionRunService _productionRunService;
    private readonly ILogger<BOMNodeService> _logger;

    public BOMNodeService(DataContext context, IProductHelperService productHelperService,
        IProductionRunService productionRunService, ILogger<BOMNodeService> logger)
    {
        _context = context;
        _productHelperService = productHelperService;
        _productionRunService = productionRunService;
        _logger = logger;
    }

    public async Task<BOMNode> CreateBOMNode(string productId, decimal quantity = 0m)
    {
        var product = await _context.Products
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.ProductId == productId);
        if (product == null)
        {
            throw new InvalidOperationException($"Product with ID {productId} not found.");
        }

        var node = new BOMNode(product);
        node.Quantity = quantity;

        return node;
    }

    public async Task LoadChildren(BOMNode node, string bomTypeId, DateTime inDate,
        List<ProductFeature> productFeatures, int type)
    {
        if (node.Product == null)
        {
            throw new InvalidOperationException("Product is null.");
        }

        // Ensure date consistency
        inDate = inDate == default ? DateTime.UtcNow : inDate;
        node.BomTypeId = bomTypeId;

        try
        {
            // Fetch ProductAssocs for the current product
            var childAssocs = await FetchProductAssocs(node, bomTypeId, inDate);

            // If no child records found, check for substituted node
            if (!childAssocs.Any() && node.SubstitutedNode != null)
            {
                childAssocs = await FetchProductAssocs(node.SubstitutedNode, bomTypeId, inDate);
            }

            node.Children = childAssocs;
            node.ChildrenNodes = new List<BOMNode>();

            foreach (var childAssoc in childAssocs)
            {
                BOMNode childNode = null;

                try
                {
                    var rootNode = node.GetRootNode();
                    var productForRules = rootNode.GetProductForRules();
                    childNode = await Configurator(rootNode, childAssoc, productFeatures, productForRules, inDate);

                    if (childNode != null)
                    {
                        childNode.SetParentNode(node); // Proper parent assignment

                        // Recursive loading based on explosion type
                        await HandleChildLoading(childNode, bomTypeId, inDate, productFeatures, type);
                        childNode.Quantity = node.Quantity * childNode.QuantityMultiplier * childNode.ScrapFactor;
                    }

                    node.ChildrenNodes.Add(childNode);
                }
                catch (Exception ex)
                {
                    // Handling child-node-specific issues and logging
                    _logger.LogError(ex, $"Error loading child node for product {childAssoc.ProductIdTo}");
                    throw new InvalidOperationException(
                        $"Error loading child node for product {childAssoc.ProductIdTo}: {ex.Message}", ex);
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Failed to load children for product {node.Product.ProductId}");
            throw new InvalidOperationException(
                $"Failed to load children for product {node.Product.ProductId}: {ex.Message}", ex);
        }
    }

    private async Task<List<ProductAssoc>> FetchProductAssocs(BOMNode node, string bomTypeId, DateTime inDate)
    {
        return await _context.ProductAssocs
            .AsNoTracking()
            .Where(pa => pa.ProductId == node.Product.ProductId && pa.ProductAssocTypeId == bomTypeId &&
                         pa.FromDate <= inDate && (pa.ThruDate == null || pa.ThruDate >= inDate))
            .OrderBy(pa => pa.SequenceNum)
            .ToListAsync();
    }

    private async Task HandleChildLoading(BOMNode childNode, string bomTypeId, DateTime inDate,
        List<ProductFeature> productFeatures, int type)
    {
        // Recursive loading logic with separation of explosion types
        if (type == BOMTree.EXPLOSION)
        {
            await LoadChildren(childNode, bomTypeId, inDate, productFeatures, BOMTree.EXPLOSION);
        }
        else if (type == BOMTree.EXPLOSION_MANUFACTURING && !IsWarehouseManaged(childNode, null))
        {
            await LoadChildren(childNode, bomTypeId, inDate, productFeatures, BOMTree.EXPLOSION);
        }
    }

    private async Task<BOMNode> Configurator(BOMNode node, ProductAssoc productAssoc,
        List<ProductFeature> productFeatures, string productIdForRules, DateTime inDate)
    {
        try
        {
            // Fetch the associated product based on the ProductAssoc
            var product = await _context.Products
                .AsNoTracking()
                .FirstOrDefaultAsync(p => p.ProductId == productAssoc.ProductIdTo);

            if (product == null)
            {
                throw new InvalidOperationException($"Product {productAssoc.ProductIdTo} not found.");
            }

            var oneChildNode = await CreateBOMNode(product.ProductId, productAssoc.Quantity ?? 1);
            oneChildNode.SetTree(node.Tree);
            oneChildNode.setProductAssoc(productAssoc);

            // Set Quantity Multiplier
            oneChildNode.SetQuantityMultiplier(productAssoc.Quantity ?? 1);

            // Set Scrap Factor
            var percScrapFactor = productAssoc.ScrapFactor ?? 1;
            // Only adjust percScrapFactor if it's not already 1
            if (percScrapFactor != 1 && percScrapFactor > -100 && percScrapFactor < 100)
            {
                percScrapFactor = 1 + percScrapFactor / 100;
            }
            else
            {
                percScrapFactor = 1;
            }


            oneChildNode.SetScrapFactor(percScrapFactor);

            // --- Added Section: Initialize Quantity ---
            // Ensures the initial quantity is set based on the parent node's quantity and multipliers
            oneChildNode.Quantity = node.Quantity * oneChildNode.QuantityMultiplier * oneChildNode.ScrapFactor;
            _logger.LogDebug("Initialized Quantity for {ProductId}: {Quantity}", product.ProductId,
                oneChildNode.Quantity);
            // --- End of Added Section ---

            var newNode = oneChildNode;

            // Check if the product is virtual
            if (oneChildNode.IsVirtual())
            {
                // First attempt to retrieve product manufacturing rules
                var productPartRules = await _context.ProductManufacturingRules
                    .AsNoTracking()
                    .Where(r => r.ProductId == productIdForRules && r.ProductIdFor == node.Product.ProductId &&
                                r.ProductIdIn == productAssoc.ProductIdTo)
                    .Where(r => r.FromDate <= inDate && (r.ThruDate == null || r.ThruDate >= inDate))
                    .ToListAsync();

                // If substituted node exists, retrieve its rules as well
                if (node.SubstitutedNode != null)
                {
                    var substitutedProductPartRules = await _context.ProductManufacturingRules
                        .AsNoTracking()
                        .Where(r => r.ProductId == productIdForRules &&
                                    r.ProductIdFor == node.SubstitutedNode.Product.ProductId &&
                                    r.ProductIdIn == productAssoc.ProductIdTo)
                        .Where(r => r.FromDate <= inDate && (r.ThruDate == null || r.ThruDate >= inDate))
                        .ToListAsync();

                    productPartRules.AddRange(substitutedProductPartRules);
                }

                // Attempt to substitute the node using the product part rules
                newNode = await SubstituteNode(oneChildNode, productFeatures, productPartRules);

                // If no substitution occurred, check generic link rules
                if (newNode == oneChildNode)
                {
                    var genericLinkRules = await _context.ProductManufacturingRules
                        .AsNoTracking()
                        .Where(r => r.ProductIdFor == productAssoc.ProductId &&
                                    r.ProductIdIn == node.ProductAssoc.ProductIdTo)
                        .Where(r => r.FromDate <= inDate && (r.ThruDate == null || r.ThruDate >= inDate))
                        .ToListAsync();

                    if (node.SubstitutedNode != null)
                    {
                        var substitutedGenericLinkRules = await _context.ProductManufacturingRules
                            .AsNoTracking()
                            .Where(r => r.ProductIdFor == node.SubstitutedNode.Product.ProductId &&
                                        r.ProductIdIn == productAssoc.ProductIdTo)
                            .Where(r => r.FromDate <= inDate && (r.ThruDate == null || r.ThruDate >= inDate))
                            .ToListAsync();

                        genericLinkRules.AddRange(substitutedGenericLinkRules);
                    }

                    // Attempt substitution using generic link rules
                    newNode = await SubstituteNode(oneChildNode, productFeatures, genericLinkRules);

                    // If still no substitution, check generic node rules
                    if (newNode == oneChildNode)
                    {
                        var genericNodeRules = await _context.ProductManufacturingRules
                            .AsNoTracking()
                            .Where(r => r.ProductIdIn == productAssoc.ProductIdTo)
                            .OrderBy(r => r.RuleSeqId)
                            .Where(r => r.FromDate <= inDate && (r.ThruDate == null || r.ThruDate >= inDate))
                            .ToListAsync();

                        newNode = await SubstituteNode(oneChildNode, productFeatures, genericNodeRules);

                        // Final attempt: Apply selected features
                        if (newNode == oneChildNode)
                        {
                            if (genericNodeRules.Any())
                            {
                                // Add logic if specific node-substitution logic is needed
                            }

                            // Attempt to apply selected features
                            var selectedFeatures = productFeatures
                                .Select(feature => new SelectedFeature
                                {
                                    FeatureTypeId = feature.ProductFeatureTypeId,
                                    FeatureId = feature.ProductFeatureId
                                })
                                .ToList();

                            if (selectedFeatures.Any())
                            {
                                newNode = await ApplySelectedFeatures(newNode, product, selectedFeatures);
                            }
                        }
                    }
                }
            }

            return newNode;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in Configurator for product assoc {ProductAssocId}", productAssoc.ProductIdTo);
            throw;
        }
    }

    private async Task<BOMNode> ApplySelectedFeatures(BOMNode oneChildNode, Product product,
        List<SelectedFeature> selectedFeatures)
    {
        try
        {
            var variantProducts = await _productHelperService.FindSelectedVariant(selectedFeatures, product.ProductId);

            if (variantProducts.Any())
            {
                var variantProduct = variantProducts.First();
                var newNode = await CreateBOMNode(variantProduct.ProductId);
                newNode.SetTree(oneChildNode.Tree);
                newNode.SetSubstitutedNode(oneChildNode);
                newNode.SetQuantityMultiplier(oneChildNode.QuantityMultiplier);
                newNode.SetScrapFactor(oneChildNode.ScrapFactor);
                newNode.setProductAssoc(oneChildNode.ProductAssoc);

                return newNode;
            }
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Error finding selected variant for product {ProductId}", product.ProductId);
        }

        return oneChildNode;
    }

    private async Task<BOMNode> SubstituteNode(BOMNode oneChildNode, List<ProductFeature> productFeatures,
        List<ProductManufacturingRule> productPartRules)
    {
        // Iterate over the product part rules to attempt substitutions
        if (productPartRules != null)
        {
            foreach (var rule in productPartRules)
            {
                // Extract relevant rule properties
                var ruleCondition = rule.ProductFeature; // Equivalent to get("productFeature")
                var ruleOperator = rule.RuleOperator; // Equivalent to get("ruleOperator")
                var newPart = rule.ProductIdInSubst; // Equivalent to get("productIdInSubst")
                decimal ruleQuantity = (decimal)(rule.Quantity ?? 0);

                bool ruleSatisfied = string.IsNullOrEmpty(ruleCondition);

                // Check if the rule condition is satisfied based on product features
                if (!ruleSatisfied && productFeatures != null)
                {
                    foreach (var feature in productFeatures)
                    {
                        if (ruleCondition == feature.ProductFeatureId)
                        {
                            ruleSatisfied = true;
                            break;
                        }
                    }
                }

                // If the rule is satisfied and it's an "OR" operator, substitute the node
                if (ruleSatisfied && ruleOperator == "OR")
                {
                    var tmpNode = oneChildNode;

                    if (string.IsNullOrEmpty(newPart))
                    {
                        oneChildNode = null;
                    }
                    else
                    {
                        var origNode = oneChildNode;

                        // Create a new BOM node for the substituted product
                        oneChildNode = await CreateBOMNode(newPart);
                        oneChildNode.SetTree(origNode.Tree);
                        oneChildNode.SetSubstitutedNode(tmpNode);
                        oneChildNode.setRuleApplied(rule);
                        oneChildNode.setProductAssoc(origNode.ProductAssoc);
                        oneChildNode.SetScrapFactor(origNode.ScrapFactor);

                        // Set quantity multiplier based on rule quantity
                        if (ruleQuantity > 0)
                        {
                            oneChildNode.SetQuantityMultiplier(ruleQuantity);
                            oneChildNode.Quantity = origNode.Quantity;
                        }


                        else
                        {
                            oneChildNode.SetQuantityMultiplier(origNode.QuantityMultiplier);
                            oneChildNode.Quantity = origNode.Quantity;
                        }
                    }

                    // Exit loop after substitution
                    break;
                }
            }
        }

        return oneChildNode;
    }


    public async Task<CreateManufacturingOrderResponse> CreateManufacturingOrder(
        BOMNode node,
        string facilityId, DateTime date, string workEffortName, string description,
        string routingId, string orderId, string orderItemSeqId, string shipGroupSeqId,
        string shipmentId, bool useSubstitute, bool ignoreSupplierProducts)
    {
        CreateManufacturingOrderResponse response = new CreateManufacturingOrderResponse();
        string productionRunId = null;
        DateTime? endDate = null;

        try
        {
            // Check if the node is manufactured
            if (await IsManufactured(node, ignoreSupplierProducts))
            {
                List<string> childProductionRuns = new List<string>();
                DateTime? maxEndDate = null;

                // Loop through children nodes and create manufacturing orders for them
                foreach (var oneChildNode in node.ChildrenNodes)
                {
                    if (oneChildNode != null)
                    {
                        var tmpResult = await CreateManufacturingOrder(
                            oneChildNode, facilityId, date, null, null, null,
                            orderId, orderItemSeqId, shipGroupSeqId, shipmentId, false, false);

                        if (tmpResult?.ProductionRunId != null)
                        {
                            string childProductionRunId = tmpResult.ProductionRunId;
                            DateTime? childEndDate = tmpResult.EndDate;

                            // Update the maxEndDate if applicable
                            if (maxEndDate == null || (childEndDate.HasValue && maxEndDate < childEndDate))
                            {
                                maxEndDate = childEndDate;
                            }

                            if (!string.IsNullOrEmpty(childProductionRunId))
                            {
                                childProductionRuns.Add(childProductionRunId);
                            }
                        }
                    }
                }

                // Use either maxEndDate or the provided date as the start date
                DateTime startDate = date;

                // Add logic to handle the "useSubstitute" flag, using the substituted node's product if applicable
                var productId = useSubstitute && node.SubstitutedNode != null
                    ? node.SubstitutedNode.Product.ProductId
                    : node.Product.ProductId;

                var facilityToUse = useSubstitute && node.SubstitutedNode != null
                    ? node.SubstitutedNode.Product.FacilityId
                    : facilityId;

                // Automatically generate workEffortName if shipmentId is present and workEffortName is empty
                if (!string.IsNullOrEmpty(shipmentId) && string.IsNullOrEmpty(workEffortName))
                {
                    workEffortName = $"SP_{shipmentId}_{productId}";
                }

                // Create the main production run
                var createProductionRunResponse = await _productionRunService.CreateProductionRun(
                    productId, maxEndDate ?? startDate, node.Quantity, facilityToUse, routingId,
                    workEffortName, description);

                productionRunId = createProductionRunResponse.ProductionRunId;

                // Associate the production run with child runs
                try
                {
                    if (!string.IsNullOrEmpty(productionRunId))
                    {
                        // Add work order fulfillment
                        if (!string.IsNullOrEmpty(orderId) && !string.IsNullOrEmpty(orderItemSeqId))
                        {
                            var workOrderItemFulfillment = new WorkOrderItemFulfillment
                            {
                                WorkEffortId = productionRunId,
                                OrderId = orderId,
                                OrderItemSeqId = orderItemSeqId,
                                ShipGroupSeqId = shipGroupSeqId
                            };
                            _context.WorkOrderItemFulfillments.Add(workOrderItemFulfillment);
                        }

                        // Associate child production runs with the parent run
                        foreach (var childRunId in childProductionRuns)
                        {
                            var workEffortAssoc = new WorkEffortAssoc
                            {
                                WorkEffortIdFrom = childRunId,
                                WorkEffortIdTo = productionRunId,
                                WorkEffortAssocTypeId = "WORK_EFF_PRECEDENCY",
                                FromDate = startDate,
                                CreatedStamp = DateTime.UtcNow,
                                LastUpdatedStamp = DateTime.UtcNow
                            };
                            _context.WorkEffortAssocs.Add(workEffortAssoc);
                        }
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error while creating WorkEffortAssoc for production run {ProductionRunId}",
                        productionRunId);
                    throw new Exception(
                        $"Error while creating WorkEffortAssoc for production run {productionRunId}: {ex.Message}");
                }

                endDate = maxEndDate ?? startDate;
            }
        }
        catch (DbUpdateException dbEx)
        {
            _logger.LogError(dbEx, "Database error occurred while creating manufacturing order.");
            throw new InvalidOperationException("Database error occurred while creating manufacturing order.", dbEx);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error occurred during the manufacturing order creation.");
            throw new Exception("An unexpected error occurred during the manufacturing order creation.", ex);
        }

        // Return the production run ID and end date
        response.ProductionRunId = productionRunId;
        response.EndDate = endDate;
        return response;
    }

    public async Task<bool> IsManufactured(BOMNode node, bool ignoreSupplierProducts)
    {
        // Retrieve related SupplierProduct records, filtered by 'supplierPrefOrderId'
        List<SupplierProduct> supplierProducts = null;

        try
        {
            // Fetch supplier products based on the node's product
            supplierProducts = await _context.SupplierProducts
                .AsNoTracking()
                .Where(sp => sp.ProductId == node.Product.ProductId && sp.SupplierPrefOrderId == "10_MAIN_SUPPL"
                                                                    && sp.MinimumOrderQuantity != null)
                .ToListAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Problem in BOMNode.IsManufactured() for product {node.Product.ProductId}");
        }

        // Filter the supplierProducts by availability date
        supplierProducts = supplierProducts
            .Where(sp => sp.AvailableFromDate <= DateTime.UtcNow
                         && (sp.AvailableThruDate == null || sp.AvailableThruDate >= DateTime.UtcNow))
            .ToList();

        // Return true if node has children and either we are ignoring supplier products, or no supplier products exist
        var result = node.ChildrenNodes.Count > 0 && (ignoreSupplierProducts || !supplierProducts.Any());
        return result;
    }


    public bool IsWarehouseManaged(BOMNode node, string facilityId)
    {
        bool isWarehouseManaged = false;

        try
        {
            // Check if the product type is WIP (Work in Progress)
            if (node.Product.ProductTypeId == "WIP")
            {
                return false;
            }

            // Retrieve the list of product facilities
            List<ProductFacility> productFacilities = null;

            if (string.IsNullOrEmpty(facilityId))
            {
                // Get all facilities if no specific facility is provided
                productFacilities = node.Product.ProductFacilities.ToList();
            }
            else
            {
                // Filter product facilities by the provided facility ID
                productFacilities = node.Product.ProductFacilities
                    .Where(pf => pf.FacilityId == facilityId).ToList();
            }

            // If no facilities are found, check for substituted node facilities
            if ((productFacilities == null || !productFacilities.Any()) && node.SubstitutedNode?.Product != null)
            {
                if (string.IsNullOrEmpty(facilityId))
                {
                    productFacilities = node.SubstitutedNode.Product.ProductFacilities.ToList();
                }
                else
                {
                    productFacilities = node.SubstitutedNode.Product.ProductFacilities
                        .Where(pf => pf.FacilityId == facilityId).ToList();
                }
            }

            // Check if any facility has minimum stock and reorder quantity defined
            if (productFacilities != null && productFacilities.Any())
            {
                foreach (var productFacility in productFacilities)
                {
                    if (productFacility.MinimumStock.HasValue && productFacility.ReorderQuantity.HasValue)
                    {
                        isWarehouseManaged = true;
                        break;
                    }
                }
            }
        }
        catch (Exception e)
        {
            _logger.LogError(e, $"Problem in BOMNode.IsWarehouseManaged() for product {node.Product.ProductId}");
        }

        return isWarehouseManaged;
    }


    public async Task LoadParents(BOMNode node, string bomTypeId, DateTime inDate, List<ProductFeature> productFeatures)
    {
        if (node.Product == null)
            throw new InvalidOperationException("Product is null");

        // Set the current date if not provided
        inDate = inDate == default ? DateTime.UtcNow : inDate;

        node.BomTypeId = bomTypeId;

        try
        {
            // Fetch parent associations for the current product
            var parentAssocs = await _context.ProductAssocs
                .AsNoTracking()
                .Where(pa => pa.ProductIdTo == node.Product.ProductId && pa.ProductAssocTypeId == bomTypeId)
                .OrderBy(pa => pa.SequenceNum)
                .ToListAsync();

            // If no parent associations are found, check the substituted node
            if (!parentAssocs.Any() && node.SubstitutedNode != null)
            {
                parentAssocs = await _context.ProductAssocs
                    .AsNoTracking()
                    .Where(pa =>
                        pa.ProductIdTo == node.SubstitutedNode.Product.ProductId && pa.ProductAssocTypeId == bomTypeId)
                    .OrderBy(pa => pa.SequenceNum)
                    .ToListAsync();
            }

            // Store the parent associations in the node
            node.Children = new List<ProductAssoc>(parentAssocs);
            node.ChildrenNodes = new List<BOMNode>();

            // Process each parent association
            foreach (var parentAssoc in parentAssocs)
            {
                // Create a BOM node for the parent product
                var parentNode = await CreateBOMNode(parentAssoc.ProductId);

                if (parentNode != null)
                {
                    // Set up the parent-child relationship and recursively load the parents
                    parentNode.SetParentNode(node);
                    parentNode.SetTree(node.Tree);
                    await LoadParents(parentNode, bomTypeId, inDate, productFeatures);
                }

                node.ChildrenNodes.Add(parentNode);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading parents for product {ProductId}", node.Product.ProductId);
            throw new InvalidOperationException(
                $"Error loading parents for product {node.Product.ProductId}: {ex.Message}", ex);
        }
    }
}