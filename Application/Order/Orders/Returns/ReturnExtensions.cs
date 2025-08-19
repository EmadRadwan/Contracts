namespace Application.Order.Orders.Returns;

public static class ReturnExtensions
{
    public static IQueryable<ReturnDto> Sort(this IQueryable<ReturnDto> query, string orderBy)
    {
        if (string.IsNullOrEmpty(orderBy)) return query.OrderBy(p => p.ReturnId);

        query = orderBy switch
        {
            "returnIdAsc" => query.OrderBy(p => p.ReturnId),
            "returnIdDesc" => query.OrderByDescending(p => p.ReturnId),
            "createdStampAsc" => query.OrderBy(p => p.EntryDate),
            "createdStampDesc" => query.OrderByDescending(p => p.EntryDate),
            _ => query.OrderBy(p => p.EntryDate)
        };

        return query;
    }

    public static IQueryable<ReturnDto> FilterOrderType(this IQueryable<ReturnDto> query, string types)
    {
        var typeList = new List<string>();


        if (!string.IsNullOrEmpty(types))
            typeList.AddRange(types.Split(",").ToList());


        query = query.Where(p => typeList.Count == 0 || typeList.Contains(p.ReturnHeaderTypeId));

        return query;
    }

    /*public static IQueryable<Order> Search(this IQueryable<Order> query, string searchTerm)
    {
        if (string.IsNullOrEmpty(searchTerm)) return query;

        var lowerCaseSearchTerm = searchTerm.Trim().ToLower();

        return query.Where(p => p.OrderName.ToLower().Contains(lowerCaseSearchTerm));
    }

    */
}