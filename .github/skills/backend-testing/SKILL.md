# Skill — Testing Backend (.NET)

## Cuándo usarla
Al escribir tests para `api/` (cuando exista el proyecto .NET) o al definir planes de prueba backend.

## Patrones recomendados

### Setup de proyecto (al crearlo)
```
api/tests/
  AuraLearn.Domain.Tests/          → xUnit + FluentAssertions
  AuraLearn.Application.Tests/     → servicios + validators (moq/NSubstitute para puertos)
  AuraLearn.IntegrationTests/      → WebApplicationFactory + Testcontainers (PostgreSQL)
```

### Test unitario de servicio (xUnit)
```csharp
public class SlotReservationServiceTests
{
    [Fact]
    public async Task Reserve_WhenSlotAlreadyTaken_ReturnsConflict()
    {
        // Arrange
        var repo = Substitute.For<ISlotRepository>();
        repo.IsAvailableAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>())
            .Returns(false);
        var sut = new SlotReservationService(repo);

        // Act
        var result = await sut.ReserveAsync(new ReserveSlotCommand(Guid.NewGuid()), CancellationToken.None);

        // Assert
        result.IsConflict.Should().BeTrue();
        await repo.DidNotReceive().SaveAsync(Arg.Any<Reservation>(), Arg.Any<CancellationToken>());
    }
}
```

### Convención de nombres
`MetodoBajoPrueba_Escenario_ResultadoEsperado` (inglés o español consistente; el repo documenta en español).

### Test de integración
- Testcontainers con PostgreSQL real para flujos críticos (pagos SINPE, slots).
- Fixture que corre migraciones y hace rollback de datos entre tests (o transacciones que se revierten).

## Anti-patrones
- Tests que dependen de orden de ejecución o estado compartido.
- Mock de `DbContext` directamente (mockear repositorios/puertos).
- `Thread.Sleep` para esperar async; usar `TaskCompletionSource`/`await` real.
- Suite monolítica sin filtros: prefijar colecciones `[Collection("db")]` para compartir contenedor.
- Escribir el test después de implementar en flujos obligatorios test-first (auth, SINPE, slots — ver `PROTOCOLS.md`).

## Checklist final
- [ ] Cubre unitario + integración según la capa tocada
- [ ] Nombres `Metodo_Escenario_Resultado`
- [ ] Fixtures con datos aislados por test
- [ ] Bug corregido → caso de regresión añadido
- [ ] `cd api && dotnet build && dotnet test` verde
