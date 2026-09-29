using AuraLearn.Application.Dto;
using AuraLearn.Application.Interfaces;
using AuraLearn.Domain.Entities;
using FluentValidation;
using Microsoft.AspNetCore.Identity;

namespace AuraLearn.Application.Services;

/// <summary>Validador de registro: email formato, password ≥8, nombre requerido.</summary>
public class RegisterRequestValidator : AbstractValidator<RegisterRequest>
{
    public RegisterRequestValidator()
    {
        RuleFor(r => r.Email).NotEmpty().EmailAddress();
        RuleFor(r => r.Password).NotEmpty().MinimumLength(8);
        RuleFor(r => r.FullName).NotEmpty().MaximumLength(200);
    }
}

/// <summary>Resultado tipado de las operaciones de auth.</summary>
public record AuthResult<T>(T? Value, bool IsConflict = false, bool IsUnauthorized = false, bool IsInvalid = false)
{
    public static AuthResult<T> Ok(T value) => new(value);
    public static AuthResult<T> Conflict() => new(default, IsConflict: true);
    public static AuthResult<T> Unauthorized() => new(default, IsUnauthorized: true);
    public static AuthResult<T> Invalid() => new(default, IsInvalid: true);
}

/// <summary>
/// Casos de uso de autenticación: registro, login y emisión de JWT.
/// Dominio crítico (auth/seguridad): TDD obligatorio, tests en AuthServiceTests.
/// </summary>
public class AuthService(
    IUserRepository users,
    IJwtTokenService tokens,
    IPasswordHasher<User> hasher,
    RegisterRequestValidator validator)
{
    public async Task<AuthResult<AuthResponse>> RegisterAsync(RegisterRequest request, CancellationToken ct)
    {
        if (!validator.Validate(request).IsValid)
            return AuthResult<AuthResponse>.Invalid();

        var existing = await users.GetByEmailAsync(request.Email, ct);
        if (existing is not null)
            return AuthResult<AuthResponse>.Conflict();

        var user = new User
        {
            Id = Guid.NewGuid(),
            Email = request.Email,
            FullName = request.FullName,
            Role = Domain.Enums.Role.Estudiante,
            CreatedAt = DateTime.UtcNow,
            PasswordHash = string.Empty, // se asigna inmediatamente abajo con el hasher
        };
        user.PasswordHash = hasher.HashPassword(user, request.Password);

        await users.AddAsync(user, ct);

        return AuthResult<AuthResponse>.Ok(ToResponse(user));
    }

    public async Task<AuthResult<AuthResponse>> LoginAsync(LoginRequest request, CancellationToken ct)
    {
        var user = await users.GetByEmailAsync(request.Email, ct);
        if (user is null)
            return AuthResult<AuthResponse>.Unauthorized();

        var verification = hasher.VerifyHashedPassword(user, user.PasswordHash, request.Password);
        if (verification == PasswordVerificationResult.Failed)
            return AuthResult<AuthResponse>.Unauthorized();

        return AuthResult<AuthResponse>.Ok(ToResponse(user));
    }

    private AuthResponse ToResponse(User user) =>
        new(user.Id, user.Email, user.FullName, user.Role.ToString(), tokens.GenerateToken(user));
}
