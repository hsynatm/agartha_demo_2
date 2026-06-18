using AMMS.Core.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace AMMS.Infrastructure.Persistence
{

    public abstract class EfUnitOfWork<TContext>(TContext context) : IUnitOfWork where TContext : DbContext
    {
        protected TContext Context { get; } = context;

        public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default) =>
            Context.SaveChangesAsync(cancellationToken);
    }




}
