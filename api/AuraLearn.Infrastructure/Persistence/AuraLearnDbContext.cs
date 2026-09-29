using AuraLearn.Domain.Entities;
using AuraLearn.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace AuraLearn.Infrastructure.Persistence;

/// <summary>
/// DbContext de AuraLearn. Configuración de entidades con nombres snake_case
/// (convención PostgreSQL) e índices para los filtros del catálogo.
/// </summary>
public class AuraLearnDbContext(DbContextOptions<AuraLearnDbContext> options) : DbContext(options)
{
    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        => optionsBuilder.ConfigureWarnings(w =>
            w.Ignore(RelationalEventId.PendingModelChangesWarning));
    public DbSet<Tutor> Tutors => Set<Tutor>();
    public DbSet<User> Users => Set<User>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<User>(entity =>
        {
            entity.ToTable("users");
            entity.HasKey(u => u.Id);

            entity.Property(u => u.Id).HasColumnName("id");
            entity.Property(u => u.Email).HasColumnName("email").HasMaxLength(320).IsRequired();
            entity.Property(u => u.PasswordHash).HasColumnName("password_hash").HasMaxLength(500).IsRequired();
            entity.Property(u => u.FullName).HasColumnName("full_name").HasMaxLength(200).IsRequired();
            entity.Property(u => u.Role).HasColumnName("role");
            entity.Property(u => u.CreatedAt).HasColumnName("created_at");

            // Invariante: email único por usuario (índice unique)
            entity.HasIndex(u => u.Email).IsUnique().HasDatabaseName("ux_users_email");
        });

