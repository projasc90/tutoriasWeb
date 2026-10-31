using AuraLearn.Application.Dto;
using AuraLearn.Application.Interfaces;
using AuraLearn.Domain.Entities;
using AuraLearn.Domain.Enums;
using FluentValidation;

namespace AuraLearn.Application.Services;

/// <summary>
/// Validador de la actualización del perfil del tutor autenticado.
/// Mismas reglas de negocio que la postulación (SubmitTutorApplicationValidator).
/// </summary>
public class UpdateTutorProfileValidator : AbstractValidator<UpdateTutorProfileRequest>
{
    public UpdateTutorProfileValidator()
    {
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
/// Casos de uso del perfil del tutor autenticado: consulta y edición de
/// título, especialización, universidad, bio, materias y tarifas.
/// No expone Rating/Reviews/Featured (solo el sistema los muta) ni el estado
/// de verificación (solo el admin, vía TutorApplicationService.DecideAsync).
/// </summary>
public class TutorProfileService(
    ITutorRepository tutors,
    UpdateTutorProfileValidator validator)
{
    public async Task<TutorProfileResult<TutorProfileResponse>> GetProfileForCurrentUserAsync(
        Guid userId, CancellationToken ct)
    {
        var tutor = await tutors.GetByUserIdAsync(userId, ct);
        if (tutor is null)
            return TutorProfileResult<TutorProfileResponse>.NotFound();

        return TutorProfileResult<TutorProfileResponse>.Found(ToResponse(tutor));
    }

    public async Task<TutorProfileResult<TutorProfileResponse>> UpdateProfileForCurrentUserAsync(
        Guid userId, UpdateTutorProfileRequest request, CancellationToken ct)
    {
        if (!validator.Validate(request).IsValid)
            return TutorProfileResult<TutorProfileResponse>.Invalid();

        var tutor = await tutors.GetByUserIdAsync(userId, ct);
        if (tutor is null)
            return TutorProfileResult<TutorProfileResponse>.NotFound();

        // Un tutor rechazado no edita el perfil: la re-postulación (/postular)
        // actualiza todos los campos y devuelve la fila a PendingReview.
        if (tutor.VerificationStatus == VerificationStatus.Rejected)
            return TutorProfileResult<TutorProfileResponse>.Conflict(TutorProfileErrors.ProfileRejected);

        tutor.Credentials = request.Credentials;
        tutor.Specialization = request.Specialization;
        tutor.University = request.University;
        tutor.Bio = request.Bio;
        tutor.Subjects = [.. request.Subjects.Select(SubjectNormalizer.Normalize)];
        tutor.PriceCrc = request.PriceCrc;
        tutor.PriceUsd = request.PriceUsd;

        await tutors.UpdateAsync(tutor, ct);
        return TutorProfileResult<TutorProfileResponse>.Updated(ToResponse(tutor));
    }

    private static TutorProfileResponse ToResponse(Tutor t) =>
        new(t.Id, t.Name, t.Credentials, t.Specialization, t.University, t.Bio,
            t.Subjects, t.PriceCrc, t.PriceUsd, t.VerificationStatus.ToString(), t.RejectionReason,
            TutorPhotoService.PhotoUrlOrNull(t));
}
