import { MaterialIcon } from "@/components/material-icon";
import { SectionEyebrow, SectionTitle } from "./section-eyebrow";

type Mentor = {
  initials: string;
  name: string;
  credentials: string;
  rating: number;
  reviews: number;
  badges: string[];
  nextSlot: string;
  price: string;
  priceUsd: string;
  tone: "primary" | "success" | "tertiary" | "secondary";
  avatarBg: string;
};

const MENTORS: Mentor[] = [
  {
    initials: "CS",
    name: "Dr. Carlos Solano",
    credentials: "PhD Matemáticas (UCR)",
    rating: 4.98,
    reviews: 184,
    badges: ["Cálculo I, II, III", "Álgebra Lineal"],
    nextSlot: "Hoy, 3:30 PM",
    price: "₡14,500",
    priceUsd: "~$28",
    tone: "primary",
    avatarBg: "from-primary to-primary-container",
  },
  {
    initials: "SH",
    name: "Ing. Sofía Hernández",
    credentials: "M.Sc. Computación (TEC)",
    rating: 5.0,
    reviews: 92,
    badges: ["Python & Algoritmos", "Estructuras"],
    nextSlot: "Mañana, 10:00 AM",
    price: "₡16,000",
    priceUsd: "~$31",
    tone: "success",
    avatarBg: "from-tertiary-container to-tertiary",
  },
  {
    initials: "DM",
    name: "Prof. David Morales",
    credentials: "M.Sc. Física General (UCR)",
    rating: 4.9,
    reviews: 115,
    badges: ["Física I & II", "Termodinámica"],
    nextSlot: "Jueves, 5:00 PM",
    price: "₡12,000",
    priceUsd: "~$23",
    tone: "tertiary",
    avatarBg: "from-secondary-container to-secondary",
  },
  {
    initials: "MV",
    name: "Dra. Marcela Vargas",
    credentials: "PhD Química (UNA)",
    rating: 4.95,
    reviews: 140,
    badges: ["Química Orgánica", "Bioquímica"],
    nextSlot: "Viernes, 2:00 PM",
    price: "₡15,000",
    priceUsd: "~$29",
    tone: "secondary",
    avatarBg: "from-tertiary to-tertiary-container",
  },
];

export function Mentors() {
  return (
    <section className="w-full bg-surface py-20">
      <div className="mx-auto max-w-[1400px] px-6 lg:px-12">
        <div className="mb-12 flex flex-col items-start justify-between gap-4 sm:flex-row sm:items-end">
          <div>
            <SectionEyebrow>Docentes Destacados</SectionEyebrow>
            <SectionTitle className="mt-1">
              Conoce a algunos de nuestros mentores líderes
            </SectionTitle>
          </div>
          <span className="text-on-surface-variant text-label-md">
            450+ docentes activos en Centroamérica
          </span>
        </div>
        <div
          id="catalogo"
          className="grid grid-cols-1 gap-6 md:grid-cols-2 lg:grid-cols-4"
        >
          {MENTORS.map((m) => (
            <MentorCard key={m.name} m={m} />
          ))}
        </div>
      </div>
    </section>
  );
}

function MentorCard({ m }: { m: Mentor }) {
  return (
    <article className="flex flex-col justify-between rounded-xl bg-surface-container-lowest p-5 shadow-sm transition-all hover:shadow-primary-card">
      <div>
        <div className="mb-4 flex items-start gap-3.5">
          <div className="relative">
            <div
              className={`grid h-16 w-16 place-items-center rounded-xl bg-gradient-to-br ${m.avatarBg} text-on-primary font-bold text-lg`}
            >
              {m.initials}
            </div>
            <span className="absolute -bottom-1 -right-1 h-3.5 w-3.5 rounded-full border-2 border-surface-container-lowest bg-primary-container" />
          </div>
          <div className="min-w-0 flex-1">
            <h3 className="truncate text-title-md font-bold text-on-surface">
              {m.name}
            </h3>
            <p className="text-primary text-label-md font-semibold">
              {m.credentials}
            </p>
            <div className="mt-1 flex items-center gap-1">
              <MaterialIcon
                name="star"
                filled
                className="text-tertiary text-[15px]"
              />
              <span className="text-xs font-bold text-on-surface">
                {m.rating.toFixed(2)}
              </span>
              <span className="text-label-sm text-on-surface-variant">
                ({m.reviews} clases)
              </span>
            </div>
          </div>
        </div>
        <div className="mb-4 flex flex-wrap gap-1.5">
          {m.badges.map((badge) => (
            <span
              key={badge}
              className="rounded bg-surface-container-low px-2 py-0.5 text-label-sm text-on-surface-variant"
            >
              {badge}
            </span>
          ))}
        </div>
        <div className="mb-4 flex items-center justify-between rounded-lg bg-surface-container-low p-2.5">
          <span className="text-label-sm text-on-surface-variant">
            Próximo cupo libre:
          </span>
          <span className="text-primary text-xs font-bold">{m.nextSlot}</span>
        </div>
      </div>
      <div className="flex items-center justify-between pt-4">
        <div>
          <span className="text-headline-sm font-extrabold text-on-surface">
            {m.price}
          </span>
          <span className="block text-label-sm text-on-surface-variant">
            / hora ({m.priceUsd})
          </span>
        </div>
        <button
          type="button"
          className="rounded-lg bg-primary px-4 py-2 text-on-primary text-label-md font-semibold hover:bg-on-primary-fixed-variant transition-colors shadow-sm"
        >
          Ver Perfil
        </button>
      </div>
    </article>
  );
}
