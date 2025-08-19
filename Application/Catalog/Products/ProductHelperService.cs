using Application.Facilities;
using Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Persistence;
using Application.Catalog.ProductStores; 


namespace Application.Catalog.Products;

public interface IProductHelperService
{
    
    Task<List<Domain.Product>> FindSelectedVariant(List<SelectedFeature> selectedFeatures, string productId);

    Task<GetAssociatedProductsResponse> ProdFindAssociatedByType(string productId, string productIdTo, string type,
        bool? checkViewAllow = false, string prodCatalogId = null, bool? bidirectional = false,
        bool? sortDescending = false);
}

public class ProductHelperService : IProductHelperService
{
    private readonly DataContext _context;
    private readonly ILogger _logger;

    public ProductHelperService(DataContext context, ILogger<ProductHelperService> logger)
    {
        _context = context;
        _logger = logger;
    }


   

    /*public async Task<ProductVariantTreeResponse> GetProductVariantTree(
        string productId,
        List<string> featureOrder,
        string productStoreId = null,
        bool checkInventory = true)
    {
        var response = new ProductVariantTreeResponse();
        var locale = CultureInfo.CurrentCulture;

        if (featureOrder == null || !featureOrder.Any())
        {
            response.ResponseMessage = "error";
            response.ErrorMessage = "Cannot find features list.";
            return response;
        }

        var variants = await GetAllProductVariants(productId);
        var virtualVariant = new List<string>();

        if (variants == null || !variants.AssocProducts.Any())
        {
            response.ResponseMessage = "success";
            return response;
        }

        var items = new List<string>();
        var outOfStockItems = new List<Product>();

        foreach (var variant in variants.AssocProducts)
        {
            var productIdTo = variant.ProductIdTo;
            var productTo = await _context.Products.FirstOrDefaultAsync(p => p.ProductId == productIdTo);

            if (productTo == null)
            {
                _logger.LogWarning($"Could not find associated variant with ID {productIdTo}, not showing in list.");
                continue;
            }

            var now = DateTime.UtcNow;

            // Check introductionDate
            if (productTo.IntroductionDate != null && now < productTo.IntroductionDate)
            {
                _logger.LogError(
                    $"Product {productTo.ProductName} (productId: {productTo.ProductId}) has not yet been made available for sale.");
                continue;
            }

            // Check salesDiscontinuationDate
            if (productTo.SalesDiscontinuationDate != null && now > productTo.SalesDiscontinuationDate)
            {
                _logger.LogError(
                    $"Product {productTo.ProductName} (productId: {productTo.ProductId}) is no longer available for sale.");
                continue;
            }

            // Inventory check, similar to the Java version's logic
            if (checkInventory)
            {
                var invReqResult =
                    _productStoreService.IsStoreInventoryAvailableOrNotRequired(productStoreId, productIdTo);

                if (invReqResult == null || invReqResult.IsError)
                {
                    response.ResponseMessage = "error";
                    response.ErrorMessage = "Error calling isStoreInventoryAvailableOrNotRequired service.";
                    return response;
                }

                if (invReqResult.IsAvailable)
                {
                    items.Add(productIdTo);
                    if (productTo.IsVirtual == "Y")
                    {
                        virtualVariant.Add(productIdTo);
                    }
                }
                else
                {
                    outOfStockItems.Add(productTo);
                }
            }
            else
            {
                items.Add(productIdTo);
                if (productTo.IsVirtual == "Y")
                {
                    virtualVariant.Add(productIdTo);
                }
            }
        }

        // Make selectable feature list
        var selectableFeatures = await (from pf in _context.ProductFeatures
            join pfa in _context.ProductFeatureAppls
                on pf.ProductFeatureId equals pfa.ProductFeatureId
            where pfa.ProductId == productId
                  && pfa.ProductFeatureApplTypeId == "SELECTABLE_FEATURE"
                  && pfa.FromDate <= DateTime.UtcNow
                  && (pfa.ThruDate == null || pfa.ThruDate >= DateTime.UtcNow)
            orderby pfa.SequenceNum
            select new
            {
                pf.ProductFeatureTypeId,
                pf.Description
            }).ToListAsync();

        var features = new Dictionary<string, List<string>>();
        foreach (var feature in selectableFeatures)
        {
            var featureType = feature.ProductFeatureTypeId;
            var featureDescription = feature.Description;

            if (!features.ContainsKey(featureType))
            {
                features[featureType] = new List<string> { featureDescription };
            }
            else
            {
                features[featureType].Add(featureDescription);
            }
        }

        // Generate tree
        Dictionary<string, List<string>> tree = null;
        try
        {
            tree = MakeGroup(features, items, featureOrder, 0);
        }
        catch (Exception e)
        {
            response.ResponseMessage = "error";
            response.ErrorMessage = e.Message;
            return response;
        }

        if (tree == null || !tree.Any())
        {
            response.ResponseMessage = "error";
            response.ErrorMessage = "Feature grouping came back empty.";
        }
        else
        {
            response.VariantTree = tree;
            response.VirtualVariant = virtualVariant;
        }

        // Create a sample variant
        Dictionary<string, Product> sample = null;
        try
        {
            sample = await MakeVariantSample(features, items, featureOrder.First());
        }
        catch (Exception e)
        {
            response.ResponseMessage = "error";
            response.ErrorMessage = e.Message;
            return response;
        }

        if (outOfStockItems.Any())
        {
            response.UnavailableVariants = outOfStockItems;
        }

        response.VariantSample = sample;
        response.ResponseMessage = "success";

        return response;
    }*/

