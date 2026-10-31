using AuraLearn.Application.Dto;
using AuraLearn.Application.Interfaces;
using AuraLearn.Application.Services;
using AuraLearn.Domain.Entities;
using AuraLearn.Domain.Enums;
using NSubstitute;
using Xunit;

namespace AuraLearn.Tests;

/// <summary>
/// Tests TDD de la subida de documentos de verificación del tutor
/// (títulos, certificaciones, identificación). Escritos ANTES de la
/// implementación (TDD obligatorio: PII/seguridad).
/// Reglas: Titulo y Certificacion acumulan (máx 5 c/u); Identificacion es
/// única (upsert). Con estado Verified se pueden AGREGAR títulos y
/// certificaciones, pero la identificación queda bloqueada (ya fue verificada).
/// </summary>
public class TutorDocumentServiceTests
{
    private readonly ITutorRepository _tutors = Substitute.For<ITutorRepository>();
    private readonly ITutorDocumentRepository _documents = Substitute.For<ITutorDocumentRepository>();
    private readonly IFileStorage _storage = Substitute.For<IFileStorage>();

    private TutorDocumentService CreateService() =>
        new(_tutors, _documents, _storage);

    private static Tutor CreateTutor(
        VerificationStatus status = VerificationStatus.PendingReview, Guid? userId = null) => new()
    {
        Id = Guid.NewGuid(),
        UserId = userId ?? Guid.NewGuid(),
        Name = "Dra. Ana Mora",
        Credentials = "PhD Matemáticas",
        University = "UCR",
        Bio = "bio",
        VerificationStatus = status,
        CreatedAt = DateTime.UtcNow,
    };

    private static UploadDocumentCommand CreateCommand(
        TutorDocumentKind kind = TutorDocumentKind.Titulo,
        string fileName = "titulo.pdf",
        string contentType = "application/pdf",
        long sizeBytes = 1024)
    {
        var content = new MemoryStream(new byte[sizeBytes > 0 ? Math.Min(sizeBytes, 64) : 1]);
        return new UploadDocumentCommand(kind, fileName, contentType, sizeBytes, content);
    }

    private static TutorDocument CreateDocument(
        TutorDocumentKind kind = TutorDocumentKind.Titulo, string storagePath = "t1/titulo/old.pdf") => new()
    {
        Id = Guid.NewGuid(),
        TutorId = Guid.NewGuid(),
        Kind = kind,
        FileName = "old.pdf",
        ContentType = "application/pdf",
        SizeBytes = 512,
        StoragePath = storagePath,
        UploadedAt = DateTime.UtcNow,
    };

    private void MockStorageSave(Tutor tutor, TutorDocumentKind kind, string fileName)
    {
        _storage.SaveAsync(tutor.Id, kind, fileName, Arg.Any<Stream>(), Arg.Any<CancellationToken>())
            .Returns($"{tutor.Id}/{kind}/{Guid.NewGuid():N}.pdf");
    }

    // ── UploadAsync: validaciones de archivo ──────────────────────

