using AuraLearn.Domain.Entities;

namespace AuraLearn.Application.Interfaces;

/// <summary>Puerto de acceso a datos de usuarios (implementado por Infrastructure).</summary>
public interface IUserRepository
{
    Task<User?> GetByEmailAsync(string email, CancellationToken ct);
    Task<User?> GetByIdAsync(Guid id, CancellationToken ct);
    Task AddAsync(User user, CancellationToken ct);
}

/// <summary>Puerto de emisión de tokens JWT (implementado por Infrastructure).</summary>
public interface IJwtTokenService
{
    string GenerateToken(User user);
}
