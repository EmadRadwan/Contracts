using Application.Catalog.ProductStores;
using Application.Core;
using Application.Order.Quotes;
using Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Persistence;

namespace Application.order.Quotes;

public interface IQuoteService
{
    Task<List<QuoteAdjustmentDto2>> CalculateTax(QuoteItemDto2 quoteItem);
    Task<Quote> CreateQuote(QuoteDto quoteDto);
    Task<Quote> UpdateQuote(QuoteDto quoteDto);

    Task<Quote> ApproveQuote(QuoteDto quoteDto);
    Task<Quote> ChangeQuoteStatusAsOrdered(QuoteDto quoteDto);
}

public class QuoteService : IQuoteService
{
    private readonly DataContext _context;
    private readonly ILogger _logger;

    private readonly IProductStoreService _productStoreService;
    private readonly IUtilityService _utilityService;


    public QuoteService(DataContext context, IUtilityService utilityService,
        IProductStoreService productStoreService, ILogger<QuoteService> logger)
    {
        _context = context;
        _utilityService = utilityService;
        _productStoreService = productStoreService;
        _logger = logger;
    }

    public async Task<List<QuoteAdjustmentDto2>> CalculateTax(QuoteItemDto2 quoteItem)
    {
        var stamp = DateTime.UtcNow;

        var productStore = await _context.ProductStores.SingleOrDefaultAsync();

        var partyTaxAuth = await _context.PartyTaxAuthInfos
            .Where(x => x.TaxAuthGeoId == productStore.VatTaxAuthGeoId
                        && x.TaxAuthPartyId == productStore.VatTaxAuthPartyId
                        && x.ThruDate == null && x.PartyId == productStore.PayToPartyId)
            .FirstOrDefaultAsync();

        var taxAuthorityRateProducts = await _context.TaxAuthorityRateProducts
            .Where(x => x.TaxAuthGeoId == partyTaxAuth.TaxAuthGeoId
                        && x.TaxAuthPartyId == partyTaxAuth.TaxAuthPartyId
            )
            .ToListAsync();

        var newTaxAdjustments = new List<QuoteAdjustmentDto2>();

        foreach (var taxRate in taxAuthorityRateProducts)
            if (taxRate.TaxAuthorityRateTypeId == "SALES_TAX")
            {
                var taxRatePercentage = taxRate.TaxPercentage;

                var quoteItemTaxAmount = quoteItem.Quantity * quoteItem.UnitListPrice * taxRatePercentage / 100;

                var newQuoteAdjustment = new QuoteAdjustmentDto2
                {
                    QuoteAdjustmentId = Guid.NewGuid().ToString(),
                    CorrespondingProductId = quoteItem.ProductId,
                    CorrespondingProductName = quoteItem.ProductName,
                    QuoteAdjustmentTypeId = "SALES_TAX",
                    QuoteAdjustmentTypeDescription = "Sales Tax",
                    QuoteId = quoteItem.QuoteId,
                    QuoteItemSeqId = quoteItem.QuoteItemSeqId,
                    TaxAuthGeoId = taxRate.TaxAuthGeoId,
                    TaxAuthPartyId = taxRate.TaxAuthPartyId,
                    Amount = quoteItemTaxAmount,
                    SourcePercentage = taxRatePercentage,
                    Description = taxRate.Description,
                    IsManual = "N",
                    IsAdjustmentDeleted = false,
                    CreatedDate = stamp,
                    LastModifiedDate = stamp
                };

                newTaxAdjustments.Add(newQuoteAdjustment);
            }

        return newTaxAdjustments;
    }


    public async Task<Quote> CreateQuote(QuoteDto quoteDto)
    {
        var stamp = DateTime.UtcNow;

        // get quote new serial
        // log before operation
        _logger.LogDebug("Starting _utilityService.GetNextSequence Quote");
        var newQuoteSerial = await _utilityService.GetNextSequence("Quote");
        _logger.LogDebug("Finished _utilityService.GetNextSequence Quote {newQuoteSerial}", newQuoteSerial);
        // get default product store currency
        var productStoreDefaultCurrencyId = await _productStoreService.GetProductStoreDefaultCurrencyId();
        // get product store
        var productStore = await _productStoreService.GetProductStoreForLoggedInUser();

        //todo add created by, requires user id to be added to user_login table
        _logger.LogDebug("Starting AddQuote");
        var newQuote = new Quote
        {
            PartyId = quoteDto.FromPartyId,
            QuoteId = newQuoteSerial,
            StatusId = "QUO_CREATED",
            ProductStoreId = productStore.ProductStoreId,
            CurrencyUomId = productStoreDefaultCurrencyId,
            QuoteTypeId = "PRODUCT_QUOTE",
            CustomerRemarks = quoteDto.CustomerRemarks,
            InternalRemarks = quoteDto.InternalRemarks,
            CurrentMileage = quoteDto.CurrentMileage,
            IssueDate = stamp,
            GrandTotal = quoteDto.GrandTotal,
            ValidFromDate = stamp,
            ValidThruDate = stamp.AddDays(30),
            CreatedStamp = stamp,
            LastUpdatedStamp = stamp
        };
        _context.Quotes.Add(newQuote);
        _logger.LogDebug("Finished AddQuote {@newQuote}", newQuote);


        await CreateQuoteItems(quoteDto.QuoteItems, newQuoteSerial);
        await CreateQuoteAdjustments(quoteDto.QuoteAdjustments, newQuoteSerial);

        return await Task.FromResult(newQuote);
    }

