public async Task<Result<List<MultiPaymentItemDto>>> Handle(Query request, CancellationToken cancellationToken)
{
    var validator = new QueryValidator();
    var validationResult = await validator.ValidateAsync(request, cancellationToken);
    if (!validationResult.IsValid)
        return Result<List<MultiPaymentItemDto>>.Failure(
            string.Join("; ", validationResult.Errors.Select(e => e.ErrorMessage)));

    try
    {
        var multiPaymentItems = await _context.WorkEfforts
            .Where(item => item.WorkEffortParentId == request.WorkEffortId 
                        && item.WorkEffortTypeId == "PAYMENT_CERTIFICATE_ITEM")

            // ── Project ──
            .GroupJoin(_context.WorkEfforts.Where(p => p.WorkEffortTypeId == "PROJECT"),
                item => item.ProjectId,
                project => project.WorkEffortId,
                (item, projects) => new { item, projects })
            .SelectMany(x => x.projects.DefaultIfEmpty(), (x, project) => new { x.item, project })

            // ── SubProject ──
            .GroupJoin(_context.WorkEfforts.Where(sp => sp.WorkEffortTypeId == "SUB_PROJECT"),
                x => x.item.SubProjectId,
                subProject => subProject.WorkEffortId,
                (x, subProjects) => new { x.item, x.project, subProjects })
            .SelectMany(x => x.subProjects.DefaultIfEmpty(), (x, subProject) => new { x.item, x.project, subProject })

            // ── Service (Product) ──
            .GroupJoin(_context.Products,
                x => x.item.ServiceId,
                service => service.ProductId,
                (x, services) => new { x.item, x.project, x.subProject, services })
            .SelectMany(x => x.services.DefaultIfEmpty(), (x, service) => new { x.item, x.project, x.subProject, service })

            // ── Product ──
            .GroupJoin(_context.Products,
                x => x.item.ProductId,
                product => product.ProductId,
                (x, products) => new { x.item, x.project, x.subProject, x.service, products })
            .SelectMany(x => x.products.DefaultIfEmpty(), (x, product) => new { x.item, x.project, x.subProject, x.service, product })

            // ── Supplier Party ──
            .GroupJoin(_context.Parties,
                x => x.item.PartyIdSupplier,
                supplier => supplier.PartyId,
                (x, suppliers) => new { x.item, x.project, x.subProject, x.service, x.product, suppliers })
            .SelectMany(x => x.suppliers.DefaultIfEmpty(), (x, supplier) => new { x.item, x.project, x.subProject, x.service, x.product, supplier })

            // ── Contractor Party ──
            .GroupJoin(_context.Parties,
                x => x.item.PartyIdContractor,
                contractor => contractor.PartyId,
                (x, contractors) => new { x.item, x.project, x.subProject, x.service, x.product, x.supplier, contractors })
            .SelectMany(x => x.contractors.DefaultIfEmpty(), (x, contractor) => new { x.item, x.project, x.subProject, x.service, x.product, x.supplier, contractor })

            // ── NEW: Left join to GlAccounts ────────────────────────────────
            .GroupJoin(_context.GlAccounts,                              // ← assuming your DbSet is named GlAccounts
                x => x.item.GlAccountId,
                gl => gl.GlAccountId,                                    // ← adjust column name if different
                (x, glAccounts) => new { x.item, x.project, x.subProject, x.service, x.product, x.supplier, x.contractor, glAccounts })
            .SelectMany(x => x.glAccounts.DefaultIfEmpty(), (x, glAccount) => new MultiPaymentItemDto
            {
                WorkEffortId          = x.item.WorkEffortId,
                GlAccountId           = x.item.GlAccountId,
                GlAccountNameArabic   = glAccount != null ? glAccount.AccountNameArabic : null,   // ← added
                // GlAccountNameEnglish  = glAccount?.AccountName ?? "",   // optional

                ItemType              = x.item.CostType,
                ServiceId             = x.item.ServiceId,
                ServiceName           = x.service != null ? x.service.ProductName : "",
                ProductId             = x.item.ProductId,
                ProductName           = x.product != null ? x.product.ProductName : "",
                Description           = x.item.Description,

                Amount                = (decimal?)x.item.Amount,
                Discount              = (decimal?)x.item.Discount,
                TransportationExpenses= (decimal?)x.item.TransportationExpenses,
                Gratuities            = (decimal?)x.item.Gratuities,
                Total                 = (decimal?)x.item.TotalAmount,

                PartyIdSupplier       = x.item.PartyIdSupplier,
                PartyIdSupplierName   = x.supplier != null ? x.supplier.Description : "",
                PartyIdContractor     = x.item.PartyIdContractor,
                PartyIdContractorName = x.contractor != null ? x.contractor.Description : ""
            })
            .ToListAsync(cancellationToken);

        var itemTypeDescriptions = new Dictionary<string, string>
        {
            { "MATERIALS",  "المواد" },
            { "LABOR",      "العمالة" },
            { "EQUIPMENT",  "المعدات" },
            { "EXPENSES",   "المصروفات" }
        };

        foreach (var item in multiPaymentItems)
        {
            item.ItemTypeDescription = itemTypeDescriptions.GetValueOrDefault(item.ItemType ?? "", "");

            item.DiscountMode = item.Discount > 0 
                ? "value" 
                : item.Discount < 0 ? "percentage" : null;   // ← improved a bit
        }

        return Result<List<MultiPaymentItemDto>>.Success(multiPaymentItems);
    }
    catch (Exception ex)
    {
        return Result<List<MultiPaymentItemDto>>.Failure($"Failed to retrieve multi-payment items: {ex.Message}");
    }
}