using AuraLearn.Application.Dto;
using AuraLearn.Application.Interfaces;
using AuraLearn.Domain.Entities;
using AuraLearn.Domain.Enums;
using FluentValidation;

namespace AuraLearn.Application.Services;

/// <summary>
/// Validador de la postulación de un profesor. Reglas de negocio:
/// nombre/título/universidad/bio requeridos, 1-8 materias, tarifa CRC 1000-50000.
/// </summary>
public class SubmitTutorApplicationValidator : AbstractValidator<SubmitTutorApplicationRequest>
{
    public SubmitTutorApplicationValidator()
    {
        RuleFor(r => r.FullName).NotEmpty().MaximumLength(200);
        RuleFor(r => r.Credentials).NotEmpty().MaximumLength(200);
        RuleFor(r => r.Specialization).MaximumLength(100);
        RuleFor(r => r.University).NotEmpty().MaximumLength(100);
        RuleFor(r => r.Bio).NotEmpty().MaximumLength(1000);
        RuleFor(r => r.Subjects).NotEmpty().Must(s => s.Count <= 8)
            .WithMessage("Se permiten entre 1 y 8 materias");
        RuleFor(r => r.PriceCrc).InclusiveBetween(1000, 50000);
        RuleFor(r => r.PriceUsd).InclusiveBetween(1, 100);
    }
}

