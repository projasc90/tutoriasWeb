using AuraLearn.Application.Dto;
using AuraLearn.Application.Interfaces;
using AuraLearn.Application.Services;
using AuraLearn.Domain.Entities;
using AuraLearn.Domain.Enums;
using FluentValidation;
using Microsoft.AspNetCore.Identity;
using NSubstitute;
using Xunit;

namespace AuraLearn.Tests;

/// <summary>
/// Tests del servicio de autenticación (TDD obligatorio: dominio auth/seguridad).
/// Escritos ANTES de la implementación de AuthService.
/// </summary>
public class AuthServiceTests
{
    private readonly IUserRepository _users = Substitute.For<IUserRepository>();
    private readonly IJwtTokenService _tokens = Substitute.For<IJwtTokenService>();
    private readonly AuthService _sut;

    public AuthServiceTests()
    {
        var validators = new RegisterRequestValidator();
        _sut = new AuthService(_users, _tokens, new PasswordHasher<User>(), validators);
    }

    private static RegisterRequest ValidRegister() =>
        new("estudiante@ucr.ac.cr", "Password123!", "Ana Estudiante");

    // ── Register ───────────────────────────────────────────────────

    [Fact]
    public async Task Register_WithValidRequest_HashesPasswordAndReturnsToken()
    {
        _users.GetByEmailAsync(Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns((User?)null);
        _tokens.GenerateToken(Arg.Any<User>()).Returns("jwt-token");

        var result = await _sut.RegisterAsync(ValidRegister(), CancellationToken.None);

        Assert.True(result.IsConflict == false);
        Assert.Equal("jwt-token", result.Value!.Token);
        Assert.Equal("estudiante@ucr.ac.cr", result.Value.Email);
        Assert.Equal(nameof(Role.Estudiante), result.Value.Role);

        // El hash guardado NO es la contraseña en claro
        await _users.Received(1).AddAsync(
            Arg.Do<User>(u => Assert.NotEqual("Password123!", u.PasswordHash)),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Register_WithDuplicateEmail_ReturnsConflict()
    {
        _users.GetByEmailAsync("estudiante@ucr.ac.cr", Arg.Any<CancellationToken>())
            .Returns(new User { Id = Guid.NewGuid(), Email = "estudiante@ucr.ac.cr", PasswordHash = "x", FullName = "Otro", Role = Role.Estudiante, CreatedAt = DateTime.UtcNow });

        var result = await _sut.RegisterAsync(ValidRegister(), CancellationToken.None);

        Assert.True(result.IsConflict);
        Assert.Null(result.Value);
    }

    [Fact]
    public async Task Register_WithInvalidEmail_FailsValidation()
    {
        var request = new RegisterRequest("no-es-un-email", "Password123!", "Ana");
        var result = await _sut.RegisterAsync(request, CancellationToken.None);
        Assert.True(result.IsInvalid);
    }

    [Fact]
    public async Task Register_WithShortPassword_FailsValidation()
    {
        var request = new RegisterRequest("ok@ucr.ac.cr", "corta", "Ana");
        var result = await _sut.RegisterAsync(request, CancellationToken.None);
        Assert.True(result.IsInvalid);
    }

    // ── Login ──────────────────────────────────────────────────────

    [Fact]
    public async Task Login_WithValidCredentials_ReturnsToken()
    {
        var hasher = new PasswordHasher<User>();
        var user = new User { Id = Guid.NewGuid(), Email = "estudiante@ucr.ac.cr", FullName = "Ana", Role = Role.Estudiante, CreatedAt = DateTime.UtcNow, PasswordHash = "" };
        user.PasswordHash = hasher.HashPassword(user, "Password123!");

        _users.GetByEmailAsync("estudiante@ucr.ac.cr", Arg.Any<CancellationToken>())
            .Returns(user);
        _tokens.GenerateToken(user).Returns("jwt-token");

        var result = await _sut.LoginAsync(new LoginRequest("estudiante@ucr.ac.cr", "Password123!"), CancellationToken.None);

        Assert.True(result.IsUnauthorized == false);
        Assert.Equal("jwt-token", result.Value!.Token);
    }

    [Fact]
    public async Task Login_WithWrongPassword_ReturnsUnauthorized()
    {
        var hasher = new PasswordHasher<User>();
        var user = new User { Id = Guid.NewGuid(), Email = "estudiante@ucr.ac.cr", FullName = "Ana", Role = Role.Estudiante, CreatedAt = DateTime.UtcNow, PasswordHash = "" };
        user.PasswordHash = hasher.HashPassword(user, "Password123!");

        _users.GetByEmailAsync(Arg.Any<string>(), Arg.Any<CancellationToken>()).Returns(user);

        var result = await _sut.LoginAsync(new LoginRequest("estudiante@ucr.ac.cr", "Incorrecta1!"), CancellationToken.None);

        Assert.True(result.IsUnauthorized);
        Assert.Null(result.Value);
    }

    [Fact]
    public async Task Login_WithUnknownEmail_ReturnsUnauthorized()
    {
        _users.GetByEmailAsync(Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns((User?)null);

        var result = await _sut.LoginAsync(new LoginRequest("fantasma@ucr.ac.cr", "Password123!"), CancellationToken.None);

        Assert.True(result.IsUnauthorized);
    }
}
