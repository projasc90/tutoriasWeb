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
    SubmitTutorApplicationValidator validator)
{
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
            existing.University = request.University;
            existing.Bio = request.Bio;
            existing.Subjects = [.. request.Subjects];
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
            University = request.University,
            Bio = request.Bio,
            Subjects = [.. request.Subjects],
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
        return [.. pending.Select(ToAdminDto)];
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

        tutor.VerificationStatus = decision == "approve"
            ? VerificationStatus.Verified
            : VerificationStatus.Rejected;
        tutor.RejectionReason = decision == "reject" ? reason : null;

        await tutors.UpdateAsync(tutor, ct);
        return TutorApplicationResult<TutorApplicationResponse>.Decided(
            new TutorApplicationResponse(tutor.Id, tutor.VerificationStatus.ToString()));
    }

    private static AdminTutorApplicationDto ToAdminDto(Tutor t) =>
        new(
            t.Id, t.UserId ?? Guid.Empty, t.Name, t.Credentials, t.University, t.Bio,
            t.Subjects, t.PriceCrc, t.PriceUsd, t.VerificationStatus.ToString(), t.CreatedAt);
}
