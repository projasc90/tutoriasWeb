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
    private readonly ITutorDocumentRepository _documents = Substitute.For<ITutorDocumentRepository>();
    private readonly SubmitTutorApplicationValidator _validator = new();

    private TutorApplicationService CreateService() =>
        new(_tutors, _users, _validator, _documents);

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
        Specialization: null,
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
    public async Task Submit_NormalizesSubjectCasing()
    {
        var user = CreateUser();
        _users.GetByIdAsync(user.Id, Arg.Any<CancellationToken>()).Returns(user);
        _tutors.GetByUserIdAsync(user.Id, Arg.Any<CancellationToken>()).Returns((Tutor?)null);

        var service = CreateService();
        var request = CreateValidRequest() with { Subjects = ["cálculo i", "estadística y probabilidad"] };
        var result = await service.SubmitAsync(user.Id, request, CancellationToken.None);

        Assert.True(result.IsCreated);
        await _tutors.Received(1).AddAsync(
            Arg.Is<Tutor>(t =>
                t.Subjects.SequenceEqual(new[] { "Cálculo I", "Estadística y Probabilidad" })),
            Arg.Any<CancellationToken>());
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
        MockDocumentsPresent(tutor.Id);

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

    // ── Promoción de rol al aprobar (Fase 0) ──────────────────────

    [Fact]
    public async Task Decide_ApprovePendingApplication_PromotesUserToTutorRole()
    {
        var user = CreateUser(); // Role.Estudiante
        var tutor = new Tutor
        {
            Id = Guid.NewGuid(),
            UserId = user.Id,
            Name = user.FullName,
            Credentials = "Lic. X",
            University = "UCR",
            Bio = "bio",
            VerificationStatus = VerificationStatus.PendingReview,
            CreatedAt = DateTime.UtcNow,
        };
        _tutors.GetByIdAsync(tutor.Id, Arg.Any<CancellationToken>()).Returns(tutor);
        _users.GetByIdAsync(user.Id, Arg.Any<CancellationToken>()).Returns(user);
        MockDocumentsPresent(tutor.Id);

        var service = CreateService();
        var result = await service.DecideAsync(tutor.Id, "approve", null, CancellationToken.None);

        Assert.True(result.IsDecided);
        Assert.Equal(Role.Tutor, user.Role);
        await _users.Received(1).UpdateAsync(
            Arg.Is<User>(u => u.Id == user.Id && u.Role == Role.Tutor),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Decide_RejectPendingApplication_DoesNotChangeUserRole()
    {
        var user = CreateUser();
        var tutor = new Tutor
        {
            Id = Guid.NewGuid(),
            UserId = user.Id,
            Name = user.FullName,
            Credentials = "Lic. X",
            University = "UCR",
            Bio = "bio",
            VerificationStatus = VerificationStatus.PendingReview,
            CreatedAt = DateTime.UtcNow,
        };
        _tutors.GetByIdAsync(tutor.Id, Arg.Any<CancellationToken>()).Returns(tutor);
        _users.GetByIdAsync(user.Id, Arg.Any<CancellationToken>()).Returns(user);

        var service = CreateService();
        var result = await service.DecideAsync(
            tutor.Id, "reject", "Título no verificable en el registro universitario", CancellationToken.None);

        Assert.True(result.IsDecided);
        Assert.Equal(Role.Estudiante, user.Role);
        await _users.DidNotReceive().UpdateAsync(Arg.Any<User>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Decide_ApproveWithoutUserId_DoesNotTouchUsers()
    {
        // Tutores seed demo sin UserId: la aprobación no debe fallar ni tocar users.
        var tutor = new Tutor
        {
            Id = Guid.NewGuid(),
            UserId = null,
            Name = "Tutor Seed",
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
        await _users.DidNotReceive().GetByIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>());
        await _users.DidNotReceive().UpdateAsync(Arg.Any<User>(), Arg.Any<CancellationToken>());
        await _documents.DidNotReceive().ListByTutorAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>());
    }

    // ── Gate de documentos al aprobar (Fase B) ────────────────────

    [Fact]
    public async Task Decide_ApproveWithoutDocuments_ReturnsConflictAndDoesNotDecide()
    {
        var user = CreateUser();
        var tutor = new Tutor
        {
            Id = Guid.NewGuid(),
            UserId = user.Id,
            Name = user.FullName,
            Credentials = "Lic. X",
            University = "UCR",
            Bio = "bio",
            VerificationStatus = VerificationStatus.PendingReview,
            CreatedAt = DateTime.UtcNow,
        };
        _tutors.GetByIdAsync(tutor.Id, Arg.Any<CancellationToken>()).Returns(tutor);
        _users.GetByIdAsync(user.Id, Arg.Any<CancellationToken>()).Returns(user);
        _documents.ListByTutorAsync(tutor.Id, Arg.Any<CancellationToken>()).Returns([]);

        var service = CreateService();
        var result = await service.DecideAsync(tutor.Id, "approve", null, CancellationToken.None);

        Assert.True(result.IsConflict);
        Assert.False(result.IsDecided);
        Assert.Equal(TutorApplicationErrors.MissingDocuments, result.ErrorCode);
        Assert.Contains("Título académico", result.ErrorDetail);
        Assert.Contains("Certificaciones", result.ErrorDetail);
        Assert.Contains("Identificación", result.ErrorDetail);
        Assert.Equal(VerificationStatus.PendingReview, tutor.VerificationStatus);
        await _tutors.DidNotReceive().UpdateAsync(Arg.Any<Tutor>(), Arg.Any<CancellationToken>());
        Assert.Equal(Role.Estudiante, user.Role);
        await _users.DidNotReceive().UpdateAsync(Arg.Any<User>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Decide_ApproveWithPartialDocuments_ListsOnlyMissingKinds()
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
        var titulo = new TutorDocument
        {
            Id = Guid.NewGuid(),
            TutorId = tutor.Id,
            Kind = TutorDocumentKind.Titulo,
            FileName = "titulo.pdf",
            ContentType = "application/pdf",
            SizeBytes = 1024,
            StoragePath = "p.pdf",
            UploadedAt = DateTime.UtcNow,
        };
        _documents.ListByTutorAsync(tutor.Id, Arg.Any<CancellationToken>()).Returns([titulo]);

        var service = CreateService();
        var result = await service.DecideAsync(tutor.Id, "approve", null, CancellationToken.None);

        Assert.True(result.IsConflict);
        Assert.DoesNotContain("Título académico", result.ErrorDetail);
        Assert.Contains("Certificaciones", result.ErrorDetail);
        Assert.Contains("Identificación", result.ErrorDetail);
    }

    [Fact]
    public async Task Decide_Reject_DoesNotRequireDocuments()
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
        var result = await service.DecideAsync(
            tutor.Id, "reject", "Título no verificable en el registro universitario", CancellationToken.None);

        Assert.True(result.IsDecided);
        await _documents.DidNotReceive().ListByTutorAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>());
    }

    /// <summary>Mockea los 3 tipos de documento presentes para el tutor dado.</summary>
    private void MockDocumentsPresent(Guid tutorId)
    {
        var docs = new List<TutorDocument>
        {
            new() { Id = Guid.NewGuid(), TutorId = tutorId, Kind = TutorDocumentKind.Titulo, FileName = "t.pdf", ContentType = "application/pdf", SizeBytes = 1, StoragePath = "t.pdf", UploadedAt = DateTime.UtcNow },
            new() { Id = Guid.NewGuid(), TutorId = tutorId, Kind = TutorDocumentKind.Certificacion, FileName = "c.pdf", ContentType = "application/pdf", SizeBytes = 1, StoragePath = "c.pdf", UploadedAt = DateTime.UtcNow },
            new() { Id = Guid.NewGuid(), TutorId = tutorId, Kind = TutorDocumentKind.Identificacion, FileName = "i.pdf", ContentType = "application/pdf", SizeBytes = 1, StoragePath = "i.pdf", UploadedAt = DateTime.UtcNow },
        };
        _documents.ListByTutorAsync(tutorId, Arg.Any<CancellationToken>()).Returns(docs);
    }

    [Fact]
    public async Task Submit_WithSpecialization_PersistsItOnNewTutor()
    {
        var user = CreateUser();
        _users.GetByIdAsync(user.Id, Arg.Any<CancellationToken>()).Returns(user);
        _tutors.GetByUserIdAsync(user.Id, Arg.Any<CancellationToken>()).Returns((Tutor?)null);

        var service = CreateService();
        var request = CreateValidRequest() with { Specialization = "Matemática Aplicada" };
        var result = await service.SubmitAsync(user.Id, request, CancellationToken.None);

        Assert.True(result.IsCreated);
        await _tutors.Received(1).AddAsync(
            Arg.Is<Tutor>(t => t.Specialization == "Matemática Aplicada"),
            Arg.Any<CancellationToken>());
    }
}
