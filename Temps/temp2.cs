public async Task<OperationResult> CreateShipmentForFacilityAndShipGroup(
    OrderHeader orderHeader,
    string facilityId,
    List<OrderItemShipGroup> orderItemShipGroupList,
    DateTime eventDate,
    string? setPackedOnly = null)
{
    var shipmentIds = new List<string>();

    foreach (var orderItemShipGroup in orderItemShipGroupList)
    {
        try
        {
            // Retrieve approved items for this ship group
            var perShipGroupItemList = await (
                from oisga in _context.OrderItemShipGroupAssocs
                join oi in _context.OrderItems on new { oisga.OrderId, oisga.OrderItemSeqId } equals new
                    { oi.OrderId, oi.OrderItemSeqId }
                where oisga.OrderId == orderHeader.OrderId &&
                      oisga.ShipGroupSeqId == orderItemShipGroup.ShipGroupSeqId &&
                      oi.StatusId == "ITEM_APPROVED"
                select new
                {
                    oi.OrderId,
                    oi.OrderItemSeqId,
                    oisga.ShipGroupSeqId,
                    oi.Quantity,
                    oi.CancelQuantity,
                    oi.ProductId
                    // Include other necessary fields
                }).ToListAsync();

            if (!perShipGroupItemList.Any())
            {
                Console.WriteLine(
                    $"No items available to ship for shipGroupSeqId {orderItemShipGroup.ShipGroupSeqId}");
                continue;
            }

            // Retrieve facilityId from OrderItemShipGrpInvRes
            var orderItemShipGrpInvResFacilityId = await (
                from oisgir in _context.OrderItemShipGrpInvRes
                where oisgir.OrderId == orderHeader.OrderId &&
                      oisgir.ShipGroupSeqId == orderItemShipGroup.ShipGroupSeqId
                select oisgir.InventoryItem.FacilityId
            ).FirstOrDefaultAsync();

            if (string.IsNullOrEmpty(orderItemShipGrpInvResFacilityId) && orderHeader.OrderTypeId == "SALES_ORDER")
            {
                throw new Exception("FacilityId could not be determined from OrderItemShipGrpInvRes.");
            }

            // Initialize shipment context
            var shipmentContext = new ShipmentContext
            {
                PrimaryOrderId = orderHeader.OrderId,
                PrimaryShipGroupSeqId = orderItemShipGroup.ShipGroupSeqId,
            };

            if (orderHeader.OrderTypeId == "SALES_ORDER")
            {
                shipmentContext.StatusId = "SHIPMENT_INPUT";
                shipmentContext.OriginFacilityId = orderItemShipGrpInvResFacilityId;

                // Determine PartyIdFrom
                string partyIdFrom = null;

                if (!string.IsNullOrEmpty(orderItemShipGroup.VendorPartyId))
                {
                    partyIdFrom = orderItemShipGroup.VendorPartyId;
                }
                else
                {
                    // Get facility.ownerPartyId
                    var facilityOwnerPartyId = await _context.Facilities
                        .Where(f => f.FacilityId == orderItemShipGrpInvResFacilityId)
                        .Select(f => f.OwnerPartyId)
                        .FirstOrDefaultAsync();

                    if (!string.IsNullOrEmpty(facilityOwnerPartyId))
                    {
                        partyIdFrom = facilityOwnerPartyId;
                    }
                    else
                    {
                        // Get OrderRole with SHIP_FROM_VENDOR
                        var orderRolePartyId = await _context.OrderRoles
                            .Where(or => or.OrderId == orderHeader.OrderId && or.RoleTypeId == "SHIP_FROM_VENDOR")
                            .Select(or => or.PartyId)
                            .FirstOrDefaultAsync();

                        if (!string.IsNullOrEmpty(orderRolePartyId))
                        {
                            partyIdFrom = orderRolePartyId;
                        }
                        else
                        {
                            // Get OrderRole with BILL_FROM_VENDOR
                            orderRolePartyId = await _context.OrderRoles
                                .Where(or => or.OrderId == orderHeader.OrderId && or.RoleTypeId == "BILL_FROM_VENDOR")
                                .Select(or => or.PartyId)
                                .FirstOrDefaultAsync();

                            if (!string.IsNullOrEmpty(orderRolePartyId))
                            {
                                partyIdFrom = orderRolePartyId;
                            }
                            else
                            {
                                throw new Exception("PartyIdFrom could not be determined.");
                            }
                        }
                    }
                }

                shipmentContext.PartyIdFrom = partyIdFrom;
            }
            else if (orderHeader.OrderTypeId == "PURCHASE_ORDER")
            {
                shipmentContext.StatusId = "PURCH_SHIP_CREATED";
                shipmentContext.DestinationFacilityId = facilityId;
                shipmentContext.ShipmentTypeId = "PURCHASE_SHIPMENT";
            }
            else
            {
                throw new Exception($"Unsupported OrderTypeId: {orderHeader.OrderTypeId}");
            }

            // Create the shipment
            string shipmentId;
            try
            {
                shipmentId = await _shipmentHelperService.CreateShipment(shipmentContext);
            }
            catch (Exception ex)
            {
                throw new Exception("Failed to create shipment.", ex);
            }

            // Update shipment status to PACKED for Sales Orders only
            if (orderHeader.OrderTypeId == "SALES_ORDER")
            {
                var packedContext = new ShipmentUpdateParameters
                {
                    ShipmentId = shipmentId,
                    StatusId = "SHIPMENT_PACKED"
                };

                try
                {
                    await _shipmentHelperService.UpdateShipment(packedContext);
                }
                catch (Exception ex)
                {
                    throw new Exception("Failed to update shipment status to PACKED.", ex);
                }
            }

            // Conditionally update shipment status to SHIPPED or PURCH_SHIP_SHIPPED
            var finalStatusContext = new ShipmentUpdateParameters
            {
                ShipmentId = shipmentId
            };

            if (orderHeader.OrderTypeId == "SALES_ORDER" && string.IsNullOrEmpty(setPackedOnly))
            {
                finalStatusContext.StatusId = "SHIPMENT_SHIPPED";
            }
            else if (orderHeader.OrderTypeId == "PURCHASE_ORDER")
            {
                finalStatusContext.StatusId = "PURCH_SHIP_SHIPPED";
            }

            if (!string.IsNullOrEmpty(finalStatusContext.StatusId))
            {
                try
                {
                    await _shipmentHelperService.UpdateShipment(finalStatusContext);
                }
                catch (Exception ex)
                {
                    throw new Exception($"Failed to update shipment status to {finalStatusContext.StatusId}.", ex);
                }
            }

            // Add the shipment ID to the list
            shipmentIds.Add(shipmentId);
        }
        catch (Exception ex)
        {
            // Handle exception and continue with the next ship group
            Console.WriteLine($"Failed to process ship group {orderItemShipGroup.ShipGroupSeqId}: {ex.Message}");
            return OperationResult.Failure(
                $"Failed to process ship group {orderItemShipGroup.ShipGroupSeqId}: {ex.Message}");
        }
    }

    return OperationResult.Success("Shipments created successfully.", shipmentIds);
}
