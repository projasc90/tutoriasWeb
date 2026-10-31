using AuraLearn.Application.Dto;
using AuraLearn.Application.Interfaces;
using AuraLearn.Application.Services;
using AuraLearn.Domain.Entities;
using AuraLearn.Infrastructure.Persistence;
using FluentValidation;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Text;

using System.Text.Json.Serialization;

var builder = WebApplication.CreateBuilder(args);

// ── Persistence (EF Core + PostgreSQL) ─────────────────────────────
builder.Services.AddDbContext<AuraLearnDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("Default")));

// ── Application (servicios + validadores) ──────────────────────────
builder.Services.AddScoped<ITutorRepository, TutorRepository>();
builder.Services.AddScoped<ISlotRepository, SlotRepository>();
builder.Services.AddScoped<IReservationRepository, ReservationRepository>();
builder.Services.AddScoped<IWalletRepository, WalletRepository>();
builder.Services.AddScoped<ITutorAvailabilityRepository, TutorAvailabilityRepository>();
builder.Services.AddScoped<ITutorAvailabilityExceptionRepository, TutorAvailabilityExceptionRepository>();
builder.Services.AddScoped<ITutorDocumentRepository, TutorDocumentRepository>();
builder.Services.AddScoped<IBookingRulesRepository, BookingRulesRepository>();

// ── Storage de archivos (documentos + fotos) — ADR-009 ─────────────
// En Vercel Functions el filesystem es efímero, así que producción usa
// Vercel Blob (store privado) o falla explícitamente si falta el token.
// Local/dev sigue en disco fuera del repo (Storage:DocumentsRoot).
builder.Services.AddSingleton<IFileStorage>(_ => CreateFileStorage(builder.Configuration));

// SlotGenerationService es estático; el resto de servicios van scoped
builder.Services.AddScoped<SinpeComprobanteValidator>();
builder.Services.AddScoped<ReservationService>();
builder.Services.AddScoped<SlotAvailabilityService>();
builder.Services.AddScoped<AvailabilityService>();
builder.Services.AddScoped<BookingRulesService>();

builder.Services.AddScoped<TutorService>();
builder.Services.AddScoped<TutorProfileService>();
builder.Services.AddScoped<TutorDocumentService>();
builder.Services.AddScoped<TutorPhotoService>();
builder.Services.AddScoped<AdminUserService>();
builder.Services.AddScoped<AdminUserSearchCriteriaValidator>();

builder.Services.AddValidatorsFromAssemblyContaining<TutorSearchCriteriaValidator>();

// ── Configuración SINPE (número receptor, sin secretos en prod) ──
builder.Services.AddSingleton(new SinpeConfig(
    ReceptorPhone: builder.Configuration["Sinpe:ReceptorPhone"] ?? "88888888"));

// ── Auth (JWT bearer; endpoints en AuthController) ──
var jwtKey = builder.Configuration["Jwt:Key"]
    ?? throw new InvalidOperationException("Jwt:Key no configurado");
var jwtIssuer = builder.Configuration["Jwt:Issuer"] ?? "AuraLearn.Api";

// ── Servicios de autenticación ──────────────────────────────────
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IPasswordHasher<User>, PasswordHasher<User>>();
builder.Services.AddScoped<RegisterRequestValidator>();
builder.Services.AddScoped<AuthService>();
builder.Services.AddScoped<TutorApplicationService>();
builder.Services.AddScoped<IJwtTokenService>(sp =>
    new JwtTokenService(jwtKey, jwtIssuer));

builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = jwtIssuer,
            ValidateAudience = true,
            ValidAudience = jwtIssuer,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey)),
        };
    });
builder.Services.AddAuthorization();

// ── CORS (frontend local + dominios de prod por variable de entorno) ──
// Cors__Origins = orígenes separados por ';' (p. ej. https://app.vercel.app;https://auralearn.com).
// En Development el default es el frontend local.
var corsOrigins = (builder.Configuration["Cors:Origins"]
        ?? (builder.Environment.IsDevelopment() ? "http://localhost:3000" : string.Empty))
    .Split(';', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
builder.Services.AddCors(options => options.AddDefaultPolicy(policy =>
{
    if (corsOrigins.Length > 0)
        policy.WithOrigins(corsOrigins);
    policy.AllowAnyHeader().AllowAnyMethod();
}));

// ── API surface ────────────────────────────────────────────────────
builder.Services.AddControllers()
    .AddJsonOptions(o =>
        o.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter()));
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddHealthChecks();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
app.MapHealthChecks("/health");

// ── Migraciones al desplegar (opt-in explícito) ────────────────────
// APPLY_MIGRATIONS=true en el arranque del contenedor aplica las migraciones
// pendientes y termina (modo one-shot para el deploy). En local se usa
// `dotnet ef database update` (ver .ai/PROTOCOLS.md); nunca automático silencioso.
if (builder.Configuration["ApplyMigrations"] == "true")
{
    using var scope = app.Services.CreateScope();
    var db = scope.ServiceProvider.GetRequiredService<AuraLearnDbContext>();
    db.Database.Migrate();
    Console.WriteLine("[auralearn] Migraciones aplicadas.");
    return;
}

// ── Puerto dinámico (Vercel/Railway/contenedores inyectan PORT) ────
// Fuente de verdad: entrypoint.sh exporta ASPNETCORE_URLS=http://0.0.0.0:$PORT.
// Este bloque es solo fallback para arrancar el binario sin ese script; si se
// ejecutan ambas fuentes Kestrel registra el binding dos veces.
// En local sigue funcionando con launchSettings (5037).
var aspnetUrls = Environment.GetEnvironmentVariable("ASPNETCORE_URLS");
var port = Environment.GetEnvironmentVariable("PORT");
if (string.IsNullOrWhiteSpace(aspnetUrls)
    && !string.IsNullOrWhiteSpace(port)
    && !app.Environment.IsDevelopment())
{
    app.Urls.Add($"http://0.0.0.0:{port}");
}

app.Run();

// ── Selección del storage según configuración ──────────────────────
// Prioridad: Storage:Provider explícito > BLOB_READ_WRITE_TOKEN presente.
// En producción (Vercel) faltar el token es un error de despliegue, no algo
// que deba degradar silenciosamente a disco (el filesystem no persiste).
static IFileStorage CreateFileStorage(IConfiguration configuration)
{
    var provider = configuration["Storage:Provider"];
    var blobToken = configuration["BLOB_READ_WRITE_TOKEN"]
        ?? configuration["Storage:VercelBlobToken"];

    var useBlob = provider?.Equals("VercelBlob", StringComparison.OrdinalIgnoreCase) == true
        || (provider is null && !string.IsNullOrWhiteSpace(blobToken));

    if (!useBlob)
    {
        return new LocalFileStorage(
            configuration["Storage:DocumentsRoot"]
            ?? Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.UserProfile),
                ".auralearn", "uploads", "documents"));
    }

    if (string.IsNullOrWhiteSpace(blobToken))
        throw new InvalidOperationException(
            "Storage:Provider=VercelBlob requiere BLOB_READ_WRITE_TOKEN");

    return new VercelBlobFileStorage(
        blobToken,
        configuration["Storage:VercelBlobAccess"] ?? "private");
}
