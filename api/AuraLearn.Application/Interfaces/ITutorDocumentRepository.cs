using AuraLearn.Domain.Entities;
using AuraLearn.Domain.Enums;

namespace AuraLearn.Application.Interfaces;

/// <summary>Puerto de acceso a datos de los documentos de verificación del tutor.</summary>
public interface ITutorDocumentRepository
{
    /// <summary>Documentos del tutor ordenados por fecha de subida.</summary>
    Task<IReadOnlyList<TutorDocument>> ListByTutorAsync(Guid tutorId, CancellationToken ct);

    /// <summary>Documento por id (para la descarga del admin).</summary>
    Task<TutorDocument?> GetByIdAsync(Guid id, CancellationToken ct);

    /// <summary>Documento existente de un tipo concreto (para el upsert de Titulo/Identificacion).</summary>
    Task<TutorDocument?> GetByKindAsync(Guid tutorId, TutorDocumentKind kind, CancellationToken ct);

    Task AddAsync(TutorDocument document, CancellationToken ct);

    Task RemoveAsync(TutorDocument document, CancellationToken ct);
}
