using System.Linq.Expressions;
using Application.Shipments.Accounting;
using Application.Shipments.InvoiceItemTypes;
using Application.Shipments.OrganizationGlSettings;
using AutoMapper;
using Domain;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Persistence;

namespace Application.Accounting.OrganizationGlSettings
{
    public class GetGlAccountOrganizationForInvoice
    {
        public class Query : IRequest<Result<InvoiceItemTypeAndGlAccountsDto>>
        {
            // Invoice object might be more complex; for simplicity, we pass needed fields.
            public string InvoiceTypeId { get; set; }          // e.g. SALES_INVOICE, PURCHASE_INVOICE, etc.
            public string PartyId { get; set; }                // For most invoice types (e.g. PURCHASE, PAYROL, COMMISSION)
            public string? PartyIdFrom { get; set; }             // For SALES_INVOICE, used to query GL accounts
        }

        public class Handler : IRequestHandler<Query, Result<InvoiceItemTypeAndGlAccountsDto>>
        {
            private readonly DataContext _context;

            public Handler(DataContext context)
            {
                _context = context;
            }

            #region Helper Methods

            // Recursively builds the hierarchy of GL accounts.
            private static List<GlAccountHierarchyViewLovDto> GetChildGlAccounts(string parentId, List<GlAccount> flatGlAccounts)
            {
                return flatGlAccounts
                    .Where(account => account.ParentGlAccountId == parentId)
                    .Select(account =>
                    {
                        var children = GetChildGlAccounts(account.GlAccountId, flatGlAccounts);
                        return new GlAccountHierarchyViewLovDto
                        {
                            GlAccountId = account.GlAccountId,
                            GlAccountTypeId = account.GlAccountTypeId,
                            GlAccountClassId = account.GlAccountClassId,
                            GlResourceTypeId = account.GlResourceTypeId,
                            ParentGlAccountId = account.ParentGlAccountId,
                            AccountCode = account.AccountCode,
                            Text = $"{account.AccountName} ({account.GlAccountId})",
                            ParentAccountName = flatGlAccounts
                                .Where(a => a.GlAccountId == account.ParentGlAccountId)
                                .Select(a => a.AccountName)
                                .FirstOrDefault(),
                            Items = children,
                            IsLeaf = !children.Any()
                        };
                    })
                    .ToList();
            }

            // Retrieves the invoice item types based on the invoice type.
            private async Task<List<InvoiceItemTypeDto>> GetInvoiceItemTypes(string invoiceTypeId, CancellationToken cancellationToken)
            {
                Expression<Func<InvoiceItemType, bool>> predicate = x => false;

                if (invoiceTypeId == "SALES_INVOICE")
                {
                    predicate = x => x.InvoiceItemTypeId == "SINVOICE_ADJ" ||
                                     x.ParentTypeId == "SINVOICE_ADJ" ||
                                     x.InvoiceItemTypeId == "SINVOICE_ITM_ADJ" ||
                                     x.ParentTypeId == "SINVOICE_ITM_ADJ" ||
                                     x.InvoiceItemTypeId == "INV_PROD_ITEM" ||
                                     x.ParentTypeId == "INV_PROD_ITEM";
                }
                else if (invoiceTypeId == "PURCHASE_INVOICE")
                {
                    predicate = x => x.InvoiceItemTypeId == "PINVOICE_ADJ" ||
                                     x.ParentTypeId == "PINVOICE_ADJ" ||
                                     x.InvoiceItemTypeId == "PINVOICE_ITM_ADJ" ||
                                     x.ParentTypeId == "PINVOICE_ITM_ADJ" ||
                                     x.InvoiceItemTypeId == "PINV_PROD_ITEM" ||
                                     x.ParentTypeId == "PINV_PROD_ITEM";
                }
                else if (invoiceTypeId == "PAYROL_INVOICE")
                {
                    predicate = x => x.InvoiceItemTypeId == "PAYROL_EARN_HOURS" ||
                                     x.ParentTypeId == "PAYROL_EARN_HOURS" ||
                                     x.InvoiceItemTypeId == "PAYROL_DD_FROM_GROSS" ||
                                     x.ParentTypeId == "PAYROL_DD_FROM_GROSS" ||
                                     x.InvoiceItemTypeId == "PAYROL_TAXES" ||
                                     x.ParentTypeId == "PAYROL_TAXES";
                }
                else if (invoiceTypeId == "COMMISSION_INVOICE")
                {
                    predicate = x => x.InvoiceItemTypeId == "COMM_INV_ITEM" ||
                                     x.ParentTypeId == "COMM_INV_ITEM" ||
                                     x.InvoiceItemTypeId == "COMM_INV_ADJ" ||
                                     x.ParentTypeId == "COMM_INV_ADJ";
                }
                else
                {
                    // Fallback: use the mapping table.
                    var mappedItemTypes = await _context.InvoiceItemTypeMaps
                        .Where(m => m.InvoiceTypeId == invoiceTypeId)
                        .Select(m => m.InvoiceItemType)
                        .ToListAsync(cancellationToken);
                    return mappedItemTypes.Select(it => new InvoiceItemTypeDto
                    {
                        InvoiceItemTypeId = it.InvoiceItemTypeId,
                        ParentTypeId = it.ParentTypeId,
                        Description = it.Description
                    }).ToList();
                }

                return await _context.InvoiceItemTypes
                    .Where(predicate)
                    .OrderBy(x => x.ParentTypeId)
                    .ThenBy(x => x.InvoiceItemTypeId)
                    .Select(x => new InvoiceItemTypeDto
                    {
                        InvoiceItemTypeId = x.InvoiceItemTypeId,
                        ParentTypeId = x.ParentTypeId,
                        Description = x.Description
                    })
                    .ToListAsync(cancellationToken);
            }

