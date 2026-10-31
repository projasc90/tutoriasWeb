using AuraLearn.Application.Dto;
using AuraLearn.Application.Interfaces;
using AuraLearn.Application.Services;
using AuraLearn.Domain.Enums;
using NSubstitute;
using Xunit;

namespace AuraLearn.Tests;

/// <summary>
/// Tests del servicio de listado de usuarios para el panel de
/// administración de estudiantes (solo Admin a nivel de controller).
/// </summary>
public class AdminUserServiceTests
{
    private readonly IUserRepository _users = Substitute.For<IUserRepository>();
    private readonly AdminUserService _sut;

    public AdminUserServiceTests()
    {
        _sut = new AdminUserService(_users, new AdminUserSearchCriteriaValidator());
    }

    [Fact]
    public async Task Search_WithValidCriteria_DelegatesToRepository()
    {
        var expected = new PagedResult<AdminUserDto>(
            [new AdminUserDto(Guid.NewGuid(), "ana@ucr.ac.cr", "Ana", Role.Estudiante, DateTime.UtcNow)],
            Page: 1, PageSize: 20, TotalCount: 1);
        _users.SearchAsync(Arg.Any<AdminUserSearchCriteria>(), Arg.Any<CancellationToken>())
            .Returns(expected);

        var result = await _sut.SearchAsync(
            new AdminUserSearchCriteria(null, Role.Estudiante, 1, 20), CancellationToken.None);

        Assert.False(result.IsInvalid);
        Assert.Equal(expected, result.Value);
        await _users.Received(1).SearchAsync(
            Arg.Is<AdminUserSearchCriteria>(c => c.Role == Role.Estudiante && c.Page == 1),
            Arg.Any<CancellationToken>());
    }

    [Theory]
    [InlineData(0, 20)]    // page < 1
    [InlineData(1, 0)]     // pageSize < 1
    [InlineData(1, 101)]   // pageSize > MaxPageSize
    public async Task Search_WithInvalidCriteria_ReturnsInvalid(int page, int pageSize)
    {
        var result = await _sut.SearchAsync(
            new AdminUserSearchCriteria(null, null, page, pageSize), CancellationToken.None);

        Assert.True(result.IsInvalid);
        Assert.Null(result.Value);
        await _users.DidNotReceiveWithAnyArgs().SearchAsync(default!, default);
    }

    [Fact]
    public async Task Search_WithTooLongQuery_ReturnsInvalid()
    {
        var result = await _sut.SearchAsync(
            new AdminUserSearchCriteria(new string('a', 201), null, 1, 20), CancellationToken.None);

        Assert.True(result.IsInvalid);
        await _users.DidNotReceiveWithAnyArgs().SearchAsync(default!, default);
    }
}
