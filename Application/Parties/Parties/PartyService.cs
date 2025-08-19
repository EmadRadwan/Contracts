using Application.Catalog.ProductStores;
using Application.Errors;
using Application.Order.Orders;
using Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Persistence;

namespace Application.Parties.Parties;

public interface IPartyService
{
    Task<List<string>> GetRelatedParties(
        string partyIdFrom,
        string roleTypeIdFrom,
        string roleTypeIdTo,
        string partyRelationshipTypeId,
        bool roleTypeIdFromIncludeAllChildTypes,
        bool? roleTypeIdToIncludeAllChildTypes = false,
        bool? includeFromToSwitched = false,
        bool? recurse = false,
        bool? useCache = false);
}

public class PartyService : IPartyService
{
    private readonly DataContext _context;
    private readonly ILogger<PartyService> _logger;


    public PartyService(DataContext context,
        ILogger<PartyService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<List<string>> GetRelatedParties(
        string partyIdFrom,
        string partyRelationshipTypeId,
        string roleTypeIdFrom,
        string roleTypeIdTo,
        bool roleTypeIdFromIncludeAllChildTypes,
        bool? roleTypeIdToIncludeAllChildTypes = false,
        bool? includeFromToSwitched = false,
        bool? recurse = false,
        bool? useCache = false)
    {
        List<string> relatedPartyIdList = new List<string> { partyIdFrom };

        // Handle inline function call by passing necessary values and returning results
        var followPartyRelationshipsResult = await FollowPartyRelationshipsInline(
            relatedPartyIdList,
            partyRelationshipTypeId,
            roleTypeIdFrom,
            roleTypeIdFromIncludeAllChildTypes,
            roleTypeIdTo,
            roleTypeIdToIncludeAllChildTypes ?? false, // Provide a default value if null
            includeFromToSwitched ?? false, // Provide a default value if null
            recurse ?? false, // Provide a default value if null
            useCache ?? false // Provide a default value if null
        );

        return followPartyRelationshipsResult;
    }

    private async Task<List<string>> FollowPartyRelationshipsInline(
        List<string> relatedPartyIdList,
        string partyRelationshipTypeId,
        string roleTypeIdFrom,
        bool roleTypeIdFromIncludeAllChildTypes,
        string roleTypeIdTo,
        bool roleTypeIdToIncludeAllChildTypes,
        bool includeFromToSwitched,
        bool recurse,
        bool useCache)
    {
        DateTime nowTimestamp = DateTime.Now;

        // Initialize lists to track the roles
        List<string> _inlineRoleTypeIdFromList = new List<string> { roleTypeIdFrom };
        List<string>
            _inline_roleTypeIdAlreadySearchedList = new List<string>(); // Define the already searched list here

        // If roleTypeIdFrom includes all child types, get the child role types
        if (roleTypeIdFromIncludeAllChildTypes)
        {
            _inlineRoleTypeIdFromList.AddRange(await GetChildRoleTypesInline(_inlineRoleTypeIdFromList,
                _inline_roleTypeIdAlreadySearchedList));
        }

        // Initialize the to list and handle the "include all child types" logic
        List<string> _inlineRoleTypeIdToList = new List<string> { roleTypeIdTo };
        if (roleTypeIdToIncludeAllChildTypes)
        {
            _inlineRoleTypeIdToList.AddRange(await GetChildRoleTypesInline(_inlineRoleTypeIdToList,
                _inline_roleTypeIdAlreadySearchedList));
        }

        // Recursively follow the party relationships
        var result = await FollowPartyRelationshipsInlineRecurse(
            relatedPartyIdList,
            _inlineRoleTypeIdFromList,
            _inlineRoleTypeIdToList,
            partyRelationshipTypeId, // Convert string to List<string>
            includeFromToSwitched,
            recurse,
            useCache
        );


        return result;
    }

    public async Task<List<string>> FollowPartyRelationshipsInlineRecurse(
        List<string> relatedPartyIdList,
        List<string> roleTypeIdFromList,
        List<string> roleTypeIdToList,
        string partyRelationshipTypeId,
        bool includeFromToSwitched,
        bool recurse,
        bool useCache)
    {
        var newRelatedPartyIdList = new List<string>();
        var nowTimestamp = DateTime.Now;

        // List to keep track of already searched related parties
        var relatedPartyIdAlreadySearchedList = new List<string>();

        foreach (var relatedPartyId in relatedPartyIdList)
        {
            if (!relatedPartyIdAlreadySearchedList.Contains(relatedPartyId))
            {
                relatedPartyIdAlreadySearchedList.Add(relatedPartyId);

                // Fetch PartyRelationships from database
                var partyRelationships = await _context.PartyRelationships
                    .Where(pr => pr.PartyIdFrom == relatedPartyId
                                 && (roleTypeIdFromList == null || roleTypeIdFromList.Contains(pr.RoleTypeIdFrom))
                                 && (roleTypeIdToList == null || roleTypeIdToList.Contains(pr.RoleTypeIdTo))
                                 && (string.IsNullOrEmpty(partyRelationshipTypeId) ||
                                     pr.PartyRelationshipTypeId == partyRelationshipTypeId)
                                 && pr.FromDate <= nowTimestamp
                                 && (pr.ThruDate == null || pr.ThruDate > nowTimestamp))
                    .OrderByDescending(pr => pr.FromDate)
                    .ToListAsync();

                foreach (var relationship in partyRelationships)
                {
                    if (!relatedPartyIdList.Contains(relationship.PartyIdTo) &&
                        !newRelatedPartyIdList.Contains(relationship.PartyIdTo))
                    {
                        newRelatedPartyIdList.Add(relationship.PartyIdTo);
                    }
                }

                if (includeFromToSwitched)
                {
                    var switchedRelationships = await _context.PartyRelationships
                        .Where(pr => pr.PartyIdTo == relatedPartyId
                                     && (roleTypeIdFromList == null || roleTypeIdFromList.Contains(pr.RoleTypeIdTo))
                                     && (roleTypeIdToList == null || roleTypeIdToList.Contains(pr.RoleTypeIdFrom))
                                     && (string.IsNullOrEmpty(partyRelationshipTypeId) ||
                                         pr.PartyRelationshipTypeId == partyRelationshipTypeId)
                                     && pr.FromDate <= nowTimestamp
                                     && (pr.ThruDate == null || pr.ThruDate > nowTimestamp))
                        .OrderByDescending(pr => pr.FromDate)
                        .ToListAsync();

                    foreach (var switchedRelationship in switchedRelationships)
                    {
                        if (!relatedPartyIdList.Contains(switchedRelationship.PartyIdFrom) &&
                            !newRelatedPartyIdList.Contains(switchedRelationship.PartyIdFrom))
                        {
                            newRelatedPartyIdList.Add(switchedRelationship.PartyIdFrom);
                        }
                    }
                }
            }
        }

        // If new related parties were found, add them to the master list and possibly recurse
        if (newRelatedPartyIdList.Any())
        {
            relatedPartyIdList.AddRange(newRelatedPartyIdList.Distinct());

            if (recurse)
            {
                // Recursively call the function for further relationships
                return await FollowPartyRelationshipsInlineRecurse(relatedPartyIdList, roleTypeIdFromList,
                    roleTypeIdToList, partyRelationshipTypeId, includeFromToSwitched, recurse, useCache);
            }
        }

        return relatedPartyIdList;
    }

    public async Task<List<string>> GetChildRoleTypesInline(
        List<string> roleTypeIdList,
        List<string> roleTypeIdAlreadySearchedList)
    {
        var newRoleTypeIdList = new List<string>();

        foreach (var roleTypeId in roleTypeIdList)
        {
            if (!roleTypeIdAlreadySearchedList.Contains(roleTypeId))
            {
                // Add to the already searched list
                roleTypeIdAlreadySearchedList.Add(roleTypeId);

                // Retrieve child role types from the RoleType entity where the parentTypeId matches the current roleTypeId
                var childRoleTypes = await _context.RoleTypes
                    .Where(rt => rt.ParentTypeId == roleTypeId)
                    .ToListAsync();

                foreach (var newRoleType in childRoleTypes)
                {
                    if (!roleTypeIdList.Contains(newRoleType.RoleTypeId) &&
                        !newRoleTypeIdList.Contains(newRoleType.RoleTypeId))
                    {
                        newRoleTypeIdList.Add(newRoleType.RoleTypeId);
                    }
                }
            }
        }

        // If we found new role types, add them to the main list and recursively call the method
        if (newRoleTypeIdList.Any())
        {
            roleTypeIdList.AddRange(newRoleTypeIdList.Distinct());

            // Log the recursive call
            _logger.LogDebug(
                $"Recursively calling GetChildRoleTypesAsync, newRoleTypeIdList={string.Join(",", newRoleTypeIdList)}");

            // Recursively call the method to fetch more child role types
            return await GetChildRoleTypesInline(roleTypeIdList, roleTypeIdAlreadySearchedList);
        }

        // Return the updated role type list
        return roleTypeIdList;
    }
}