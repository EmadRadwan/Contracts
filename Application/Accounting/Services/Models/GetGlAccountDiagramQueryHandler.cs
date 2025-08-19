


using Persistence;

namespace Application.Accounting.Services.Models;

using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

public class GetGlAccountDiagramQuery : IRequest<GetGlAccountDiagramResult>
{
    public string AcctgTransId { get; set; } = string.Empty;
}
public class GetGlAccountDiagramQueryHandler : IRequestHandler<GetGlAccountDiagramQuery, GetGlAccountDiagramResult>
{
    private readonly DataContext _context;
    private readonly  IAcctgMiscService _acctgMiscService;
    private readonly ILogger<GetGlAccountDiagramQueryHandler> _logger;

    public GetGlAccountDiagramQueryHandler(DataContext context, ILogger<GetGlAccountDiagramQueryHandler> logger, IAcctgMiscService acctgMiscService)
    {
        _context = context;
        _logger = logger;
        _acctgMiscService = acctgMiscService;
    }
    

    public async Task<GetGlAccountDiagramResult> Handle(GetGlAccountDiagramQuery request, CancellationToken cancellationToken)
    {
        // Retrieve the accounting transaction entries for the provided AcctgTransId
        var entries = await _context.AcctgTransEntries
            .Where(e => e.AcctgTransId == request.AcctgTransId)
            .ToListAsync(cancellationToken);

        if (!entries.Any())
        {
            _logger.LogWarning($"No accounting transaction entries found for AcctgTransId: {request.AcctgTransId}");
            return new GetGlAccountDiagramResult
            {
                Diagram = "flowchart LR\n  Start((Start))\n  Start --> NoData[No entries found]"
            };
        }

        // For this example, we assume that the overall transaction type and DebitCreditFlag 
        // can be determined from the first entry. Adjust as needed.
        string acctgTransTypeId = entries.First().AcctgTransEntryTypeId ?? "";
        string debitCreditFlag = entries.First().DebitCreditFlag ?? "";

        // Generate the simplified Mermaid diagram text
        var diagramText = _acctgMiscService.GenerateGlAccountDiagram(entries, acctgTransTypeId, debitCreditFlag);

        return new GetGlAccountDiagramResult { Diagram = diagramText };
    }
}
