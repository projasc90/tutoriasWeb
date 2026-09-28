# Skill — Tipado Frontend estricto (TypeScript 5)

## Cuándo usarla
Al definir tipos de dominio, props de componentes, respuestas de API o refactorizar tipos en `web/`.

## Patrones recomendados

### Tipos de dominio co-localizados (patrón real del repo)
```tsx
// En el módulo que los usa (ej. app/tutores/page.tsx); al compartirse → web/lib/types/
type Tutor = {
  id: number;
  initials: string;
  name: string;
  credentials: string;
  university: string;
  rating: number;
  reviews: number;
  subjects: string[];
  price: number;      // CRC
  priceUsd: number;   // USD
  nextSlot: string;
  featured: boolean;
  bio: string;
  avatarTone: string;
};
```

### Estado de filtros con default tipado
```tsx
type FilterState = {
  query: string;
  university: string;
  level: string;
  availability: string;
  minRating: string;
  priceMin: number;
  priceMax: number;
};
const DEFAULT_FILTERS: FilterState = { /* ... */ };
```

### Uniones cerradas para opciones finitas
```tsx
type SortOption = "rating" | "price_asc" | "price_desc" | "reviews";
const [sort, setSort] = useState<SortOption>("rating");
// en el select: value={sort} onChange={(e) => setSort(e.target.value as SortOption)}
```

### Props explícitas (no `any`, no JSX implícito)
```tsx
function TutorCard({ tutor }: { tutor: Tutor }) { /* ... */ }
```

### Respuestas de API (cuando exista backend)
```tsx
// Contrato espejo del backend; discriminar errores con uniones
type ApiResult<T> = { ok: true; data: T } | { ok: false; error: string };
```

## Anti-patrones
- `any` o `as any` "para que compile".
- Tipos duplicados entre módulos (extraer a `web/lib/types/` cuando se dupliquen).
- `useState<string>` para opciones finitas en vez de uniones cerradas.
- Omitir campos opcionales con `!` (non-null assertion) para satisfacer el compilador.
- Tipos de la API hardcodeados a mano sin concordancia con el backend (documentar el contrato al integrarlo).

## Checklist final
- [ ] `strict: true` respetado (es la config del repo)
- [ ] Uniones cerradas para datos finitos (universidades, niveles, orden)
- [ ] Sin `any`; sin casts injustificados (`as` documentado si existe)
- [ ] Tipos compartidos en `web/lib/types/` (no duplicados)
- [ ] `npx tsc --noEmit` verde