    public async Task<GetAllProductVariantsResponse> GetAllProductVariants(string productId)
    {
        var response = new GetAllProductVariantsResponse();
        var assocProductsResponse = await FindAssociatedProductsByType(
            productId, // productId
            null, // productIdTo, since we're only using productId in this context
            "PRODUCT_VARIANT", // type for the association
            false, // checkViewAllow
            null, // prodCatalogId
            false, // bidirectional
            false // sortDescending
        );

        if (assocProductsResponse == null || !assocProductsResponse.AssocProducts.Any())
        {
            _logger.LogWarning($"No associated products found for product ID {productId}");
            return response;
        }

        response.AssocProducts = assocProductsResponse.AssocProducts;
        response.ResponseMessage = "success";
        return response;
    }

    public async Task<GetAssociatedProductsResponse> FindAssociatedProductsByType(
        string productId,
        string productIdTo,
        string type,
        bool checkViewAllow = false,
        string prodCatalogId = null,
        bool bidirectional = false,
        bool sortDescending = false)
    {
        var response = new GetAssociatedProductsResponse();

        if (string.IsNullOrEmpty(productId) && string.IsNullOrEmpty(productIdTo))
        {
            response.ResponseMessage = "error";
            response.ErrorMessage = "Both productId and productIdTo cannot be null.";
            return response;
        }

        if (!string.IsNullOrEmpty(productId) && !string.IsNullOrEmpty(productIdTo))
        {
            response.ResponseMessage = "error";
            response.ErrorMessage = "Both productId and productIdTo cannot be defined.";
            return response;
        }

        productId = productId ?? productIdTo;

        var product = await _context.Products.FirstOrDefaultAsync(p => p.ProductId == productId);

        if (product == null)
        {
            response.ResponseMessage = "error";
            response.ErrorMessage = $"Product with ID {productId} not found.";
            return response;
        }

        try
        {
            List<ProductAssoc> productAssocs = null;

            // Bidirectional logic - checking both productId and productIdTo
            if (bidirectional)
            {
                productAssocs = await _context.ProductAssocs
                    .Where(pa => (pa.ProductId == productId || pa.ProductIdTo == productId)
                                 && pa.ProductAssocTypeId == type)
                    .OrderBy(pa => sortDescending ? pa.SequenceNum * -1 : pa.SequenceNum)
                    .ToListAsync();
            }
            else
            {
                // Only consider ProductId or ProductIdTo
                productAssocs = await _context.ProductAssocs
                    .Where(pa => pa.ProductId == productId && pa.ProductAssocTypeId == type)
                    .OrderBy(pa => sortDescending ? pa.SequenceNum * -1 : pa.SequenceNum)
                    .ToListAsync();
            }

            // Filter by date (assuming FromDate and ThruDate are DateTime? fields)
            productAssocs = productAssocs
                .Where(pa => pa.FromDate <= DateTime.UtcNow && (pa.ThruDate == null || pa.ThruDate >= DateTime.UtcNow))
                .ToList();

            // Optional view check logic based on the catalog
            if (checkViewAllow && !string.IsNullOrEmpty(prodCatalogId))
            {
                var viewCategory = await _context.ProdCatalogCategories
                    .Where(c => c.ProdCatalogId == prodCatalogId && c.ProdCatalogCategoryTypeId == "PCCT_VIEW_ALLW")
                    .OrderBy(c => c.SequenceNum)
                    .Select(c => c.ProductCategoryId)
                    .FirstOrDefaultAsync();

                if (!string.IsNullOrEmpty(viewCategory))
                {
                    // Filter the products based on view allow category (adjust based on your schema)
                    productAssocs = productAssocs
                        .Where(pa => _context.ProductCategoryMembers
                            .Any(cm => cm.ProductId == pa.ProductId && cm.ProductCategoryId == viewCategory))
                        .ToList();
                }
            }

            response.AssocProducts = productAssocs;
            response.ResponseMessage = "success";
        }
        catch (Exception ex)
        {
            response.ResponseMessage = "error";
            response.ErrorMessage = $"An error occurred while retrieving associated products: {ex.Message}";
        }

        return response;
    }