    public async Task<Quote> UpdateQuote(QuoteDto quoteDto)
    {
        var stamp = DateTime.UtcNow;

        var quote = await GetQuoteById(quoteDto.QuoteId);

        _logger.LogDebug("Starting Actual Quote Update record");

        quote.PartyId = quoteDto.FromPartyId;
        quote.GrandTotal = quoteDto.GrandTotal;
        quote.CurrentMileage = quoteDto.CurrentMileage;
        quote.CustomerRemarks = quoteDto.CustomerRemarks;
        quote.InternalRemarks = quoteDto.InternalRemarks;
        quote.LastUpdatedStamp = stamp;

        _logger.LogDebug("Finished  Actual Quote Update record {@quote}", quote);


        // update quote items
        await UpdateQuoteItems(quoteDto.QuoteItems, quoteDto.QuoteId);
        await UpdateQuoteAdjustments(quoteDto.QuoteAdjustments, quoteDto.QuoteId);

        return quote;
    }

    public async Task<Quote> ApproveQuote(QuoteDto quoteDto)
    {
        var stamp = DateTime.UtcNow;

        var quote = await GetQuoteById(quoteDto.QuoteId);

        quote.LastUpdatedStamp = stamp;
        quote.StatusId = "QUOTE_APPROVED";


        // approve quote items
        await ApproveQuoteItems(quoteDto.QuoteItems);

        return quote;
    }

    public async Task<Quote> ChangeQuoteStatusAsOrdered(QuoteDto quoteDto)
    {
        var stamp = DateTime.UtcNow;

        var quote = await GetQuoteById(quoteDto.QuoteId);
        _logger.LogDebug("Starting Change quote status {@quote}", quote);

        quote.LastUpdatedStamp = stamp;
        quote.StatusId = "QUO_ORDERED";

        _logger.LogDebug("Finished Change quote status {@quote}", quote);

        return quote;
    }

    public async Task<Quote> GetQuoteById(string quoteId)
    {
        var quote = await _context.Quotes.SingleOrDefaultAsync(x => x.QuoteId == quoteId);

        if (quote == null) throw new ArgumentException("Quote not found", nameof(quoteId));

        return quote;
    }

    public async Task<List<QuoteItem>> GetQuoteItemsById(string quoteId)
    {
        var quoteItems = await _context.QuoteItems.Where(x => x.QuoteId == quoteId).ToListAsync();

        if (quoteItems == null) throw new ArgumentException("Quote items not found", nameof(quoteId));

        return quoteItems;
    }

    private async Task<QuoteAdjustment> GetQuoteAdjustmentById(string quoteAdjustmentId)
    {
        var quoteAdjustment = await _context.QuoteAdjustments.Where(x => x.QuoteAdjustmentId == quoteAdjustmentId)
            .FirstOrDefaultAsync();

        //if (quoteAdjustments == null) throw new ArgumentException("Quote adjustments not found", nameof(quoteId));

        return quoteAdjustment;
    }

    public async Task<QuoteItem> GetQuoteItemById(string quoteId, string quoteItemSeqId)
    {
        var quoteItem =
            await _context.QuoteItems.SingleOrDefaultAsync(x =>
                x.QuoteId == quoteId && x.QuoteItemSeqId == quoteItemSeqId);

        //if (quoteItem == null) throw new ArgumentException("Quote item not found", quoteId);

        return quoteItem;
    }

