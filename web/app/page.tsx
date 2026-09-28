import { SiteHeader } from "@/components/landing/currency-toggle";
import { Hero } from "@/components/landing/hero";
import { TrustStrip } from "@/components/landing/trust-strip";
import { Rigor } from "@/components/landing/rigor";
import { Disciplines } from "@/components/landing/disciplines";
import { Mentors } from "@/components/landing/mentors";
import { Steps } from "@/components/landing/steps";
import { Testimonials } from "@/components/landing/testimonials";
import { TutorCta } from "@/components/landing/tutor-cta";
import { Faq } from "@/components/landing/faq";
import { FinalCta } from "@/components/landing/final-cta";
import { SiteFooter } from "@/components/landing/footer";

export default function Home() {
  return (
    <>
      <SiteHeader />
      <main className="flex flex-1 flex-col">
        <Hero />
        <TrustStrip />
        <Rigor />
        <Disciplines />
        <Mentors />
        <Steps />
        <Testimonials />
        <TutorCta />
        <Faq />
        <FinalCta />
      </main>
      <SiteFooter />
    </>
  );
}