    private async Task<Dictionary<string, Domain.Product>> MakeVariantSample(
        Dictionary<string, List<string>> featureList,
        List<string> items,
        string feature)
    {
        var tempSample = new Dictionary<string, Domain.Product>();
        var sample = new Dictionary<string, Domain.Product>();

        foreach (var productId in items)
        {
            List<ProductFeatureAppl> features = null;

            try
            {
                // Get the features and filter out expired dates
                features = _context.ProductFeatureAppls
                    .Where(pfa => pfa.ProductId == productId
                                  && pfa.ProductFeature.ProductFeatureTypeId == feature
                                  && pfa.ProductFeatureApplTypeId == "STANDARD_FEATURE"
                                  && pfa.FromDate <= DateTime.UtcNow
                                  && (pfa.ThruDate == null || pfa.ThruDate >= DateTime.UtcNow))
                    .OrderBy(pfa => pfa.SequenceNum)
                    .ThenBy(pfa => pfa.ProductFeature.Description)
                    .ToList();
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Problem reading relation: {ex.Message}");
            }

            foreach (var featureAppl in features)
            {
                try
                {
                    var product = _context.Products
                        .FirstOrDefault(p => p.ProductId == productId);

                    if (product != null)
                    {
                        tempSample[featureAppl.ProductFeature.Description] = product;
                    }
                }
                catch (Exception ex)
                {
                    throw new InvalidOperationException($"Cannot get product entity: {ex.Message}");
                }
            }
        }

        // Sort the sample based on the feature list
        if (featureList.TryGetValue(feature, out var featureDescriptions))
        {
            foreach (var featureDesc in featureDescriptions)
            {
                if (tempSample.ContainsKey(featureDesc))
                {
                    sample[featureDesc] = tempSample[featureDesc];
                }
            }
        }

        return sample;
    }


