using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Persistence;

namespace Application.Accounting.Services;

public interface IPaymentWorkerService
{
    Task<List<Dictionary<string, object>>> GetPartyPaymentMethodValueMaps(string partyId, bool showOld = false);
    Task<Dictionary<string, object>> GetPaymentMethodAndRelated(string paymentMethodId, string donePage, bool hasError);
    Task<PostalAddress> GetPaymentAddress(string partyId);
    decimal GetPaymentsTotal(List<Payment> payments);
    Task<decimal> GetPaymentApplied(string paymentId, bool actual = false);
    Task<decimal> GetPaymentApplied(Payment payment, bool actual = false);
    Task<decimal> GetPaymentAppliedAmount(string paymentApplicationId);
    Task<decimal> GetPaymentNotApplied(Payment payment, bool actual = false);
    Task<decimal> GetPaymentNotApplied(string paymentId, bool actual = false);
}

public class PaymentWorkerService : IPaymentWorkerService
{
    private readonly DataContext _context;
    private readonly ILogger<PaymentWorkerService> _logger;
    private const int DECIMALS = 2;
    private const MidpointRounding ROUNDING_MODE = MidpointRounding.AwayFromZero;

    public PaymentWorkerService(DataContext context, ILogger<PaymentWorkerService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<List<Dictionary<string, object>>> GetPartyPaymentMethodValueMaps(string partyId,
        bool showOld = false)
    {
        var paymentMethodValueMaps = new List<Dictionary<string, object>>();

        try
        {
            var paymentMethods = await _context.PaymentMethods
                .Where(pm => pm.PartyId == partyId)
                .ToListAsync();

            if (!showOld)
            {
                paymentMethods = paymentMethods
                    .Where(pm => (pm.FromDate == null || pm.FromDate <= DateTime.UtcNow) &&
                                 (pm.ThruDate == null || pm.ThruDate >= DateTime.UtcNow))
                    .ToList();
            }

            foreach (var paymentMethod in paymentMethods)
            {
                var valueMap = new Dictionary<string, object> { { "paymentMethod", paymentMethod } };

                switch (paymentMethod.PaymentMethodTypeId)
                {
                    case "CREDIT_CARD":
                        var creditCard = await _context.CreditCards
                            .FirstOrDefaultAsync(cc => cc.PaymentMethodId == paymentMethod.PaymentMethodId);
                        if (creditCard != null)
                            valueMap.Add("creditCard", creditCard);
                        break;

                    case "GIFT_CARD":
                        var giftCard = await _context.GiftCards
                            .FirstOrDefaultAsync(gc => gc.PaymentMethodId == paymentMethod.PaymentMethodId);
                        if (giftCard != null)
                            valueMap.Add("giftCard", giftCard);
                        break;

                    case "EFT_ACCOUNT":
                        var eftAccount = await _context.EftAccounts
                            .FirstOrDefaultAsync(ea => ea.PaymentMethodId == paymentMethod.PaymentMethodId);
                        if (eftAccount != null)
                            valueMap.Add("eftAccount", eftAccount);
                        break;

                    case "COMPANY_CHECK":
                    case "PERSONAL_CHECK":
                    case "CERTIFIED_CHECK":
                        var checkAccount = await _context.CheckAccounts
                            .FirstOrDefaultAsync(ca => ca.PaymentMethodId == paymentMethod.PaymentMethodId);
                        if (checkAccount != null)
                            valueMap.Add($"{paymentMethod.PaymentMethodTypeId.ToLower()}Account", checkAccount);
                        break;
                }

                paymentMethodValueMaps.Add(valueMap);
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Error retrieving payment method value maps for partyId: {PartyId}", partyId);
        }

        return paymentMethodValueMaps;
    }

    public async Task<Dictionary<string, object>> GetPaymentMethodAndRelated(string paymentMethodId, string donePage,
        bool hasError)
    {
        var results = new Dictionary<string, object>();
        bool tryEntity = !hasError;

        if (string.IsNullOrEmpty(donePage))
            donePage = "viewprofile";
        results.Add("donePage", donePage);

        results.Add("paymentMethodId", paymentMethodId);

        PaymentMethod paymentMethod = null;
        CreditCard creditCard = null;
        GiftCard giftCard = null;
        EftAccount eftAccount = null;
        CheckAccount checkAccount = null;

        if (!string.IsNullOrEmpty(paymentMethodId))
        {
            try
            {
                paymentMethod = await _context.PaymentMethods
                    .FirstOrDefaultAsync(pm => pm.PaymentMethodId == paymentMethodId);
                creditCard = await _context.CreditCards
                    .FirstOrDefaultAsync(cc => cc.PaymentMethodId == paymentMethodId);
                giftCard = await _context.GiftCards
                    .FirstOrDefaultAsync(gc => gc.PaymentMethodId == paymentMethodId);
                eftAccount = await _context.EftAccounts
                    .FirstOrDefaultAsync(ea => ea.PaymentMethodId == paymentMethodId);
                checkAccount = await _context.CheckAccounts
                    .FirstOrDefaultAsync(ca => ca.PaymentMethodId == paymentMethodId);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex,
                    "Error retrieving payment method and related entities for paymentMethodId: {PaymentMethodId}",
                    paymentMethodId);
            }
        }

        if (paymentMethod != null)
            results.Add("paymentMethod", paymentMethod);
        else
            tryEntity = false;

        if (creditCard != null)
            results.Add("creditCard", creditCard);
        if (giftCard != null)
            results.Add("giftCard", giftCard);
        if (eftAccount != null)
            results.Add("eftAccount", eftAccount);
        if (checkAccount != null)
            results.Add("checkAccount", checkAccount);

        string curContactMechId = null;
        if (creditCard != null)
            curContactMechId = tryEntity ? creditCard.ContactMechId : null;
        else if (giftCard != null)
            curContactMechId = tryEntity ? giftCard.ContactMechId : null;
        else if (eftAccount != null)
            curContactMechId = tryEntity ? eftAccount.ContactMechId : null;
        else if (checkAccount != null)
            curContactMechId = tryEntity ? checkAccount.ContactMechId : null;

        if (!string.IsNullOrEmpty(curContactMechId))
            results.Add("curContactMechId", curContactMechId);

        results.Add("tryEntity", tryEntity);

        return results;
    }

    public async Task<PostalAddress> GetPaymentAddress(string partyId)
    {
        PostalAddress postalAddress = null;
        try
        {
            var purposeQuery = from pcmp in _context.PartyContactMechPurposes
                join cm in _context.ContactMeches on pcmp.ContactMechId equals cm.ContactMechId
                join pcm in _context.PartyContactMeches on new { pcmp.PartyId, pcmp.ContactMechId } equals new { pcm.PartyId, pcm.ContactMechId }
                where pcmp.PartyId == partyId && pcmp.ContactMechPurposeTypeId == "PAYMENT_LOCATION"
                                              && (pcm.FromDate == null || pcm.FromDate <= DateTime.UtcNow)
                                              && (pcm.ThruDate == null || pcm.ThruDate >= DateTime.UtcNow)
                                              && (pcmp.FromDate == null || pcmp.FromDate <= DateTime.UtcNow)
                                              && (pcmp.ThruDate == null || pcmp.ThruDate >= DateTime.UtcNow)
                orderby pcmp.FromDate descending
                select new { pcmp.ContactMechId };

            var purpose = await purposeQuery.FirstOrDefaultAsync();

            if (purpose != null)
            {
                postalAddress = await _context.PostalAddresses
                    .FirstOrDefaultAsync(pa => pa.ContactMechId == purpose.ContactMechId);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving PartyContactMechPurpose for partyId: {PartyId}", partyId);
        }

        return postalAddress;
    }
    
    public decimal GetPaymentsTotal(List<Payment> payments)
    {
        if (payments == null)
            throw new ArgumentException("Payment list cannot be null");

        decimal paymentsTotal = 0;
        foreach (var payment in payments)
        {
            paymentsTotal += Math.Round(payment.Amount, DECIMALS, ROUNDING_MODE);
        }

        return paymentsTotal;
    }

    public async Task<decimal> GetPaymentApplied(string paymentId, bool actual = false)
    {
        if (string.IsNullOrEmpty(paymentId))
            throw new ArgumentException("PaymentId cannot be null or empty");

        var payment = await _context.Payments
            .FirstOrDefaultAsync(p => p.PaymentId == paymentId);

        if (payment == null)
            throw new ArgumentException("The paymentId passed does not match an existing payment");

        return await GetPaymentApplied(payment, actual);
    }

    public async Task<decimal> GetPaymentApplied(Payment payment, bool actual = false)
    {
        if (payment == null)
            throw new ArgumentException("Payment cannot be null");

        decimal paymentApplied = 0;
        try
        {
            var paymentApplications = await _context.PaymentApplications
                .Where(pa => pa.PaymentId == payment.PaymentId || pa.ToPaymentId == payment.PaymentId)
                .OrderBy(pa => pa.InvoiceId)
                .ThenBy(pa => pa.BillingAccountId)
                .ToListAsync();

            foreach (var paymentApplication in paymentApplications)
            {
                decimal amountApplied = (decimal)paymentApplication.AmountApplied;

                if (!actual && paymentApplication.InvoiceId != null && payment.ActualCurrencyAmount != null &&
                    payment.ActualCurrencyUomId != null)
                {
                    var invoice = await _context.Invoices
                        .FirstOrDefaultAsync(i => i.InvoiceId == paymentApplication.InvoiceId);
                    if (invoice != null && payment.ActualCurrencyUomId == invoice.CurrencyUomId)
                    {
                        amountApplied = amountApplied * payment.Amount / payment.ActualCurrencyAmount.Value;
                    }
                }

                paymentApplied += Math.Round(amountApplied, DECIMALS, ROUNDING_MODE);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving payment applications for paymentId: {PaymentId}", payment.PaymentId);
            throw new Exception("Trouble getting entities", ex);
        }

        return paymentApplied;
    }

    public async Task<decimal> GetPaymentAppliedAmount(string paymentApplicationId)
    {
        decimal appliedAmount = 0;
        try
        {
            var paymentApplication = await _context.PaymentApplications
                .FirstOrDefaultAsync(pa => pa.PaymentApplicationId == paymentApplicationId);

            if (paymentApplication != null)
            {
                appliedAmount = (decimal)paymentApplication.AmountApplied;

                if (!string.IsNullOrEmpty(paymentApplication.PaymentId))
                {
                    var payment = await _context.Payments
                        .FirstOrDefaultAsync(p => p.PaymentId == paymentApplication.PaymentId);

                    if (!string.IsNullOrEmpty(paymentApplication.InvoiceId) && payment.ActualCurrencyAmount != null &&
                        payment.ActualCurrencyUomId != null)
                    {
                        var invoice = await _context.Invoices
                            .FirstOrDefaultAsync(i => i.InvoiceId == paymentApplication.InvoiceId);

                        if (invoice != null && payment.ActualCurrencyUomId == invoice.CurrencyUomId)
                        {
                            appliedAmount = appliedAmount * payment.Amount / payment.ActualCurrencyAmount.Value;
                        }
                    }
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Error retrieving payment application for paymentApplicationId: {PaymentApplicationId}",
                paymentApplicationId);
            throw new Exception("Problem getting Payment", ex);
        }

        return appliedAmount;
    }

    public async Task<decimal> GetPaymentNotApplied(Payment payment, bool actual = false)
    {
        if (payment == null)
            return 0;

        decimal paymentAmount = actual && payment.ActualCurrencyAmount.HasValue
            ? payment.ActualCurrencyAmount.Value
            : payment.Amount;

        return Math.Round(paymentAmount - await GetPaymentApplied(payment, actual), DECIMALS, ROUNDING_MODE);
    }

    public async Task<decimal> GetPaymentNotApplied(string paymentId, bool actual = false)
    {
        if (string.IsNullOrEmpty(paymentId))
            throw new ArgumentException("PaymentId cannot be null or empty");

        var payment = await _context.Payments
            .FirstOrDefaultAsync(p => p.PaymentId == paymentId);

        if (payment == null)
            throw new ArgumentException("The paymentId passed does not match an existing payment");

        return await GetPaymentNotApplied(payment, actual);
    }
}