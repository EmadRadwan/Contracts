using Application.Core;
using Application.Interfaces;
using Application.Shipments;
using Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Persistence;

namespace Application.Facilities;

public interface IPackService
{
    PackBulkItemsResult PackBulk(PackBulkItemsInput input);
    Task<PackItemsResult> PackItems(PackItemsInput input);
}

public class PackService : IPackService
{
    private readonly DataContext _context;
    private readonly ILogger _logger;
    private readonly ILogger<PackingSession> _logger2;
    private readonly IUtilityService _utilityService;
    private readonly IUserAccessor _userAccessor;
    private readonly IIssuanceService _issuanceService;
    private readonly IShipmentHelperService _shipmentHelperService;
    private readonly IPickListService _pickListService;


    public PackService(DataContext context, IUtilityService utilityService,
        ILogger<PackService> logger, IUserAccessor userAccessor, IIssuanceService issuanceService,
        IShipmentHelperService shipmentHelperService, IPickListService pickListService, ILogger<PackingSession> packingSessionLogger)
    {
        _context = context;
        _utilityService = utilityService;
        _logger = logger;
        _logger2 = packingSessionLogger;
        _userAccessor = userAccessor;
        _issuanceService = issuanceService;
        _shipmentHelperService = shipmentHelperService;
        _pickListService = pickListService;
    }

    /// <summary>
    /// Main function replicating Groovy's packBulk, 
    /// but no session fields. All data is in 'input' 
    /// or passed between functions as parameters.
    /// </summary>
    public PackBulkItemsResult PackBulk(PackBulkItemsInput input)
    {
        // ----------------- 1) Validate Mandatory -----------------
        if (string.IsNullOrEmpty(input.OrderId))
        {
            return PackBulkItemsResult.Error("orderId is required.");
        }

        if (string.IsNullOrEmpty(input.ShipGroupSeqId))
        {
            return PackBulkItemsResult.Error("shipGroupSeqId is required.");
        }

        // In Groovy, updateQuantity defaults to false if missing
        bool updateQuantity = input.UpdateQuantity;

        // If no lines => do nothing
        if (input.Lines == null || input.Lines.Count == 0)
        {
            return PackBulkItemsResult.Success();
        }

        // The instructions & picker are not stored. 
        // We'll pass them to AddOrIncreaseLine if needed.

        // ----------------- 2) Loop Over Lines -----------------
        foreach (var line in input.Lines)
        {
            // Groovy checks if line is selected => skip otherwise
            if (!line.IsSelected)
            {
                continue;
            }

            // parse order item seq
            var orderItemSeqId = line.OrderItemSeqId;
            // parse product ID => null if empty
            var productId = string.IsNullOrEmpty(line.ProductId) ? null : line.ProductId;

            // read package, quantity, weight from the line
            var pkgStr = line.PackageStr;
            var qtyStr = line.QuantityStr;
            var wgtStr = line.WeightStr;

            if (string.IsNullOrEmpty(wgtStr)) wgtStr = "0"; // fallback

            // 2.1) Parse packages (like 'pkgStr' in Groovy)
            string[] packages;
            if (!string.IsNullOrEmpty(pkgStr) && pkgStr.Contains(","))
            {
                packages = pkgStr.Split(',');
            }
            else
            {
                packages = new[] { pkgStr };
            }

            if (packages.Length == 0)
            {
                return PackBulkItemsResult.Error("No packages defined for a selected line.");
            }

            // 2.2) Parse quantity
            // If qtyStr is null => might do multi approach. We'll keep it simpler:
            var multiQuantities = new List<string>();
            if (!string.IsNullOrEmpty(qtyStr))
            {
                multiQuantities.Add(qtyStr);
            }
            else
            {
                // fallback to 0 if missing
                multiQuantities.Add("0");
            }

            // 2.3) Single array for weight
            var weights = new[] { wgtStr };

            // 2.4) Process each package
            for (int p = 0; p < packages.Length; p++)
            {
                var parsedQtyStr = (multiQuantities.Count > p) ? multiQuantities[p] : multiQuantities[0];
                if (string.IsNullOrEmpty(parsedQtyStr)) parsedQtyStr = "0";

                decimal quantity;
                decimal weightDec;
                int packageSeq;
                try
                {
                    quantity = decimal.Parse(parsedQtyStr);
                    packageSeq = int.Parse(packages[p]);
                    weightDec = decimal.Parse(weights[0]);
                }
                catch (Exception e)
                {
                    return PackBulkItemsResult.Error($"Error parsing quantity/package/weight: {e.Message}");
                }

                // parse numPackages => default 1
                int numPackages = 1;
                if (!string.IsNullOrEmpty(line.NumPackagesStr))
                {
                    if (int.TryParse(line.NumPackagesStr, out int np) && np > 0)
                    {
                        numPackages = np;
                    }
                }

                // 2.5) For each repeated package => call AddOrIncreaseLine
                for (int pkgCount = 0; pkgCount < numPackages; pkgCount++)
                {
                    try
                    {
                        // pass instructions & picker as parameters
                        // if you need them in the line logic
                        /*AddOrIncreaseLine(
                            input.OrderId,
                            orderItemSeqId,
                            input.ShipGroupSeqId,
                            productId,
                            quantity,
                            packageSeq + pkgCount,
                            weightDec,
                            updateQuantity,
                            input.HandlingInstructions,
                            input.PickerPartyId
                        );*/
                    }
                    catch (Exception ex)
                    {
                        return PackBulkItemsResult.Error(ex.Message);
                    }
                }
            }
        }

        return PackBulkItemsResult.Success();
    }