            // Retrieves hierarchical GL accounts using GlAccountOrganizations.
            private async Task<List<GlAccountOrganizationAndClassDto>> GetHierarchicalGlAccounts(string organizationPartyId, CancellationToken cancellationToken)
            {
                // Query GL accounts from GlAccountOrganizations based on organizationPartyId.
                var flatGlAccounts = await _context.GlAccountOrganizations
                    .Include(x => x.GlAccount)
                    .Where(x => x.OrganizationPartyId == organizationPartyId)
                    .OrderBy(x => x.GlAccount.GlAccountClassId)
                    .Select(x => x.GlAccount)
                    .ToListAsync(cancellationToken);

                // Build the hierarchical tree.
                var hierarchicalGlAccounts = flatGlAccounts
                    .Where(account => account.ParentGlAccountId == null)
                    .Select(account =>
                    {
                        var children = GetChildGlAccounts(account.GlAccountId, flatGlAccounts);
                        return new GlAccountHierarchyViewLovDto
                        {
                            GlAccountId = account.GlAccountId,
                            GlAccountTypeId = account.GlAccountTypeId,
                            GlAccountClassId = account.GlAccountClassId,
                            GlResourceTypeId = account.GlResourceTypeId,
                            ParentGlAccountId = account.ParentGlAccountId,
                            AccountCode = account.AccountCode,
                            Text = $"{account.AccountName} ({account.GlAccountId})",
                            ParentAccountName = flatGlAccounts
                                .Where(a => a.GlAccountId == account.ParentGlAccountId)
                                .Select(a => a.AccountName)
                                .FirstOrDefault(),
                            Items = children,
                            IsLeaf = !children.Any()
                        };
                    }).ToList();

                // Map the hierarchical tree to a DTO shape.
                // (You might want to return the tree directly; here we flatten it for the LOV.)
                return hierarchicalGlAccounts.Select(g => new GlAccountOrganizationAndClassDto
                {
                    GlAccountId = g.GlAccountId,
                    AccountName = g.Text, // Tree node text with account name and ID.
                    GlAccountTypeId = g.GlAccountTypeId,
                    GlAccountClassId = g.GlAccountClassId
                }).ToList();
            }

            #endregion

            public async Task<Result<InvoiceItemTypeAndGlAccountsDto>> Handle(Query request, CancellationToken cancellationToken)
            {
                try
                {
                    // Determine the party id to use for GL account lookup.
                    string glOrgPartyId = request.InvoiceTypeId == "SALES_INVOICE" && !string.IsNullOrEmpty(request.PartyIdFrom)
                        ? request.PartyIdFrom
                        : request.PartyId;

                    // Retrieve Invoice Item Types.
                    var invoiceItemTypes = await GetInvoiceItemTypes(request.InvoiceTypeId, cancellationToken);

                    // Retrieve hierarchical GL accounts.
                    var glAccounts = await GetHierarchicalGlAccounts(glOrgPartyId, cancellationToken);

                    var resultDto = new InvoiceItemTypeAndGlAccountsDto
                    {
                        InvoiceItemTypes = invoiceItemTypes,
                        GlAccounts = glAccounts
                    };

                    return Result<InvoiceItemTypeAndGlAccountsDto>.Success(resultDto);
                }
                catch (Exception ex)
                {
                    return Result<InvoiceItemTypeAndGlAccountsDto>.Failure(ex.Message);
                }
            }
        }
    }
}