    private Dictionary<string, List<string>> MakeGroup(
        Dictionary<string, List<string>> featureList,
        List<string> items,
        List<string> order,
        int index)
    {
        if (featureList == null)
        {
            throw new ArgumentException("Cannot build feature tree: featureList is null");
        }

        if (index < 0)
        {
            throw new ArgumentException($"Invalid index '{index}', min index '0'");
        }

        if (index + 1 > order.Count)
        {
            throw new ArgumentException($"Invalid index '{index}', max index '{order.Count - 1}'");
        }

        // Temporary group for holding the result
        var tempGroup = new Dictionary<string, List<string>>();
        var group = new Dictionary<string, List<string>>();
        string orderKey = order[index];

        // Loop through the items and create temporary groups
        foreach (var thisItem in items)
        {
            _logger?.LogDebug($"Processing item: {thisItem}");

            // Simulate fetching features (ProductFeatureAndAppl equivalent in your schema)
            var features = _context.ProductFeatures
                .Join(_context.ProductFeatureAppls,
                    pf => pf.ProductFeatureId,
                    pfa => pfa.ProductFeatureId,
                    (pf, pfa) => new { pf, pfa })
                .Where(x => x.pfa.ProductId == thisItem &&
                            x.pf.ProductFeatureTypeId == orderKey &&
                            x.pfa.ProductFeatureApplTypeId == "STANDARD_FEATURE" &&
                            x.pfa.FromDate <= DateTime.UtcNow &&
                            (x.pfa.ThruDate == null || x.pfa.ThruDate >= DateTime.UtcNow))
                .OrderBy(x => x.pfa.SequenceNum)
                .Select(x => x.pf.Description)
                .ToList();

            _logger?.LogDebug($"Features found: {string.Join(", ", features)}");

            // Add each item to the temporary group
            foreach (var feature in features)
            {
                if (tempGroup.ContainsKey(feature))
                {
                    var itemList = tempGroup[feature];
                    if (!itemList.Contains(thisItem))
                    {
                        itemList.Add(thisItem);
                    }
                }
                else
                {
                    tempGroup[feature] = new List<string> { thisItem };
                }
            }
        }

        _logger?.LogDebug($"Temporary group built: {string.Join(", ", tempGroup.Keys)}");

        // Sort the feature list based on the provided order
        if (!featureList.ContainsKey(orderKey))
        {
            throw new ArgumentException($"Cannot build feature tree: orderFeatureList is null for orderKey={orderKey}");
        }

        var orderFeatureList = featureList[orderKey];

        foreach (var featureStr in orderFeatureList)
        {
            if (tempGroup.ContainsKey(featureStr))
            {
                group[featureStr] = tempGroup[featureStr]; // Add the list to the group
            }
        }

        _logger?.LogDebug($"Final group: {string.Join(", ", group.Keys)}");

        // If there are no groups, return an empty dictionary
        if (group.Count == 0)
        {
            return group;
        }

        // If we are at the last index, return the group as is
        if (index + 1 == order.Count)
        {
            return group;
        }

        // Recursively create subgroups for the next index
        foreach (var key in group.Keys.ToList())
        {
            var itemList = group[key];

            if (itemList != null && itemList.Any())
            {
                var subGroup = MakeGroup(featureList, itemList, order, index + 1);
                group[key] = subGroup.SelectMany(x => x.Value).ToList(); // Flatten the nested subgroup
            }
        }

        return group;
    }

