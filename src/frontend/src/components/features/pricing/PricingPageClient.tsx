"use client";

import { useState } from "react";
import Link from "next/link";
import { Check, ChevronDown, Minus } from "lucide-react";
import { Button } from "@/components/ui/button";
import { Badge } from "@/components/ui/badge";
import { Card, CardContent, CardHeader } from "@/components/ui/card";
import {
  type FeatureKey,
  FEATURE_PLAN_MAP,
  type SubscriptionPlan,
} from "@/lib/feature-flags";

/* ─── Types ──────────────────────────────────────────────────────────── */

interface PlanMessages {
  name: string;
  price_monthly: string;
  price_annual: string;
  usd_hint_monthly?: string;
  usd_hint_annual?: string;
  per_month: string;
  description: string;
  features: string[];
  cta: string;
  badge?: string;
}

interface ComparisonMessages {
  title: string;
  subtitle: string;
  feature: string;
  starter_col: string;
  pro_col: string;
  enterprise_col: string;
  [key: string]: string;
}

interface FaqMessages {
  title: string;
  subtitle: string;
  items: Array<{ question: string; answer: string }>;
}

interface FinalCtaMessages {
  headline: string;
  subtitle: string;
  cta: string;
  reassurance: string;
}

interface PricingPageMessages {
  heading: string;
  subtitle: string;
  toggle_monthly: string;
  toggle_annual: string;
  annual_savings: string;
  trial_note: string;
  vat_note: string;
  early_access_badge?: string;
  trial_under_plan?: string;
  starter: PlanMessages;
  pro: PlanMessages;
  enterprise: PlanMessages;
  comparison: ComparisonMessages;
  faq: FaqMessages;
  final_cta: FinalCtaMessages;
}

interface Props {
  locale: string;
  signupHref: string;
  messages: PricingPageMessages;
}

/* ─── Feature comparison data derived from FEATURE_PLAN_MAP ──────────── */

const PLAN_RANK: Record<SubscriptionPlan, number> = {
  starter: 0,
  professional: 1,
  enterprise: 2,
};

function hasAccess(plan: SubscriptionPlan, feature: FeatureKey): boolean {
  return PLAN_RANK[plan] >= PLAN_RANK[FEATURE_PLAN_MAP[feature]];
}

const FEATURE_ROWS: Array<{ key: FeatureKey; labelKey: string }> = [
  { key: "scheduling", labelKey: "scheduling" },
  { key: "medical_records", labelKey: "medical_records" },
  { key: "email_reminders", labelKey: "email_reminders" },
  { key: "client_portal", labelKey: "client_portal" },
  { key: "bilingual_interface", labelKey: "bilingual_interface" },
  { key: "soap_notes", labelKey: "soap_notes" },
  { key: "whatsapp_reminders", labelKey: "whatsapp_reminders" },
  { key: "ai_health_alerts", labelKey: "ai_health_alerts" },
  { key: "ai_triage", labelKey: "ai_triage" },
  { key: "ai_soap_scribe", labelKey: "ai_soap_scribe" },
  { key: "stock_management", labelKey: "stock_management" },
  { key: "breeding_module", labelKey: "breeding_module" },
  { key: "analytics_dashboard", labelKey: "analytics_dashboard" },
  { key: "recurring_appointments", labelKey: "recurring_appointments" },
  { key: "qr_checkin", labelKey: "qr_checkin" },
  { key: "multi_clinic", labelKey: "multi_clinic" },
  { key: "api_access", labelKey: "api_access" },
  { key: "custom_roles", labelKey: "custom_roles" },
  { key: "priority_support", labelKey: "priority_support" },
  { key: "dedicated_account_manager", labelKey: "dedicated_account_manager" },
  { key: "custom_integrations", labelKey: "custom_integrations" },
];

const LIMIT_ROWS: Array<{
  labelKey: string;
  starterKey: string;
  proKey: string;
  enterpriseKey: string;
}> = [
  {
    labelKey: "patients",
    starterKey: "patients_starter",
    proKey: "patients_pro",
    enterpriseKey: "patients_enterprise",
  },
  {
    labelKey: "vets",
    starterKey: "vets_starter",
    proKey: "vets_pro",
    enterpriseKey: "vets_enterprise",
  },
];

/* ─── Component ──────────────────────────────────────────────────────── */

