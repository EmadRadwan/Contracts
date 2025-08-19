using Application.Core;
using Microsoft.Extensions.Logging;
using Persistence;

namespace Application._Base;

public abstract class BaseService
{
    protected readonly DataContext _context;
    protected readonly ILogger _logger;
    protected readonly IUtilityService _utilityService;

    protected BaseService(DataContext context, IUtilityService utilityService, ILogger logger)
    {
        _context = context;
        _utilityService = utilityService;
        _logger = logger;
    }
}