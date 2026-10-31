using AuraLearn.Application.Dto;
using AuraLearn.Domain.Entities;
using AuraLearn.Domain.Enums;

namespace AuraLearn.Application.Interfaces;

/// <summary>Criterios de búsqueda de usuarios para el panel de administración.</summary>
public record AdminUserSearchCriteria(string? Query, Role? Role, int Page, int PageSize);

/// <summary>Puerto de acceso a datos de usuarios (implementado por Infrastructure).</summary>
public interface IUserRepository
{
    Task<User?> GetByEmailAsync(string email, CancellationToken ct);
    Task<User?> GetByIdAsync(Guid id, CancellationToken ct);
    Task AddAsync(User user, CancellationToken ct);

    /// <summary>Persiste cambios sobre un usuario existente (ej. promoción de rol a Tutor).</summary>
    Task UpdateAsync(User user, CancellationToken ct);

    /// <summary>
    /// Listado paginado de usuarios para el panel Admin, con filtro opcional
    /// por rol y búsqueda por texto sobre Email/FullName.
    /// </summary>
    Task<PagedResult<AdminUserDto>> SearchAsync(AdminUserSearchCriteria criteria, CancellationToken ct);
}

/// <summary>Puerto de emisión de tokens JWT (implementado por Infrastructure).</summary>
public interface IJwtTokenService
{
    string GenerateToken(User user);
}