/// <summary>
/// Casos de uso de la postulación de tutores: envío (PendingReview),
/// consulta de estado, listado pendiente para admin y decisión (approve/reject).
/// Dominio crítico (verificación de tutores): TDD obligatorio, tests en
/// TutorApplicationServiceTests (escritos antes de esta implementación).
/// </summary>
public class TutorApplicationService(
    ITutorRepository tutors,
    IUserRepository users,
    SubmitTutorApplicationValidator validator,
    ITutorDocumentRepository documents)
{
    /// <summary>Tipos de documento exigidos para aprobar una postulación con usuario real.</summary>
    private static readonly TutorDocumentKind[] RequiredDocumentKinds =
        [TutorDocumentKind.Titulo, TutorDocumentKind.Certificacion, TutorDocumentKind.Identificacion];

    public async Task<TutorApplicationResult<TutorApplicationResponse>> SubmitAsync(
        Guid userId, SubmitTutorApplicationRequest request, CancellationToken ct)
    {
        if (!validator.Validate(request).IsValid)
            return TutorApplicationResult<TutorApplicationResponse>.Invalid();

        var user = await users.GetByIdAsync(userId, ct);
        if (user is null)
            return TutorApplicationResult<TutorApplicationResponse>.Unauthorized();

        var existing = await tutors.GetByUserIdAsync(userId, ct);
        if (existing is not null && existing.VerificationStatus != VerificationStatus.Rejected)
            return TutorApplicationResult<TutorApplicationResponse>.Conflict();

        if (existing is not null)
        {
            // Re-postulación tras rechazo: se actualiza la fila y vuelve a PendingReview.
            existing.Name = request.FullName;
            existing.Credentials = request.Credentials;
            existing.Specialization = request.Specialization;
            existing.University = request.University;
            existing.Bio = request.Bio;
            existing.Subjects = [.. request.Subjects.Select(SubjectNormalizer.Normalize)];
            existing.PriceCrc = request.PriceCrc;
            existing.PriceUsd = request.PriceUsd;
            existing.VerificationStatus = VerificationStatus.PendingReview;
            existing.RejectionReason = null;

            await tutors.UpdateAsync(existing, ct);
            return TutorApplicationResult<TutorApplicationResponse>.Created(
                new TutorApplicationResponse(existing.Id, existing.VerificationStatus.ToString()));
        }

        var tutor = new Tutor
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            Name = request.FullName,
            Credentials = request.Credentials,
            Specialization = request.Specialization,
            University = request.University,
            Bio = request.Bio,
            Subjects = [.. request.Subjects.Select(SubjectNormalizer.Normalize)],
            PriceCrc = request.PriceCrc,
            PriceUsd = request.PriceUsd,
            Rating = 0m,
            Reviews = 0,
            Featured = false,
            VerificationStatus = VerificationStatus.PendingReview,
            CreatedAt = DateTime.UtcNow,
        };

        await tutors.AddAsync(tutor, ct);
        return TutorApplicationResult<TutorApplicationResponse>.Created(
            new TutorApplicationResponse(tutor.Id, tutor.VerificationStatus.ToString()));
    }

    public async Task<TutorApplicationResult<TutorApplicationStatusResponse>> GetStatusAsync(
        Guid userId, CancellationToken ct)
    {
        var tutor = await tutors.GetByUserIdAsync(userId, ct);
        if (tutor is null)
            return TutorApplicationResult<TutorApplicationStatusResponse>.NotFound();

        return TutorApplicationResult<TutorApplicationStatusResponse>.Found(
            new TutorApplicationStatusResponse(
                tutor.Id, tutor.VerificationStatus.ToString(), tutor.RejectionReason, tutor.CreatedAt));
    }

    public async Task<IReadOnlyList<AdminTutorApplicationDto>> ListPendingAsync(CancellationToken ct)
    {
        var pending = await tutors.GetPendingAsync(ct);
        var result = new List<AdminTutorApplicationDto>(pending.Count);
        foreach (var tutor in pending)
        {
            var docs = await documents.ListByTutorAsync(tutor.Id, ct);
            result.Add(ToAdminDto(tutor, docs));
        }

        return result;
    }

    public async Task<TutorApplicationResult<TutorApplicationResponse>> DecideAsync(
        Guid id, string decision, string? reason, CancellationToken ct)
    {
        if (decision is not ("approve" or "reject"))
            return TutorApplicationResult<TutorApplicationResponse>.Invalid();

        var tutor = await tutors.GetByIdAsync(id, ct);
        if (tutor is null)
            return TutorApplicationResult<TutorApplicationResponse>.NotFound();

        if (decision == "reject" && string.IsNullOrWhiteSpace(reason))
            return TutorApplicationResult<TutorApplicationResponse>.Invalid();

        // Gate de documentos: aprobar exige los 3 tipos cargados (solo tutores con
        // usuario real; los seed demo sin UserId quedan exentos).
        if (decision == "approve" && tutor.UserId is not null)
        {
            var docs = await documents.ListByTutorAsync(tutor.Id, ct);
            var missing = RequiredDocumentKinds
                .Where(kind => !docs.Any(d => d.Kind == kind))
                .ToList();
            if (missing.Count > 0)
            {
                return TutorApplicationResult<TutorApplicationResponse>.Conflict(
                    TutorApplicationErrors.MissingDocuments,
                    "Faltan documentos: " + string.Join(", ", missing.Select(KindLabel)));
            }
        }

        tutor.VerificationStatus = decision == "approve"
            ? VerificationStatus.Verified
            : VerificationStatus.Rejected;
        tutor.RejectionReason = decision == "reject" ? reason : null;

        await tutors.UpdateAsync(tutor, ct);

        // Promoción de rol: al aprobar, el usuario pasa a ser Tutor. Los endpoints
        // [Authorize(Roles = "Tutor")] (disponibilidad, sesiones, complete) dependen
        // del claim role del JWT, que se emite al iniciar sesión.
        if (decision == "approve" && tutor.UserId is { } tutorUserId)
        {
            var user = await users.GetByIdAsync(tutorUserId, ct);
            if (user is not null && user.Role != Role.Tutor)
            {
                user.Role = Role.Tutor;
                await users.UpdateAsync(user, ct);
            }
        }

        return TutorApplicationResult<TutorApplicationResponse>.Decided(
            new TutorApplicationResponse(tutor.Id, tutor.VerificationStatus.ToString()));
    }

    private static AdminTutorApplicationDto ToAdminDto(Tutor t, IReadOnlyList<TutorDocument> docs) =>
        new(
            t.Id, t.UserId ?? Guid.Empty, t.Name, t.Credentials, t.Specialization, t.University, t.Bio,
            t.Subjects, t.PriceCrc, t.PriceUsd, t.VerificationStatus.ToString(), t.CreatedAt,
            [.. docs.Select(d => new TutorDocumentItemDto(d.Id, d.Kind, d.FileName, d.ContentType, d.SizeBytes, d.UploadedAt))]);

    /// <summary>Etiqueta en español del tipo de documento (para el detalle del 409).</summary>
    private static string KindLabel(TutorDocumentKind kind) => kind switch
    {
        TutorDocumentKind.Titulo => "Título académico",
        TutorDocumentKind.Certificacion => "Certificaciones",
        TutorDocumentKind.Identificacion => "Identificación",
        _ => kind.ToString(),
    };
}