export function PricingPageClient({ signupHref, messages: m }: Props) {
  const [annual, setAnnual] = useState(false);
  const [faqOpen, setFaqOpen] = useState<number | null>(null);

  const plans: Array<{
    key: string;
    plan: PlanMessages;
    highlighted: boolean;
    ctaVariant: "outline" | "default" | "ghost";
    isEnterprise: boolean;
  }> = [
    {
      key: "starter",
      plan: m.starter,
      highlighted: false,
      ctaVariant: "outline",
      isEnterprise: false,
    },
    {
      key: "pro",
      plan: m.pro,
      highlighted: true,
      ctaVariant: "default",
      isEnterprise: false,
    },
    {
      key: "enterprise",
      plan: m.enterprise,
      highlighted: false,
      ctaVariant: "outline",
      isEnterprise: true,
    },
  ];

  function toggleFaq(index: number) {
    setFaqOpen(faqOpen === index ? null : index);
  }

  return (
    <>
      {/* ── Hero / Plans Section ─────────────────────────────────── */}
      <section
        data-testid="pricing-hero"
        className="bg-gradient-to-br from-secondary/50 via-white to-accent/50 pb-20 pt-16 sm:pb-28 sm:pt-20"
      >
        <div className="mx-auto max-w-7xl px-4 sm:px-6 lg:px-8">
          {/* Header */}
          <div className="mx-auto max-w-2xl text-center">
            {m.early_access_badge && (
              <div
                className="mb-4 inline-flex items-center gap-2 rounded-full bg-amber-50 px-4 py-1.5 text-sm font-semibold text-amber-700 ring-1 ring-amber-200"
                data-testid="pricing-early-access-badge"
              >
                <span className="relative flex h-2 w-2">
                  <span className="absolute inline-flex h-full w-full animate-ping rounded-full bg-amber-400 opacity-75" />
                  <span className="relative inline-flex h-2 w-2 rounded-full bg-amber-500" />
                </span>
                {m.early_access_badge}
              </div>
            )}
            <h1
              className="text-3xl font-bold tracking-tight text-stone-900 sm:text-4xl lg:text-5xl"
              data-testid="pricing-heading"
            >
              {m.heading}
            </h1>
            <p className="mt-4 text-lg text-stone-600">{m.subtitle}</p>

            {/* Billing toggle */}
            <div
              className="mt-8 inline-flex items-center gap-3 rounded-full border border-stone-200 bg-white p-1 shadow-sm"
              data-testid="pricing-billing-toggle"
            >
              <button
                type="button"
                data-testid="pricing-toggle-monthly"
                onClick={() => setAnnual(false)}
                aria-pressed={!annual}
                className={`rounded-full px-5 py-2 text-sm font-medium transition-colors ${
                  !annual
                    ? "bg-primary text-white shadow"
                    : "text-stone-600 hover:text-stone-900"
                }`}
              >
                {m.toggle_monthly}
              </button>
              <button
                type="button"
                data-testid="pricing-toggle-annual"
                onClick={() => setAnnual(true)}
                aria-pressed={annual}
                className={`flex items-center gap-2 rounded-full px-5 py-2 text-sm font-medium transition-colors ${
                  annual
                    ? "bg-primary text-white shadow"
                    : "text-stone-600 hover:text-stone-900"
                }`}
              >
                {m.toggle_annual}
                {!annual && (
                  <span className="rounded-full bg-secondary px-2 py-0.5 text-xs font-semibold text-primary">
                    {m.annual_savings}
                  </span>
                )}
              </button>
            </div>
          </div>

          {/* Plan cards */}
          <div className="mt-12 grid gap-6 sm:grid-cols-2 lg:grid-cols-3">
            {plans.map(
              ({ key, plan, highlighted, ctaVariant, isEnterprise }) => (
                <Card
                  key={key}
                  data-testid={`pricing-plan-${key}`}
                  className={`flex flex-col transition-all duration-300 hover:shadow-lg hover:-translate-y-1 ${
                    highlighted
                      ? "relative border-2 border-primary shadow-lg"
                      : "border border-stone-200 shadow-sm"
                  }`}
                >
                  {plan.badge && (
                    <div className="absolute -top-3.5 left-1/2 -translate-x-1/2">
                      <Badge className="bg-primary px-4 py-1 text-xs font-semibold text-white">
                        {plan.badge}
                      </Badge>
                    </div>
                  )}
                  <CardHeader
                    className={`p-6 pb-0 ${plan.badge ? "pt-8" : ""}`}
                  >
                    <p
                      className={`text-sm font-semibold uppercase tracking-wide ${
                        highlighted ? "text-primary" : "text-stone-500"
                      }`}
                    >
                      {plan.name}
                    </p>
                    <div className="mt-3 flex items-end gap-1">
                      <span
                        className="text-3xl font-extrabold text-stone-900 lg:text-4xl"
                        data-testid={`pricing-price-${key}`}
                      >
                        {annual ? plan.price_annual : plan.price_monthly}
                      </span>
                      <span className="mb-1 text-sm text-stone-500">
                        {plan.per_month}
                      </span>
                    </div>
                    {(annual
                      ? plan.usd_hint_annual
                      : plan.usd_hint_monthly) && (
                      <p className="mt-1 text-xs text-stone-400">
                        {annual ? plan.usd_hint_annual : plan.usd_hint_monthly}
                      </p>
                    )}
                    <p className="mt-2 text-sm text-stone-600">
                      {plan.description}
                    </p>
                  </CardHeader>
                  <CardContent className="flex flex-1 flex-col p-6">
                    <ul className="flex-1 space-y-3">
                      {plan.features.map((feature, i) => (
                        <li key={i} className="flex items-start gap-3">
                          <Check
                            className="mt-0.5 h-4 w-4 shrink-0 text-primary/85"
                            aria-hidden="true"
                          />
                          <span className="text-sm text-stone-700">
                            {feature}
                          </span>
                        </li>
                      ))}
                    </ul>
                    {isEnterprise ? (
                      <a
                        href="mailto:hello@vetara.ae?subject=Enterprise%20Sales"
                        className="mt-8 block"
                        data-testid={`pricing-cta-${key}`}
                      >
                        <Button
                          variant="outline"
                          className="w-full border-stone-300 font-semibold text-stone-700 hover:bg-stone-50"
                          data-testid={`btn-pricing-${key}`}
                        >
                          {plan.cta}
                        </Button>
                      </a>
                    ) : (
                      <Link
                        href={
                          key === "pro"
                            ? `${signupHref}?plan=professional`
                            : signupHref
                        }
                        className="mt-8 block"
                        data-testid={`pricing-cta-${key}`}
                      >
                        <Button
                          variant={ctaVariant}
                          className={`w-full font-semibold ${
                            highlighted
                              ? "bg-primary text-white hover:bg-primary/90"
                              : "border-primary text-primary hover:bg-accent"
                          }`}
                          data-testid={`btn-pricing-${key}`}
                        >
                          {plan.cta}
                        </Button>
                      </Link>
                    )}
                    {m.trial_under_plan && (
                      <p className="mt-2 text-center text-xs text-stone-400">
                        {m.trial_under_plan}
                      </p>
                    )}
                  </CardContent>
                </Card>
              ),
            )}
          </div>

          {/* Footer notes */}
          <p className="mt-8 text-center text-sm text-stone-500">
            {m.trial_note}{" "}
            <span className="text-stone-500">{m.vat_note}</span>
          </p>
        </div>
      </section>

      {/* ── Feature Comparison Table ─────────────────────────────── */}
      <section
        data-testid="pricing-comparison"
        className="bg-white py-20 sm:py-28"
      >
        <div className="mx-auto max-w-5xl px-4 sm:px-6 lg:px-8">
          <div className="mx-auto max-w-2xl text-center">
            <h2 className="text-2xl font-bold tracking-tight text-stone-900 sm:text-3xl">
              {m.comparison.title}
            </h2>
            <p className="mt-4 text-lg text-stone-600">
              {m.comparison.subtitle}
            </p>
          </div>

          <div className="mt-12 overflow-x-auto">
            <table
              className="w-full min-w-[600px] text-sm"
              data-testid="pricing-comparison-table"
            >
              <thead>
                <tr className="border-b-2 border-stone-200">
                  <th className="py-4 pe-4 text-start font-semibold text-stone-900">
                    {m.comparison.feature}
                  </th>
                  <th className="px-4 py-4 text-center font-semibold text-stone-500">
                    {m.comparison.starter_col}
                  </th>
                  <th className="px-4 py-4 text-center font-semibold text-primary">
                    {m.comparison.pro_col}
                  </th>
                  <th className="px-4 py-4 text-center font-semibold text-stone-500">
                    {m.comparison.enterprise_col}
                  </th>
                </tr>
              </thead>
              <tbody>
                {/* Limit rows (patients, vets) */}
                {LIMIT_ROWS.map((row) => (
                  <tr
                    key={row.labelKey}
                    className="border-b border-stone-100"
                    data-testid={`comparison-row-${row.labelKey}`}
                  >
                    <td className="py-3.5 pe-4 text-stone-700">
                      {m.comparison[row.labelKey]}
                    </td>
                    <td className="px-4 py-3.5 text-center text-stone-600">
                      {m.comparison[row.starterKey]}
                    </td>
                    <td className="px-4 py-3.5 text-center font-medium text-stone-900">
                      {m.comparison[row.proKey]}
                    </td>
                    <td className="px-4 py-3.5 text-center text-stone-600">
                      {m.comparison[row.enterpriseKey]}
                    </td>
                  </tr>
                ))}

                {/* Feature rows derived from FEATURE_PLAN_MAP */}
                {FEATURE_ROWS.map((row) => (
                  <tr
                    key={row.key}
                    className="border-b border-stone-100"
                    data-testid={`comparison-row-${row.key}`}
                  >
                    <td className="py-3.5 pe-4 text-stone-700">
                      {m.comparison[row.labelKey]}
                    </td>
                    {(
                      ["starter", "professional", "enterprise"] as const
                    ).map((plan) => (
                      <td key={plan} className="px-4 py-3.5 text-center">
                        {hasAccess(plan, row.key) ? (
                          <Check
                            className="mx-auto h-5 w-5 text-primary"
                            aria-label="Included"
                          />
                        ) : (
                          <Minus
                            className="mx-auto h-5 w-5 text-stone-300"
                            aria-label="Not included"
                          />
                        )}
                      </td>
                    ))}
                  </tr>
                ))}
              </tbody>
            </table>
          </div>
        </div>
      </section>

      {/* ── Pricing FAQ ──────────────────────────────────────────── */}
      <section
        data-testid="pricing-faq"
        className="bg-stone-50 py-20 sm:py-28"
      >
        <div className="mx-auto max-w-3xl px-4 sm:px-6 lg:px-8">
          <div className="text-center">
            <h2 className="text-2xl font-bold tracking-tight text-stone-900 sm:text-3xl">
              {m.faq.title}
            </h2>
            <p className="mt-4 text-lg text-stone-600">{m.faq.subtitle}</p>
          </div>

          <div className="mt-12 divide-y divide-stone-200 rounded-2xl border border-stone-200 bg-white shadow-sm">
            {m.faq.items.map((item, index) => {
              const isOpen = faqOpen === index;
              return (
                <div key={index} data-testid={`pricing-faq-${index}`}>
                  <button
                    type="button"
                    data-testid={`pricing-faq-${index}-trigger`}
                    aria-expanded={isOpen}
                    onClick={() => toggleFaq(index)}
                    className="flex w-full items-center justify-between px-6 py-5 text-start transition-colors duration-200 hover:bg-stone-50/50"
                  >
                    <span className="text-sm font-semibold text-stone-900 sm:text-base">
                      {item.question}
                    </span>
                    <ChevronDown
                      className={`ms-4 h-5 w-5 shrink-0 text-stone-400 transition-transform duration-300 ${
                        isOpen ? "rotate-180" : ""
                      }`}
                      aria-hidden="true"
                    />
                  </button>
                  <div
                    data-testid={`pricing-faq-${index}-content`}
                    className={`grid transition-all duration-300 ease-in-out ${
                      isOpen
                        ? "grid-rows-[1fr] opacity-100"
                        : "grid-rows-[0fr] opacity-0"
                    }`}
                  >
                    <div className="overflow-hidden">
                      <div className="px-6 pb-5">
                        <p className="text-sm leading-relaxed text-stone-600">
                          {item.answer}
                        </p>
                      </div>
                    </div>
                  </div>
                </div>
              );
            })}
          </div>
        </div>
      </section>

      {/* ── Final CTA ────────────────────────────────────────────── */}
      <section
        data-testid="pricing-final-cta"
        className="bg-gradient-to-br from-primary/5 via-white to-accent/30 py-20 sm:py-28"
      >
        <div className="mx-auto max-w-2xl px-4 text-center sm:px-6 lg:px-8">
          <h2 className="text-2xl font-bold tracking-tight text-stone-900 sm:text-3xl">
            {m.final_cta.headline}
          </h2>
          <p className="mt-4 text-lg text-stone-600">
            {m.final_cta.subtitle}
          </p>
          <div className="mt-8">
            <Link href={signupHref} data-testid="pricing-final-cta-link">
              <Button
                size="lg"
                className="bg-primary px-10 text-base font-semibold text-white hover:bg-primary/90"
                data-testid="btn-pricing-final-cta"
              >
                {m.final_cta.cta}
              </Button>
            </Link>
          </div>
          <p className="mt-4 text-sm text-stone-500">
            {m.final_cta.reassurance}
          </p>
        </div>
      </section>
    </>
  );
}
