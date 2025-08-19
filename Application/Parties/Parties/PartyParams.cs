using Application.Core;

namespace Application.Parties;

public class PartyParams : PaginationParams
{
    public string? OrderBy { get; set; }
    public string? SearchTerm { get; set; }
    public string? RoleTypes { get; set; }
}