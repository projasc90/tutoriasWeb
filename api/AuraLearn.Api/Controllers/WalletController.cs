using AuraLearn.Application.Dto;
using AuraLearn.Application.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace AuraLearn.Api.Controllers;

/// <summary>
/// Endpoint del monedero del estudiante.
/// </summary>
[ApiController]
[Route("api/wallet")]
[Authorize]
public class WalletController : ControllerBase
{
    private readonly ReservationService _service;

    public WalletController(ReservationService service)
    {
        _service = service;
    }

    /// <summary>Saldo actual del monedero en colones.</summary>
    [HttpGet]
    public async Task<ActionResult<WalletBalanceDto>> GetBalance(CancellationToken ct)
    {
        var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)
                              ?? User.FindFirstValue("sub")!);
        var balance = await _service.GetWalletBalanceAsync(userId, ct);
        return Ok(new WalletBalanceDto(balance));
    }
}
