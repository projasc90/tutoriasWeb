namespace AuraLearn.Domain.Enums;

/// <summary>
/// Tipo de documento que el tutor presenta para la verificación de credenciales.
/// Titulo e Identificacion son únicos por tutor (upsert); Certificacion es
/// acumulativa (máx 5 archivos).
/// </summary>
public enum TutorDocumentKind
{
    Titulo = 1,
    Certificacion = 2,
    Identificacion = 3,
}
