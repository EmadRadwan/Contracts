using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Application.Catalog.Products.Services.Inventory;
using Application.Facilities.PhysicalInventories;
using Application.Interfaces;
using Domain;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Persistence;

namespace Application.Facilities.FacilityInventories
{
    public class GetInventoryAvailableByFacility
    {
        public class Query : IRequest<Result<List<ProductInventoryItemCombined>>>
        {
            public string FacilityId { get; set; }
            public string ProductId { get; set; }
        }

        public class Handler : IRequestHandler<Query, Result<List<ProductInventoryItemCombined>>>
        {
            private readonly DataContext _context;
            private readonly IInventoryHelperService _inventoryHelperService;

            public Handler(DataContext context, IInventoryHelperService inventoryHelperService)
            {
                _context = context;
                _inventoryHelperService = inventoryHelperService;
            }

            public async Task<Result<List<ProductInventoryItemCombined>>> Handle(Query request,
                CancellationToken cancellationToken)
            {
                // Build query to fetch inventory items based on facilityId and productId
                var inventoryItems = await (
                    from ii in _context.InventoryItems
                    join pr in _context.Products on ii.ProductId equals pr.ProductId
                    where ii.FacilityId == request.FacilityId
                          && ii.InventoryItemTypeId == "NON_SERIAL_INV_ITEM"
                          && (string.IsNullOrEmpty(request.ProductId) ||
                              pr.ProductId == request.ProductId.Trim())
                    select new
                    {
                        ii.InventoryItemId,
                        pr.ProductId,
                        pr.InternalName,
                        ii.QuantityOnHandTotal,
                        ii.AvailableToPromiseTotal,
                        ii.AccountingQuantityTotal,
                        ii.StatusId,
                        ii.FacilityId,
                        ii.Comments
                    }).ToListAsync(cancellationToken);

                var productInventoryCombined = new List<ProductInventoryItemCombined>();

                if (inventoryItems.Any())
                {
                    var firstItem = inventoryItems.First();
                    var productTotals =
                        await _inventoryHelperService.GetInventoryAvailableByFacility(firstItem.FacilityId,
                            firstItem.ProductId);

                    if (productTotals.IsSuccess)
                    {
                        foreach (var item in inventoryItems)
                        {
                            productInventoryCombined.Add(new ProductInventoryItemCombined
                            {
                                InventoryItemId = item.InventoryItemId,
                                ProductId = item.ProductId,
                                InternalName = item.InternalName,
                                ItemATP = item.AvailableToPromiseTotal,
                                ItemQOH = item.QuantityOnHandTotal,
                                ProductATP = productTotals.Value.AvailableToPromiseTotal,
                                ProductQOH = productTotals.Value.QuantityOnHandTotal,
                            });
                        }
                    }
                }

                return Result<List<ProductInventoryItemCombined>>.Success(productInventoryCombined);
            }
        }
    }
}