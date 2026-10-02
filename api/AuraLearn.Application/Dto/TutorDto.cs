namespace AuraLearn.Application.Dto;

/// <summary>
/// DTO de tutor para el catálogo público (contrato con el frontend).
/// NextSlotAt: próximo slot disponible en UTC (null si no hay slots).
/// El frontend formatea a la zona CR usando Intl + America/Costa_Rica.
/// </summary>
public record TutorDto(
    Guid Id,
    string Name,
    string Credentials,
    string University,
    decimal Rating,
    int Reviews,
    IReadOnlyList<string> Subjects,
    int PriceCrc,
    int PriceUsd,
    string Bio,
    bool Featured,
    DateTime? NextSlotAt);

/// <summary>Resultado paginado genérico.</summary>
public record PagedResult<T>(IReadOnlyList<T> Items, int Page, int PageSize, int TotalCount);
