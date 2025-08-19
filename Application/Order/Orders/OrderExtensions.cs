using Persistence;

namespace Application.Order.Orders;

public static class OrderExtensions
{
    public static IQueryable<OrderDto2> Sort(this IQueryable<OrderDto2> query, string orderBy)
    {
        if (string.IsNullOrEmpty(orderBy)) return query.OrderBy(p => p.OrderId);

        query = orderBy switch
        {
            "orderIdAsc" => query.OrderBy(p => p.OrderId),
            "orderIdDesc" => query.OrderByDescending(p => p.OrderId),
            "createdStampAsc" => query.OrderBy(p => p.OrderDate),
            "createdStampDesc" => query.OrderByDescending(p => p.OrderDate),
            _ => query.OrderBy(p => p.OrderDate)
        };

        return query;
    }

    public static IQueryable<OrderDto2> FilterOrderType(this IQueryable<OrderDto2> query, string types)
    {
        var typeList = new List<string>();


        if (!string.IsNullOrEmpty(types))
            typeList.AddRange(types.Split(",").ToList());


        query = query.Where(p => typeList.Count == 0 || typeList.Contains(p.OrderTypeId));

        return query;
    }

    public static IQueryable<OrderDto2> SearchCustomerName(this IQueryable<OrderDto2> query, string customerName)
    {
        if (string.IsNullOrEmpty(customerName)) return query;

        var lowerCaseSearchTerm = customerName.Trim().ToLower();

        return query.Where(order =>
            order.FromPartyId.FromPartyName.ToLower().Contains(lowerCaseSearchTerm)
        );
    }

    public static IQueryable<OrderDto2> SearchCustomerPhone(this IQueryable<OrderDto2> query, DataContext context,
        string phoneNumber)
    {
        if (string.IsNullOrEmpty(phoneNumber)) return query;

        var lowerCasePhoneNumber = phoneNumber.Trim().ToLower();

        // Use the context parameter here to access your database context
        var phoneQuery = from ord in query
            join pty in context.Parties on ord.FromPartyId.FromPartyId equals pty.PartyId
            join partyContactMech in context.PartyContactMeches on pty.PartyId equals partyContactMech.PartyId
            join contactMech in context.ContactMeches on partyContactMech.ContactMechId equals contactMech.ContactMechId
            join telecomNumber in context.TelecomNumbers on contactMech.ContactMechId equals telecomNumber.ContactMechId
            join partyContactMechPurpose in context.PartyContactMechPurposes on new
                    { partyContactMech.PartyId, partyContactMech.ContactMechId } equals
                new { partyContactMechPurpose.PartyId, partyContactMechPurpose.ContactMechId }
            join contactMechPurposeTypes in context.ContactMechPurposeTypes on partyContactMechPurpose
                .ContactMechPurposeTypeId equals contactMechPurposeTypes
                .ContactMechPurposeTypeId
            where partyContactMechPurpose.ContactMechPurposeTypeId == "PRIMARY_PHONE"
            select new { ord.OrderId, PhoneNumber = telecomNumber.ContactNumber };

        var filteredQuery = query.Where(order =>
            phoneQuery.Any(p => p.OrderId == order.OrderId && p.PhoneNumber.Contains(lowerCasePhoneNumber))
        );

        return filteredQuery;
    }
}