# Template — Test base

## Frontend (Vitest + Testing Library, al configurar la primera suite en web/)

```tsx
// web/__tests__/tutores/filter.test.tsx
import { describe, expect, it } from "vitest";
import { render, screen } from "@testing-library/react";
import userEvent from "@testing-library/user-event";
// import { TutoresPage } from "@/app/tutores/page"; // extract logic as needed for pure tests

// Para lógica pura (filtrado/orden) preferir tests unitarios sin render:
import { applyFilters } from "@/lib/filters"; // extraer lógica a lib/ para testear

const TUTORS = [
  { id: 1, name: "Dr. Carlos Solano", university: "UCR", rating: 4.98, price: 14500, subjects: ["Cálculo I"] },
  { id: 2, name: "Ing. Sofía Hernández", university: "TEC", rating: 5.0, price: 16000, subjects: ["Python"] },
];

describe("applyFilters", () => {
  it("filtra por universidad UCR", () => {
    const result = applyFilters(TUTORS, { university: "UCR" });
    expect(result.map((t) => t.id)).toEqual([1]);
  });

  it("ordena por precio ascendente", () => {
    const result = applyFilters(TUTORS, { sort: "price_asc" });
    expect(result[0].price).toBeLessThan(result[1].price);
  });
});
```

Convención de nombres: `describe("<unidad>")` + `it("<comportamiento esperado>")` en español o inglés consistente.

## Backend (xUnit + FluentAssertions, al crear api/)

```csharp
// api/tests/AuraLearn.Application.Tests/SlotReservationServiceTests.cs
using FluentAssertions;
using NSubstitute;
using Xunit;

namespace AuraLearn.Application.Tests;

public class SlotReservationServiceTests
{
    [Fact]
    public async Task Reserve_WhenSlotAlreadyTaken_ReturnsConflict()
    {
        // Arrange
        var repo = Substitute.For<ISlotRepository>();
        repo.IsAvailableAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>()).Returns(false);
        var sut = new SlotReservationService(repo);

        // Act
        var result = await sut.ReserveAsync(
            new ReserveSlotCommand(Guid.NewGuid()), CancellationToken.None);

        // Assert
        result.IsConflict.Should().BeTrue();
        await repo.DidNotReceive().SaveAsync(Arg.Any<Reservation>(), Arg.Any<CancellationToken>());
    }
}
```

Convención: `MetodoBajoPrueba_Escenario_ResultadoEsperado`.

## Reglas
- Datos aislados por test (sin orden dependiente).
- Testear comportamiento, no implementación.
- Bug corregido → caso de regresión en el plan correspondiente.
- Gate: `cd web && npm run lint && npx tsc --noEmit` · `cd api && dotnet build && dotnet test`.
