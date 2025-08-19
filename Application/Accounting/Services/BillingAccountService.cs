using Application.Order.Orders;
using Application.Parties.Parties;
using Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Persistence;

namespace Application.Accounting.Services
{
    public interface IBillingAccountService
    {
        Task<List<BillingAccountModel>> MakePartyBillingAccountList(string partyId, string? currencyUomId = null);
    }
    public class BillingAccountService : IBillingAccountService
    {
        private readonly DataContext _context;
        private readonly IPartyService _partyService;
        private readonly ILogger<BillingAccountService> _logger;

        public BillingAccountService(
            DataContext context,
            IPartyService partyService,
            ILogger<BillingAccountService> logger)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _partyService = partyService ?? throw new ArgumentNullException(nameof(partyService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<List<BillingAccountModel>> MakePartyBillingAccountList(string partyId, string? currencyUomId = null)
        {
            var billingAccountList = new List<BillingAccountModel>();
            currencyUomId ??= "EGP";

            try
            {
                // Get related parties where the party is an agent for customers
                var relatedParties = await _partyService.GetRelatedParties(
                    partyIdFrom: partyId,
                    partyRelationshipTypeId: "AGENT",
                    roleTypeIdFrom: "AGENT",
                    roleTypeIdTo: "CUSTOMER",
                    roleTypeIdFromIncludeAllChildTypes: false,
                    includeFromToSwitched: true);

                if (!relatedParties.Any())
                {
                    _logger.LogInformation("No related parties found for partyId {PartyId}. Returning empty billing account list.", partyId);
                    return billingAccountList;
                }

                // Get billing accounts for related customers where this party is a bill-to customer
                var billingAccountRoles = await _context.BillingAccountRoles
                    .Where(bar => relatedParties.Contains(bar.PartyId)
                                  && bar.RoleTypeId == "BILL_TO_CUSTOMER"
                                  && bar.FromDate <= DateTime.Now
                                  && (bar.ThruDate == null || bar.ThruDate >= DateTime.Now))
                    .ToListAsync();

                if (billingAccountRoles.Any())
                {
                    decimal totalAvailable = 0;

                    foreach (var billingAccountRole in billingAccountRoles)
                    {
                        var billingAccount = await _context.BillingAccounts
                            .FirstOrDefaultAsync(ba => ba.BillingAccountId == billingAccountRole.BillingAccountId);

                        if (billingAccount == null || (billingAccount.ThruDate.HasValue && DateTime.Now > billingAccount.ThruDate.Value))
                        {
                            continue;
                        }

                        if (billingAccount.AccountCurrencyUomId == currencyUomId)
                        {
                            var orh = new OrderReadHelper() { Context = _context };
                            orh.InitializeOrder();

                            decimal accountBalance = await orh.GetBillingAccountBalance(billingAccount);
                            decimal accountLimit = orh.GetAccountLimit(billingAccount);

                            var accountAvailable = accountLimit - accountBalance;
                            totalAvailable += accountAvailable;

                            var billingAccountModel = new BillingAccountModel
                            {
                                BillingAccountId = billingAccount.BillingAccountId,
                                AccountCurrencyUomId = billingAccount.AccountCurrencyUomId,
                                AccountBalance = accountBalance,
                                AccountLimit = accountLimit,
                                AccountAvailable = accountAvailable,
                                Description = billingAccount.Description
                            };

                            billingAccountList.Add(billingAccountModel);
                        }
                    }

                    // Sort by accountBalance ascending to match Java BillingAccountComparator
                    billingAccountList = billingAccountList
                        .OrderBy(ba => ba.AccountBalance)
                        .ToList();
                }

                return billingAccountList;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving billing accounts for partyId {PartyId} and currencyUomId {CurrencyUomId}.", partyId, currencyUomId);
                return billingAccountList;
            }
        }

        public async Task<List<OrderHeader>> GetBillingAccountOpenOrders(string billingAccountId)
        {
            try
            {
                return await _context.OrderHeaders
                    .Where(oh => oh.BillingAccountId == billingAccountId
                                 && oh.StatusId != "ORDER_REJECTED"
                                 && oh.StatusId != "ORDER_CANCELLED"
                                 && oh.StatusId != "ORDER_COMPLETED")
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving open orders for billingAccountId {BillingAccountId}.", billingAccountId);
                return new List<OrderHeader>();
            }
        }

        public async Task<decimal> GetBillingAccountAvailableBalance(BillingAccount billingAccount)
        {
            if (billingAccount?.AccountLimit == null)
            {
                _logger.LogWarning("Available balance requested for null billing account or undefined account limit, returning zero.");
                return 0m;
            }

            try
            {
                var orh = new OrderReadHelper() { Context = _context };
                orh.InitializeOrder();
                decimal accountLimit = billingAccount.AccountLimit.Value;
                decimal currentBalance = await orh.GetBillingAccountBalance(billingAccount);
                decimal availableBalance = accountLimit - currentBalance;
                availableBalance = Math.Round(availableBalance, 2, MidpointRounding.AwayFromZero);
                return availableBalance;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error calculating available balance for billing account ID {BillingAccountId}.", billingAccount.BillingAccountId);
                return 0m;
            }
        }

        public async Task<decimal> GetBillingAccountAvailableBalance(string billingAccountId)
        {
            try
            {
                var billingAccount = await _context.BillingAccounts
                    .FirstOrDefaultAsync(ba => ba.BillingAccountId == billingAccountId);

                if (billingAccount == null)
                {
                    _logger.LogWarning("Billing account with ID {BillingAccountId} not found.", billingAccountId);
                    return 0m;
                }

                return await GetBillingAccountAvailableBalance(billingAccount);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving billing account available balance for ID {BillingAccountId}.", billingAccountId);
                return 0m;
            }
        }

        public async Task<decimal> GetBillingAccountNetBalance(string billingAccountId)
        {
            decimal balance = 0m;

            try
            {
                var paymentApplications = await _context.PaymentApplications
                    .Where(pa => pa.BillingAccountId == billingAccountId)
                    .ToListAsync();

                foreach (var paymentApplication in paymentApplications)
                {
                    var amountApplied = paymentApplication.AmountApplied ?? 0m;
                    var invoice = await _context.Invoices
                        .FirstOrDefaultAsync(inv => inv.InvoiceId == paymentApplication.InvoiceId);

                    if (invoice != null)
                    {
                        if (invoice.InvoiceTypeId != "CUST_RTN_INVOICE" && invoice.StatusId != "INVOICE_CANCELLED")
                        {
                            balance += amountApplied;
                        }
                    }
                    else
                    {
                        balance -= amountApplied;
                    }
                }

                balance = Math.Round(balance, 2, MidpointRounding.AwayFromZero);
                return balance;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error calculating billing account net balance for ID {BillingAccountId}.", billingAccountId);
                return balance;
            }
        }

        public async Task<decimal> AvailableToCapture(BillingAccount billingAccount)
        {
            if (billingAccount?.AccountLimit == null)
            {
                _logger.LogWarning("Available to capture requested for null billing account or undefined account limit, returning zero.");
                return 0m;
            }

            try
            {
                decimal netBalance = await GetBillingAccountNetBalance(billingAccount.BillingAccountId);
                decimal accountLimit = billingAccount.AccountLimit.Value;
                decimal availableToCapture = accountLimit - netBalance;
                availableToCapture = Math.Round(availableToCapture, 2, MidpointRounding.AwayFromZero);
                return availableToCapture;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error calculating available to capture for billing account ID {BillingAccountId}.", billingAccount.BillingAccountId);
                return 0m;
            }
        }

        public async Task<BillingAccountBalanceResult> CalcBillingAccountBalance(string billingAccountId)
        {
            if (string.IsNullOrEmpty(billingAccountId))
            {
                _logger.LogWarning("Billing account ID is null or empty.");
                return new BillingAccountBalanceResult
                {
                    AccountBalance = 0,
                    NetAccountBalance = 0,
                    AvailableBalance = 0,
                    AvailableToCapture = 0,
                    BillingAccount = null
                };
            }

            try
            {
                var billingAccount = await _context.BillingAccounts
                    .FirstOrDefaultAsync(ba => ba.BillingAccountId == billingAccountId);

                if (billingAccount == null)
                {
                    _logger.LogWarning("Billing account with ID {BillingAccountId} not found.", billingAccountId);
                    return new BillingAccountBalanceResult
                    {
                        AccountBalance = 0,
                        NetAccountBalance = 0,
                        AvailableBalance = 0,
                        AvailableToCapture = 0,
                        BillingAccount = null
                    };
                }

                try
                {
                    var orh = new OrderReadHelper() { Context = _context };
                    orh.InitializeOrder();
                    var accountBalance = await orh.GetBillingAccountBalance(billingAccount);
                    var netAccountBalance = await GetBillingAccountNetBalance(billingAccountId);
                    var availableBalance = await GetBillingAccountAvailableBalance(billingAccount);
                    var availableToCapture = await AvailableToCapture(billingAccount);

                    return new BillingAccountBalanceResult
                    {
                        AccountBalance = accountBalance,
                        NetAccountBalance = netAccountBalance,
                        AvailableBalance = availableBalance,
                        AvailableToCapture = availableToCapture,
                        BillingAccount = billingAccount
                    };
                }
                catch (Exception balanceEx)
                {
                    _logger.LogError(balanceEx, "Error calculating balances for billing account ID {BillingAccountId}.", billingAccountId);
                    throw new InvalidOperationException("Failed to calculate billing account balances.", balanceEx);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving billing account balance for ID {BillingAccountId}.", billingAccountId);
                return new BillingAccountBalanceResult
                {
                    AccountBalance = 0,
                    NetAccountBalance = 0,
                    AvailableBalance = 0,
                    AvailableToCapture = 0,
                    BillingAccount = null
                };
            }
        }
    }
}