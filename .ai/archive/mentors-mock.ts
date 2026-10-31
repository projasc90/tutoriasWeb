/**
 * Datos mock de mentores destacados para la landing (TRANSITORIO).
 *
 * Mismo criterio que web/lib/data/tutors.ts: desacopla la UI del origen de
 * datos y se reemplazará por la API real cuando exista el backend .NET.
 */

import type { Mentor } from "@/lib/types/mentor";

export const MENTORS: Mentor[] = [
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
