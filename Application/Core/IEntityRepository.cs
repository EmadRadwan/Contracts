namespace Application.Core;

public interface IEntityRepository
{
    /// <summary>
    /// Combines local and database entities, deduplicating based on primary keys.
    /// </summary>
    /// <typeparam name="TEntity">The entity type, must be a class.</typeparam>
    /// <param name="localEntities">List of entities from the change tracker.</param>
    /// <param name="dbEntities">List of entities from the database.</param>
    /// <returns>A deduplicated list of entities, prioritizing local entities.</returns>
    List<TEntity> CombineEntities<TEntity>(List<TEntity> localEntities, List<TEntity> dbEntities) where TEntity : class;
}