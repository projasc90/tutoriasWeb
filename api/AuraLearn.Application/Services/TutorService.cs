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
/// tutores sin verificar).
/// </summary>
public class TutorService(ITutorRepository repository)
{
    /// <summary>Próximo cupo presentacional (mock hasta que exista el motor de slots).</summary>
    private static readonly string[] DemoSlots =
    [
        "Hoy, 3:30 PM", "Mañana, 10:00 AM", "Jueves, 5:00 PM", "Viernes, 2:00 PM",
        "Mañana, 9:00 AM", "Hoy, 6:00 PM", "Miércoles, 4:00 PM", "Sábado, 11:00 AM",
        "Hoy, 8:00 PM", "Domingo, 10:00 AM", "Viernes, 9:00 AM", "Jueves, 3:00 PM",
    ];

    public async Task<PagedResult<TutorDto>> SearchAsync(TutorSearchCriteria criteria, CancellationToken ct)
    {
        var (items, totalCount) = await repository.SearchAsync(criteria, ct);

        var dtos = items
            .Select((t, i) => new TutorDto(
                t.Id, t.Name, t.Credentials, t.University, t.Rating, t.Reviews,
                t.Subjects, t.PriceCrc, t.PriceUsd, t.Bio, t.Featured,
                NextSlot: DemoSlots[i % DemoSlots.Length]))
            .ToList();

        return new PagedResult<TutorDto>(dtos, criteria.Page, criteria.PageSize, totalCount);
    }
}
