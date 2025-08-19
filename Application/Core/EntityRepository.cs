using Microsoft.EntityFrameworkCore;

namespace Application.Core;

public class EntityRepository : IEntityRepository
{
    private readonly DbContext _context;

    public EntityRepository(DbContext context)
    {
        _context = context;
    }

    public List<TEntity> CombineEntities<TEntity>(List<TEntity> localEntities, List<TEntity> dbEntities) where TEntity : class
    {
        object[] ExtractPrimaryKeyValues(TEntity entity)
        {
            var keyNames = _context.Model.FindEntityType(typeof(TEntity)).FindPrimaryKey().Properties.Select(x => x.Name).ToArray();
            return keyNames.Select(keyName => typeof(TEntity).GetProperty(keyName)?.GetValue(entity, null)).ToArray();
        }

        return localEntities
            .Concat(dbEntities)
            .GroupBy(e => string.Join(",", ExtractPrimaryKeyValues(e)))
            .Select(g => g.First())
            .ToList();
    }
}