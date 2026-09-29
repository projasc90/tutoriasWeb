namespace AuraLearn.Application.Dto;

/// <summary>Solicitud de registro de usuario.</summary>
public record RegisterRequest(string Email, string Password, string FullName);

/// <summary>Solicitud de inicio de sesión.</summary>
public record LoginRequest(string Email, string Password);

/// <summary>Respuesta de autenticación (nunca expone el hash).</summary>
public record AuthResponse(Guid Id, string Email, string FullName, string Role, string Token);