    public async Task CreateQuoteItems(List<QuoteItemDto2> quoteItems, string quoteId)
    {
        _logger.LogDebug("Starting CreateQuoteItems");
        var itemsToAdd = quoteItems.Where(qi => !qi.IsProductDeleted)
            .Select(qi => CreateQuoteItem(qi, quoteId))
            .ToList();
        _context.QuoteItems.AddRange(itemsToAdd);
        _logger.LogDebug("Finished CreateQuoteItems {itemsToAdd}", itemsToAdd);
    }

    public async Task CreateQuoteAdjustments(List<QuoteAdjustmentDto2> quoteAdjustments, string quoteId)
    {
        _logger.LogDebug("Starting CreateQuoteAdjustments");
        var adjustmentsToAdd = await Task.WhenAll(quoteAdjustments.Where(qa => !qa.IsAdjustmentDeleted)
            .Select(async qa => await CreateQuoteAdjustment(qa, quoteId)));

        _context.QuoteAdjustments.AddRange(adjustmentsToAdd);
        _logger.LogDebug("Finished CreateQuoteAdjustments {adjustmentsToAdd}", adjustmentsToAdd);
    }

    public QuoteItem CreateQuoteItem(QuoteItemDto2 quoteItem, string quoteId)
    {
        var stamp = DateTime.UtcNow;

        _logger.LogDebug("Starting CreateQuoteItem");
        var newItem = new QuoteItem
        {
            QuoteId = quoteId,
            QuoteItemSeqId = quoteItem.QuoteItemSeqId,
            ProductId = quoteItem.ProductId,
            Quantity = quoteItem.Quantity,
            QuoteUnitListPrice = quoteItem.UnitListPrice,
            QuoteUnitPrice =
                quoteItem
                    .UnitListPrice, // updated to use list price instead of unit price as it may include adjustments
            CreatedStamp = stamp,
            LastUpdatedStamp = stamp
        };
        _context.QuoteItems.Add(newItem);
        _logger.LogDebug("Finished CreateQuoteItem {newItem}", newItem);

        return newItem;
    }

    public async Task<QuoteAdjustment> CreateQuoteAdjustment(QuoteAdjustmentDto2 quoteAdjustment, string quoteId)
    {
        var stamp = DateTime.UtcNow;

        _logger.LogDebug("Starting _utilityService.GetNextSequence QuoteAdjustment");
        var newQuoteAdjustmentSerial = await _utilityService.GetNextSequence("QuoteAdjustment");
        _logger.LogDebug("Finished _utilityService.GetNextSequence QuoteAdjustment {newQuoteAdjustmentSerial}",
            newQuoteAdjustmentSerial);


        _logger.LogDebug("Starting CreateQuoteAdjustment");
        var newAdjustment = new QuoteAdjustment
        {
            QuoteAdjustmentId = newQuoteAdjustmentSerial,
            QuoteAdjustmentTypeId = quoteAdjustment.QuoteAdjustmentTypeId,
            QuoteId = quoteId,
            QuoteItemSeqId = quoteAdjustment.QuoteItemSeqId,
            CorrespondingProductId = quoteAdjustment.CorrespondingProductId,
            Description = quoteAdjustment.Description,
            Amount = quoteAdjustment.Amount,
            IsManual = quoteAdjustment.IsManual,
            SourcePercentage = quoteAdjustment.SourcePercentage,
            OverrideGlAccountId = quoteAdjustment.OverrideGlAccountId,
            TaxAuthGeoId = quoteAdjustment.TaxAuthGeoId,
            TaxAuthPartyId = quoteAdjustment.TaxAuthPartyId,
            LastUpdatedStamp = stamp,
            CreatedStamp = stamp
        };
        _context.QuoteAdjustments.Add(newAdjustment);
        return newAdjustment;
    }

    private async Task UpdateQuoteItems(List<QuoteItemDto2> updatedQuoteItems, string originalQuoteId)
    {
        _logger.LogDebug("Starting UpdateQuoteItems");

        foreach (var quoteItem in updatedQuoteItems) await UpdateQuoteItem(quoteItem, originalQuoteId);
        _logger.LogDebug("Finished UpdateQuoteItems");

        await Task.CompletedTask;
    }

    private async Task UpdateQuoteAdjustments(List<QuoteAdjustmentDto2> updatedQuoteAdjustments, string originalQuoteId)
    {
        _logger.LogDebug("Starting UpdateQuoteAdjustments");

        foreach (var quoteAdjustment in updatedQuoteAdjustments)
            await UpdateQuoteAdjustment(quoteAdjustment, originalQuoteId);
        _logger.LogDebug("Finished UpdateQuoteAdjustments");

        await Task.CompletedTask;
    }

