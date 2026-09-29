using AuraLearn.Application.Dto;
using AuraLearn.Application.Interfaces;
using AuraLearn.Application.Services;
using AuraLearn.Domain.Entities;
using AuraLearn.Domain.Enums;
using FluentValidation.TestHelper;
using Microsoft.AspNetCore.Identity;
using NSubstitute;
using Xunit;

namespace AuraLearn.Tests;

/// <summary>
/// Tests TDD del flujo de postulación de tutores (onboarding del profesor).
/// Escritos ANTES de la implementación de TutorApplicationService (regla del repo:
/// TDD obligatorio en verificación de tutores).
/// </summary>
public class TutorApplicationServiceTests
{
    private readonly ITutorRepository _tutors = Substitute.For<ITutorRepository>();
    private readonly IUserRepository _users = Substitute.For<IUserRepository>();
    private readonly SubmitTutorApplicationValidator _validator = new();

    private TutorApplicationService CreateService() =>
        new(_tutors, _users, _validator);

    private static User CreateUser() => new()
    {
        Id = Guid.NewGuid(),
        Email = "profesor@ucr.ac.cr",
        PasswordHash = "hash",
        FullName = "Dra. Ana Mora",
        Role = Role.Estudiante,
        CreatedAt = DateTime.UtcNow,
    };

    private static SubmitTutorApplicationRequest CreateValidRequest() => new(
        FullName: "Dra. Ana Mora",
        Credentials: "PhD Matemáticas",
        University: "UCR",
        Bio: "10 años de experiencia en tutoría universitaria.",
        Subjects: ["Cálculo I", "Álgebra Lineal"],
        PriceCrc: 14000,
        PriceUsd: 27);

    // ── SubmitAsync ────────────────────────────────────────────────