    public async Task<GetAssociatedProductsResponse> ProdFindAssociatedByType(string productId, string productIdTo, string type, bool? checkViewAllow = false, string prodCatalogId = null, bool? bidirectional = false, bool? sortDescending = false)
{
    var response = new GetAssociatedProductsResponse();
    string errMsg = null;

    // Ensure that one and only one of productId or productIdTo is defined
    if (string.IsNullOrEmpty(productId) && string.IsNullOrEmpty(productIdTo))
    {
        response.ResponseMessage = "error";
        response.ErrorMessage = "Both productId and productIdTo cannot be null.";
        return response;
    }

    if (!string.IsNullOrEmpty(productId) && !string.IsNullOrEmpty(productIdTo))
    {
        response.ResponseMessage = "error";
        response.ErrorMessage = "Both productId and productIdTo cannot be defined.";
        return response;
    }

    // Use productId or productIdTo, default to productId if both are provided
    productId = productId ?? productIdTo;
    var product = await _context.Products.FirstOrDefaultAsync(p => p.ProductId == productId);

    if (product == null)
    {
        response.ResponseMessage = "error";
        response.ErrorMessage = "Product not found.";
        return response;
    }

    try
    {
        List<ProductAssoc> productAssocs;

        // Determine the sort order for the query
        var orderBy = sortDescending == true ? "sequenceNum DESC" : "sequenceNum";

        // Handle bidirectional associations
        if (bidirectional == true)
        {
            productAssocs = await _context.ProductAssocs
                .Where(pa => (pa.ProductId == productId || pa.ProductIdTo == productId) && pa.ProductAssocTypeId == type)
                .OrderBy(pa => orderBy)
                .ToListAsync();
        }
        else
        {
            // Query for associations from productId or to productId
            if (string.IsNullOrEmpty(productIdTo))
            {
                productAssocs = await _context.ProductAssocs
                    .Where(pa => pa.ProductId == productId && pa.ProductAssocTypeId == type)
                    .OrderBy(pa => orderBy)
                    .ToListAsync();
            }
            else
            {
                productAssocs = await _context.ProductAssocs
                    .Where(pa => pa.ProductIdTo == productId && pa.ProductAssocTypeId == type)
                    .OrderBy(pa => orderBy)
                    .ToListAsync();
            }
        }

        // Filter the associations by date
        productAssocs = productAssocs
            .Where(pa => pa.FromDate <= DateTime.UtcNow && (pa.ThruDate == null || pa.ThruDate >= DateTime.UtcNow))
            .ToList();

        // If checkViewAllow is true and prodCatalogId is provided, filter by product category
        if (checkViewAllow == true && !string.IsNullOrEmpty(prodCatalogId) && productAssocs.Any())
        {
            var viewProductCategoryId = await GetCatalogViewAllowCategoryId(prodCatalogId);
            if (!string.IsNullOrEmpty(viewProductCategoryId))
            {
                if (string.IsNullOrEmpty(productIdTo))
                {
                    var filteredCategories = await FilterProductsInCategory(viewProductCategoryId, "ProductIdTo", prodCatalogId);
                    productAssocs = productAssocs
                        .Where(pa => filteredCategories.Any(fc => fc.ProductCategoryId == pa.ProductIdTo))
                        .ToList();
                }
                else
                {
                    var filteredCategories = await FilterProductsInCategory(viewProductCategoryId, "ProductId", prodCatalogId);
                    productAssocs = productAssocs
                        .Where(pa => filteredCategories.Any(fc => fc.ProductCategoryId == pa.ProductId))
                        .ToList();
                }
            }
        }

        // Return the associated products
        response.AssocProducts = productAssocs;
        response.ResponseMessage = "success";
    }
    catch (Exception ex)
    {
        response.ResponseMessage = "error";
        response.ErrorMessage = $"An error occurred while retrieving associated products: {ex.Message}";
    }

    return response;
}

    public async Task<string> GetCatalogViewAllowCategoryId(string prodCatalogId)
    {
        if (string.IsNullOrEmpty(prodCatalogId))
        {
            return null;
        }

        var prodCatalogCategories = await GetProdCatalogCategories(prodCatalogId, "PCCT_VIEW_ALLW");
        if (prodCatalogCategories != null && prodCatalogCategories.Any())
        {
            var prodCatalogCategory = prodCatalogCategories.FirstOrDefault();
            return prodCatalogCategory?.ProductCategoryId;
        }
        else
        {
            return null;
        }
    }

