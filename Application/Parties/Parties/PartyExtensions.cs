namespace Application.Parties.Parties;

public static class PartyExtensions
{
    public static IQueryable<PartyDto> Sort(this IQueryable<PartyDto> query, string orderBy)
    {
        if (string.IsNullOrEmpty(orderBy)) return query.OrderBy(p => p.Description);

        query = orderBy switch
        {
            "name" => query.OrderBy(p => p.Description),
            "nameDesc" => query.OrderByDescending(p => p.Description),
            _ => query.OrderBy(p => p.Description)
        };

        return query;
    }

    public static IQueryable<PartyDto> Search(this IQueryable<PartyDto> query, string searchTerm)
    {
        if (string.IsNullOrEmpty(searchTerm)) return query;

        var lowerCaseSearchTerm = searchTerm.Trim().ToLower();

        return query.Where(p => p.Description.ToLower().Contains(lowerCaseSearchTerm));
    }

    /*public static IQueryable<Party> Filter(this IQueryable<Party> query, string categories, string types)
    {
        var categoryList = new List<string>();
        var typeList = new List<string>();

        if (!string.IsNullOrEmpty(categories))
            categoryList.AddRange(categories.ToLower().Split(",").ToList());

        if (!string.IsNullOrEmpty(types))
            typeList.AddRange(types.ToLower().Split(",").ToList());

        query = query.Where(p =>
            categoryList.Count == 0 || categoryList.Contains(p.PrimaryProductCategoryId.ToLower()));
        query = query.Where(p => typeList.Count == 0 || typeList.Contains(p.ProductTypeId.ToLower()));

        return query;
    }*/
}