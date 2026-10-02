using AuraLearn.Application.Dto;
using AuraLearn.Application.Interfaces;
using FluentValidation;

namespace AuraLearn.Application.Services;

/// <summary>Validador de criterios de búsqueda del catálogo.</summary>
public class TutorSearchCriteriaValidator : AbstractValidator<TutorSearchCriteria>
{
    public const int MaxPageSize = 50;

    public TutorSearchCriteriaValidator()
    {
        RuleFor(c => c.Page).GreaterThanOrEqualTo(1);
        RuleFor(c => c.PageSize).GreaterThanOrEqualTo(1).LessThanOrEqualTo(MaxPageSize);
        RuleFor(c => c.MinRating).InclusiveBetween(0, 5).When(c => c.MinRating.HasValue);
        RuleFor(c => c.PriceMinCrc).GreaterThanOrEqualTo(0).When(c => c.PriceMinCrc.HasValue);
        RuleFor(c => c.PriceMaxCrc).GreaterThanOrEqualTo(0).When(c => c.PriceMaxCrc.HasValue);
    }
}

/// <summary>
/// Caso de uso: búsqueda del catálogo de tutores. Solo expone tutores
/// <c>Verified</c> (regla de negocio: el catálogo público nunca muestra
/// tutores sin verificar). Reemplaza el array DemoSlots por slots reales.
/// </summary>
public class TutorService(
    ITutorRepository repository,
    ISlotRepository slotRepository,
    SlotAvailabilityService slotAvailability)
{
    public async Task<PagedResult<TutorDto>> SearchAsync(TutorSearchCriteria criteria, CancellationToken ct)
    {
        var (items, totalCount) = await repository.SearchAsync(criteria, ct);

        // Ensure-ahead + próximo slot disponible para los tutores de esta página
        var tutorIds = items.Select(t => t.Id).ToList();
        var now = DateTime.UtcNow;
        var twoWeeksLater = now.AddDays(14);
        var slotsMap = new Dictionary<Guid, DateTime?>();

        if (tutorIds.Count > 0)
        {
            var slots = await slotAvailability.EnsureAheadAsync(tutorIds, now, twoWeeksLater, ct);
            slotsMap = slots
                .GroupBy(s => s.TutorId)
                .ToDictionary(
                    g => g.Key,
                    g => g.Where(s => s.StartAt > now)
                          .OrderBy(s => s.StartAt)
                          .FirstOrDefault()?.StartAt);
        }

        var dtos = items
            .Select(t => new TutorDto(
                t.Id, t.Name, t.Credentials, t.University, t.Rating, t.Reviews,
                t.Subjects, t.PriceCrc, t.PriceUsd, t.Bio, t.Featured,
                slotsMap.TryGetValue(t.Id, out var ns) ? ns : null))
            .ToList();

        return new PagedResult<TutorDto>(dtos, criteria.Page, criteria.PageSize, totalCount);
    }

    /// <summary>
    /// Detalle de un tutor para la página de perfil /tutores/[id].
    /// </summary>
    public async Task<TutorDto?> GetByIdAsync(Guid id, CancellationToken ct)
    {
        var tutor = await repository.GetByIdAsync(id, ct);
        if (tutor is null) return null;

        // Ensure-ahead para que el perfil tenga slots reservables en los próximos 14 días
        var now = DateTime.UtcNow;
        await slotAvailability.EnsureAheadAsync([id], now, now.AddDays(14), ct);
        var nextSlot = await slotRepository.GetNextAvailableStartAsync(id, now, ct);
        return new TutorDto(
            tutor.Id, tutor.Name, tutor.Credentials, tutor.University,
            tutor.Rating, tutor.Reviews, tutor.Subjects,
            tutor.PriceCrc, tutor.PriceUsd, tutor.Bio, tutor.Featured,
            nextSlot);
    }
}
