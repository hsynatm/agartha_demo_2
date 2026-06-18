using AMMS.Core.Entities;
using AMMS.Core.Interfaces;
using AMMS.Core.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;

namespace AMMS.Infrastructure.Persistence
{

    public class EfRepository<T> : IRepository<T> where T : BaseEntity
    {
        private readonly DbContext _context;
        private readonly DbSet<T> _dbSet;

        public EfRepository(DbContext context)
        {
            _context = context;
            _dbSet = context.Set<T>();
        }

        public async Task<T?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default) =>
            await _dbSet.FirstOrDefaultAsync(entity => entity.Id == id, cancellationToken);

        public async Task<IReadOnlyList<T>> GetAllAsync(CancellationToken cancellationToken = default) =>
            await _dbSet.AsNoTracking().ToListAsync(cancellationToken);

        public async Task<PagedResult<T>> GetPagedAsync(int page, int pageSize, CancellationToken cancellationToken = default)
        {
            var normalizedPage = page < 1 ? 1 : page;
            var normalizedPageSize = pageSize < 1 ? 10 : pageSize;

            var query = _dbSet.AsNoTracking();
            var totalCount = await query.CountAsync(cancellationToken);
            var items = await query
                .OrderBy(entity => entity.CreatedAt)
                .Skip((normalizedPage - 1) * normalizedPageSize)
                .Take(normalizedPageSize)
                .ToListAsync(cancellationToken);

            return new PagedResult<T>
            {
                Items = items,
                Page = normalizedPage,
                PageSize = normalizedPageSize,
                TotalCount = totalCount
            };
        }

        public async Task<T> AddAsync(T entity, CancellationToken cancellationToken = default)
        {
            await _dbSet.AddAsync(entity, cancellationToken);
            return entity;
        }

        public void Update(T entity) => _dbSet.Update(entity);

        public void Remove(T entity)
        {
            entity.IsDeleted = true;
            _dbSet.Update(entity);
        }
    }




}
