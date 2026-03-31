import type { Metadata } from "next";
import { getTranslations } from "next-intl/server";
import Link from "next/link";
import {
  FileText,
  CalendarCheck,
  Bell,
  Share2,
  Search,
  UserPlus,
  Smartphone,
} from "lucide-react";
import { Button } from "@/components/ui/button";
import { ScrollReveal } from "@/components/features/landing/ScrollReveal";
import { Footer } from "@/components/features/landing/Footer";
import { NavLanguageSwitcher } from "@/components/features/landing/NavLanguageSwitcher";
import { MobileLandingNav } from "@/components/features/landing/MobileLandingNav";
import { ClinicSearchBar } from "@/components/features/pet-owners/ClinicSearchBar";
import { AskVetForm } from "@/components/features/pet-owners/AskVetForm";

interface Props {
  params: Promise<{ locale: string }>;
}

export async function generateMetadata({ params }: Props): Promise<Metadata> {
  const { locale } = await params;
  const t = await getTranslations({ locale, namespace: "pet_owners.meta" });

  return {
    title: t("title"),
    description: t("description"),
    openGraph: {
      title: t("title"),
      description: t("description"),
      type: "website",
    },
  };
}

const JSON_LD_SERVICE = {
  "@context": "https://schema.org",
  "@type": "WebApplication",
  name: "Vetara for Pet Owners",
  applicationCategory: "HealthApplication",
  operatingSystem: "Web, iOS, Android",
  description:
    "Access your pet's medical records, book vet appointments, get vaccination reminders, and share records with any clinic.",
  offers: {
    "@type": "Offer",
    price: "0",
    priceCurrency: "AED",
    description: "Free for pet owners",
  },
  featureList:
    "Digital medical records, Appointment booking, Vaccination reminders, Record sharing, Clinic search",
  availableOnDevice: "Desktop, Tablet, Mobile",
  countriesSupported: "AE",
};

const VALUE_PROPS = [
  { key: "medical_records", Icon: FileText },
  { key: "booking", Icon: CalendarCheck },
  { key: "vaccination_reminders", Icon: Bell },
  { key: "record_sharing", Icon: Share2 },
] as const;

const STEPS = [
  { key: "step1", Icon: Search },
  { key: "step2", Icon: UserPlus },
  { key: "step3", Icon: Smartphone },
] as const;