    public async Task<CompletePackingResult> CompletePacking(CompletePackingInput input)
    {
        try
        {
            var user = await _context.Users.SingleOrDefaultAsync(x => x.UserName == _userAccessor.GetUsername());
            var userLogin = _context.UserLogins.SingleOrDefault(x => x.UserLoginId == user.UserLoginId);
            // If a session was provided, use it; otherwise, create a new one.
            // (You could also require that the caller provide a session.)
            var session = input.PackingSession ?? new PackingSession(userLogin, input.FacilityId);

            // Set session properties from input.
            session.SetHandlingInstructions(input.HandlingInstructions);
            session.SetPickerPartyId(input.PickerPartyId);
            session.SetAdditionalShippingCharge(input.AdditionalShippingCharge);
            session.SetWeightUomId(input.WeightUomId);

            // Update the session's package weights and shipment box types.
            SetSessionPackageWeights(session, input.PackageWeights);
            SetSessionShipmentBoxTypes(session, input.BoxTypes);

            // Determine whether to force completion.
            bool force = input.ForceComplete;

            // Execute the complete routine on the session.
            var completePackingResult = await session.Complete(force);

            // Return an error if no items were packed.
            if (completePackingResult.ShipmentId == "EMPTY")
            {
                return CompletePackingResult.Error("No items were packed.");
            }
            else
            {
                // Return success with the shipmentId.
                return CompletePackingResult.Success(completePackingResult.ShipmentId);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error completing packing process");
            return CompletePackingResult.Error($"Exception: {ex.Message}");
        }
    }

    public decimal SetSessionPackageWeights(PackingSession session, IDictionary<string, string> packageWeights)
    {
        decimal shippableWeight = 0m;

        if (packageWeights != null && packageWeights.Count > 0)
        {
            foreach (var kvp in packageWeights)
            {
                string packageSeqId = kvp.Key;
                string packageWeightStr = kvp.Value;
                int seq = int.Parse(packageSeqId);

                if (!string.IsNullOrEmpty(packageWeightStr))
                {
                    decimal packageWeight = decimal.Parse(packageWeightStr);
                    session.SetPackageWeight(seq, packageWeight);
                    shippableWeight += packageWeight;
                }
                else
                {
                    session.SetPackageWeight(seq, null);
                }
            }
        }

        return shippableWeight;
    }


    public void SetSessionShipmentBoxTypes(PackingSession session, IDictionary<string, string> boxTypes)
    {
        if (boxTypes != null && boxTypes.Count > 0)
        {
            foreach (var entry in boxTypes)
            {
                string packageSeqId = entry.Key;
                string boxTypeStr = entry.Value;
                int seq = int.Parse(packageSeqId);
                if (!string.IsNullOrEmpty(boxTypeStr))
                {
                    session.SetShipmentBoxType(seq, boxTypeStr);
                }
                else
                {
                    session.SetShipmentBoxType(seq, null);
                }
            }
        }
    }

    public async Task<PackItemsResult> PackItems(PackItemsInput input)
    {
        var result = new PackItemsResult
        {
            PackedLines = new List<PackingSessionLineDto>()
        };

        try
        {
            if (string.IsNullOrEmpty(input.FacilityId))
                throw new ArgumentException("FacilityId is required.", nameof(input.FacilityId));

            // Get current user & userLogin
            var currentUser = await _context.Users
                .SingleOrDefaultAsync(x => x.UserName == _userAccessor.GetUsername());
            var userLogin = await _context.UserLogins
                .SingleOrDefaultAsync(x => x.PartyId == currentUser.PartyId);

            // Create PackingSession
            var packSession = new PackingSession(
                userLogin,
                input.FacilityId,
                input.PicklistBinId,
                input.OrderId,
                input.ShipGroupSeqId
            )
            {
                Context = _context,
                IssuanceService = _issuanceService,
                PickListService = _pickListService,
                ShipmentHelperService = _shipmentHelperService,
                UtilityService = _utilityService,
                Logger = _logger2
            };

            // Handle "orderId/shipGroup" format
            var orderId = input.OrderId;
            var shipGroupSeqId = input.ShipGroupSeqId;
            if (!string.IsNullOrEmpty(orderId) && string.IsNullOrEmpty(shipGroupSeqId) && orderId.Contains("/"))
            {
                var parts = orderId.Split('/');
                orderId = parts[0];
                shipGroupSeqId = parts[1];
            }
            else if (!string.IsNullOrEmpty(orderId) && string.IsNullOrEmpty(shipGroupSeqId))
            {
                shipGroupSeqId = "01";
            }

            // If picklistBin is specified, you can optionally load pending items
            if (!string.IsNullOrEmpty(input.PicklistBinId))
            {
                var bin = await _context.PicklistBins
                    .FirstOrDefaultAsync(b => b.PicklistBinId == input.PicklistBinId);
                if (bin != null)
                {
                    orderId = bin.PrimaryOrderId;
                    shipGroupSeqId = bin.PrimaryShipGroupSeqId;

                    // Example of pulling PENDING items to session if needed
                    // var pendingItems = ...
                    // packSession.AddItemInfo(pendingItems);
                }
            }

            // Bulk-pack items: call AddOrIncreaseLine for each line
            foreach (var lineDto in input.ItemsToPack)
            {
                await packSession.AddOrIncreaseLine(
                    lineDto.OrderId,
                    lineDto.OrderItemSeqId,
                    lineDto.ShipGroupSeqId,
                    lineDto.ProductId,
                    lineDto.Quantity,
                    lineDto.PackageSeqId,
                    lineDto.Weight,
                    update: false // If you'd rather replace the quantity, set true
                );
            }

            await packSession.Complete(true); // force completion
            
            result.Success = true;
            result.Message = "PackBulk completed successfully.";
            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in PackItems for facility {FacilityId}", input.FacilityId);
            return new PackItemsResult
            {
                Success = false,
                Message = $"Error packing items: {ex.Message}",
                PackedLines = new List<PackingSessionLineDto>()
            };
        }
    }
}