    public async Task<List<ProdCatalogCategory>> GetProdCatalogCategories(string prodCatalogId,
        string prodCatalogCategoryTypeId)
    {
        try
        {
            // Fetch ProdCatalogCategories filtered by prodCatalogId
            var prodCatalogCategories = await _context.ProdCatalogCategories
                .Where(pcc => pcc.ProdCatalogId == prodCatalogId)
                .OrderBy(pcc => pcc.SequenceNum)
                .ThenBy(pcc => pcc.ProductCategoryId)
                .ToListAsync();

            // Filter the categories by prodCatalogCategoryTypeId if provided
            if (!string.IsNullOrEmpty(prodCatalogCategoryTypeId) && prodCatalogCategories.Any())
            {
                prodCatalogCategories = prodCatalogCategories
                    .Where(pcc => pcc.ProdCatalogCategoryTypeId == prodCatalogCategoryTypeId)
                    .ToList();
            }

            return prodCatalogCategories;
        }
        catch (Exception ex)
        {
            // Log the error (assuming _logger is defined)
            _logger?.LogError(ex, $"Error looking up ProdCatalogCategories for prodCatalog with id {prodCatalogId}");
            return null;
        }
    }

    public async Task<List<ProductCategory>> FilterProductsInCategory(
        string productCategoryId, string productIdFieldName = "ProductId", string prodCatalogId = null,
        string prodCatalogCategoryTypeId = null)
    {
        var newList = new List<ProductCategory>();

        // If productCategoryId is null, return an empty list
        if (string.IsNullOrEmpty(productCategoryId))
        {
            return newList;
        }

        // Get Product Catalog Categories
        var prodCatalogCategories = await GetProdCatalogCategories(prodCatalogId, prodCatalogCategoryTypeId);

        // If productIdFieldName is not passed, default it to "ProductId"
        var valueObjects = await _context.ProductCategoryMembers.ToListAsync();

        if (valueObjects == null)
        {
            return newList;
        }

        // Loop through each valueObject and check if the product is in the category
        foreach (var curValue in valueObjects)
        {
            var productId = curValue.GetType().GetProperty(productIdFieldName)?.GetValue(curValue)?.ToString();
            if (!string.IsNullOrEmpty(productId) && await IsProductInCategory(productId, productCategoryId))
            {
                newList.Add(curValue
                    .ProductCategory); // Assuming ProductCategoryMember has a ProductCategory navigation property
            }
        }

        return newList;
    }

    
    // Helper method to check if a product is in a category
    private async Task<bool> IsProductInCategory(string productId, string productCategoryId)
    {
        if (string.IsNullOrEmpty(productId) || string.IsNullOrEmpty(productCategoryId))
        {
            return false;
        }

        var productCategoryMembers = await _context.ProductCategoryMembers
            .Where(pcm => pcm.ProductCategoryId == productCategoryId && pcm.ProductId == productId)
            .ToListAsync();

        if (productCategoryMembers.Count > 0)
        {
            return true;
        }

        // If the product is a variant, check the virtual product
        var product = await _context.Products.FirstOrDefaultAsync(p => p.ProductId == productId);
        var productAssocs = await _context.ProductAssocs
            .Where(pa => pa.ProductId == productId)
            .ToListAsync();

        foreach (var productAssoc in productAssocs)
        {
            if (await IsProductInCategory(productAssoc.ProductIdTo, productCategoryId))
            {
                return true;
            }
        }

        return false;
    }
    