    [Fact]
    public async Task Submit_WithValidRequest_CreatesTutorPendingReview()
    {
        var user = CreateUser();
        _users.GetByIdAsync(user.Id, Arg.Any<CancellationToken>())
            .Returns(user);
        _tutors.GetByUserIdAsync(user.Id, Arg.Any<CancellationToken>())
            .Returns((Tutor?)null);

        var service = CreateService();
        var result = await service.SubmitAsync(user.Id, CreateValidRequest(), CancellationToken.None);

        Assert.True(result.IsCreated);
        Assert.False(result.IsConflict);
        Assert.False(result.IsInvalid);
        Assert.Equal(nameof(VerificationStatus.PendingReview), result.Value!.Status);
        await _tutors.Received(1).AddAsync(
            Arg.Is<Tutor>(t =>
                t.UserId == user.Id &&
                t.Name == "Dra. Ana Mora" &&
                t.University == "UCR" &&
                t.VerificationStatus == VerificationStatus.PendingReview &&
                t.Rating == 0m && t.Reviews == 0 && !t.Featured),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Submit_WhenUserAlreadyHasActiveApplication_ReturnsConflict()
    {
        var user = CreateUser();
        _users.GetByIdAsync(user.Id, Arg.Any<CancellationToken>()).Returns(user);
        _tutors.GetByUserIdAsync(user.Id, Arg.Any<CancellationToken>())
            .Returns(new Tutor
            {
                Id = Guid.NewGuid(),
                UserId = user.Id,
                Name = "Dra. Ana Mora",
                Credentials = "Lic. Física",
                University = "UCR",
                Bio = "bio",
                VerificationStatus = VerificationStatus.PendingReview,
                CreatedAt = DateTime.UtcNow,
            });

        var service = CreateService();
        var result = await service.SubmitAsync(user.Id, CreateValidRequest(), CancellationToken.None);

        Assert.True(result.IsConflict);
        await _tutors.DidNotReceive().AddAsync(Arg.Any<Tutor>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Submit_WhenPreviousApplicationRejected_UpdatesAndReturnsPendingReview()
    {
        var user = CreateUser();
        _users.GetByIdAsync(user.Id, Arg.Any<CancellationToken>()).Returns(user);
        var rejected = new Tutor
        {
            Id = Guid.NewGuid(),
            UserId = user.Id,
            Name = "Dra. Ana Mora",
            Credentials = "Lic. Física",
            University = "UCR",
            Bio = "bio",
            VerificationStatus = VerificationStatus.Rejected,
            CreatedAt = DateTime.UtcNow,
        };
        _tutors.GetByUserIdAsync(user.Id, Arg.Any<CancellationToken>()).Returns(rejected);

        var service = CreateService();
        var result = await service.SubmitAsync(user.Id, CreateValidRequest(), CancellationToken.None);

        Assert.True(result.IsCreated);
        Assert.Equal(nameof(VerificationStatus.PendingReview), result.Value!.Status);
        Assert.Equal("PhD Matemáticas", rejected.Credentials);
        await _tutors.Received(1).UpdateAsync(rejected, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Submit_WithInvalidRequest_ReturnsInvalid()
    {
        var user = CreateUser();
        _users.GetByIdAsync(user.Id, Arg.Any<CancellationToken>()).Returns(user);
        _tutors.GetByUserIdAsync(user.Id, Arg.Any<CancellationToken>()).Returns((Tutor?)null);

        var service = CreateService();
        var invalid = CreateValidRequest() with { PriceCrc = 0 };
        var result = await service.SubmitAsync(user.Id, invalid, CancellationToken.None);

        Assert.True(result.IsInvalid);
        await _tutors.DidNotReceive().AddAsync(Arg.Any<Tutor>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Submit_WithUnknownUser_ReturnsUnauthorized()
    {
        _users.GetByIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>())
            .Returns((User?)null);

        var service = CreateService();
        var result = await service.SubmitAsync(Guid.NewGuid(), CreateValidRequest(), CancellationToken.None);

        Assert.True(result.IsUnauthorized);
    }

    // ── GetStatusAsync ─────────────────────────────────────────────

    [Fact]
    public async Task GetStatus_WithExistingApplication_ReturnsStatus()
    {
        var user = CreateUser();
        var tutor = new Tutor
        {
            Id = Guid.NewGuid(),
            UserId = user.Id,
            Name = user.FullName,
            Credentials = "PhD Matemáticas",
            University = "UCR",
            Bio = "bio",
            VerificationStatus = VerificationStatus.PendingReview,
            CreatedAt = DateTime.UtcNow,
        };
        _tutors.GetByUserIdAsync(user.Id, Arg.Any<CancellationToken>()).Returns(tutor);

        var service = CreateService();
        var result = await service.GetStatusAsync(user.Id, CancellationToken.None);

        Assert.True(result.IsFound);
        Assert.Equal(nameof(VerificationStatus.PendingReview), result.Value!.Status);
        Assert.Equal(tutor.Id, result.Value.Id);
    }

    [Fact]
    public async Task GetStatus_WithoutApplication_ReturnsNotFound()
    {
        _tutors.GetByUserIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>())
            .Returns((Tutor?)null);

        var service = CreateService();
        var result = await service.GetStatusAsync(Guid.NewGuid(), CancellationToken.None);

        Assert.True(result.IsNotFound);
    }

    // ── ListPendingAsync ───────────────────────────────────────────

    [Fact]
    public async Task ListPending_ReturnsOnlyPendingApplications()
    {
        var pending = new List<Tutor>
        {
            new()
            {
                Id = Guid.NewGuid(),
                UserId = Guid.NewGuid(),
                Name = "Tutor A",
                Credentials = "Lic. X",
                University = "UCR",
                Bio = "bio",
                VerificationStatus = VerificationStatus.PendingReview,
                CreatedAt = DateTime.UtcNow,
            },
        };
        _tutors.GetPendingAsync(Arg.Any<CancellationToken>()).Returns(pending);

        var service = CreateService();
        var result = await service.ListPendingAsync(CancellationToken.None);

        Assert.Single(result);
        Assert.Equal(nameof(VerificationStatus.PendingReview), result[0].Status);
    }

    // ── DecideAsync ────────────────────────────────────────────────

    [Fact]
    public async Task Decide_ApprovePendingApplication_MarksVerified()
    {
        var tutor = new Tutor
        {
            Id = Guid.NewGuid(),
            UserId = Guid.NewGuid(),
            Name = "Tutor A",
            Credentials = "Lic. X",
            University = "UCR",
            Bio = "bio",
            VerificationStatus = VerificationStatus.PendingReview,
            CreatedAt = DateTime.UtcNow,
        };
        _tutors.GetByIdAsync(tutor.Id, Arg.Any<CancellationToken>()).Returns(tutor);

        var service = CreateService();
        var result = await service.DecideAsync(tutor.Id, "approve", null, CancellationToken.None);

        Assert.True(result.IsDecided);
        Assert.Equal(VerificationStatus.Verified, tutor.VerificationStatus);
        await _tutors.Received(1).UpdateAsync(tutor, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Decide_RejectWithoutReason_ReturnsInvalid()
    {
        var tutor = new Tutor
        {
            Id = Guid.NewGuid(),
            UserId = Guid.NewGuid(),
            Name = "Tutor A",
            Credentials = "Lic. X",
            University = "UCR",
            Bio = "bio",
            VerificationStatus = VerificationStatus.PendingReview,
            CreatedAt = DateTime.UtcNow,
        };
        _tutors.GetByIdAsync(tutor.Id, Arg.Any<CancellationToken>()).Returns(tutor);

        var service = CreateService();
        var result = await service.DecideAsync(tutor.Id, "reject", null, CancellationToken.None);

        Assert.True(result.IsInvalid);
        await _tutors.DidNotReceive().UpdateAsync(Arg.Any<Tutor>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Decide_RejectWithReason_MarksRejectedAndStoresReason()
    {
        var tutor = new Tutor
        {
            Id = Guid.NewGuid(),
            UserId = Guid.NewGuid(),
            Name = "Tutor A",
            Credentials = "Lic. X",
            University = "UCR",
            Bio = "bio",
            VerificationStatus = VerificationStatus.PendingReview,
            CreatedAt = DateTime.UtcNow,
        };
        _tutors.GetByIdAsync(tutor.Id, Arg.Any<CancellationToken>()).Returns(tutor);

        var service = CreateService();
        var result = await service.DecideAsync(tutor.Id, "reject", "Título no verificable en el registro universitario", CancellationToken.None);

        Assert.True(result.IsDecided);
        Assert.Equal(VerificationStatus.Rejected, tutor.VerificationStatus);
        Assert.Equal("Título no verificable en el registro universitario", tutor.RejectionReason);
        await _tutors.Received(1).UpdateAsync(tutor, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Decide_WithUnknownId_ReturnsNotFound()
    {
        _tutors.GetByIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>())
            .Returns((Tutor?)null);

        var service = CreateService();
        var result = await service.DecideAsync(Guid.NewGuid(), "approve", null, CancellationToken.None);

        Assert.True(result.IsNotFound);
    }

    [Fact]
    public async Task Decide_WithInvalidDecision_ReturnsInvalid()
    {
        var service = CreateService();
        var result = await service.DecideAsync(Guid.NewGuid(), "maybe", null, CancellationToken.None);

        Assert.True(result.IsInvalid);
    }
}
