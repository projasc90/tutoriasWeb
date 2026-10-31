using AuraLearn.Application.Dto;
using AuraLearn.Application.Interfaces;
using AuraLearn.Application.Services;
using AuraLearn.Domain.Entities;
using NSubstitute;
using Xunit;

namespace AuraLearn.Tests;

/// <summary>
/// Tests TDD de la foto de perfil del tutor (subida desde /tutor/perfil).
/// Escritos ANTES de la implementación de TutorPhotoService. La foto es
/// pública (no es documento de verificación): upsert con borrado del archivo
/// previo; solo JPEG/PNG/WebP ≤ 5 MB con extensión coherente.
/// </summary>
public class TutorPhotoServiceTests
{
    private readonly ITutorRepository _tutors = Substitute.For<ITutorRepository>();
    private readonly IFileStorage _storage = Substitute.For<IFileStorage>();

    private TutorPhotoService CreateService() => new(_tutors, _storage);

    private static Tutor CreateTutor(string? photoPath = null) => new()
    {
        Id = Guid.NewGuid(),
        UserId = Guid.NewGuid(),
        Name = "Dra. Ana Mora",
        Credentials = "PhD Matemáticas",
        University = "UCR",
        Bio = "bio",
        VerificationStatus = Domain.Enums.VerificationStatus.Verified,
        PhotoPath = photoPath,
        CreatedAt = DateTime.UtcNow,
    };

    private static (string FileName, string ContentType, long Size, Stream Content) Image(
        string fileName = "foto.jpg", string contentType = "image/jpeg", long size = 2048) =>
        (fileName, contentType, size, new MemoryStream(new byte[32]));

    // ── UploadAsync ────────────────────────────────────────────────

    [Fact]
    public async Task Upload_WithValidJpeg_StoresUpdatesEntityAndReturnsPhotoUrl()
    {
        var tutor = CreateTutor();
        _tutors.GetByUserIdAsync(tutor.UserId!.Value, Arg.Any<CancellationToken>()).Returns(tutor);
        _storage.SavePhotoAsync(tutor.Id, "foto.jpg", Arg.Any<Stream>(), Arg.Any<CancellationToken>())
            .Returns($"{tutor.Id}/photo/{Guid.NewGuid():N}.jpg");

        var service = CreateService();
        var (fileName, contentType, size, content) = Image();
        var result = await service.UploadAsync(tutor.UserId!.Value, fileName, contentType, size, content, CancellationToken.None);

        Assert.True(result.IsUpdated);
        Assert.Equal($"/api/tutors/{tutor.Id}/photo", result.Value!.PhotoUrl);
        Assert.NotEmpty(tutor.PhotoPath!);
        await _storage.Received(1).SavePhotoAsync(
            tutor.Id, "foto.jpg", Arg.Any<Stream>(), Arg.Any<CancellationToken>());
        await _tutors.Received(1).UpdateAsync(tutor, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Upload_WithUnsupportedContentType_ReturnsInvalid()
    {
        var tutor = CreateTutor();
        _tutors.GetByUserIdAsync(tutor.UserId!.Value, Arg.Any<CancellationToken>()).Returns(tutor);

        var service = CreateService();
        var result = await service.UploadAsync(
            tutor.UserId!.Value, "doc.pdf", "application/pdf", 1024, new MemoryStream(new byte[8]), CancellationToken.None);

        Assert.True(result.IsInvalid);
        await _storage.DidNotReceive().SavePhotoAsync(
            Arg.Any<Guid>(), Arg.Any<string>(), Arg.Any<Stream>(), Arg.Any<CancellationToken>());
        await _tutors.DidNotReceive().UpdateAsync(Arg.Any<Tutor>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Upload_WithMismatchedExtension_ReturnsInvalid()
    {
        var tutor = CreateTutor();
        _tutors.GetByUserIdAsync(tutor.UserId!.Value, Arg.Any<CancellationToken>()).Returns(tutor);

        var service = CreateService();
        var result = await service.UploadAsync(
            tutor.UserId!.Value, "foto.gif", "image/png", 1024, new MemoryStream(new byte[8]), CancellationToken.None);

        Assert.True(result.IsInvalid);
    }

    [Fact]
    public async Task Upload_Oversized_ReturnsInvalid()
    {
        var tutor = CreateTutor();
        _tutors.GetByUserIdAsync(tutor.UserId!.Value, Arg.Any<CancellationToken>()).Returns(tutor);

        var service = CreateService();
        var result = await service.UploadAsync(
            tutor.UserId!.Value, "foto.png", "image/png", 5 * 1024 * 1024 + 1, new MemoryStream(new byte[8]), CancellationToken.None);

        Assert.True(result.IsInvalid);
    }

    [Fact]
    public async Task Upload_WhenUserHasNoTutorRow_ReturnsNotFound()
    {
        _tutors.GetByUserIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>()).Returns((Tutor?)null);

        var service = CreateService();
        var (fileName, contentType, size, content) = Image();
        var result = await service.UploadAsync(Guid.NewGuid(), fileName, contentType, size, content, CancellationToken.None);

        Assert.True(result.IsNotFound);
    }

    [Fact]
    public async Task Upload_ReplacesExistingPhoto_DeletesOldFile()
    {
        var tutor = CreateTutor("old-path.png");
        _tutors.GetByUserIdAsync(tutor.UserId!.Value, Arg.Any<CancellationToken>()).Returns(tutor);
        _storage.SavePhotoAsync(tutor.Id, "foto.png", Arg.Any<Stream>(), Arg.Any<CancellationToken>())
            .Returns("new-path.png");

        var service = CreateService();
        var (fileName, contentType, size, content) = Image(fileName: "foto.png", contentType: "image/png");
        var result = await service.UploadAsync(tutor.UserId!.Value, fileName, contentType, size, content, CancellationToken.None);

        Assert.True(result.IsUpdated);
        Assert.Equal("new-path.png", tutor.PhotoPath);
        await _storage.Received(1).DeleteAsync("old-path.png", Arg.Any<CancellationToken>());
    }

    // ── GetPhotoAsync ──────────────────────────────────────────────

    [Fact]
    public async Task GetPhoto_WithPhoto_ReturnsStreamAndContentType()
    {
        var tutor = CreateTutor("p/foto.jpg");
        _tutors.GetByIdAsync(tutor.Id, Arg.Any<CancellationToken>()).Returns(tutor);
        var stream = new MemoryStream(new byte[16]);
        _storage.OpenAsync("p/foto.jpg", Arg.Any<CancellationToken>()).Returns(stream);

        var service = CreateService();
        var result = await service.GetPhotoAsync(tutor.Id, CancellationToken.None);

        Assert.True(result.IsFound);
        Assert.Equal("image/jpeg", result.Value!.ContentType);
        Assert.Equal(stream, result.Value.Content);
    }

    [Fact]
    public async Task GetPhoto_WithoutPhoto_ReturnsNotFound()
    {
        var tutor = CreateTutor(null);
        _tutors.GetByIdAsync(tutor.Id, Arg.Any<CancellationToken>()).Returns(tutor);

        var service = CreateService();
        var result = await service.GetPhotoAsync(tutor.Id, CancellationToken.None);

        Assert.True(result.IsNotFound);
    }

    [Fact]
    public async Task GetPhoto_WithUnknownTutor_ReturnsNotFound()
    {
        _tutors.GetByIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>()).Returns((Tutor?)null);

        var service = CreateService();
        var result = await service.GetPhotoAsync(Guid.NewGuid(), CancellationToken.None);

        Assert.True(result.IsNotFound);
    }
}