    [Fact]
    public async Task Upload_WithValidPdf_CreatesDocumentAndStoresFile()
    {
        var tutor = CreateTutor();
        _tutors.GetByUserIdAsync(tutor.UserId!.Value, Arg.Any<CancellationToken>()).Returns(tutor);
        _documents.ListByTutorAsync(tutor.Id, Arg.Any<CancellationToken>()).Returns([]);
        MockStorageSave(tutor, TutorDocumentKind.Titulo, "titulo.pdf");

        var service = CreateService();
        var result = await service.UploadAsync(tutor.UserId!.Value, CreateCommand(), CancellationToken.None);

        Assert.True(result.IsCreated);
        Assert.Equal(TutorDocumentKind.Titulo, result.Value!.Kind);
        Assert.Equal("titulo.pdf", result.Value.FileName);
        await _storage.Received(1).SaveAsync(
            tutor.Id, TutorDocumentKind.Titulo, "titulo.pdf", Arg.Any<Stream>(), Arg.Any<CancellationToken>());
        await _documents.Received(1).AddAsync(
            Arg.Is<TutorDocument>(d =>
                d.TutorId == tutor.Id &&
                d.Kind == TutorDocumentKind.Titulo &&
                d.ContentType == "application/pdf" &&
                d.SizeBytes == 1024 &&
                d.StoragePath.EndsWith(".pdf")),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Upload_WithUnsupportedContentType_ReturnsInvalid()
    {
        var tutor = CreateTutor();
        _tutors.GetByUserIdAsync(tutor.UserId!.Value, Arg.Any<CancellationToken>()).Returns(tutor);

        var service = CreateService();
        var result = await service.UploadAsync(
            tutor.UserId!.Value, CreateCommand(contentType: "text/plain", fileName: "notas.txt"), CancellationToken.None);

        Assert.True(result.IsInvalid);
        await _storage.DidNotReceive().SaveAsync(
            Arg.Any<Guid>(), Arg.Any<TutorDocumentKind>(), Arg.Any<string>(), Arg.Any<Stream>(), Arg.Any<CancellationToken>());
        await _documents.DidNotReceive().AddAsync(Arg.Any<TutorDocument>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Upload_WithMismatchedExtension_ReturnsInvalid()
    {
        var tutor = CreateTutor();
        _tutors.GetByUserIdAsync(tutor.UserId!.Value, Arg.Any<CancellationToken>()).Returns(tutor);

        var service = CreateService();
        var result = await service.UploadAsync(
            tutor.UserId!.Value, CreateCommand(fileName: "titulo.doc", contentType: "application/pdf"), CancellationToken.None);

        Assert.True(result.IsInvalid);
        await _storage.DidNotReceive().SaveAsync(
            Arg.Any<Guid>(), Arg.Any<TutorDocumentKind>(), Arg.Any<string>(), Arg.Any<Stream>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Upload_WithOversizedFile_ReturnsInvalid()
    {
        var tutor = CreateTutor();
        _tutors.GetByUserIdAsync(tutor.UserId!.Value, Arg.Any<CancellationToken>()).Returns(tutor);

        var service = CreateService();
        var result = await service.UploadAsync(
            tutor.UserId!.Value, CreateCommand(sizeBytes: 5 * 1024 * 1024 + 1), CancellationToken.None);

        Assert.True(result.IsInvalid);
        await _storage.DidNotReceive().SaveAsync(
            Arg.Any<Guid>(), Arg.Any<TutorDocumentKind>(), Arg.Any<string>(), Arg.Any<Stream>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Upload_WithEmptyFileName_ReturnsInvalid()
    {
        var tutor = CreateTutor();
        _tutors.GetByUserIdAsync(tutor.UserId!.Value, Arg.Any<CancellationToken>()).Returns(tutor);

        var service = CreateService();
        var result = await service.UploadAsync(
            tutor.UserId!.Value, CreateCommand(fileName: ""), CancellationToken.None);

        Assert.True(result.IsInvalid);
    }

    [Fact]
    public async Task Upload_WithUnknownKind_ReturnsInvalid()
    {
        var tutor = CreateTutor();
        _tutors.GetByUserIdAsync(tutor.UserId!.Value, Arg.Any<CancellationToken>()).Returns(tutor);

        var service = CreateService();
        var result = await service.UploadAsync(
            tutor.UserId!.Value, CreateCommand(kind: (TutorDocumentKind)99), CancellationToken.None);

        Assert.True(result.IsInvalid);
    }

    [Fact]
    public async Task Upload_WhenUserHasNoTutorRow_ReturnsNotFound()
    {
        _tutors.GetByUserIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>()).Returns((Tutor?)null);

        var service = CreateService();
        var result = await service.UploadAsync(Guid.NewGuid(), CreateCommand(), CancellationToken.None);

        Assert.True(result.IsNotFound);
        await _storage.DidNotReceive().SaveAsync(
            Arg.Any<Guid>(), Arg.Any<TutorDocumentKind>(), Arg.Any<string>(), Arg.Any<Stream>(), Arg.Any<CancellationToken>());
    }

    // ── UploadAsync: límites por tipo ─────────────────────────────

    [Fact]
    public async Task Upload_Titulo_AccumulatesWithoutRemovingExisting()
    {
        var tutor = CreateTutor();
        _tutors.GetByUserIdAsync(tutor.UserId!.Value, Arg.Any<CancellationToken>()).Returns(tutor);
        var existing = CreateDocument(TutorDocumentKind.Titulo);
        existing.TutorId = tutor.Id;
        _documents.ListByTutorAsync(tutor.Id, Arg.Any<CancellationToken>()).Returns([existing]);
        MockStorageSave(tutor, TutorDocumentKind.Titulo, "titulo2.pdf");

        var service = CreateService();
        var result = await service.UploadAsync(
            tutor.UserId!.Value, CreateCommand(fileName: "titulo2.pdf"), CancellationToken.None);

        Assert.True(result.IsCreated);
        await _documents.DidNotReceive().RemoveAsync(Arg.Any<TutorDocument>(), Arg.Any<CancellationToken>());
        await _storage.DidNotReceive().DeleteAsync(Arg.Any<string>(), Arg.Any<CancellationToken>());
        await _documents.Received(1).AddAsync(
            Arg.Is<TutorDocument>(d => d.Kind == TutorDocumentKind.Titulo), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Upload_Titulo_WhenFiveAlreadyUploaded_ReturnsInvalid()
    {
        var tutor = CreateTutor();
        _tutors.GetByUserIdAsync(tutor.UserId!.Value, Arg.Any<CancellationToken>()).Returns(tutor);
        var existing = Enumerable.Range(0, 5)
            .Select(_ => CreateDocument(TutorDocumentKind.Titulo))
            .ToList();
        _documents.ListByTutorAsync(tutor.Id, Arg.Any<CancellationToken>()).Returns(existing);

        var service = CreateService();
        var result = await service.UploadAsync(tutor.UserId!.Value, CreateCommand(), CancellationToken.None);

        Assert.True(result.IsInvalid);
        await _storage.DidNotReceive().SaveAsync(
            Arg.Any<Guid>(), Arg.Any<TutorDocumentKind>(), Arg.Any<string>(), Arg.Any<Stream>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Upload_Certificacion_WhenFiveAlreadyUploaded_ReturnsInvalid()
    {
        var tutor = CreateTutor();
        _tutors.GetByUserIdAsync(tutor.UserId!.Value, Arg.Any<CancellationToken>()).Returns(tutor);
        var existing = Enumerable.Range(0, 5)
            .Select(_ => CreateDocument(TutorDocumentKind.Certificacion))
            .ToList();
        _documents.ListByTutorAsync(tutor.Id, Arg.Any<CancellationToken>()).Returns(existing);

        var service = CreateService();
        var result = await service.UploadAsync(
            tutor.UserId!.Value, CreateCommand(kind: TutorDocumentKind.Certificacion), CancellationToken.None);

        Assert.True(result.IsInvalid);
        await _storage.DidNotReceive().SaveAsync(
            Arg.Any<Guid>(), Arg.Any<TutorDocumentKind>(), Arg.Any<string>(), Arg.Any<Stream>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Upload_Certificacion_WhenFourUploaded_Accumulates()
    {
        var tutor = CreateTutor();
        _tutors.GetByUserIdAsync(tutor.UserId!.Value, Arg.Any<CancellationToken>()).Returns(tutor);
        var existing = Enumerable.Range(0, 4)
            .Select(_ => CreateDocument(TutorDocumentKind.Certificacion))
            .ToList();
        _documents.ListByTutorAsync(tutor.Id, Arg.Any<CancellationToken>()).Returns(existing);
        MockStorageSave(tutor, TutorDocumentKind.Certificacion, "cert.pdf");

        var service = CreateService();
        var result = await service.UploadAsync(
            tutor.UserId!.Value,
            CreateCommand(kind: TutorDocumentKind.Certificacion, fileName: "cert.pdf"),
            CancellationToken.None);

        Assert.True(result.IsCreated);
        await _documents.DidNotReceive().RemoveAsync(Arg.Any<TutorDocument>(), Arg.Any<CancellationToken>());
        await _documents.Received(1).AddAsync(
            Arg.Is<TutorDocument>(d => d.Kind == TutorDocumentKind.Certificacion), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Upload_Identificacion_ReplacesExisting_RemovesOldRowAndFile()
    {
        var tutor = CreateTutor();
        _tutors.GetByUserIdAsync(tutor.UserId!.Value, Arg.Any<CancellationToken>()).Returns(tutor);
        var old = CreateDocument(TutorDocumentKind.Identificacion, "old-ident.png");
        old.TutorId = tutor.Id;
        _documents.ListByTutorAsync(tutor.Id, Arg.Any<CancellationToken>()).Returns([old]);
        _documents.GetByKindAsync(tutor.Id, TutorDocumentKind.Identificacion, Arg.Any<CancellationToken>()).Returns(old);
        MockStorageSave(tutor, TutorDocumentKind.Identificacion, "ident.png");

        var service = CreateService();
        var result = await service.UploadAsync(
            tutor.UserId!.Value,
            CreateCommand(kind: TutorDocumentKind.Identificacion, fileName: "ident.png", contentType: "image/png"),
            CancellationToken.None);

        Assert.True(result.IsCreated);
        await _documents.Received(1).RemoveAsync(old, Arg.Any<CancellationToken>());
        await _storage.Received(1).DeleteAsync("old-ident.png", Arg.Any<CancellationToken>());
        await _documents.Received(1).AddAsync(
            Arg.Is<TutorDocument>(d => d.Kind == TutorDocumentKind.Identificacion), Arg.Any<CancellationToken>());
    }

    // ── UploadAsync: post-aprobación (Verified) ───────────────────

    [Fact]
    public async Task Upload_WhenVerified_AllowsAddingTitulo()
    {
        var tutor = CreateTutor(VerificationStatus.Verified);
        _tutors.GetByUserIdAsync(tutor.UserId!.Value, Arg.Any<CancellationToken>()).Returns(tutor);
        _documents.ListByTutorAsync(tutor.Id, Arg.Any<CancellationToken>()).Returns([]);
        MockStorageSave(tutor, TutorDocumentKind.Titulo, "maestria.pdf");

        var service = CreateService();
        var result = await service.UploadAsync(
            tutor.UserId!.Value, CreateCommand(fileName: "maestria.pdf"), CancellationToken.None);

        Assert.True(result.IsCreated);
        await _documents.Received(1).AddAsync(
            Arg.Is<TutorDocument>(d => d.Kind == TutorDocumentKind.Titulo), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Upload_WhenVerified_AllowsAddingCertificacion()
    {
        var tutor = CreateTutor(VerificationStatus.Verified);
        _tutors.GetByUserIdAsync(tutor.UserId!.Value, Arg.Any<CancellationToken>()).Returns(tutor);
        _documents.ListByTutorAsync(tutor.Id, Arg.Any<CancellationToken>()).Returns([]);
        MockStorageSave(tutor, TutorDocumentKind.Certificacion, "colegiatura.pdf");

        var service = CreateService();
        var result = await service.UploadAsync(
            tutor.UserId!.Value,
            CreateCommand(kind: TutorDocumentKind.Certificacion, fileName: "colegiatura.pdf"),
            CancellationToken.None);

        Assert.True(result.IsCreated);
        await _documents.Received(1).AddAsync(
            Arg.Is<TutorDocument>(d => d.Kind == TutorDocumentKind.Certificacion), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Upload_WhenVerified_BlocksIdentificacion()
    {
        var tutor = CreateTutor(VerificationStatus.Verified);
        _tutors.GetByUserIdAsync(tutor.UserId!.Value, Arg.Any<CancellationToken>()).Returns(tutor);

        var service = CreateService();
        var result = await service.UploadAsync(
            tutor.UserId!.Value,
            CreateCommand(kind: TutorDocumentKind.Identificacion, fileName: "ident.png", contentType: "image/png"),
            CancellationToken.None);

        Assert.True(result.IsConflict);
        Assert.Equal(TutorDocumentErrors.IdentificationLocked, result.ErrorCode);
        await _storage.DidNotReceive().SaveAsync(
            Arg.Any<Guid>(), Arg.Any<TutorDocumentKind>(), Arg.Any<string>(), Arg.Any<Stream>(), Arg.Any<CancellationToken>());
        await _documents.DidNotReceive().AddAsync(Arg.Any<TutorDocument>(), Arg.Any<CancellationToken>());
    }

    // ── ListAsync ──────────────────────────────────────────────────

    [Fact]
    public async Task List_WithTutorRow_ReturnsDocuments()
    {
        var tutor = CreateTutor();
        _tutors.GetByUserIdAsync(tutor.UserId!.Value, Arg.Any<CancellationToken>()).Returns(tutor);
        var docs = new List<TutorDocument>
        {
            CreateDocument(TutorDocumentKind.Titulo),
            CreateDocument(TutorDocumentKind.Certificacion),
        };
        foreach (var d in docs) d.TutorId = tutor.Id;
        _documents.ListByTutorAsync(tutor.Id, Arg.Any<CancellationToken>()).Returns(docs);

        var service = CreateService();
        var result = await service.ListAsync(tutor.UserId!.Value, CancellationToken.None);

        Assert.True(result.IsFound);
        Assert.Equal(2, result.Value!.Count);
        Assert.Contains(result.Value, d => d.Kind == TutorDocumentKind.Titulo);
        Assert.Contains(result.Value, d => d.Kind == TutorDocumentKind.Certificacion);
        Assert.All(result.Value, d => Assert.NotEqual(Guid.Empty, d.Id));
    }

    [Fact]
    public async Task List_WhenNoTutorRow_ReturnsNotFound()
    {
        _tutors.GetByUserIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>()).Returns((Tutor?)null);

        var service = CreateService();
        var result = await service.ListAsync(Guid.NewGuid(), CancellationToken.None);

        Assert.True(result.IsNotFound);
    }

    // ── AttachAsync: subida directa confirmada (ADR-009) ──────────

    /// <summary>Pathname válido: cuelga del prefijo del usuario autenticado.</summary>
    private static string ValidPathname(Guid userId) =>
        $"{userId:N}/titulo/{Guid.NewGuid():N}.pdf";

    [Fact]
    public async Task Attach_WithOwnedPathAndMatchingMetadata_CreatesDocument()
    {
        var tutor = CreateTutor();
        var userId = tutor.UserId!.Value;
        var pathname = ValidPathname(userId);

        _tutors.GetByUserIdAsync(userId, Arg.Any<CancellationToken>()).Returns(tutor);
        _documents.ListByTutorAsync(tutor.Id, Arg.Any<CancellationToken>()).Returns([]);
        _storage.GetInfoAsync(pathname, Arg.Any<CancellationToken>())
            .Returns(new StoredFileInfo(1024, "application/pdf"));

        var service = CreateService();
        var result = await service.AttachAsync(
            userId,
            new AttachDocumentCommand(TutorDocumentKind.Titulo, pathname, "titulo.pdf", "application/pdf", 1024),
            CancellationToken.None);

        Assert.True(result.IsCreated);
        Assert.Equal("titulo.pdf", result.Value!.FileName);
        Assert.Equal(1024, result.Value.SizeBytes);
        await _documents.Received(1).AddAsync(
            Arg.Is<TutorDocument>(d => d.TutorId == tutor.Id && d.StoragePath == pathname),
            Arg.Any<CancellationToken>());
        // La subida directa nunca escribe bytes desde el API.
        await _storage.DidNotReceive().SaveAsync(
            Arg.Any<Guid>(), Arg.Any<TutorDocumentKind>(), Arg.Any<string>(),
            Arg.Any<Stream>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Attach_WithPathFromAnotherUser_IsInvalidAndDoesNotPersist()
    {
        var tutor = CreateTutor();
        // El pathname lo construyó otro usuario: adoptar ese blob sería un bypass de autorización.
        var foreignPathname = ValidPathname(Guid.NewGuid());

        _tutors.GetByUserIdAsync(tutor.UserId!.Value, Arg.Any<CancellationToken>()).Returns(tutor);

        var service = CreateService();
        var result = await service.AttachAsync(
            tutor.UserId!.Value,
            new AttachDocumentCommand(TutorDocumentKind.Titulo, foreignPathname, "titulo.pdf", "application/pdf", 1024),
            CancellationToken.None);

        Assert.True(result.IsInvalid);
        await _storage.DidNotReceive().GetInfoAsync(Arg.Any<string>(), Arg.Any<CancellationToken>());
        await _documents.DidNotReceive().AddAsync(Arg.Any<TutorDocument>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Attach_WhenStoredSizeDiffersFromDeclared_IsInvalid()
    {
        var tutor = CreateTutor();
        var userId = tutor.UserId!.Value;
        var pathname = ValidPathname(userId);

        _tutors.GetByUserIdAsync(userId, Arg.Any<CancellationToken>()).Returns(tutor);
        // El cliente declara 1 KB pero el store dice 9 MB: se descarta.
        _storage.GetInfoAsync(pathname, Arg.Any<CancellationToken>())
            .Returns(new StoredFileInfo(9 * 1024 * 1024, "application/pdf"));

        var service = CreateService();
        var result = await service.AttachAsync(
            userId,
            new AttachDocumentCommand(TutorDocumentKind.Titulo, pathname, "titulo.pdf", "application/pdf", 1024),
            CancellationToken.None);

        Assert.True(result.IsInvalid);
        await _documents.DidNotReceive().AddAsync(Arg.Any<TutorDocument>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Attach_WhenStorageContentTypeIsNotAllowed_IsInvalid()
    {
        var tutor = CreateTutor();
        var userId = tutor.UserId!.Value;
        var pathname = $"{userId:N}/titulo/{Guid.NewGuid():N}.exe";

        _tutors.GetByUserIdAsync(userId, Arg.Any<CancellationToken>()).Returns(tutor);
        _storage.GetInfoAsync(pathname, Arg.Any<CancellationToken>())
            .Returns(new StoredFileInfo(1024, "application/x-msdownload"));

        var service = CreateService();
        var result = await service.AttachAsync(
            userId,
            new AttachDocumentCommand(TutorDocumentKind.Titulo, pathname, "titulo.exe", "application/pdf", 1024),
            CancellationToken.None);

        Assert.True(result.IsInvalid);
    }

    [Fact]
    public async Task Attach_WhenBlobMissingInStore_IsInvalid()
    {
        var tutor = CreateTutor();
        var userId = tutor.UserId!.Value;
        var pathname = ValidPathname(userId);

        _tutors.GetByUserIdAsync(userId, Arg.Any<CancellationToken>()).Returns(tutor);
        _storage.GetInfoAsync(pathname, Arg.Any<CancellationToken>()).Returns((StoredFileInfo?)null);

        var service = CreateService();
        var result = await service.AttachAsync(
            userId,
            new AttachDocumentCommand(TutorDocumentKind.Titulo, pathname, "titulo.pdf", "application/pdf", 1024),
            CancellationToken.None);

        Assert.True(result.IsInvalid);
    }

    [Fact]
    public async Task Attach_WhenIdentificationIsLockedInVerified_ReturnsConflict()
    {
        var tutor = CreateTutor(VerificationStatus.Verified);
        var userId = tutor.UserId!.Value;
        var pathname = $"{userId:N}/identificacion/{Guid.NewGuid():N}.pdf";

        _tutors.GetByUserIdAsync(userId, Arg.Any<CancellationToken>()).Returns(tutor);

        var service = CreateService();
        var result = await service.AttachAsync(
            userId,
            new AttachDocumentCommand(TutorDocumentKind.Identificacion, pathname, "id.pdf", "application/pdf", 1024),
            CancellationToken.None);

        Assert.True(result.IsConflict);
        Assert.Equal(TutorDocumentErrors.IdentificationLocked, result.ErrorCode);
    }
}
