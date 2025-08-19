using Application.Shipments.OrganizationGlSettings;
using Application.Catalog.ProductPromos;
using Application.Catalog.Products;
using Application.Catalog.Products.Services.Inventory;
using Application.Interfaces;
using Application.Uoms;
using Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Persistence;

namespace Application.Catalog.ProductStores;

public interface IProductStoreService
{
    Task<ProductStore> GetProductStoreForLoggedInUser();
    Task<string> GetPurchaseOrderIdPrefix();
    Task<string> GetSalesOrderIdPrefix();
    Task<string> GetProductStoreDefaultCurrencyId();
    Task<string> GetProductStoreReserveOrderEnumId();
    Task<string> GetProductStorePayToPartId();
    Task<string> GetProductFacilityId();
    Task<CurrencyDto> GetAcctgBaseCurrencyId();

    Task<List<ProductPromoDto>> GetProductPromos();

    Task<List<AvailableProductPromoDto>> GetAvailableProductPromotions(string productId);

    Task<List<ProductStorePaymentSettingDto>> GetProductStorePaymentSettings();
    
}

public class ProductStoreService : IProductStoreService
{
    private readonly IUserAccessor _userAccessor;
    private readonly DataContext _context;
    private readonly ILogger<ProductStoreService> _logger;

    public ProductStoreService(DataContext context, IUserAccessor userAccessor, ILogger<ProductStoreService> logger) 
    {
        _userAccessor = userAccessor;
        _context = context;
        _logger = logger;
    }

    public async Task<string> GetProductStorePayToPartId()
    {
        var productStore = await GetProductStoreForLoggedInUser();
        return productStore.PayToPartyId;
    }

    public async Task<ProductStore> GetProductStoreForLoggedInUser()
    {
        // get the user
        var user = await _context.Users.SingleOrDefaultAsync(x => x.UserName == _userAccessor.GetUsername());
        // todo: if more than one store, need to select it based on store roles for logged in user
        var productStore = await _context.ProductStores.SingleOrDefaultAsync();
        return productStore;
    }

    public async Task<string> GetPurchaseOrderIdPrefix()
    {
        var productStore = await GetProductStoreForLoggedInUser();
        return productStore.POrderNumberPrefix;
    }

    public async Task<string> GetSalesOrderIdPrefix()
    {
        var productStore = await GetProductStoreForLoggedInUser();
        return productStore.SOrderNumberPrefix;
    }

    public async Task<string> GetProductStoreDefaultCurrencyId()
    {
        var productStore = await GetProductStoreForLoggedInUser();
        return productStore.DefaultCurrencyUomId;
    }

    public async Task<string> GetProductStoreReserveOrderEnumId()
    {
        var productStore = await GetProductStoreForLoggedInUser();
        return productStore.ReserveOrderEnumId;
    }

    public async Task<List<AvailableProductPromoDto>> GetAvailableProductPromotions(string productId)
    {
        var productStore = await GetProductStoreForLoggedInUser();
        var promoDataByProduct = (from promo in _context.ProductPromos
            join promoAction in _context.ProductPromoActions on promo.ProductPromoId equals promoAction
                .ProductPromoId
            join promoProduct in _context.ProductPromoProducts on promo.ProductPromoId equals promoProduct
                .ProductPromoId
            join promoStoreAppl in _context.ProductStorePromoAppls on promo.ProductPromoId equals promoStoreAppl
                .ProductPromoId
            join product in _context.Products on promoAction.ProductId ?? productId equals product.ProductId
            where promoStoreAppl.ThruDate == null && promoProduct.ProductId == productId &&
                  promoStoreAppl.ProductStoreId == productStore.ProductStoreId
            select new AvailableProductPromoDto
            {
                ProductPromoId = promo.ProductPromoId,
                ProductId = productId,
                PromoText = promo.PromoText + " for " + product.ProductName,
                ProductPromoActionEnumId = promoAction.ProductPromoActionEnumId
            }).ToList();
        return promoDataByProduct;
    }

