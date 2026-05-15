using Application.Core;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Persistence;

namespace Application.Common.DataSources;

public class ListDataSources
{
    public record Query : IRequest<Result<List<DataSourceDto>>>
    {
        public string Language {get; set;}
    };

    public class Handler : IRequestHandler<Query, Result<List<DataSourceDto>>>
    {
        private readonly DataContext _context;

        public Handler(DataContext context)
        {
            _context = context;
        }

        public async Task<Result<List<DataSourceDto>>> Handle(Query request, CancellationToken ct)
        {
            var dataSources = await _context.DataSources
                .OrderBy(ds => ds.Description)
                .Select(ds => new DataSourceDto
                {
                    DataSourceId = ds.DataSourceId,
                    Description = (request.Language == "ar" ? ds.DescriptionArabic : ds.Description) ?? ds.DataSourceId
                })
                .ToListAsync(ct);

            return Result<List<DataSourceDto>>.Success(dataSources);
        }
    }
}

public class DataSourceDto
{
    public string DataSourceId { get; set; } = null!;
    public string Description { get; set; } = null!;
}
