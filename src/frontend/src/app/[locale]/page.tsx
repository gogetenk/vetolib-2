import type { Metadata } from "next";
import { getTranslations } from "next-intl/server";
import Link from "next/link";
import Image from "next/image";
import {
  CalendarCheck,
  ClipboardList,
  FileText,
  Users,
  UserPlus,
  Building2,
  Quote,
  ShieldCheck,
} from "lucide-react";
import { Button } from "@/components/ui/button";
import { Card, CardContent } from "@/components/ui/card";
import { PricingSection } from "@/components/features/landing/PricingSection";
import { FaqSection } from "@/components/features/landing/FaqSection";
import { FeaturesSection } from "@/components/features/landing/FeaturesSection";
import { FinalCtaSection } from "@/components/features/landing/FinalCtaSection";
import { Footer } from "@/components/features/landing/Footer";

interface Props {
  params: Promise<{ locale: string }>;
}

export async function generateMetadata({ params }: Props): Promise<Metadata> {
  const { locale } = await params;
  const t = await getTranslations({ locale, namespace: "landing.meta" });

  return {
    title: t("title"),
    description: t("description"),
    openGraph: {
      title: t("title"),
      description: t("description"),
      type: "website",
      images: [
        {
          url: "/dashboard-placeholder.svg",
          width: 720,
          height: 460,
          alt: "Vetolib Dashboard",
        },
      ],
    },
  };
}

const FEATURE_ICONS = [
  CalendarCheck,
  ClipboardList,
  FileText,
  Users,
] as const;

const FEATURE_KEYS = [
  "scheduling",
  "medical_records",
  "invoicing",
  "team",
] as const;

const SOCIAL_PROOF_KEYS = [
  { stat: "clinics", label: "clinics_label" },
  { stat: "patients", label: "patients_label" },
  { stat: "invoiced", label: "invoiced_label" },
  { stat: "uptime", label: "uptime_label" },
] as const;

const JSON_LD = {
  "@context": "https://schema.org",
  "@type": "SoftwareApplication",
  name: "Vetolib",
  applicationCategory: "BusinessApplication",
  operatingSystem: "Web",
  offers: {
    "@type": "AggregateOffer",
    priceCurrency: "AED",
    lowPrice: "249",
    highPrice: "999",
  },
};