    public async Task<List<ProductPromoDto>> GetProductPromos()
    {
        var productStore = await GetProductStoreForLoggedInUser();

        var result = await (from pp in _context.ProductPromos
            join ppr in _context.ProductPromoRules on pp.ProductPromoId equals ppr.ProductPromoId
            join pspa in _context.ProductStorePromoAppls on pp.ProductPromoId equals pspa.ProductPromoId
            join ppc in _context.ProductPromoConds on new { pp.ProductPromoId, ppr.ProductPromoRuleId } equals new
                { ppc.ProductPromoId, ppc.ProductPromoRuleId }
            join ppa in _context.ProductPromoActions on new { pp.ProductPromoId, ppr.ProductPromoRuleId } equals new
                { ppa.ProductPromoId, ppa.ProductPromoRuleId }
            join ppp in _context.ProductPromoProducts on new
            {
                pp.ProductPromoId, ppr.ProductPromoRuleId, ppa.ProductPromoActionSeqId, ppc.ProductPromoCondSeqId
            } equals new
            {
                ppp.ProductPromoId, ppp.ProductPromoRuleId, ppp.ProductPromoActionSeqId, ppp.ProductPromoCondSeqId
            }
            where pspa.ProductStoreId == productStore.ProductStoreId
            select new ProductPromoDto
            {
                ProductPromoId = pp.ProductPromoId,
                PromoName = pp.PromoName,
                PromoText = pp.PromoText,
                CreatedStamp = pp.CreatedStamp,
                LastUpdatedStamp = pp.LastUpdatedStamp,
                ProductPromoRuleId = ppr.ProductPromoRuleId,
                ProductPromoCondSeqId = ppc.ProductPromoCondSeqId,
                InputParamEnumId = ppc.InputParamEnumId,
                OperatorEnumId = ppc.OperatorEnumId,
                CondValue = ppc.CondValue,
                ProductPromoActionSeqId = ppa.ProductPromoActionSeqId,
                ProductPromoActionEnumId = ppa.ProductPromoActionEnumId,
                Quantity = ppa.Quantity,
                Amount = ppa.Amount,
                ProductId = ppp.ProductId,
                FromDate = pspa.FromDate,
                ThruDate = pspa.ThruDate,
                ProductCategoryId = null // Set ProductCategoryId to null
            }).ToListAsync();


        //var result = query.ToListAsync();

        // loop through the result and set ProductPromoActionEnumDescription from Enumerations
        foreach (var item in result)
        {
            item.ProductPromoActionEnumDescription = Enumerable.Where(
                _context.Enumerations,
                x => x.EnumId == item.ProductPromoActionEnumId
            ).Select(x => x.Description).FirstOrDefault();

            item.InputParamEnumDescription = Enumerable.Where(
                _context.Enumerations,
                x => x.EnumId == item.InputParamEnumId
            ).Select(x => x.Description).FirstOrDefault();

            item.OperatorEnumDescription = Enumerable.Where(
                _context.Enumerations,
                x => x.EnumId == item.OperatorEnumId
            ).Select(x => x.Description).FirstOrDefault();
        }


        // loop through the result and check if prop is used in order adjustments
        foreach (var item in result)
            item.IsUsed = Enumerable.Any(
                _context.OrderAdjustments,
                x => x.ProductPromoId == item.ProductPromoId
            );

        return result;
    }

    public async Task<string> GetProductFacilityId()
    {
        var productStore = await GetProductStoreForLoggedInUser();
        return productStore.InventoryFacilityId;
    }

    public async Task<ProductStore> GetProductStoreById(string productStoreId)
    {
        var productStore = await _context.ProductStores.SingleOrDefaultAsync(s => s.ProductStoreId == productStoreId);
        return productStore;
    }
    
    public async Task<CurrencyDto> GetAcctgBaseCurrencyId()
    {
        var store = await GetProductStoreForLoggedInUser();

        var companyId = store.PayToPartyId;

        var partyAccountingPreference = await (from pap in _context.PartyAcctgPreferences
            where pap.PartyId == companyId
            select new PartyAcctgPreferenceDto
            {
                PartyId = pap.PartyId,
                BaseCurrencyUomId = pap.BaseCurrencyUomId
            }).FirstOrDefaultAsync();

        var baseCurrency = partyAccountingPreference!.BaseCurrencyUomId;
        var query = await _context.Uoms
            .Where(z => z.UomTypeId == "CURRENCY_MEASURE" && z.UomId == baseCurrency)
            .Select(u => new CurrencyDto
            {
                CurrencyUomId = u.UomId,
                Description = u.Description!
            }).Take(1).FirstOrDefaultAsync();

        
        return query;
    }
    
    public async Task<List<ProductStorePaymentSettingDto>> GetProductStorePaymentSettings()
    {
        List<ProductStorePaymentSettingDto> storePayments;
        storePayments = new List<ProductStorePaymentSettingDto>();

        try
        {
            var productStore = await GetProductStoreForLoggedInUser();
            

            storePayments = await (from psps in _context.ProductStorePaymentSettings
                join pmt in _context.PaymentMethodTypes
                    on psps.PaymentMethodTypeId equals pmt.PaymentMethodTypeId
                where psps.ProductStoreId == productStore.ProductStoreId
                select new ProductStorePaymentSettingDto
                {
                    ProductStoreId = psps.ProductStoreId,
                    PaymentMethodTypeId = psps.PaymentMethodTypeId,
                    PaymentMethodTypeDescription = pmt.Description,
                }).ToListAsync();
        }
        catch (Exception ex)
        {
            // Catch all other exceptions
            _logger.LogError(ex, "An error occurred while looking up store payment settings.");
        }

        return storePayments; // Ensure the list is returned, even if empty
    }
    
    
}