using AuraLearn.Application.Dto;
using AuraLearn.Application.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace AuraLearn.Api.Controllers;

/// <summary>
/// Endpoints de reservas: crear, comprobante, cancelar, consultar.
/// </summary>
[ApiController]
[Route("api/reservations")]
[Authorize]
public class ReservationsController : ControllerBase
{
    private readonly ReservationService _service;

    public ReservationsController(ReservationService service)
    {
        _service = service;
    }

    private Guid GetUserId()
    {
        var id = User.FindFirstValue(ClaimTypes.NameIdentifier)
              ?? User.FindFirstValue("sub");
        return Guid.Parse(id!);
    }

    /// <summary>Reservas del estudiante autenticado (panel /mis-tutorias).</summary>
    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<ReservationDto>>> GetMine(CancellationToken ct)
    {
        var id = GetUserId();
        var reservations = await _service.GetByStudentAsync(id, ct);
        return Ok(reservations);
    }

    /// <summary>Sesiones que imparte el tutor autenticado (panel /tutor/sesiones).</summary>
    [HttpGet("tutor-sessions")]
    public async Task<ActionResult<IReadOnlyList<ReservationDto>>> GetTutorSessions(CancellationToken ct)
    {
        if (!User.IsInRole("Tutor"))
            return Forbid();

        var sessions = await _service.GetByTutorUserAsync(GetUserId(), ct);
        return Ok(sessions);
    }

    /// <summary>Detalle de una reserva (polling del checkout).</summary>
    [HttpGet("{id:guid}")]
    public async Task<ActionResult<ReservationDto>> GetById(Guid id, CancellationToken ct)
    {
        var userId = GetUserId();
        var isAdmin = User.IsInRole("Admin");
        var result = await _service.GetByIdAsync(id, userId, isAdmin, ct);

        if (result.IsNotFound) return NotFound();
        if (result.IsForbidden) return Forbid();

        return Ok(result.Value);
    }

    /// <summary>Crea una reserva idempotente (idempotencyKey en el body).</summary>
    [HttpPost]
    public async Task<ActionResult<ReservationDto>> Create(
        [FromBody] CreateReservationRequest request,
        CancellationToken ct)
    {
        var studentId = GetUserId();
        var result = await _service.CreateAsync(studentId, request, ct);

        if (result.IsNotFound) return NotFound();
        if (result.IsConflict) return Conflict(new { error = "SLOT_ALREADY_RESERVED" });
        if (result.IsInvalid) return UnprocessableEntity(new { error = "SLOT_IN_PAST_OR_SELF_BOOKING" });

        if (result.IsFound)
            return Ok(result.Value); // replay idempotente → 200

        return CreatedAtAction(nameof(GetById), new { id = result.Value!.Id }, result.Value);
    }

    /// <summary>Estudiante sube el comprobante SINPE (queda en cola del Admin).</summary>
    [HttpPost("{id:guid}/comprobante")]
    public async Task<ActionResult<ReservationDto>> SubmitComprobante(
        Guid id,
        [FromBody] SubmitComprobanteRequest request,
        CancellationToken ct)
    {
        var studentId = GetUserId();
        var reservation = await _service.GetByIdAsync(id, studentId, false, ct);
        if (reservation.IsNotFound) return NotFound();
        if (reservation.IsForbidden) return Forbid();

        var result = await _service.SubmitComprobanteAsync(id, request, ct);

        if (result.IsConflict)
            return Conflict(new { error = "COMPROBANTE_REUTILIZADO_O_RESERVA_NO_PENDIENTE" });
        if (result.IsInvalid)
            return UnprocessableEntity(new { error = "VALIDATION_FAILED" });

        return Accepted(result.Value);
    }

    /// <summary>Estudiante cancela su reserva.</summary>
    [HttpPatch("{id:guid}/cancel")]
    public async Task<ActionResult<ReservationDto>> Cancel(Guid id, CancellationToken ct)
    {
        var studentId = GetUserId();
        var result = await _service.CancelAsync(id, studentId, ct);

        if (result.IsNotFound) return NotFound();
        if (result.IsForbidden) return Forbid();
        if (result.IsConflict)
            return Conflict(new { error = "CANNOT_CANCEL_IN_TERMINAL_STATE" });

        return Ok(result.Value);
    }

    /// <summary>El tutor dueño marca la sesión como concluida (con liquidación).</summary>
    [HttpPatch("{id:guid}/complete")]
    public async Task<ActionResult> Complete(Guid id, CancellationToken ct)
    {
        if (!User.IsInRole("Tutor"))
            return Forbid();

        var result = await _service.CompleteAsync(id, GetUserId(), ct);

        if (result.IsNotFound) return NotFound();
        if (result.IsForbidden) return Forbid();
        if (result.IsConflict)
            return Conflict(new { error = "RESERVATION_NOT_CONFIRMED" });
        if (result.IsInvalid)
            return UnprocessableEntity(new { error = "SESSION_NOT_ENDED" });

        return NoContent();
    }
}