    public async Task<List<Domain.Product>> FindSelectedVariant(List<SelectedFeature> selectedFeatures, string productId)
    {
        var matchingProducts = new List<Domain.Product>();

        // Retrieve all variants for the product
        var variantsResponse = await FindAllVariants(productId); // Fetch all product variants
        var allVariants = variantsResponse.Variants; // Extract the list of variants

        foreach (var variant in allVariants)
        {
            var variantProductId = variant.ProductIdTo; // Get the variant's product ID

            // Retrieve all the standard features for the variant
            var variantFeatures = await ProdFindFeatureTypes(variantProductId, "STANDARD_FEATURE");

            bool doesVariantMatch = true; // Assume that the variant matches by default

            // Check if the variant matches the selected features
            foreach (var feature in variantFeatures)
            {
                var featureTypeId = feature.ProductFeatureTypeId; // Get the feature type
                var featureId = feature.ProductFeatureId; // Get the feature ID

                // Compare the selected features with the variant's features
                var matchingFeature = selectedFeatures.FirstOrDefault(sf => sf.FeatureTypeId == featureTypeId);
                if (matchingFeature != null && matchingFeature.FeatureId != featureId)
                {
                    doesVariantMatch = false; // The variant does not match the selected feature
                    break;
                }
            }

            // Add the product corresponding to this variant if it matches
            if (doesVariantMatch)
            {
                var matchingProduct = await _context.Products.FirstOrDefaultAsync(p => p.ProductId == variantProductId);
                if (matchingProduct != null)
                {
                    matchingProducts.Add(matchingProduct); // Add the matching product to the result list
                }
            }
        }

        // Return the list of matching products
        return matchingProducts;
    }

    public async Task<List<ProductFeatureDto>> ProdFindFeatureTypes(string productId, string productFeatureApplTypeId = null)
    {
        var defaultFeatureApplTypeId = "SELECTABLE_FEATURE";
        productFeatureApplTypeId ??= defaultFeatureApplTypeId;

        var featureList = new List<ProductFeatureDto>();

        try
        {
            // Query the ProductFeature and ProductFeatureAppl tables directly
            var features = await (
                from pf in _context.ProductFeatures
                join pfa in _context.ProductFeatureAppls
                    on pf.ProductFeatureId equals pfa.ProductFeatureId
                where pfa.ProductId == productId
                      && pfa.ProductFeatureApplTypeId == productFeatureApplTypeId
                      && pfa.FromDate <= DateTime.UtcNow
                      && (pfa.ThruDate == null || pfa.ThruDate >= DateTime.UtcNow)
                orderby pfa.SequenceNum, pf.ProductFeatureTypeId
                select new ProductFeatureDto
                {
                    ProductFeatureId = pf.ProductFeatureId,
                    ProductFeatureTypeId = pf.ProductFeatureTypeId,
                    ProductFeatureCategoryId = pf.ProductFeatureCategoryId,
                    Description = pf.Description,
                    UomId = pf.UomId,
                    SequenceNum = (int)pfa.SequenceNum
                }
            ).ToListAsync();

            featureList.AddRange(features);
        }
        catch (Exception e)
        {
            var errMsg = $"Error reading product features for product {productId}: {e.Message}";
            throw new Exception(errMsg, e);
        }

        if (!featureList.Any())
        {
            var errMsg = $"No product features found for product {productId}.";
            throw new KeyNotFoundException(errMsg);
        }

        return featureList;
    }

    public async Task<VariantResponse> FindAllVariants(string productId)
    {
        // Prepare a context for the associated products query
        var associatedProductType = "PRODUCT_VARIANT";
        var associatedProducts = await FindAssociatedProductsByType(productId, associatedProductType);
    
        return associatedProducts;
    }

    public async Task<VariantResponse> FindAssociatedProductsByType(string productId, string type)
    {
        // Query the associated products by type (e.g., "PRODUCT_VARIANT")
        var variants = await (
            from assoc in _context.ProductAssocs
            where assoc.ProductId == productId && assoc.ProductAssocTypeId == type
            select new VariantProduct
            {
                ProductIdTo = assoc.ProductIdTo,
                ProductAssocTypeId = assoc.ProductAssocTypeId
            }
        ).ToListAsync();

        return new VariantResponse { Variants = variants };
    }

}