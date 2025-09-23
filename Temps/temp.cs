using Application.Core;
using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore;
using Persistence;

namespace Application._Base;

public abstract class BaseService
{
    protected readonly DataContext _context;
    protected readonly IDbContextFactory<DataContext> _contextFactory;
    protected readonly ILogger _logger;
    protected readonly IUtilityService? _utilityService; // Made optional to avoid circular dependency

    // Constructor for transactional use
    protected BaseService(DataContext context, ILogger logger, IUtilityService? utilityService = null)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _utilityService = utilityService;
    }

    // Constructor for non-transactional use or flexibility
    protected BaseService(IDbContextFactory<DataContext> contextFactory, ILogger logger, IUtilityService? utilityService = null)
    {
        _contextFactory = contextFactory ?? throw new ArgumentNullException(nameof(contextFactory));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _utilityService = utilityService;
    }

    // Helper method to create a new DbContext when needed
    protected DataContext CreateDbContext()
    {
        if (_contextFactory == null)
        {
            throw new InvalidOperationException("IDbContextFactory not provided. Use the appropriate constructor.");
        }
        return _contextFactory.CreateDbContext();
    }
}