export default async function LandingPage({ params }: Props) {
  const { locale } = await params;
  const t = await getTranslations({ locale, namespace: "landing" });
  const loginHref = `/${locale}/login`;
  const signupHref = `/${locale}/signup`;

  const faqItems = Array.from({ length: 8 }, (_, i) => ({
    question: t(`faq.${i}.question`),
    answer: t(`faq.${i}.answer`),
  }));

  return (
    <div className="min-h-screen bg-white">
      {/* JSON-LD Structured Data */}
      <script
        type="application/ld+json"
        dangerouslySetInnerHTML={{ __html: JSON.stringify(JSON_LD) }}
      />

      {/* ── Nav ────────────────────────────────────────────────────── */}
      <header className="sticky top-0 z-50 border-b border-gray-100 bg-white/95 backdrop-blur-sm">
        <nav className="mx-auto flex max-w-7xl items-center justify-between px-4 py-4 sm:px-6 lg:px-8">
          <div className="flex items-center gap-2">
            <span className="text-xl font-bold tracking-tight text-emerald-700">
              Vetolib
            </span>
          </div>
          <div className="hidden items-center gap-6 md:flex">
            <a
              href="#features"
              className="text-sm font-medium text-gray-600 transition-colors hover:text-emerald-700"
            >
              {t("nav.features")}
            </a>
            <a
              href="#pricing"
              className="text-sm font-medium text-gray-600 transition-colors hover:text-emerald-700"
            >
              {t("nav.pricing")}
            </a>
            <a
              href="#faq"
              className="text-sm font-medium text-gray-600 transition-colors hover:text-emerald-700"
            >
              {t("nav.faq")}
            </a>
            <Link href={loginHref} data-testid="nav-signin-link">
              <Button variant="outline" size="sm" data-testid="btn-nav-signin">
                {t("nav.sign_in")}
              </Button>
            </Link>
            <Link href={signupHref} data-testid="nav-cta-start-trial">
              <Button
                size="sm"
                className="bg-emerald-700 text-white hover:bg-emerald-800"
              >
                {t("hero.cta_primary")}
              </Button>
            </Link>
          </div>
          {/* Mobile sign-in */}
          <div className="flex items-center gap-2 md:hidden">
            <Link href={signupHref} data-testid="nav-mobile-cta">
              <Button
                size="sm"
                className="bg-emerald-700 text-white hover:bg-emerald-800"
              >
                {t("hero.cta_primary")}
              </Button>
            </Link>
          </div>
        </nav>
      </header>

      <main>
        {/* ── Hero ───────────────────────────────────────────────────── */}
        <section
          data-testid="section-hero"
          className="relative overflow-hidden bg-gradient-to-br from-emerald-50 via-white to-teal-50"
        >
          <div className="mx-auto max-w-7xl px-4 pb-16 pt-16 sm:px-6 sm:pb-24 sm:pt-20 lg:px-8 lg:pb-32 lg:pt-24">
            <div className="grid items-center gap-12 lg:grid-cols-2">
              {/* Text side */}
              <div className="text-center lg:text-start">
                <h1 className="text-3xl font-extrabold leading-tight tracking-tight text-gray-900 sm:text-4xl lg:text-5xl">
                  {t("hero.headline")}
                </h1>
                <p className="mt-6 text-lg leading-relaxed text-gray-600">
                  {t("hero.subtitle")}
                </p>
                <div className="mt-8 flex flex-col items-center gap-3 sm:flex-row sm:justify-center lg:justify-start">
                  <Link href={signupHref} data-testid="hero-cta-start-trial">
                    <Button
                      size="lg"
                      className="w-full bg-emerald-700 px-8 text-base font-semibold text-white hover:bg-emerald-800 sm:w-auto"
                    >
                      {t("hero.cta_primary")}
                    </Button>
                  </Link>
                  <a
                    href="mailto:hello@vetolib.ae?subject=Demo%20Request"
                    data-testid="hero-cta-book-demo"
                  >
                    <Button
                      variant="outline"
                      size="lg"
                      className="w-full border-emerald-700 px-8 text-base font-semibold text-emerald-700 hover:bg-emerald-50 sm:w-auto"
                    >
                      {t("hero.cta_secondary")}
                    </Button>
                  </a>
                </div>
                <p
                  data-testid="hero-trust-badge"
                  className="mt-5 flex items-center justify-center gap-1.5 text-sm text-gray-500 lg:justify-start"
                >
                  <ShieldCheck
                    className="h-4 w-4 shrink-0 text-emerald-600"
                    aria-hidden="true"
                  />
                  {t("hero.trust_badge")}
                </p>
              </div>

              {/* Visual side */}
              <div className="flex justify-center lg:justify-end">
                <div className="relative w-full max-w-xl overflow-hidden rounded-2xl border border-gray-200 shadow-2xl">
                  <Image
                    src="/dashboard-placeholder.svg"
                    alt={t("hero.image_alt")}
                    width={720}
                    height={460}
                    className="h-auto w-full"
                    priority
                    unoptimized
                  />
                </div>
              </div>
            </div>
          </div>
        </section>

        {/* ── Social Proof Bar ───────────────────────────────────────── */}
        <section
          data-testid="section-social-proof"
          className="border-y border-gray-100 bg-gray-50"
        >
          <div className="mx-auto max-w-7xl px-4 py-10 sm:px-6 lg:px-8">
            <dl className="grid grid-cols-2 gap-6 sm:grid-cols-4">
              {SOCIAL_PROOF_KEYS.map(({ stat, label }) => (
                <div
                  key={stat}
                  className="flex flex-col items-center text-center"
                  data-testid={`social-proof-${stat}`}
                >
                  <dt className="text-2xl font-extrabold text-emerald-700 sm:text-3xl">
                    {t(`social_proof.${stat}`)}
                  </dt>
                  <dd className="mt-1 text-sm font-medium text-gray-500">
                    {t(`social_proof.${label}`)}
                  </dd>
                </div>
              ))}
            </dl>
          </div>
        </section>

        {/* ── Features Grid ──────────────────────────────────────────── */}
        <section
          id="features"
          data-testid="section-features"
          className="bg-white py-20 sm:py-28"
        >
          <div className="mx-auto max-w-7xl px-4 sm:px-6 lg:px-8">
            <div className="mx-auto max-w-2xl text-center">
              <h2 className="text-2xl font-bold tracking-tight text-gray-900 sm:text-3xl lg:text-4xl">
                {t("features.title")}
              </h2>
              <p className="mt-4 text-lg text-gray-600">
                {t("features.subtitle")}
              </p>
            </div>

            <div className="mt-16 grid gap-6 sm:grid-cols-2 lg:grid-cols-3">
              {FEATURE_KEYS.map((key, i) => {
                const Icon = FEATURE_ICONS[i];
                return (
                  <Card
                    key={key}
                    className="group border border-gray-100 shadow-sm transition-shadow hover:shadow-md"
                    data-testid={`feature-card-${key}`}
                  >
                    <CardContent className="p-6">
                      <div className="flex h-12 w-12 items-center justify-center rounded-xl bg-emerald-50 text-emerald-700 transition-colors group-hover:bg-emerald-100">
                        <Icon className="h-6 w-6" aria-hidden="true" />
                      </div>
                      <h3 className="mt-4 text-base font-semibold text-gray-900">
                        {t(`features.${key}.title`)}
                      </h3>
                      <p className="mt-2 text-sm leading-relaxed text-gray-600">
                        {t(`features.${key}.description`)}
                      </p>
                    </CardContent>
                  </Card>
                );
              })}
            </div>
          </div>
        </section>

        {/* ── New Features ───────────────────────────────────────────── */}
        <FeaturesSection
          title={t("new_features.title")}
          subtitle={t("new_features.subtitle")}
          cards={{
            health_passport: {
              title: t("new_features.health_passport.title"),
              description: t("new_features.health_passport.description"),
              badge: t("new_features.health_passport.badge"),
            },
            ai: {
              title: t("new_features.ai.title"),
              description: t("new_features.ai.description"),
              badge: t("new_features.ai.badge"),
            },
            messaging: {
              title: t("new_features.messaging.title"),
              description: t("new_features.messaging.description"),
            },
            stock: {
              title: t("new_features.stock.title"),
              description: t("new_features.stock.description"),
            },
            multilingual: {
              title: t("new_features.multilingual.title"),
              description: t("new_features.multilingual.description"),
            },
          }}
        />

        {/* ── How It Works ───────────────────────────────────────────── */}
        <section
          data-testid="section-how-it-works"
          className="bg-gradient-to-br from-emerald-50 via-white to-teal-50 py-20 sm:py-28"
        >
          <div className="mx-auto max-w-7xl px-4 sm:px-6 lg:px-8">
            <div className="mx-auto max-w-2xl text-center">
              <h2 className="text-2xl font-bold tracking-tight text-gray-900 sm:text-3xl lg:text-4xl">
                {t("how_it_works.title")}
              </h2>
              <p className="mt-4 text-lg text-gray-600">
                {t("how_it_works.subtitle")}
              </p>
            </div>

            {/* Steps */}
            <div className="relative mt-16">
              {/* Connector line — desktop only */}
              <div
                className="absolute left-0 right-0 top-10 hidden h-px bg-emerald-200 lg:block"
                aria-hidden="true"
              />

              <ol className="relative grid gap-10 lg:grid-cols-3 lg:gap-8">
                {(
                  [
                    {
                      key: "step1",
                      Icon: UserPlus,
                    },
                    {
                      key: "step2",
                      Icon: Users,
                    },
                    {
                      key: "step3",
                      Icon: Building2,
                    },
                  ] as const
                ).map(({ key, Icon }) => (
                  <li
                    key={key}
                    data-testid={`how-it-works-${key}`}
                    className="flex flex-col items-center text-center"
                  >
                    <div className="relative z-10 flex h-20 w-20 items-center justify-center rounded-full border-4 border-emerald-100 bg-white shadow-md">
                      <Icon
                        className="h-8 w-8 text-emerald-700"
                        aria-hidden="true"
                      />
                      <span className="absolute -right-1 -top-1 flex h-6 w-6 items-center justify-center rounded-full bg-emerald-700 text-xs font-bold text-white">
                        {t(`how_it_works.${key}.number`)}
                      </span>
                    </div>
                    <h3 className="mt-6 text-base font-semibold text-gray-900">
                      {t(`how_it_works.${key}.title`)}
                    </h3>
                    <p className="mt-2 text-sm leading-relaxed text-gray-600">
                      {t(`how_it_works.${key}.description`)}
                    </p>
                  </li>
                ))}
              </ol>
            </div>

            <div className="mt-14 text-center">
              <Link href={signupHref} data-testid="how-it-works-cta">
                <Button
                  size="lg"
                  className="bg-emerald-700 px-10 text-base font-semibold text-white hover:bg-emerald-800"
                >
                  {t("how_it_works.cta")}
                </Button>
              </Link>
            </div>
          </div>
        </section>

        {/* ── Pricing ────────────────────────────────────────────────── */}
        <PricingSection
          loginHref={signupHref}
          messages={{
            title: t("pricing.title"),
            subtitle: t("pricing.subtitle"),
            toggle_monthly: t("pricing.toggle_monthly"),
            toggle_annual: t("pricing.toggle_annual"),
            annual_savings: t("pricing.annual_savings"),
            trial_note: t("pricing.trial_note"),
            vat_note: t("pricing.vat_note"),
            starter: {
              name: t("pricing.starter.name"),
              price_monthly: t("pricing.starter.price_monthly"),
              price_annual: t("pricing.starter.price_annual"),
              per_month: t("pricing.starter.per_month"),
              description: t("pricing.starter.description"),
              features: [
                t("pricing.starter.features.0"),
                t("pricing.starter.features.1"),
                t("pricing.starter.features.2"),
                t("pricing.starter.features.3"),
                t("pricing.starter.features.4"),
                t("pricing.starter.features.5"),
                t("pricing.starter.features.6"),
              ],
              cta: t("pricing.starter.cta"),
            },
            pro: {
              name: t("pricing.pro.name"),
              badge: t("pricing.pro.badge"),
              price_monthly: t("pricing.pro.price_monthly"),
              price_annual: t("pricing.pro.price_annual"),
              per_month: t("pricing.pro.per_month"),
              description: t("pricing.pro.description"),
              features: [
                t("pricing.pro.features.0"),
                t("pricing.pro.features.1"),
                t("pricing.pro.features.2"),
                t("pricing.pro.features.3"),
                t("pricing.pro.features.4"),
                t("pricing.pro.features.5"),
                t("pricing.pro.features.6"),
                t("pricing.pro.features.7"),
                t("pricing.pro.features.8"),
              ],
              cta: t("pricing.pro.cta"),
            },
            enterprise: {
              name: t("pricing.enterprise.name"),
              from: t("pricing.enterprise.from"),
              price: t("pricing.enterprise.price"),
              per_month: t("pricing.enterprise.per_month"),
              description: t("pricing.enterprise.description"),
              features: [
                t("pricing.enterprise.features.0"),
                t("pricing.enterprise.features.1"),
                t("pricing.enterprise.features.2"),
                t("pricing.enterprise.features.3"),
                t("pricing.enterprise.features.4"),
                t("pricing.enterprise.features.5"),
                t("pricing.enterprise.features.6"),
                t("pricing.enterprise.features.7"),
                t("pricing.enterprise.features.8"),
                t("pricing.enterprise.features.9"),
                t("pricing.enterprise.features.10"),
              ],
              cta: t("pricing.enterprise.cta"),
            },
          }}
        />

        {/* ── Testimonials ───────────────────────────────────────────── */}
        <section
          data-testid="section-testimonials"
          className="bg-white py-20 sm:py-28"
        >
          <div className="mx-auto max-w-7xl px-4 sm:px-6 lg:px-8">
            <div className="mx-auto max-w-2xl text-center">
              <h2 className="text-2xl font-bold tracking-tight text-gray-900 sm:text-3xl lg:text-4xl">
                {t("testimonials.title")}
              </h2>
              <p className="mt-4 text-lg text-gray-600">
                {t("testimonials.subtitle")}
              </p>
            </div>

            {/* Cards — 3 columns desktop, vertical stack mobile */}
            <div className="mt-14 grid gap-8 sm:grid-cols-2 lg:grid-cols-3">
              {(["quote1", "quote2", "quote3"] as const).map((key) => (
                <figure
                  key={key}
                  data-testid={`testimonial-${key}`}
                  className="flex flex-col rounded-2xl border border-gray-100 bg-gray-50 p-6 shadow-sm"
                >
                  <Quote
                    className="h-8 w-8 text-emerald-200"
                    aria-hidden="true"
                  />
                  <blockquote className="mt-4 flex-1">
                    <p className="text-sm leading-relaxed text-gray-700">
                      &ldquo;{t(`testimonials.${key}.text`)}&rdquo;
                    </p>
                  </blockquote>
                  <figcaption className="mt-6 flex items-center gap-3 border-t border-gray-100 pt-4">
                    <div className="flex h-10 w-10 shrink-0 items-center justify-center rounded-full bg-emerald-100 text-sm font-bold text-emerald-700">
                      {t(`testimonials.${key}.name`)
                        .split(" ")
                        .slice(-1)[0]
                        .charAt(0)}
                    </div>
                    <div>
                      <p className="text-sm font-semibold text-gray-900">
                        {t(`testimonials.${key}.name`)}
                      </p>
                      <p className="text-xs text-gray-500">
                        {t(`testimonials.${key}.role`)} &bull;{" "}
                        {t(`testimonials.${key}.clinic`)}
                      </p>
                    </div>
                  </figcaption>
                </figure>
              ))}
            </div>

            {/* Mobile carousel hint — subtle scroll indicator */}
            <p className="mt-6 text-center text-xs text-gray-400 sm:hidden">
              ← Scroll to see more →
            </p>
          </div>
        </section>

        {/* ── FAQ ────────────────────────────────────────────────────── */}
        <FaqSection
          title={t("faq.title")}
          subtitle={t("faq.subtitle")}
          items={faqItems}
        />

        {/* ── Final CTA ──────────────────────────────────────────────── */}
        <FinalCtaSection
          loginHref={signupHref}
          headline={t("final_cta.headline")}
          subtitle={t("final_cta.subtitle")}
          cta={t("final_cta.cta")}
          reassurance={t("final_cta.reassurance")}
        />
      </main>

      {/* ── Footer ─────────────────────────────────────────────────── */}
      <Footer
        locale={locale}
        messages={{
          copyright: t("footer.copyright"),
          tagline: t("footer.tagline"),
          lang_en: t("footer.lang_en"),
          lang_ar: t("footer.lang_ar"),
          contact_hello: "hello@vetolib.ae",
          contact_support: "support@vetolib.ae",
          columns: {
            product: {
              title: t("footer.product.title"),
              features: t("footer.product.features"),
              pricing: t("footer.product.pricing"),
              integrations: t("footer.product.integrations"),
              changelog: t("footer.product.changelog"),
            },
            company: {
              title: t("footer.company.title"),
              about: t("footer.company.about"),
              contact: t("footer.company.contact"),
              careers: t("footer.company.careers"),
            },
            resources: {
              title: t("footer.resources.title"),
              help_center: t("footer.resources.help_center"),
              api_docs: t("footer.resources.api_docs"),
              status: t("footer.resources.status"),
            },
            legal: {
              title: t("footer.legal.title"),
              privacy: t("footer.legal.privacy"),
              terms: t("footer.legal.terms"),
              dpa: t("footer.legal.dpa"),
            },
          },
        }}
      />
    </div>
  );
}
