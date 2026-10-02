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

// SlotGenerationService es estático; el resto de servicios van scoped
builder.Services.AddScoped<SinpeComprobanteValidator>();
builder.Services.AddScoped<ReservationService>();
builder.Services.AddScoped<SlotAvailabilityService>();
builder.Services.AddScoped<AvailabilityService>();

builder.Services.AddScoped<TutorService>();

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

// ── CORS (frontend local) ──────────────────────────────────────────
builder.Services.AddCors(options => options.AddDefaultPolicy(policy =>
    policy.WithOrigins("http://localhost:3000").AllowAnyHeader().AllowAnyMethod()));

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

app.Run();
