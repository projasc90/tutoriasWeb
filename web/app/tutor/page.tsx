import { redirect } from "next/navigation";

/** Índice del panel del tutor: redirige a la pantalla de perfil. */
export default function TutorIndexPage() {
  redirect("/tutor/perfil");
}
