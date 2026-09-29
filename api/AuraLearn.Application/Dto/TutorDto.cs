namespace AuraLearn.Application.Dto;

/// <summary>DTO de tutor para el catálogo público (contrato con el frontend).</summary>
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
    string NextSlot);

/// <summary>Resultado paginado genérico.</summary>
public record PagedResult<T>(IReadOnlyList<T> Items, int Page, int PageSize, int TotalCount);
