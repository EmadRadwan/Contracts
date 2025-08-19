namespace Application.Facilities.FacilityInventories;

public static class FacilityInventoryItemExtensions
{
    public static IQueryable<FacilityInventoryItemDto> Sort(this IQueryable<FacilityInventoryItemDto> query,
        string orderBy)
    {
        if (string.IsNullOrEmpty(orderBy)) return query.OrderBy(p => p.ProductName);

        query = orderBy switch
        {
            "name" => query.OrderBy(p => p.ProductName),
            "nameDesc" => query.OrderByDescending(p => p.ProductName),
            _ => query.OrderBy(p => p.ProductName)
        };

        return query;
    }

    /*public static IQueryable<FacilityInventoryItemDto> Search(this IQueryable<FacilityInventoryItemDto> query, string searchTerm)
    {
        if (string.IsNullOrEmpty(searchTerm)) return query;

        var lowerCaseSearchTerm = searchTerm.Trim().ToLower();

        return query.Where(p => p.ProductName.ToLower().Contains(lowerCaseSearchTerm));
    }*/

    public static IQueryable<FacilityInventoryItemDto> Filter(this IQueryable<FacilityInventoryItemDto> query,
        FacilityInventoryParams? Params)
    {
        if (Params?.ProductId != null) query = query.Where(p => p.ProductId == Params.ProductId);

        return query;
    }
}