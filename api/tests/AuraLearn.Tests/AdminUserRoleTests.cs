using AuraLearn.Application.Dto;
using AuraLearn.Application.Interfaces;
using AuraLearn.Application.Services;
using AuraLearn.Domain.Entities;
using AuraLearn.Domain.Enums;
using NSubstitute;
using Xunit;

namespace AuraLearn.Tests;

/// <summary>
/// Cambio de rol desde el panel Admin. La regla crítica es no dejar la
/// plataforma sin administradores: nadie podría aprobar pagos, verificar
/// tutores ni editar las reglas de reserva.
/// </summary>
public class AdminUserRoleTests
{
    private readonly IUserRepository _users = Substitute.For<IUserRepository>();
    private readonly AdminUserService _sut;

    public AdminUserRoleTests()
        => _sut = new AdminUserService(_users, new AdminUserSearchCriteriaValidator());

    private static User NewUser(Role role) => new()
    {
        Id = Guid.NewGuid(),
        Email = "ana@ucr.ac.cr",
        FullName = "Ana Vargas",
        PasswordHash = "hash",
        Role = role,
        CreatedAt = DateTime.UtcNow,
    };

    [Fact]
    public async Task ChangeRole_PromotesStudentToAdmin()
    {
        var user = NewUser(Role.Estudiante);
        _users.GetByIdAsync(user.Id, Arg.Any<CancellationToken>()).Returns(user);

        var result = await _sut.ChangeRoleAsync(user.Id, Role.Admin, CancellationToken.None);

        Assert.False(result.IsInvalid);
        Assert.False(result.IsNotFound);
        Assert.Equal(Role.Admin, result.Value!.Role);
        await _users.Received(1).UpdateAsync(
            Arg.Is<User>(u => u.Id == user.Id && u.Role == Role.Admin),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ChangeRole_UserNotFound_ReturnsNotFound()
    {
        var result = await _sut.ChangeRoleAsync(Guid.NewGuid(), Role.Admin, CancellationToken.None);

        Assert.True(result.IsNotFound);
        Assert.Null(result.Value);
        await _users.DidNotReceiveWithAnyArgs().UpdateAsync(default!, default);
    }

    [Fact]
    public async Task ChangeRole_SameRole_ReturnsRoleUnchanged()
    {
        var user = NewUser(Role.Tutor);
        _users.GetByIdAsync(user.Id, Arg.Any<CancellationToken>()).Returns(user);

        var result = await _sut.ChangeRoleAsync(user.Id, Role.Tutor, CancellationToken.None);

        Assert.True(result.IsInvalid);
        Assert.Equal(AdminUserErrors.RoleUnchanged, result.ErrorCode);
        await _users.DidNotReceiveWithAnyArgs().UpdateAsync(default!, default);
    }

    [Fact]
    public async Task ChangeRole_DemotingLastAdmin_IsRejected()
    {
        var admin = NewUser(Role.Admin);
        _users.GetByIdAsync(admin.Id, Arg.Any<CancellationToken>()).Returns(admin);
        _users.CountByRoleAsync(Role.Admin, Arg.Any<CancellationToken>()).Returns(1);

        var result = await _sut.ChangeRoleAsync(admin.Id, Role.Estudiante, CancellationToken.None);

        Assert.True(result.IsInvalid);
        Assert.Equal(AdminUserErrors.LastAdmin, result.ErrorCode);
        await _users.DidNotReceiveWithAnyArgs().UpdateAsync(default!, default);
    }

    [Fact]
    public async Task ChangeRole_DemotingAdminWithAnotherAdmin_Succeeds()
    {
        var admin = NewUser(Role.Admin);
        _users.GetByIdAsync(admin.Id, Arg.Any<CancellationToken>()).Returns(admin);
        _users.CountByRoleAsync(Role.Admin, Arg.Any<CancellationToken>()).Returns(2);

        var result = await _sut.ChangeRoleAsync(admin.Id, Role.Tutor, CancellationToken.None);

        Assert.False(result.IsInvalid);
        Assert.Equal(Role.Tutor, result.Value!.Role);
        await _users.Received(1).UpdateAsync(Arg.Any<User>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ChangeRole_NonAdminDemotion_DoesNotCountAdmins()
    {
        var tutor = NewUser(Role.Tutor);
        _users.GetByIdAsync(tutor.Id, Arg.Any<CancellationToken>()).Returns(tutor);

        var result = await _sut.ChangeRoleAsync(tutor.Id, Role.Estudiante, CancellationToken.None);

        Assert.False(result.IsInvalid);
        await _users.DidNotReceiveWithAnyArgs()
            .CountByRoleAsync(Arg.Any<Role>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ChangeRole_UpdatePersistsTheNewRoleOnTheSameEntity()
    {
        var user = NewUser(Role.Estudiante);
        _users.GetByIdAsync(user.Id, Arg.Any<CancellationToken>()).Returns(user);

        await _sut.ChangeRoleAsync(user.Id, Role.Tutor, CancellationToken.None);

        // El repositorio escribe la entidad completa: el rol ya debe estar mutado.
        Assert.Equal(Role.Tutor, user.Role);
    }
}
