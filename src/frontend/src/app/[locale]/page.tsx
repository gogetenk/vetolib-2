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
  Star,
  ShieldCheck,
} from "lucide-react";
import { Button } from "@/components/ui/button";
import { Card, CardContent } from "@/components/ui/card";
import { PricingSection } from "@/components/features/landing/PricingSection";
import { FaqSection } from "@/components/features/landing/FaqSection";
import { FeaturesSection } from "@/components/features/landing/FeaturesSection";
import { FinalCtaSection } from "@/components/features/landing/FinalCtaSection";
import { Footer } from "@/components/features/landing/Footer";
import { NavLanguageSwitcher } from "@/components/features/landing/NavLanguageSwitcher";
import { MobileLandingNav } from "@/components/features/landing/MobileLandingNav";
import { ScrollReveal } from "@/components/features/landing/ScrollReveal";
import {
  HeroStagger,
  HeroDashboardReveal,
} from "@/components/features/landing/HeroAnimations";
import { AnimatedStat } from "@/components/features/landing/AnimatedStat";
import { TrustSignalsSection } from "@/components/features/landing/TrustSignalsSection";
import { DemoFormSection } from "@/components/features/landing/DemoFormSection";
import { CompetitiveTableSection } from "@/components/features/landing/CompetitiveTableSection";
import { StickyCtaBar } from "@/components/features/landing/StickyCtaBar";
import { ExitIntentPopup } from "@/components/features/landing/ExitIntentPopup";
import { LatestBlogSection } from "@/components/features/blog/LatestBlogSection";

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
  description:
    "UAE veterinary clinic management platform with AI triage, WhatsApp integration, Arabic + English support, and UAE-compliant invoicing.",
  applicationSubCategory: "Veterinary Practice Management",
  offers: {
    "@type": "AggregateOffer",
    priceCurrency: "AED",
    lowPrice: "0",
    highPrice: "549",
  },
  featureList:
    "Appointment scheduling, Medical records, VAT-compliant invoicing, AI triage, WhatsApp messaging, Arabic RTL support",
  availableOnDevice: "Desktop, Tablet, Mobile",
  countriesSupported: "AE",
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
    <div className="min-h-screen bg-white scroll-smooth">
      {/* JSON-LD Structured Data */}
      <script
        type="application/ld+json"
        dangerouslySetInnerHTML={{ __html: JSON.stringify(JSON_LD) }}
      />

      {/* ── Nav ────────────────────────────────────────────────────── */}
      <header className="sticky top-0 z-50 border-b border-stone-100 bg-white/80 backdrop-blur-md transition-all duration-300">
        <nav className="mx-auto flex max-w-7xl items-center justify-between px-4 py-4 sm:px-6 lg:px-8">
          <div className="flex items-center gap-2">
            <span className="text-xl font-bold tracking-tight text-emerald-700">
              Vetolib
            </span>
          </div>
          <div className="hidden items-center gap-6 md:flex">
            <a
              href="#features"
              className="text-sm font-medium text-stone-600 transition-all duration-200 hover:text-emerald-700 relative after:absolute after:bottom-0 after:left-0 after:h-0.5 after:w-0 after:bg-emerald-700 after:transition-all after:duration-300 hover:after:w-full"
              data-testid="nav-link-features"
            >
              {t("nav.features")}
            </a>
            <a
              href="#pricing"
              className="text-sm font-medium text-stone-600 transition-all duration-200 hover:text-emerald-700 relative after:absolute after:bottom-0 after:left-0 after:h-0.5 after:w-0 after:bg-emerald-700 after:transition-all after:duration-300 hover:after:w-full"
              data-testid="nav-link-pricing"
            >
              {t("nav.pricing")}
            </a>
            <a
              href="#demo"
              className="text-sm font-medium text-stone-600 transition-all duration-200 hover:text-emerald-700 relative after:absolute after:bottom-0 after:left-0 after:h-0.5 after:w-0 after:bg-emerald-700 after:transition-all after:duration-300 hover:after:w-full"
              data-testid="nav-link-demo"
            >
              {t("nav.demo")}
            </a>
            <a
              href="#faq"
              className="text-sm font-medium text-stone-600 transition-all duration-200 hover:text-emerald-700 relative after:absolute after:bottom-0 after:left-0 after:h-0.5 after:w-0 after:bg-emerald-700 after:transition-all after:duration-300 hover:after:w-full"
              data-testid="nav-link-faq"
            >
              {t("nav.faq")}
            </a>
            <Link
              href={`/${locale}/blog`}
              className="text-sm font-medium text-stone-600 transition-all duration-200 hover:text-emerald-700 relative after:absolute after:bottom-0 after:left-0 after:h-0.5 after:w-0 after:bg-emerald-700 after:transition-all after:duration-300 hover:after:w-full"
              data-testid="nav-link-blog"
            >
              Blog
            </Link>
            <NavLanguageSwitcher locale={locale} />
            <Link href={loginHref} data-testid="nav-signin-link">
              <Button variant="outline" size="sm" className="transition-all duration-200 hover:scale-[1.02]" data-testid="btn-nav-signin">
                {t("nav.sign_in")}
              </Button>
            </Link>
            <Link href={signupHref} data-testid="nav-cta-start-trial">
              <Button
                size="sm"
                className="bg-emerald-700 text-white transition-all duration-200 hover:bg-emerald-800 hover:shadow-md hover:shadow-emerald-700/20 hover:scale-[1.02]"
                data-testid="btn-nav-start-trial"
              >
                {t("hero.cta_primary")}
              </Button>
            </Link>
          </div>
          {/* Mobile nav */}
          <div className="flex items-center gap-2 md:hidden">
            <NavLanguageSwitcher locale={locale} />
            <MobileLandingNav
              links={[
                { label: t("nav.features"), href: "#features", testId: "nav-link-features" },
                { label: t("nav.pricing"), href: "#pricing", testId: "nav-link-pricing" },
                { label: t("nav.demo"), href: "#demo", testId: "nav-link-demo" },
                { label: t("nav.faq"), href: "#faq", testId: "nav-link-faq" },
                { label: "Blog", href: `/${locale}/blog`, testId: "nav-link-blog" },
              ]}
              signInLabel={t("nav.sign_in")}
              ctaLabel={t("hero.cta_primary")}
              loginHref={loginHref}
              signupHref={signupHref}
            />
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
                <HeroStagger index={0}>
                  <h1 className="text-3xl font-extrabold leading-tight tracking-tight text-stone-900 sm:text-4xl lg:text-5xl">
                    {t("hero.headline")}
                  </h1>
                </HeroStagger>
                <HeroStagger index={1}>
                  <p className="mt-6 text-lg leading-relaxed text-stone-600">
                    {t("hero.subtitle")}
                  </p>
                </HeroStagger>
                <HeroStagger index={2}>
                  <div className="mt-8 flex flex-col items-center gap-4 sm:flex-row sm:justify-center lg:justify-start">
                    <Link href={signupHref} data-testid="hero-cta-start-trial">
                      <Button size="lg" className="group/cta relative w-full overflow-hidden bg-emerald-700 px-10 py-3 text-base font-semibold text-white transition-all duration-300 hover:bg-emerald-800 hover:shadow-lg hover:shadow-emerald-700/25 hover:scale-[1.02] sm:w-auto" data-testid="btn-hero-start-trial">{t("hero.cta_primary")}</Button>
                    </Link>
                  </div>
                </HeroStagger>
                <HeroStagger index={3}>
                  <div className="mt-5 flex flex-col items-center gap-2 lg:items-start">
                    <p data-testid="hero-trust-badge" className="flex items-center gap-1.5 text-sm text-stone-500">
                      <ShieldCheck className="h-4 w-4 shrink-0 text-emerald-600" aria-hidden="true" />
                      {t("hero.trust_badge")}
                    </p>
                    <p data-testid="hero-social-proof-badge" className="inline-flex items-center gap-1.5 rounded-full bg-emerald-50 px-3 py-1 text-sm font-medium text-emerald-700">
                      <span className="relative flex h-2 w-2">
                        <span className="absolute inline-flex h-full w-full animate-ping rounded-full bg-emerald-400 opacity-75" />
                        <span className="relative inline-flex h-2 w-2 rounded-full bg-emerald-500" />
                      </span>
                      {t("hero.social_proof_badge")}
                    </p>
                  </div>
                </HeroStagger>
              </div>

              {/* Visual side */}
              <HeroDashboardReveal className="flex justify-center lg:justify-end">
                <div className="relative w-full max-w-xl overflow-hidden rounded-2xl border border-stone-200 shadow-2xl transition-shadow duration-500 hover:shadow-3xl">
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
              </HeroDashboardReveal>
            </div>
          </div>
        </section>

        {/* ── Social Proof Bar ───────────────────────────────────────── */}
        <section
          data-testid="section-social-proof"
          className="border-y border-stone-100 bg-stone-50"
        >
          <div className="mx-auto max-w-7xl px-4 py-10 sm:px-6 lg:px-8">
            <dl className="grid grid-cols-2 gap-6 sm:grid-cols-4">
              {SOCIAL_PROOF_KEYS.map(({ stat, label }, index) => (
                <ScrollReveal key={stat} delay={index * 100} direction="fade-up">
                  <div
                    className="flex flex-col items-center text-center"
                    data-testid={`social-proof-${stat}`}
                  >
                    <dt className="text-2xl font-extrabold text-emerald-700 sm:text-3xl">
                      <AnimatedStat
                        value={t(`social_proof.${stat}`)}
                        delay={index * 150}
                      />
                    </dt>
                    <dd className="mt-1 text-sm font-medium text-stone-500">
                      {t(`social_proof.${label}`)}
                    </dd>
                  </div>
                </ScrollReveal>
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
            <ScrollReveal direction="fade-up">
              <div className="mx-auto max-w-2xl text-center">
                <h2 className="text-2xl font-bold tracking-tight text-stone-900 sm:text-3xl lg:text-4xl">
                  {t("features.title")}
                </h2>
                <p className="mt-4 text-lg text-stone-600">
                  {t("features.subtitle")}
                </p>
              </div>
            </ScrollReveal>

            <div className="mt-16 grid gap-6 sm:grid-cols-2">
              {FEATURE_KEYS.map((key, i) => {
                const Icon = FEATURE_ICONS[i];
                return (
                  <ScrollReveal key={key} delay={i * 100} direction="fade-up">
                    <Card
                      className="group border border-stone-100 shadow-sm transition-all duration-300 hover:shadow-lg hover:-translate-y-1"
                      data-testid={`feature-card-${key}`}
                    >
                      <CardContent className="p-6">
                        <div className="flex h-12 w-12 items-center justify-center rounded-xl bg-emerald-50 text-emerald-700 transition-all duration-300 group-hover:bg-emerald-100 group-hover:scale-110">
                          <Icon className="h-6 w-6" aria-hidden="true" />
                        </div>
                        <h3 className="mt-4 text-base font-semibold text-stone-900">
                          {t(`features.${key}.title`)}
                        </h3>
                        <p className="mt-2 text-sm leading-relaxed text-stone-600">
                          {t(`features.${key}.description`)}
                        </p>
                      </CardContent>
                    </Card>
                  </ScrollReveal>
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
                ).map(({ key, Icon }, stepIndex) => (
                  <ScrollReveal key={key} delay={stepIndex * 200} direction="fade-up">
                    <li
                      data-testid={`how-it-works-${key}`}
                      className="flex flex-col items-center text-center"
                    >
                      <div className="relative z-10 flex h-20 w-20 items-center justify-center rounded-full border-4 border-emerald-100 bg-white shadow-md transition-all duration-300 hover:shadow-lg hover:scale-105">
                        <Icon
                          className="h-8 w-8 text-emerald-700"
                          aria-hidden="true"
                        />
                        <span className="absolute -right-1 -top-1 flex h-6 w-6 items-center justify-center rounded-full bg-emerald-700 text-xs font-bold text-white">
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

            <ScrollReveal direction="fade-up" delay={600}>
              <div className="mt-14 text-center">
                <Link href={signupHref} data-testid="how-it-works-cta">
                  <Button
                    size="lg"
                    className="bg-emerald-700 px-10 text-base font-semibold text-white transition-all duration-300 hover:bg-emerald-800 hover:shadow-lg hover:shadow-emerald-700/25 hover:scale-[1.02]"
                    data-testid="btn-how-it-works-start-trial"
                  >
                    {t("how_it_works.cta")}
                  </Button>
                </Link>
              </div>
            </ScrollReveal>
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
            early_access_badge: t("pricing_badge.early_access"),
            trial_under_plan: t("pricing_badge.trial_under_plan"),
            free: {
              name: t("pricing.free.name"),
              price_monthly: t("pricing.free.price_monthly"),
              price_annual: t("pricing.free.price_annual"),
              per_month: t("pricing.free.per_month"),
              description: t("pricing.free.description"),
              features: [
                t("pricing.free.features.0"),
                t("pricing.free.features.1"),
                t("pricing.free.features.2"),
                t("pricing.free.features.3"),
              ],
              cta: t("pricing.free.cta"),
            },
            starter: {
              name: t("pricing.starter.name"),
              price_monthly: t("pricing.starter.price_monthly"),
              price_annual: t("pricing.starter.price_annual"),
              usd_hint_monthly: t("pricing.starter.usd_hint_monthly"),
              usd_hint_annual: t("pricing.starter.usd_hint_annual"),
              per_month: t("pricing.starter.per_month"),
              description: t("pricing.starter.description"),
              features: [
                t("pricing.starter.features.0"),
                t("pricing.starter.features.1"),
                t("pricing.starter.features.2"),
                t("pricing.starter.features.3"),
                t("pricing.starter.features.4"),
                t("pricing.starter.features.5"),
              ],
              cta: t("pricing.starter.cta"),
            },
            pro: {
              name: t("pricing.pro.name"),
              badge: t("pricing.pro.badge"),
              price_monthly: t("pricing.pro.price_monthly"),
              price_annual: t("pricing.pro.price_annual"),
              usd_hint_monthly: t("pricing.pro.usd_hint_monthly"),
              usd_hint_annual: t("pricing.pro.usd_hint_annual"),
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
              ],
              cta: t("pricing.pro.cta"),
            },
            enterprise: {
              name: t("pricing.enterprise.name"),
              price_monthly: t("pricing.enterprise.price_monthly"),
              price_annual: t("pricing.enterprise.price_annual"),
              usd_hint_monthly: t("pricing.enterprise.usd_hint_monthly"),
              usd_hint_annual: t("pricing.enterprise.usd_hint_annual"),
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
              ],
              cta: t("pricing.enterprise.cta"),
            },
          }}
        />

        {/* ── Competitive Table ─────────────────────────────────────── */}
        <CompetitiveTableSection
          messages={{
            title: t("competitive.title"),
            subtitle: t("competitive.subtitle"),
            columns: {
              feature: t("competitive.columns.feature"),
              vetolib: t("competitive.columns.vetolib"),
              ezyvet: t("competitive.columns.ezyvet"),
              digitail: t("competitive.columns.digitail"),
            },
            rows: [
              {
                feature: t("competitive.rows.arabic.feature"),
                vetolib: "yes",
                ezyvet: "no",
                digitail: "no",
              },
              {
                feature: t("competitive.rows.whatsapp.feature"),
                vetolib: "yes",
                ezyvet: "no",
                digitail: "partial",
              },
              {
                feature: t("competitive.rows.ai_triage.feature"),
                vetolib: "yes",
                ezyvet: "no",
                digitail: "partial",
              },
              {
                feature: t("competitive.rows.uae_optimized.feature"),
                vetolib: "yes",
                ezyvet: "partial",
                digitail: "no",
              },
              {
                feature: t("competitive.rows.price.feature"),
                vetolib: "yes",
                ezyvet: "no",
                digitail: "partial",
              },
            ],
          }}
        />

        {/* ── Trust Signals ────────────────────────────────────────── */}
        <TrustSignalsSection
          messages={{
            title: t("trust_signals.title"),
            badges: {
              arabic_english: t("trust_signals.badges.arabic_english"),
              whatsapp: t("trust_signals.badges.whatsapp"),
              uae_hosting: t("trust_signals.badges.uae_hosting"),
              moccae: t("trust_signals.badges.moccae"),
            },
          }}
        />

        {/* ── Testimonials ───────────────────────────────────────────── */}
        <section
          data-testid="section-testimonials"
          className="bg-white py-20 sm:py-28"
        >
          <div className="mx-auto max-w-7xl px-4 sm:px-6 lg:px-8">
            <ScrollReveal direction="fade-up">
              <div className="mx-auto max-w-2xl text-center">
                <h2 className="text-2xl font-bold tracking-tight text-stone-900 sm:text-3xl lg:text-4xl">
                  {t("testimonials.title")}
                </h2>
                <p className="mt-4 text-lg text-stone-600">
                  {t("testimonials.subtitle")}
                </p>
              </div>
            </ScrollReveal>

            <div className="mt-14 grid gap-8 sm:grid-cols-2 lg:grid-cols-3">
              {(["quote1", "quote2", "quote3"] as const).map((key, idx) => {
                const rating = parseInt(t(`testimonials.${key}.rating`), 10) || 5;
                return (
                  <ScrollReveal key={key} delay={idx * 150} direction="fade-up">
                    <figure data-testid={`testimonial-${key}`} className="flex flex-col rounded-2xl border border-stone-100 bg-stone-50 p-6 shadow-sm transition-all duration-300 hover:shadow-md hover:-translate-y-1">
                      <div className="flex gap-0.5" aria-label={`${rating} out of 5 stars`}>
                        {Array.from({ length: 5 }).map((_, i) => (
                          <Star key={i} className={`h-4 w-4 ${i < rating ? "fill-amber-400 text-amber-400" : "fill-stone-200 text-stone-200"}`} aria-hidden="true" />
                        ))}
                      </div>
                      <blockquote className="mt-4 flex-1">
                        <p className="text-sm leading-relaxed text-stone-700">&ldquo;{t(`testimonials.${key}.text`)}&rdquo;</p>
                      </blockquote>
                      <figcaption className="mt-6 flex items-center gap-3 border-t border-stone-100 pt-4">
                        <Image src={`/avatars/testimonial-${idx + 1}.svg`} alt={t(`testimonials.${key}.name`)} width={40} height={40} className="h-10 w-10 shrink-0 rounded-full object-cover" unoptimized />
                        <div>
                          <p className="text-sm font-semibold text-stone-900">{t(`testimonials.${key}.name`)}</p>
                          <p className="text-xs text-stone-500">{t(`testimonials.${key}.role`)} &bull;{" "}{t(`testimonials.${key}.clinic`)}</p>
                          <p className="text-xs text-stone-400">{t(`testimonials.${key}.emirate`)}</p>
                        </div>
                      </figcaption>
                    </figure>
                  </ScrollReveal>
                );
              })}
            </div>

            {/* Mobile carousel hint — subtle scroll indicator */}
            <p className="mt-6 text-center text-xs text-stone-400 sm:hidden">
              {t("testimonials.scroll_hint")}
            </p>
          </div>
        </section>

        {/* ── Latest from the Blog ────────────────────────────────── */}
        <LatestBlogSection locale={locale} />

        {/* ── Demo Form ────────────────────────────────────────────── */}
        <DemoFormSection
          messages={{
            title: t("demo_form.title"),
            subtitle: t("demo_form.subtitle"),
            clinic_name: t("demo_form.clinic_name"),
            clinic_name_placeholder: t("demo_form.clinic_name_placeholder"),
            email: t("demo_form.email"),
            email_placeholder: t("demo_form.email_placeholder"),
            phone: t("demo_form.phone"),
            phone_placeholder: t("demo_form.phone_placeholder"),
            preferred_time: t("demo_form.preferred_time"),
            preferred_time_placeholder: t("demo_form.preferred_time_placeholder"),
            submit: t("demo_form.submit"),
            success_title: t("demo_form.success_title"),
            success_description: t("demo_form.success_description"),
          }}
        />

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
        signupHref={signupHref}
        footerCta={{
          headline: t("footer_cta.headline"),
          cta: t("footer_cta.cta"),
        }}
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
