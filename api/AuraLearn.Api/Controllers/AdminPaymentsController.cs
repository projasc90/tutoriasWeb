using AuraLearn.Application.Dto;
using AuraLearn.Application.Interfaces;
using AuraLearn.Application.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AuraLearn.Api.Controllers;

/// <summary>
/// Cola de comprobantes pendientes de revisión (confirmación manual SINPE).
/// </summary>
[ApiController]
[Route("api/admin/payments")]
[Authorize(Roles = "Admin")]
public class AdminPaymentsController(
    ReservationService service,
    IReservationRepository reservations) : ControllerBase
{
    /// <summary>Lista todos los comprobantes pendientes de revisión.</summary>
    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<AdminPaymentDto>>> GetPending(CancellationToken ct)
    {
        var pending = await reservations.GetPendingComprobantesAsync(ct);
        var dtos = pending.Select(r => new AdminPaymentDto(
            r.Id,
            r.StudentId,
            r.Student.FullName,
            r.Slot.TutorId,
            r.Slot.Tutor.Name,
            r.Slot.StartAt,
            r.PriceCrc,
            r.ConfirmationNumber!,
            r.ComprobanteAmountCrc,
            r.ComprobantePhone,
            r.ComprobanteSubmittedAt!.Value)).ToList();

        return Ok(dtos);
    }

    /// <summary>El Admin aprueba o rechaza un comprobante.</summary>
    [HttpPatch("{reservationId:guid}")]
    public async Task<ActionResult> Decide(
        Guid reservationId,
        [FromBody] DecidePaymentRequest request,
        CancellationToken ct)
    {
        if (request.Decision is not ("approve" or "reject"))
            return BadRequest();

        if (request.Decision == "reject" && string.IsNullOrWhiteSpace(request.Reason))
            return BadRequest(new { error = "REJECTION_REQUIRES_REASON" });

        var result = await service.DecideAsync(reservationId, request.Decision, request.Reason, ct);

        if (result.IsNotFound) return NotFound();
        if (result.IsConflict)
            return Conflict(new { error = "RESERVATION_NOT_PENDING" });
        if (result.IsInvalid)
            return UnprocessableEntity(new { error = "INVALID_REQUEST" });

        return NoContent();
    }
}
