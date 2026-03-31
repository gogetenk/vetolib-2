import type { Metadata } from "next";
import { getTranslations } from "next-intl/server";
import Link from "next/link";
import { PricingPageClient } from "@/components/features/pricing/PricingPageClient";

interface Props {
  params: Promise<{ locale: string }>;
}

export async function generateMetadata({ params }: Props): Promise<Metadata> {
  const { locale } = await params;
  const t = await getTranslations({ locale, namespace: "pricing_page" });

  return {
    title: t("meta_title"),
    description: t("meta_description"),
    alternates: {
      canonical: `/${locale}/pricing`,
      languages: {
        en: "/en/pricing",
        ar: "/ar/pricing",
        fr: "/fr/pricing",
      },
    },
  };
}

export default async function PricingPage({ params }: Props) {
  const { locale } = await params;
  const t = await getTranslations({ locale, namespace: "pricing_page" });
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
        dangerouslySetInnerHTML={{
          __html: JSON.stringify({
            "@context": "https://schema.org",
            "@type": "WebPage",
            name: t("meta_title"),
            description: t("meta_description"),
            url: `https://vetara.com/${locale}/pricing`,
          }),
        }}
      />

      {/* Header */}
      <header className="sticky top-0 z-50 border-b border-stone-100 bg-white/80 backdrop-blur-md">
        <nav className="mx-auto flex max-w-7xl items-center justify-between px-4 py-4 sm:px-6 lg:px-8">
          <Link
            href={`/${locale}`}
            className="text-xl font-bold tracking-tight text-primary"
            data-testid="pricing-nav-logo"
          >
            Vetara
          </Link>
          <div className="flex items-center gap-4">
            <Link
              href={`/${locale}`}
              className="text-sm font-medium text-stone-600 transition-colors hover:text-primary"
              data-testid="pricing-nav-home"
            >
              {t("nav_home")}
            </Link>
            <span
              className="text-sm font-medium text-primary"
              data-testid="pricing-nav-current"
            >
              {t("nav_pricing")}
            </span>
          </div>
        </nav>
      </header>

      <main>
        <PricingPageClient
          locale={locale}
          signupHref={signupHref}
          messages={{
            heading: t("heading"),
            subtitle: t("subtitle"),
            toggle_monthly: t("toggle_monthly"),
            toggle_annual: t("toggle_annual"),
            annual_savings: t("annual_savings"),
            trial_note: t("trial_note"),
            vat_note: t("vat_note"),
            early_access_badge: t("early_access_badge"),
            trial_under_plan: t("trial_under_plan"),
            starter: {
              name: t("starter.name"),
              price_monthly: t("starter.price_monthly"),
              price_annual: t("starter.price_annual"),
              usd_hint_monthly: t("starter.usd_hint_monthly"),
              usd_hint_annual: t("starter.usd_hint_annual"),
              per_month: t("starter.per_month"),
              description: t("starter.description"),
              features: Array.from({ length: 8 }, (_, i) =>
                t(`starter.features.${i}`),
              ),
              cta: t("starter.cta"),
            },
            pro: {
              name: t("pro.name"),
              badge: t("pro.badge"),
              price_monthly: t("pro.price_monthly"),
              price_annual: t("pro.price_annual"),
              usd_hint_monthly: t("pro.usd_hint_monthly"),
              usd_hint_annual: t("pro.usd_hint_annual"),
              per_month: t("pro.per_month"),
              description: t("pro.description"),
              features: Array.from({ length: 10 }, (_, i) =>
                t(`pro.features.${i}`),
              ),
              cta: t("pro.cta"),
            },
            enterprise: {
              name: t("enterprise.name"),
              price_monthly: t("enterprise.price_monthly"),
              price_annual: t("enterprise.price_annual"),
              usd_hint_monthly: t("enterprise.usd_hint_monthly"),
              usd_hint_annual: t("enterprise.usd_hint_annual"),
              per_month: t("enterprise.per_month"),
              description: t("enterprise.description"),
              features: Array.from({ length: 9 }, (_, i) =>
                t(`enterprise.features.${i}`),
              ),
              cta: t("enterprise.cta"),
            },
            comparison: {
              title: t("comparison.title"),
              subtitle: t("comparison.subtitle"),
              feature: t("comparison.feature"),
              starter_col: t("comparison.starter_col"),
              pro_col: t("comparison.pro_col"),
              enterprise_col: t("comparison.enterprise_col"),
              scheduling: t("comparison.scheduling"),
              medical_records: t("comparison.medical_records"),
              email_reminders: t("comparison.email_reminders"),
              client_portal: t("comparison.client_portal"),
              bilingual_interface: t("comparison.bilingual_interface"),
              soap_notes: t("comparison.soap_notes"),
              whatsapp_reminders: t("comparison.whatsapp_reminders"),
              ai_health_alerts: t("comparison.ai_health_alerts"),
              ai_triage: t("comparison.ai_triage"),
              ai_soap_scribe: t("comparison.ai_soap_scribe"),
              stock_management: t("comparison.stock_management"),
              breeding_module: t("comparison.breeding_module"),
              analytics_dashboard: t("comparison.analytics_dashboard"),
              recurring_appointments: t("comparison.recurring_appointments"),
              qr_checkin: t("comparison.qr_checkin"),
              multi_clinic: t("comparison.multi_clinic"),
              api_access: t("comparison.api_access"),
              custom_roles: t("comparison.custom_roles"),
              priority_support: t("comparison.priority_support"),
              dedicated_account_manager: t(
                "comparison.dedicated_account_manager",
              ),
              custom_integrations: t("comparison.custom_integrations"),
              patients: t("comparison.patients"),
              patients_starter: t("comparison.patients_starter"),
              patients_pro: t("comparison.patients_pro"),
              patients_enterprise: t("comparison.patients_enterprise"),
              vets: t("comparison.vets"),
              vets_starter: t("comparison.vets_starter"),
              vets_pro: t("comparison.vets_pro"),
              vets_enterprise: t("comparison.vets_enterprise"),
            },
            faq: {
              title: t("faq.title"),
              subtitle: t("faq.subtitle"),
              items: faqItems,
            },
            final_cta: {
              headline: t("final_cta.headline"),
              subtitle: t("final_cta.subtitle"),
              cta: t("final_cta.cta"),
              reassurance: t("final_cta.reassurance"),
            },
          }}
        />
      </main>
    </div>
  );
}
