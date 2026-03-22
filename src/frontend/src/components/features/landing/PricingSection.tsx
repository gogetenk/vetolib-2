"use client";

import { useState } from "react";
import Link from "next/link";
import { Check } from "lucide-react";
import { Button } from "@/components/ui/button";
import { Badge } from "@/components/ui/badge";
import { Card, CardContent, CardHeader } from "@/components/ui/card";
import { ScrollReveal } from "./ScrollReveal";

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

interface PricingMessages {
  title: string;
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
}

interface Props {
  messages: PricingMessages;
  loginHref: string;
}

export function PricingSection({ messages: m, loginHref }: Props) {
  const [annual, setAnnual] = useState(false);

  const plans: Array<{
    key: string;
    plan: PlanMessages;
    highlighted: boolean;
    ctaVariant: "outline" | "default" | "ghost";
    isEnterprise: boolean;
  }> = [
    { key: "starter", plan: m.starter, highlighted: false, ctaVariant: "outline", isEnterprise: false },
    { key: "pro", plan: m.pro, highlighted: true, ctaVariant: "default", isEnterprise: false },
    { key: "enterprise", plan: m.enterprise, highlighted: false, ctaVariant: "outline", isEnterprise: true },
  ];

  return (
    <section
      id="pricing"
      data-testid="section-pricing"
      className="bg-stone-50 py-20 sm:py-28"
    >
      <div className="mx-auto max-w-7xl px-4 sm:px-6 lg:px-8">
        {/* Header */}
        <ScrollReveal direction="fade-up">
          <div className="mx-auto max-w-2xl text-center">
            {m.early_access_badge && (
              <div className="mb-4 inline-flex items-center gap-2 rounded-full bg-amber-50 px-4 py-1.5 text-sm font-semibold text-amber-700 ring-1 ring-amber-200" data-testid="pricing-early-access-badge">
                <span className="relative flex h-2 w-2">
                  <span className="absolute inline-flex h-full w-full animate-ping rounded-full bg-amber-400 opacity-75" />
                  <span className="relative inline-flex h-2 w-2 rounded-full bg-amber-500" />
                </span>
                {m.early_access_badge}
              </div>
            )}
            <h2 className="text-2xl font-bold tracking-tight text-stone-900 sm:text-3xl lg:text-4xl">
              {m.title}
            </h2>
            <p className="mt-4 text-lg text-stone-600">{m.subtitle}</p>

            {/* Toggle */}
            <div className="mt-8 inline-flex items-center gap-3 rounded-full border border-stone-200 bg-white p-1 shadow-sm">
              <button
                type="button"
                data-testid="pricing-toggle-monthly"
                onClick={() => setAnnual(false)}
                aria-pressed={!annual}
                className={`rounded-full px-5 py-2 text-sm font-medium transition-colors ${
                  !annual
                    ? "bg-emerald-700 text-white shadow"
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
                    ? "bg-emerald-700 text-white shadow"
                    : "text-stone-600 hover:text-stone-900"
                }`}
              >
                {m.toggle_annual}
                {!annual && (
                  <span className="rounded-full bg-emerald-100 px-2 py-0.5 text-xs font-semibold text-emerald-700">
                    {m.annual_savings}
                  </span>
                )}
              </button>
            </div>
          </div>
        </ScrollReveal>

        {/* Plans grid */}
        <div className="mt-12 grid gap-6 sm:grid-cols-2 lg:grid-cols-3">
          {plans.map(({ key, plan, highlighted, ctaVariant, isEnterprise }, idx) => (
            <ScrollReveal key={key} direction="fade-up" delay={idx * 100}>
              <Card
                data-testid={`pricing-plan-${key}`}
                className={`flex flex-col transition-all duration-300 hover:shadow-lg hover:-translate-y-1 ${
                  highlighted
                    ? "relative border-2 border-emerald-700 shadow-lg"
                    : "border border-stone-200 shadow-sm"
                }`}
              >
                {plan.badge && (
                  <div className="absolute -top-3.5 left-1/2 -translate-x-1/2">
                    <Badge className="bg-emerald-700 px-4 py-1 text-xs font-semibold text-white">
                      {plan.badge}
                    </Badge>
                  </div>
                )}
                <CardHeader className={`p-6 pb-0 ${plan.badge ? "pt-8" : ""}`}>
                  <p
                    className={`text-sm font-semibold uppercase tracking-wide ${
                      highlighted ? "text-emerald-700" : "text-stone-500"
                    }`}
                  >
                    {plan.name}
                  </p>
                  <div className="mt-3 flex items-end gap-1">
                    <span className="text-3xl font-extrabold text-stone-900 lg:text-4xl">
                      {annual ? plan.price_annual : plan.price_monthly}
                    </span>
                    <span className="mb-1 text-sm text-stone-500">
                      {plan.per_month}
                    </span>
                  </div>
                  {/* USD hint */}
                  {(annual ? plan.usd_hint_annual : plan.usd_hint_monthly) && (
                    <p className="mt-1 text-xs text-stone-400">
                      {annual ? plan.usd_hint_annual : plan.usd_hint_monthly}
                    </p>
                  )}
                  <p className="mt-2 text-sm text-stone-600">{plan.description}</p>
                </CardHeader>
                <CardContent className="flex flex-1 flex-col p-6">
                  <ul className="flex-1 space-y-3">
                    {plan.features.map((feature, i) => (
                      <li key={i} className="flex items-start gap-3">
                        <Check
                          className="mt-0.5 h-4 w-4 shrink-0 text-emerald-600"
                          aria-hidden="true"
                        />
                        <span className="text-sm text-stone-700">{feature}</span>
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
                      href={loginHref}
                      className="mt-8 block"
                      data-testid={`pricing-cta-${key}`}
                    >
                      <Button
                        variant={ctaVariant}
                        className={`w-full font-semibold ${
                          highlighted
                            ? "bg-emerald-700 text-white hover:bg-emerald-800"
                            : "border-emerald-700 text-emerald-700 hover:bg-emerald-50"
                        }`}
                        data-testid={`btn-pricing-${key}`}
                      >
                        {plan.cta}
                      </Button>
                    </Link>
                  )}
                  {m.trial_under_plan && (
                    <p className="mt-2 text-center text-xs text-stone-400">{m.trial_under_plan}</p>
                  )}
                </CardContent>
              </Card>
            </ScrollReveal>
          ))}
        </div>

        {/* Footer notes */}
        <p className="mt-8 text-center text-sm text-stone-500">
          {m.trial_note}{" "}
          <span className="text-stone-500">{m.vat_note}</span>
        </p>
      </div>
    </section>
  );
}
