using AuraLearn.Application.Dto;
using AuraLearn.Application.Interfaces;
using AuraLearn.Application.Services;
using AuraLearn.Domain.Entities;
using AuraLearn.Domain.Enums;
using NSubstitute;
using Xunit;

namespace AuraLearn.Tests;

/// <summary>
/// Tests TDD del perfil editable del tutor (GET/PUT /api/tutors/me/profile).
/// Escritos junto a la implementación de TutorProfileService (dominio de
/// verificación de tutores: TDD obligatorio).
/// </summary>
public class TutorProfileServiceTests
{
    private readonly ITutorRepository _tutors = Substitute.For<ITutorRepository>();
    private readonly UpdateTutorProfileValidator _validator = new();

    private TutorProfileService CreateService() =>
        new(_tutors, _validator);

    private static Tutor CreateTutor(VerificationStatus status = VerificationStatus.Verified) => new()
    {
        Id = Guid.NewGuid(),
        UserId = Guid.NewGuid(),
        Name = "Dra. Ana Mora",
        Credentials = "PhD Matemáticas",
        Specialization = "Matemática Aplicada",
        University = "UCR",
        Rating = 4.9m,
        Reviews = 12,
        Subjects = ["Cálculo I", "Álgebra Lineal"],
        PriceCrc = 14000,
        PriceUsd = 27,
        Bio = "10 años de experiencia en tutoría universitaria.",
        Featured = false,
        VerificationStatus = status,
        CreatedAt = DateTime.UtcNow,
    };

    private static UpdateTutorProfileRequest CreateValidRequest() => new(
        Credentials: "PhD Matemáticas",
        Specialization: "Estadística Aplicada",
        University: "UCR",
        Bio: "10 años de experiencia en tutoría universitaria.",
        Subjects: ["Cálculo I", "Estadística"],
        PriceCrc: 15000,
        PriceUsd: 29);

    // ── GetProfileForCurrentUserAsync ──────────────────────────────

    [Fact]
    public async Task GetProfile_WithTutorRow_ReturnsMappedProfile()
    {
        var tutor = CreateTutor();
        _tutors.GetByUserIdAsync(tutor.UserId!.Value, Arg.Any<CancellationToken>())
            .Returns(tutor);

        var service = CreateService();
        var result = await service.GetProfileForCurrentUserAsync(tutor.UserId!.Value, CancellationToken.None);

        Assert.True(result.IsFound);
        var profile = result.Value!;
        Assert.Equal(tutor.Id, profile.Id);
        Assert.Equal("Dra. Ana Mora", profile.Name);
        Assert.Equal("PhD Matemáticas", profile.Credentials);
        Assert.Equal("Matemática Aplicada", profile.Specialization);
        Assert.Equal("UCR", profile.University);
        Assert.Equal(2, profile.Subjects.Count);
        Assert.Equal(nameof(VerificationStatus.Verified), profile.VerificationStatus);
    }

    [Fact]
    public async Task GetProfile_WithoutTutorRow_ReturnsNotFound()
    {
        _tutors.GetByUserIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>())
            .Returns((Tutor?)null);

        var service = CreateService();
        var result = await service.GetProfileForCurrentUserAsync(Guid.NewGuid(), CancellationToken.None);

