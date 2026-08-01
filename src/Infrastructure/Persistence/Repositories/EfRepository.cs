using Application.Common.Interfaces;
using Application.Common.Specifications;
using Domain.Common;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence.Repositories;

public class EfRepository<T> : IRepository<T> where T : BaseEntity
{
    private readonly ApplicationDbContext _context;

    public EfRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<T?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        => await _context.Set<T>().FindAsync(new object[] { id }, cancellationToken);

    public async Task<IReadOnlyList<T>> ListAsync(CancellationToken cancellationToken = default)
        => await _context.Set<T>().ToListAsync(cancellationToken);

    public async Task<IReadOnlyList<T>> ListAsync(ISpecification<T> spec, CancellationToken cancellationToken = default)
        => await SpecificationEvaluator.GetQuery(_context.Set<T>().AsQueryable(), spec).ToListAsync(cancellationToken);

    public async Task<int> CountAsync(ISpecification<T> spec, CancellationToken cancellationToken = default)
    {
        // Re-run criteria only - paging/ordering are irrelevant to a total count.
        var countSpec = new CountOnlySpecification<T>(spec);
        return await SpecificationEvaluator.GetQuery(_context.Set<T>().AsQueryable(), countSpec).CountAsync(cancellationToken);
    }

    public async Task AddAsync(T entity, CancellationToken cancellationToken = default)
        => await _context.Set<T>().AddAsync(entity, cancellationToken);

    public void Update(T entity)
        => _context.Entry(entity).State = EntityState.Modified;

    public void Remove(T entity)
        => _context.Set<T>().Remove(entity);
}
