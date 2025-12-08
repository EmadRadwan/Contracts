var withItemsQuery = joined
    .GroupJoin(
        // CERTIFICATE_ITEM + Product join
        _context.WorkEfforts
                .Where(i => i.WorkEffortTypeId == "CERTIFICATE_ITEM")
                .GroupJoin(
                    _context.Products,
                    item => item.ProductId,
                    prod => prod.ProductId,
                    (item, prodGroup) => new { item, prodGroup })
                .SelectMany(
                    x => x.prodGroup.DefaultIfEmpty(),
                    (x, prod) => new
                    {
                        ParentId               = x.item.WorkEffortParentId,
                        ItemId                 = x.item.WorkEffortId,
                        x.item.ProductId,
                        x.item.Description,
                        x.item.Quantity,
                        x.item.MaterialPrice,
                        x.item.LaborPrice,
                        x.item.AchievementPercent,
                        x.item.Deductions,
                        x.item.Insurance,
                        x.item.AdditionalInsurance,
                        // LEGACY FIELDS FOR NON-WORKMANSHIP CERTIFICATES
                        Rate                   = x.item.Rate,
                        TransportationExpenses = x.item.TransportationExpenses,
                        Gratuities             = x.item.Gratuities,
                        Discount               = x.item.Discount,
                        // Product name
                        ProductName = prod != null ? prod.ProductName : (string?)null
                    }),
        cert => cert.we.WorkEffortId,
        item => item.ParentId,
        (cert, items) => new { cert.we, cert.si, cert.proj, cert.sup, cert.con, cert.fac, items })
    .SelectMany(
        x => x.items.DefaultIfEmpty(),
        (x, item) => new
        {
            Certificate = x.we,
            StatusItem  = x.si,
            Project     = x.proj,
            Supplier    = x.sup,
            Contractor  = x.con,
            Facility    = x.fac,

            // Flattened item fields
            ItemId                 = item?.ItemId,
            ProductId              = item?.ProductId,
            Description            = item?.Description,
            Quantity               = item?.Quantity,
            MaterialPrice          = item?.MaterialPrice,
            LaborPrice             = item?.LaborPrice,
            AchievementPercent     = item?.AchievementPercent,
            Deductions             = item?.Deductions,
            Insurance              = item?.Insurance,
            AdditionalInsurance    = item?.AdditionalInsurance,

            // Legacy fields (used by Supply/Procurement certificates)
            Rate                   = item?.Rate,
            TransportationExpenses = item?.TransportationExpenses,
            Gratuities             = item?.Gratuities,
            Discount               = item?.Discount,

            ProductName            = item?.ProductName
        });