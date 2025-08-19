using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Persistence;

namespace Application.Common;

public interface ICommonService
{
    Task<decimal?> ConvertUom(
        string uomId,
        string uomIdTo,
        DateTime? asOfDate,
        decimal originalValue,
        string? purposeEnumI);
}

public class CommonService : ICommonService
{
    private readonly DataContext _context;
    private readonly ILogger<CommonService> _logger;

    public CommonService(DataContext context, ILogger<CommonService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<decimal?> ConvertUom(
        string uomId,
        string uomIdTo,
        DateTime? asOfDate,
        decimal originalValue,
        string? purposeEnumId = null)
    {
        if (uomId == uomIdTo) return originalValue;

        asOfDate ??= DateTime.Now;

        var uomConversionDated = await _context.UomConversionDateds
            .Where(e => e.UomId == uomId && e.UomIdTo == uomIdTo && e.PurposeEnumId == purposeEnumId)
            .OrderByDescending(e => e.FromDate)
            .FirstOrDefaultAsync(e => e.FromDate <= asOfDate);

        if (uomConversionDated == null && !string.IsNullOrEmpty(purposeEnumId))
            uomConversionDated = await _context.UomConversionDateds
                .Where(e => e.UomId == uomId && e.UomIdTo == uomIdTo)
                .OrderByDescending(e => e.FromDate)
                .FirstOrDefaultAsync(e => e.FromDate <= asOfDate);

        if (uomConversionDated != null) return originalValue * (decimal)uomConversionDated.ConversionFactor;

        var uomConversion = await _context.UomConversions
            .FirstOrDefaultAsync(e => e.UomId == uomId && e.UomIdTo == uomIdTo);

        return uomConversion != null ? originalValue * (decimal)uomConversion.ConversionFactor : null;
    }
}