        Assert.True(result.IsNotFound);
    }

    // ── UpdateProfileForCurrentUserAsync ───────────────────────────

    [Fact]
    public async Task UpdateProfile_WithValidRequest_UpdatesEditableFieldsOnly()
    {
        var tutor = CreateTutor();
        _tutors.GetByUserIdAsync(tutor.UserId!.Value, Arg.Any<CancellationToken>())
            .Returns(tutor);

        var service = CreateService();
        var result = await service.UpdateProfileForCurrentUserAsync(
            tutor.UserId!.Value, CreateValidRequest(), CancellationToken.None);

        Assert.True(result.IsUpdated);
        Assert.Equal("Estadística Aplicada", tutor.Specialization);
        Assert.Equal("PhD Matemáticas", tutor.Credentials);
        Assert.Equal("UCR", tutor.University);
        Assert.Equal(15000, tutor.PriceCrc);
        Assert.Equal(29, tutor.PriceUsd);
        // Materias normalizadas: primera letra mayúscula, conectores en minúscula.
        Assert.Equal(["Cálculo I", "Estadística"], tutor.Subjects);
        // Campos no editables por el tutor quedan intactos.
        Assert.Equal(4.9m, tutor.Rating);
        Assert.Equal(12, tutor.Reviews);
        Assert.False(tutor.Featured);
        Assert.Equal(VerificationStatus.Verified, tutor.VerificationStatus);
        await _tutors.Received(1).UpdateAsync(tutor, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task UpdateProfile_NormalizesSubjectCasing()
    {
        var tutor = CreateTutor();
        _tutors.GetByUserIdAsync(tutor.UserId!.Value, Arg.Any<CancellationToken>())
            .Returns(tutor);

        var service = CreateService();
        var request = CreateValidRequest() with { Subjects = ["cálculo i", "estadística y probabilidad", "TOEFL"] };
        var result = await service.UpdateProfileForCurrentUserAsync(
            tutor.UserId!.Value, request, CancellationToken.None);

        Assert.True(result.IsUpdated);
        Assert.Equal(["Cálculo I", "Estadística y Probabilidad", "TOEFL"], tutor.Subjects);
    }

    [Fact]
    public async Task UpdateProfile_WithInvalidRequest_ReturnsInvalid()
    {
        var tutor = CreateTutor();
        _tutors.GetByUserIdAsync(tutor.UserId!.Value, Arg.Any<CancellationToken>())
            .Returns(tutor);

        var service = CreateService();
        var invalid = CreateValidRequest() with { PriceCrc = 0 };
        var result = await service.UpdateProfileForCurrentUserAsync(
            tutor.UserId!.Value, invalid, CancellationToken.None);

        Assert.True(result.IsInvalid);
        await _tutors.DidNotReceive().UpdateAsync(Arg.Any<Tutor>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task UpdateProfile_WithoutTutorRow_ReturnsNotFound()
    {
        _tutors.GetByUserIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>())
            .Returns((Tutor?)null);

        var service = CreateService();
        var result = await service.UpdateProfileForCurrentUserAsync(
            Guid.NewGuid(), CreateValidRequest(), CancellationToken.None);

        Assert.True(result.IsNotFound);
    }

    [Fact]
    public async Task UpdateProfile_WhenRejected_ReturnsConflictWithReapplyCode()
    {
        var tutor = CreateTutor(VerificationStatus.Rejected);
        tutor.RejectionReason = "Título no verificable";
        _tutors.GetByUserIdAsync(tutor.UserId!.Value, Arg.Any<CancellationToken>())
            .Returns(tutor);

        var service = CreateService();
        var result = await service.UpdateProfileForCurrentUserAsync(
            tutor.UserId!.Value, CreateValidRequest(), CancellationToken.None);

        Assert.True(result.IsConflict);
        Assert.Equal(TutorProfileErrors.ProfileRejected, result.ErrorCode);
        await _tutors.DidNotReceive().UpdateAsync(Arg.Any<Tutor>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task UpdateProfile_WhenPendingReview_AllowsEditing()
    {
        // Un postulante pendiente puede corregir su alta mientras espera revisión.
        var tutor = CreateTutor(VerificationStatus.PendingReview);
        _tutors.GetByUserIdAsync(tutor.UserId!.Value, Arg.Any<CancellationToken>())
            .Returns(tutor);

        var service = CreateService();
        var result = await service.UpdateProfileForCurrentUserAsync(
            tutor.UserId!.Value, CreateValidRequest(), CancellationToken.None);

        Assert.True(result.IsUpdated);
        Assert.Equal(VerificationStatus.PendingReview, tutor.VerificationStatus);
        await _tutors.Received(1).UpdateAsync(tutor, Arg.Any<CancellationToken>());
    }
}
