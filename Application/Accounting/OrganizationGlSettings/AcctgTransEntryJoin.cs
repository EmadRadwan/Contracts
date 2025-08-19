using Domain;

namespace Application.Accounting.OrganizationGlSettings;

/// <summary>
/// Holds a combined row of AcctgTransEntry + AcctgTrans for EF queries, 
/// so we don't rely on 'dynamic'.
/// </summary>
public class AcctgTransEntryJoin
{
    public AcctgTransEntry Ate { get; set; } // 'ate'
    public AcctgTran Act { get; set; }      // 'act'
}