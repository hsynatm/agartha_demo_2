using AMMS.Core.Exceptions;

namespace AMMS.Core.Services;

public static class EntityServiceHelpers
{
    public static async Task<TEntity> RequireAsync<TEntity>(Func<Guid, CancellationToken, Task<TEntity?>> getter, Guid id, string localizationKey, string errorCode, CancellationToken cancellationToken = default) 
        where TEntity : class
    {
        var entity = await getter(id, cancellationToken);

        if (entity is null)
            throw AmmsException.NotFound.ForEntity(localizationKey, errorCode, id);


        return entity;
    }
}