export default async function PetOwnersPage({ params }: Props) {
  const { locale } = await params;
  const t = await getTranslations({ locale, namespace: "pet_owners" });
  const tLanding = await getTranslations({ locale, namespace: "landing" });
  const loginHref = `/${locale}/login`;
  const clinicsLandingHref = `/${locale}`;

  return (
    <div className="min-h-screen bg-white scroll-smooth">
      {/* JSON-LD Structured Data */}
      <script
        type="application/ld+json"
        dangerouslySetInnerHTML={{ __html: JSON.stringify(JSON_LD_SERVICE) }}
      />

      {/* ── Nav ────────────────────────────────────────────────────── */}
      <header className="sticky top-0 z-50 border-b border-stone-100 bg-white/80 backdrop-blur-md transition-all duration-300">
        <nav
          data-testid="pet-owners-nav"
          className="mx-auto flex max-w-7xl items-center justify-between px-4 py-4 sm:px-6 lg:px-8"
        >
          <Link href={`/${locale}`} className="flex items-center gap-2">
            <span className="text-xl font-bold tracking-tight text-primary">
              Vetara
            </span>
          </Link>
          <div className="hidden items-center gap-6 md:flex">
            <a
              href="#features"
              className="text-sm font-medium text-stone-600 transition-all duration-200 hover:text-primary"
              data-testid="pet-owners-nav-features"
            >
              {t("nav.features")}
            </a>
            <a
              href="#how-it-works"
              className="text-sm font-medium text-stone-600 transition-all duration-200 hover:text-primary"
              data-testid="pet-owners-nav-how-it-works"
            >
              {t("nav.how_it_works")}
            </a>
            <Link
              href={clinicsLandingHref}
              className="text-sm font-medium text-stone-600 transition-all duration-200 hover:text-primary"
              data-testid="pet-owners-nav-for-clinics"
            >
              {t("nav.for_clinics")}
            </Link>
            <NavLanguageSwitcher locale={locale} />
            <Link href={loginHref} data-testid="pet-owners-nav-signin">
              <Button
                variant="outline"
                size="sm"
                className="transition-all duration-200 hover:scale-[1.02]"
                data-testid="btn-pet-owners-signin"
              >
                {t("nav.sign_in")}
              </Button>
            </Link>
          </div>
          {/* Mobile nav */}
          <div className="flex items-center gap-2 md:hidden">
            <NavLanguageSwitcher locale={locale} />
            <MobileLandingNav
              links={[
                {
                  label: t("nav.features"),
                  href: "#features",
                  testId: "pet-owners-nav-features",
                },
                {
                  label: t("nav.how_it_works"),
                  href: "#how-it-works",
                  testId: "pet-owners-nav-how-it-works",
                },
                {
                  label: t("nav.for_clinics"),
                  href: clinicsLandingHref,
                  testId: "pet-owners-nav-for-clinics",
                },
              ]}
              signInLabel={t("nav.sign_in")}
              ctaLabel={t("clinic_search.button")}
              loginHref={loginHref}
              signupHref="#clinic-search"
            />
          </div>
        </nav>
      </header>

      <main>
        {/* ── Hero ───────────────────────────────────────────────────── */}
        <section
          data-testid="pet-owners-hero"
          className="relative overflow-hidden bg-gradient-to-br from-secondary/50 via-white to-accent/50"
        >
          <div className="mx-auto max-w-7xl px-4 pb-16 pt-16 sm:px-6 sm:pb-24 sm:pt-20 lg:px-8 lg:pb-32 lg:pt-24">
            <div className="mx-auto max-w-3xl text-center">
              <ScrollReveal direction="fade-up">
                <h1
                  data-testid="pet-owners-hero-headline"
                  className="text-3xl font-extrabold leading-tight tracking-tight text-stone-900 sm:text-4xl lg:text-5xl"
                >
                  {t("hero.headline")}
                </h1>
              </ScrollReveal>
              <ScrollReveal direction="fade-up" delay={150}>
                <p
                  data-testid="pet-owners-hero-subtitle"
                  className="mt-6 text-lg leading-relaxed text-stone-600"
                >
                  {t("hero.subtitle")}
                </p>
              </ScrollReveal>
              <ScrollReveal direction="fade-up" delay={300}>
                <div className="mt-8 mx-auto max-w-xl">
                  <ClinicSearchBar
                    messages={{
                      placeholder: t("hero.search_placeholder"),
                      button: t("hero.search_button"),
                      no_results: t("clinic_search.no_results"),
                      loading: t("clinic_search.loading"),
                    }}
                  />
                </div>
              </ScrollReveal>
              <ScrollReveal direction="fade-up" delay={450}>
                <p
                  data-testid="pet-owners-trust-badge"
                  className="mt-6 text-sm text-stone-500"
                >
                  {t("hero.trust_badge")}
                </p>
              </ScrollReveal>
            </div>
          </div>
        </section>

        {/* ── Value Props ────────────────────────────────────────────── */}
        <section
          id="features"
          data-testid="pet-owners-value-props"
          className="bg-white py-20 sm:py-28"
        >
          <div className="mx-auto max-w-7xl px-4 sm:px-6 lg:px-8">
            <ScrollReveal direction="fade-up">
              <div className="mx-auto max-w-2xl text-center">
                <h2 className="text-2xl font-bold tracking-tight text-stone-900 sm:text-3xl lg:text-4xl">
                  {t("value_props.title")}
                </h2>
                <p className="mt-4 text-lg text-stone-600">
                  {t("value_props.subtitle")}
                </p>
              </div>
            </ScrollReveal>

            <div className="mt-16 grid gap-8 sm:grid-cols-2 lg:grid-cols-4">
              {VALUE_PROPS.map(({ key, Icon }, idx) => (
                <ScrollReveal key={key} delay={idx * 150} direction="fade-up">
                  <div
                    data-testid={`value-prop-${key}`}
                    className="flex flex-col items-center rounded-2xl border border-stone-100 bg-stone-50 p-6 text-center transition-all duration-300 hover:shadow-md hover:-translate-y-1"
                  >
                    <div className="flex h-14 w-14 items-center justify-center rounded-full bg-primary/10">
                      <Icon
                        className="h-7 w-7 text-primary"
                        aria-hidden="true"
                      />
                    </div>
                    <h3 className="mt-4 text-base font-semibold text-stone-900">
                      {t(`value_props.${key}.title`)}
                    </h3>
                    <p className="mt-2 text-sm leading-relaxed text-stone-600">
                      {t(`value_props.${key}.description`)}
                    </p>
                  </div>
                </ScrollReveal>
              ))}
            </div>
          </div>
        </section>

        {/* ── Clinic Search ──────────────────────────────────────────── */}
        <section
          id="clinic-search"
          data-testid="pet-owners-clinic-search"
          className="bg-gradient-to-br from-secondary/50 via-white to-accent/50 py-20 sm:py-28"
        >
          <div className="mx-auto max-w-2xl px-4 sm:px-6 lg:px-8">
            <ScrollReveal direction="fade-up">
              <div className="text-center">
                <h2 className="text-2xl font-bold tracking-tight text-stone-900 sm:text-3xl">
                  {t("clinic_search.title")}
                </h2>
                <p className="mt-4 text-lg text-stone-600">
                  {t("clinic_search.subtitle")}
                </p>
              </div>
            </ScrollReveal>
            <ScrollReveal direction="fade-up" delay={200}>
              <div className="mt-8">
                <ClinicSearchBar
                  messages={{
                    placeholder: t("clinic_search.placeholder"),
                    button: t("clinic_search.button"),
                    no_results: t("clinic_search.no_results"),
                    loading: t("clinic_search.loading"),
                  }}
                />
              </div>
            </ScrollReveal>
          </div>
        </section>

        {/* ── How It Works ───────────────────────────────────────────── */}
        <section
          id="how-it-works"
          data-testid="pet-owners-how-it-works"
          className="bg-white py-20 sm:py-28"
        >
          <div className="mx-auto max-w-7xl px-4 sm:px-6 lg:px-8">
            <ScrollReveal direction="fade-up">
              <div className="mx-auto max-w-2xl text-center">
                <h2 className="text-2xl font-bold tracking-tight text-stone-900 sm:text-3xl lg:text-4xl">
                  {t("how_it_works.title")}
                </h2>
                <p className="mt-4 text-lg text-stone-600">
                  {t("how_it_works.subtitle")}
                </p>
              </div>
            </ScrollReveal>

            <div className="relative mt-16">
              <div
                className="absolute left-0 right-0 top-10 hidden h-px bg-primary/20 lg:block"
                aria-hidden="true"
              />
              <ol className="relative grid gap-10 lg:grid-cols-3 lg:gap-8">
                {STEPS.map(({ key, Icon }, stepIndex) => (
                  <ScrollReveal
                    key={key}
                    delay={stepIndex * 200}
                    direction="fade-up"
                  >
                    <li
                      data-testid={`pet-owners-${key}`}
                      className="flex flex-col items-center text-center"
                    >
                      <div className="relative z-10 flex h-20 w-20 items-center justify-center rounded-full border-4 border-primary/15 bg-white shadow-md transition-all duration-300 hover:shadow-lg hover:scale-105">
                        <Icon
                          className="h-8 w-8 text-primary"
                          aria-hidden="true"
                        />
                        <span className="absolute -right-1 -top-1 flex h-6 w-6 items-center justify-center rounded-full bg-primary text-xs font-bold text-white">
                          {t(`how_it_works.${key}.number`)}
                        </span>
                      </div>
                      <h3 className="mt-6 text-base font-semibold text-stone-900">
                        {t(`how_it_works.${key}.title`)}
                      </h3>
                      <p className="mt-2 text-sm leading-relaxed text-stone-600">
                        {t(`how_it_works.${key}.description`)}
                      </p>
                    </li>
                  </ScrollReveal>
                ))}
              </ol>
            </div>
          </div>
        </section>

        {/* ── Ask Your Vet ───────────────────────────────────────────── */}
        <section
          id="ask-vet"
          data-testid="pet-owners-ask-vet"
          className="bg-gradient-to-br from-secondary/50 via-white to-accent/50 py-20 sm:py-28"
        >
          <div className="mx-auto max-w-2xl px-4 sm:px-6 lg:px-8">
            <ScrollReveal direction="fade-up">
              <div className="text-center">
                <h2 className="text-2xl font-bold tracking-tight text-stone-900 sm:text-3xl">
                  {t("ask_vet.title")}
                </h2>
                <p className="mt-4 text-lg text-stone-600">
                  {t("ask_vet.subtitle")}
                </p>
              </div>
            </ScrollReveal>
            <ScrollReveal direction="fade-up" delay={200}>
              <div className="mt-8 rounded-2xl border border-stone-100 bg-white p-6 shadow-sm sm:p-8">
                <AskVetForm
                  messages={{
                    vet_email_label: t("ask_vet.vet_email_label"),
                    vet_email_placeholder: t("ask_vet.vet_email_placeholder"),
                    your_name_label: t("ask_vet.your_name_label"),
                    your_name_placeholder: t("ask_vet.your_name_placeholder"),
                    pet_name_label: t("ask_vet.pet_name_label"),
                    pet_name_placeholder: t("ask_vet.pet_name_placeholder"),
                    message_label: t("ask_vet.message_label"),
                    message_placeholder: t("ask_vet.message_placeholder"),
                    submit: t("ask_vet.submit"),
                    submitting: t("ask_vet.submitting"),
                    success_title: t("ask_vet.success_title"),
                    success_description: t("ask_vet.success_description"),
                  }}
                />
              </div>
            </ScrollReveal>
          </div>
        </section>

        {/* ── Final CTA ──────────────────────────────────────────────── */}
        <section
          data-testid="pet-owners-final-cta"
          className="bg-primary py-20 sm:py-28"
        >
          <div className="mx-auto max-w-3xl px-4 text-center sm:px-6 lg:px-8">
            <ScrollReveal direction="fade-up">
              <h2 className="text-2xl font-extrabold tracking-tight text-white sm:text-3xl lg:text-4xl">
                {t("cta.headline")}
              </h2>
            </ScrollReveal>
            <ScrollReveal direction="fade-up" delay={150}>
              <p className="mt-6 text-lg leading-relaxed text-primary-foreground">
                {t("cta.subtitle")}
              </p>
            </ScrollReveal>
            <ScrollReveal direction="fade-up" delay={300}>
              <div className="mt-8">
                <a href="#clinic-search" data-testid="pet-owners-cta-button">
                  <Button
                    size="lg"
                    className="bg-white px-10 text-base font-semibold text-primary transition-all duration-300 hover:bg-accent hover:shadow-lg hover:shadow-white/25 hover:scale-[1.02]"
                    data-testid="btn-pet-owners-cta"
                  >
                    {t("cta.button")}
                  </Button>
                </a>
              </div>
              <p className="mt-4 text-sm text-primary-foreground/70">
                {t("cta.reassurance")}
              </p>
            </ScrollReveal>
          </div>
        </section>

        {/* ── Footer link to clinic landing ───────────────────────────── */}
        <section
          data-testid="pet-owners-for-clinics-banner"
          className="border-t border-stone-100 bg-stone-50 py-10"
        >
          <div className="mx-auto flex max-w-7xl flex-col items-center gap-4 px-4 sm:flex-row sm:justify-between sm:px-6 lg:px-8">
            <p className="text-base font-medium text-stone-700">
              {t("footer.for_clinics")}
            </p>
            <Link
              href={clinicsLandingHref}
              data-testid="pet-owners-for-clinics-link"
            >
              <Button
                variant="outline"
                className="transition-all duration-200 hover:scale-[1.02]"
                data-testid="btn-pet-owners-for-clinics"
              >
                {t("footer.for_clinics_cta")}
              </Button>
            </Link>
          </div>
        </section>
      </main>

      {/* ── Footer ─────────────────────────────────────────────────── */}
      <Footer
        locale={locale}
        signupHref={`/${locale}/signup`}
        messages={{
          copyright: tLanding("footer.copyright"),
          tagline: tLanding("footer.tagline"),
          lang_en: tLanding("footer.lang_en"),
          lang_ar: tLanding("footer.lang_ar"),
          contact_hello: "hello@vetara.ae",
          contact_support: "support@vetara.ae",
          columns: {
            product: {
              title: tLanding("footer.product.title"),
              features: tLanding("footer.product.features"),
              pricing: tLanding("footer.product.pricing"),
              integrations: tLanding("footer.product.integrations"),
              changelog: tLanding("footer.product.changelog"),
            },
            company: {
              title: tLanding("footer.company.title"),
              about: tLanding("footer.company.about"),
              contact: tLanding("footer.company.contact"),
              careers: tLanding("footer.company.careers"),
            },
            resources: {
              title: tLanding("footer.resources.title"),
              help_center: tLanding("footer.resources.help_center"),
              api_docs: tLanding("footer.resources.api_docs"),
              status: tLanding("footer.resources.status"),
            },
            legal: {
              title: tLanding("footer.legal.title"),
              privacy: tLanding("footer.legal.privacy"),
              terms: tLanding("footer.legal.terms"),
              dpa: tLanding("footer.legal.dpa"),
            },
          },
        }}
      />
    </div>
  );
}