    private async Task UpdateQuoteItem(QuoteItemDto2 updatedQuoteItem, string originalQuoteId)
    {
        // get quote items
        var savedQuoteItem = await GetQuoteItemById(updatedQuoteItem.QuoteId, updatedQuoteItem.QuoteItemSeqId);
        if (savedQuoteItem == null)
        {
            CreateQuoteItem(updatedQuoteItem, originalQuoteId);
            return;
        }

        if (updatedQuoteItem.IsProductDeleted) await DeleteQuoteItem(savedQuoteItem);

        savedQuoteItem.Quantity = updatedQuoteItem.Quantity;
        savedQuoteItem.QuoteUnitListPrice = updatedQuoteItem.UnitListPrice;
        savedQuoteItem.QuoteUnitPrice =
            updatedQuoteItem
                .UnitListPrice; // updated to use list price instead of unit price as it may include adjustments
        savedQuoteItem.LastUpdatedStamp = DateTime.UtcNow;

        await Task.CompletedTask;
    }

    private async Task UpdateQuoteAdjustment(QuoteAdjustmentDto2 updatedQuoteAdjustment, string originalQuoteId)
    {
        // get quote adjustment
        var savedQuoteAdjustment = await GetQuoteAdjustmentById(updatedQuoteAdjustment.QuoteAdjustmentId);
        if (savedQuoteAdjustment == null)
        {
            await CreateQuoteAdjustment(updatedQuoteAdjustment, originalQuoteId);
            return;
        }

        if (updatedQuoteAdjustment.IsAdjustmentDeleted) await DeleteQuoteAdjustment(savedQuoteAdjustment);

        savedQuoteAdjustment.Amount = updatedQuoteAdjustment.Amount;

        await Task.CompletedTask;
    }

    private async Task ApproveQuoteItems(List<QuoteItemDto2> updatedQuoteItems)
    {
        foreach (var quoteItem in updatedQuoteItems) await ApproveQuoteItem(quoteItem);

        await Task.CompletedTask;
    }

    private async Task ApproveQuoteItem(QuoteItemDto2 updatedQuoteItem)
    {
        // get quote items
        var savedQuoteItem = await GetQuoteItemById(updatedQuoteItem.QuoteId, updatedQuoteItem.QuoteItemSeqId);
        if (savedQuoteItem == null)
        {
            CreateQuoteItem(updatedQuoteItem, updatedQuoteItem.QuoteId);
            return;
        }

        if (updatedQuoteItem.IsProductDeleted) await DeleteQuoteItem(savedQuoteItem);

        savedQuoteItem.Quantity = updatedQuoteItem.Quantity;
        savedQuoteItem.QuoteUnitListPrice = updatedQuoteItem.UnitListPrice;
        savedQuoteItem.QuoteUnitPrice =
            updatedQuoteItem
                .UnitListPrice; // updated to use list price instead of unit price as it may include adjustments
        savedQuoteItem.LastUpdatedStamp = DateTime.UtcNow;

        await Task.CompletedTask;
    }

    private async Task DeleteQuoteItem(QuoteItem quoteItem)
    {
        // delete quote item
        _context.QuoteItems.Remove(quoteItem);
    }

    private async Task DeleteQuoteAdjustment(QuoteAdjustment quoteAdjustment)
    {
        // delete quote item
        _context.QuoteAdjustments.Remove(quoteAdjustment);
    }
    
    /*public async Task<string> UpdateQuote(UpdateQuoteDto parameters, UserLogin userLogin)
    {
        try
        {
            if (!Security.HasEntityPermission("ORDERMGR", "_UPDATE", userLogin))
            {
                return "You do not have permission to update quotes.";
            }

            var quote = await _context.Quotes.SingleOrDefaultAsync(q => q.QuoteId == parameters.QuoteId);
            if (quote == null) return "Quote not found.";

            if (string.IsNullOrEmpty(parameters.StatusId))
            {
                parameters.StatusId = quote.StatusId;
            }

            if (parameters.StatusId != quote.StatusId)
            {
                var validChange = await _context.StatusValidChanges
                    .SingleOrDefaultAsync(s => s.StatusId == quote.StatusId && s.StatusIdTo == parameters.StatusId);

                if (validChange == null)
                {
                    Console.WriteLine($"The status change from {quote.StatusId} to {parameters.StatusId} is not valid.");
                    return "The status change is not valid.";
                }
            }

            quote.StatusId = parameters.StatusId;
            quote.UpdateFromDto(parameters);
            _context.Quotes.Update(quote);
            await _context.SaveChangesAsync();

            return "Quote updated successfully.";
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error updating quote: {ex.Message}");
            throw;
        }
    }
    */

}