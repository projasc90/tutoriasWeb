using AuraLearn.Application.Dto;
using AuraLearn.Application.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AuraLearn.Api.Controllers;

/// <summary>
/// Endpoints de consulta de slots disponibles de un tutor.
/// </summary>
[ApiController]
[Route("api/tutors/{tutorId:guid}/slots")]
public class SlotsController(SlotAvailabilityService availability) : ControllerBase
{
    /// <summary>
    /// Slots disponibles de un tutor en un rango [from, to) UTC.
    /// Ensure-ahead (ADR-005): antes de responder, genera y persiste
    /// idempotentemente los slots faltantes del tutor en el rango.
    /// </summary>
    [HttpGet]
    [AllowAnonymous] // el catálogo es público; la reserva requiere auth
    public async Task<ActionResult<IReadOnlyList<SlotDto>>> GetSlots(
        Guid tutorId,
        [FromQuery] DateTime? from,
        [FromQuery] DateTime? to,
        CancellationToken ct)
    {
        var now = DateTime.UtcNow;
        var fromUtc = from ?? now;
        var toUtc = to ?? now.AddDays(14);

        var slots = await availability.EnsureAheadAsync([tutorId], fromUtc, toUtc, ct);
        return Ok(SlotQueryService.ToDtos(slots));
    }
}
