using AuraLearn.Api.HealthChecks;
using AuraLearn.Application.Dto;
using AuraLearn.Application.Interfaces;
using AuraLearn.Application.Services;
using AuraLearn.Domain.Entities;
using AuraLearn.Infrastructure.Configuration;
using AuraLearn.Infrastructure.Persistence;
using FluentValidation;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.IdentityModel.Tokens;
using System.Text;

using System.Text.Json.Serialization;

var builder = WebApplication.CreateBuilder(args);

// Modo one-shot de migraciones (lo activa el job `migrate` del CI y el
// entrypoint con APPLY_MIGRATIONS=true). Se calcula antes de la conexión
// porque cambia qué variable se prefiere: las migraciones van por la URL
// directa (sin PgBouncer), el runtime por la pooled.
var isMigrationMode = builder.Configuration["ApplyMigrations"] == "true";

// ── Persistence (EF Core + PostgreSQL) ─────────────────────────────
// La cadena sale de la primera fuente disponible: ConnectionStrings__Default
// (override de dev/compose/CI) o las variables que inyecta la integración
// Neon ↔ Vercel (DATABASE_URL, POSTGRES_URL, PG*). Ver DatabaseConnectionString.
var database = DatabaseConnectionString.Resolve(
    builder.Configuration.GetConnectionString("Default"),
    preferUnpooled: isMigrationMode,
    read: name => builder.Configuration[name]);

// Solo el origen (nombre de la variable), nunca el valor.
Console.WriteLine($"[auralearn] Conexión a PostgreSQL desde {database.Source}");

builder.Services.AddDbContext<AuraLearnDbContext>(options =>
    options.UseNpgsql(database.Value));

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

// La factory de DI es perezosa: sin esto, una credencial de Blob inválida no
// aparecería hasta la primera subida (403 opacos en producción). Se valida el
// par BLOB_READ_WRITE_TOKEN + BLOB_STORE_ID aquí para fallar en el arranque.
if (UsesBlobStorage(builder.Configuration))
{
    var blobCredentials = VercelBlobConfiguration.Resolve(name => builder.Configuration[name]);
    Console.WriteLine($"[auralearn] Archivos en Vercel Blob (store {blobCredentials.StoreId})");
}

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

// Health checks divididos por intención:
//  /health       → readiness: proceso vivo + PostgreSQL accesible (lo que importa
//                  para decidir si la instancia sirve tráfico).
//  /health/live  → liveness: solo el proceso responde, sin tocar la BD.
// Así un monitor que solo vigila el proceso no despierta Neon en cada ping.
builder.Services.AddHealthChecks()
    .AddCheck<DatabaseHealthCheck>(
        "database",
        failureStatus: HealthStatus.Unhealthy,
        tags: ["ready"],
        timeout: TimeSpan.FromSeconds(5));

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
// Predicate vacío = no ejecuta ningún check registrado.
app.MapHealthChecks("/health/live", new HealthCheckOptions { Predicate = _ => false });

// ── Migraciones al desplegar (opt-in explícito) ────────────────────
// APPLY_MIGRATIONS=true en el arranque del contenedor aplica las migraciones
// pendientes y termina (modo one-shot para el deploy). En local se usa
// `dotnet ef database update` (ver .ai/PROTOCOLS.md); nunca automático silencioso.
if (isMigrationMode)
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
// Prioridad: Storage:Provider explícito > credenciales de Blob presentes
// (BLOB_READ_WRITE_TOKEN y/o BLOB_STORE_ID, los nombres que inyecta Vercel).
// En producción faltar el token es un error de despliegue, no algo que deba
// degradar silenciosamente a disco (el filesystem no persiste).
static bool UsesBlobStorage(IConfiguration configuration)
{
    var provider = configuration["Storage:Provider"];

    if (provider?.Equals("VercelBlob", StringComparison.OrdinalIgnoreCase) == true)
        return true;

    if (provider is not null)
        return false;

    return !string.IsNullOrWhiteSpace(configuration[VercelBlobConfiguration.TokenVariable])
        || !string.IsNullOrWhiteSpace(configuration["Storage:VercelBlobToken"])
        || !string.IsNullOrWhiteSpace(configuration[VercelBlobConfiguration.StoreIdVariable]);
}

static IFileStorage CreateFileStorage(IConfiguration configuration)
{
    if (!UsesBlobStorage(configuration))
    {
        return new LocalFileStorage(
            configuration["Storage:DocumentsRoot"]
            ?? Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.UserProfile),
                ".auralearn", "uploads", "documents"));
    }

    var credentials = VercelBlobConfiguration.Resolve(name => configuration[name]);

    return new VercelBlobFileStorage(credentials.Token, credentials.StoreId, credentials.Access);
}
