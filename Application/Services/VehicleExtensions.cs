namespace Application.Services;

public static class VehicleExtensions
{
    public static IQueryable<VehicleDto> Sort(this IQueryable<VehicleDto> query, string orderBy)
    {
        if (string.IsNullOrEmpty(orderBy)) return query.OrderBy(p => p.CreatedStamp);

        query = orderBy switch
        {
            "chassisNumberAsc" => query.OrderBy(p => p.ChassisNumber),
            "createdStampDesc" => query.OrderByDescending(p => p.ChassisNumber),
            _ => query.OrderBy(p => p.ChassisNumber)
        };

        return query;
    }

    public static IQueryable<VehicleDto> FilterOrderType(this IQueryable<VehicleDto> query, string types)
    {
        var typeList = new List<string>();


        if (!string.IsNullOrEmpty(types))
            typeList.AddRange(types.Split(",").ToList());


        query = query.Where(p => typeList.Count == 0 || typeList.Contains(p.VehicleTypeId));

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