        modelBuilder.Entity<Tutor>(entity =>
        {
            entity.ToTable("tutors");
            entity.HasKey(t => t.Id);

            entity.Property(t => t.Id).HasColumnName("id");
            entity.Property(t => t.UserId).HasColumnName("user_id");
            entity.Property(t => t.Name).HasColumnName("name").HasMaxLength(200).IsRequired();
            entity.Property(t => t.Credentials).HasColumnName("credentials").HasMaxLength(200).IsRequired();
            entity.Property(t => t.University).HasColumnName("university").HasMaxLength(100).IsRequired();
            entity.Property(t => t.Rating).HasColumnName("rating").HasPrecision(3, 2);
            entity.Property(t => t.Reviews).HasColumnName("reviews");
            entity.Property(t => t.Subjects).HasColumnName("subjects");
            entity.Property(t => t.PriceCrc).HasColumnName("price_crc");
            entity.Property(t => t.PriceUsd).HasColumnName("price_usd");
            entity.Property(t => t.Bio).HasColumnName("bio").HasMaxLength(1000).IsRequired();
            entity.Property(t => t.Featured).HasColumnName("featured");
            entity.Property(t => t.VerificationStatus).HasColumnName("verification_status");
            entity.Property(t => t.RejectionReason).HasColumnName("rejection_reason").HasMaxLength(500);
            entity.Property(t => t.CreatedAt).HasColumnName("created_at");

            // Vínculo con el usuario postulante (1 tutor por usuario; nullable para el seed demo)
            entity.HasOne<User>()
                .WithMany()
                .HasForeignKey(t => t.UserId)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("fk_tutors_users_user_id");
            entity.HasIndex(t => t.UserId).IsUnique().HasDatabaseName("ix_tutors_user_id");

            // Índices para los filtros reales del catálogo (universidad + rating)
            entity.HasIndex(t => new { t.University, t.Rating }).HasDatabaseName("ix_tutors_university_rating");
            entity.HasIndex(t => t.VerificationStatus).HasDatabaseName("ix_tutors_verification_status");

            // Seed de datos del directorio (los 12 tutores del mock transitorio de web/)
            entity.HasData(
                new Tutor { Id = Guid.Parse("3f2504e0-4f89-11d3-9a0c-0305e82c3301"), Name = "Dr. Carlos Solano", Credentials = "PhD Matemáticas", University = "UCR", Rating = 4.98m, Reviews = 184, Subjects = ["Cálculo I", "Cálculo II", "Álgebra Lineal", "EDOs"], PriceCrc = 14500, PriceUsd = 28, Bio = "8 años de experiencia en tutoría universitaria. Metodología orientada a resultados.", Featured = true, VerificationStatus = VerificationStatus.Verified, CreatedAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc) },
                new Tutor { Id = Guid.Parse("3f2504e0-4f89-11d3-9a0c-0305e82c3302"), Name = "Ing. Sofía Hernández", Credentials = "M.Sc. Computación", University = "TEC", Rating = 5.00m, Reviews = 92, Subjects = ["Python", "Algoritmos", "Estructuras de Datos", "IA"], PriceCrc = 16000, PriceUsd = 31, Bio = "Especialista en preparación técnica para entrevistas en big tech.", Featured = true, VerificationStatus = VerificationStatus.Verified, CreatedAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc) },
                new Tutor { Id = Guid.Parse("3f2504e0-4f89-11d3-9a0c-0305e82c3303"), Name = "Prof. David Morales", Credentials = "M.Sc. Física", University = "UCR", Rating = 4.90m, Reviews = 115, Subjects = ["Física I", "Física II", "Termodinámica", "Mecánica"], PriceCrc = 12000, PriceUsd = 23, Bio = "Profesor agregado UCR. Enfoque práctico con más de 500 estudiantes ayudados.", Featured = false, VerificationStatus = VerificationStatus.Verified, CreatedAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc) },
                new Tutor { Id = Guid.Parse("3f2504e0-4f89-11d3-9a0c-0305e82c3304"), Name = "Dra. Marcela Vargas", Credentials = "PhD Química", University = "UNA", Rating = 4.95m, Reviews = 140, Subjects = ["Química Orgánica", "Bioquímica", "Farmacología"], PriceCrc = 15000, PriceUsd = 29, Bio = "Investigadora postdoctoral. Especialista en química orgánica para carreras de salud.", Featured = false, VerificationStatus = VerificationStatus.Verified, CreatedAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc) },
                new Tutor { Id = Guid.Parse("3f2504e0-4f89-11d3-9a0c-0305e82c3305"), Name = "M.Sc. Javier Rodríguez", Credentials = "M.Sc. Economía", University = "TEC", Rating = 4.85m, Reviews = 78, Subjects = ["Econometría", "Microeconomía", "Macroeconomía", "Finanzas"], PriceCrc = 13500, PriceUsd = 26, Bio = "Economista senior con experiencia en organismos internacionales.", Featured = false, VerificationStatus = VerificationStatus.Verified, CreatedAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc) },
                new Tutor { Id = Guid.Parse("3f2504e0-4f89-11d3-9a0c-0305e82c3306"), Name = "Dra. Laura Prop", Credentials = "PhD Biología", University = "UCR", Rating = 4.92m, Reviews = 103, Subjects = ["Biología Celular", "Genética", "Microbiología"], PriceCrc = 14000, PriceUsd = 27, Bio = "Docente-investigadora con énfasis en biología molecular y genética.", Featured = false, VerificationStatus = VerificationStatus.Verified, CreatedAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc) },
                new Tutor { Id = Guid.Parse("3f2504e0-4f89-11d3-9a0c-0305e82c3307"), Name = "Ing. Andrés González", Credentials = "M.Sc. Ing. Eléctrica", University = "TEC", Rating = 4.78m, Reviews = 56, Subjects = ["Circuitos", "Electrónica", "Señales", "Control"], PriceCrc = 13000, PriceUsd = 25, Bio = "Ingeniero electricista con Maestría en TEC. 6 años de experiencia.", Featured = false, VerificationStatus = VerificationStatus.Verified, CreatedAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc) },
                new Tutor { Id = Guid.Parse("3f2504e0-4f89-11d3-9a0c-0305e82c3308"), Name = "Dra. Carolina Morales", Credentials = "PhD Estadística", University = "UCR", Rating = 4.88m, Reviews = 67, Subjects = ["Estadística", "Probabilidad", "R", "Análisis de Datos"], PriceCrc = 15500, PriceUsd = 30, Bio = "Profesora jubilada UCR. 20 años en estadística aplicada.", Featured = false, VerificationStatus = VerificationStatus.Verified, CreatedAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc) },
                new Tutor { Id = Guid.Parse("3f2504e0-4f89-11d3-9a0c-0305e82c3309"), Name = "M.Sc. Ricardo Fernández", Credentials = "M.Sc. Matemáticas", University = "UNA", Rating = 4.97m, Reviews = 201, Subjects = ["Cálculo III", "Variable Compleja", "Topología"], PriceCrc = 12500, PriceUsd = 24, Bio = "Matemático puro. Doctorado en vías. Dominio completo del cálculo avanzado.", Featured = true, VerificationStatus = VerificationStatus.Verified, CreatedAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc) },
                new Tutor { Id = Guid.Parse("3f2504e0-4f89-11d3-9a0c-0305e82c3310"), Name = "M.Sc. María Pérez", Credentials = "M.Sc. Lingüística", University = "UCR", Rating = 4.93m, Reviews = 88, Subjects = ["Inglés Académico", "TOEFL", "Redacción", "Español"], PriceCrc = 11000, PriceUsd = 21, Bio = "Preparadora certificada TOEFL. Metodología inmersiva con materiales auténticos.", Featured = false, VerificationStatus = VerificationStatus.Verified, CreatedAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc) },
                new Tutor { Id = Guid.Parse("3f2504e0-4f89-11d3-9a0c-0305e82c3311"), Name = "Dr. Jorge Torres", Credentials = "PhD Física Médica", University = "TEC", Rating = 4.81m, Reviews = 44, Subjects = ["Física Médica", "Radiología", "Protección Radiológica"], PriceCrc = 17000, PriceUsd = 33, Bio = "Físico médico hospitalario. Prepara para exámenes de boards profesionales.", Featured = false, VerificationStatus = VerificationStatus.Verified, CreatedAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc) },
                new Tutor { Id = Guid.Parse("3f2504e0-4f89-11d3-9a0c-0305e82c3312"), Name = "Ing. Ana Salas", Credentials = "Ing. Civil", University = "TEC", Rating = 4.76m, Reviews = 39, Subjects = ["Estática", "Resistencia", "Hormigón", "Diseño Estructural"], PriceCrc = 14000, PriceUsd = 27, Bio = "Ingeniera civil con maestría en estructuras. Experiencia en proyecto y supervisión.", Featured = false, VerificationStatus = VerificationStatus.Verified, CreatedAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc) }
            );
        });
    }
}
