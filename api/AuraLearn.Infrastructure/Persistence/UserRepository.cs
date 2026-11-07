using AuraLearn.Application.Dto;
using AuraLearn.Application.Interfaces;
using AuraLearn.Domain.Entities;
using AuraLearn.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace AuraLearn.Infrastructure.Persistence;

/// <summary>Implementación del puerto IUserRepository con EF Core.</summary>
public class UserRepository(AuraLearnDbContext db) : IUserRepository
{
    public async Task<User?> GetByEmailAsync(string email, CancellationToken ct) =>
        await db.Users.AsNoTracking().FirstOrDefaultAsync(u => u.Email == email, ct);

    public async Task<User?> GetByIdAsync(Guid id, CancellationToken ct) =>
        await db.Users.AsNoTracking().FirstOrDefaultAsync(u => u.Id == id, ct);

    public async Task AddAsync(User user, CancellationToken ct)
    {
        db.Users.Add(user);
        await db.SaveChangesAsync(ct);
    }

    public async Task UpdateAsync(User user, CancellationToken ct)
    {
        db.Users.Update(user);
        await db.SaveChangesAsync(ct);
    }

    public async Task<PagedResult<AdminUserDto>> SearchAsync(
        AdminUserSearchCriteria criteria, CancellationToken ct)
    {
        var query = db.Users.AsNoTracking().AsQueryable();

        if (criteria.Role is not null)
            query = query.Where(u => u.Role == criteria.Role);

        if (!string.IsNullOrWhiteSpace(criteria.Query))
        {
            var term = criteria.Query.Trim().ToLower();
            query = query.Where(u =>
                u.Email.ToLower().Contains(term) ||
                u.FullName.ToLower().Contains(term));
        }

        var totalCount = await query.CountAsync(ct);

        var items = await query
            .OrderByDescending(u => u.CreatedAt)
            .Skip((criteria.Page - 1) * criteria.PageSize)
            .Take(criteria.PageSize)
            .Select(u => new AdminUserDto(u.Id, u.Email, u.FullName, u.Role, u.CreatedAt))
            .ToListAsync(ct);

        return new PagedResult<AdminUserDto>(items, criteria.Page, criteria.PageSize, totalCount);
    }

    public async Task<int> CountByRoleAsync(Role role, CancellationToken ct) =>
        await db.Users.CountAsync(u => u.Role == role, ct);
}
