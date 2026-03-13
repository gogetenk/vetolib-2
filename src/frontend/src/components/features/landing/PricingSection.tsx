"use client";

import { useState } from "react";
import Link from "next/link";
import { Check } from "lucide-react";
import { Button } from "@/components/ui/button";
import { Badge } from "@/components/ui/badge";
import { Card, CardContent, CardHeader } from "@/components/ui/card";
import { ScrollReveal } from "./ScrollReveal";

interface PricingMessages {
  title: string;
  subtitle: string;
  toggle_monthly: string;
  toggle_annual: string;
  annual_savings: string;
  trial_note: string;
  vat_note: string;
  starter: {
    name: string;
    price_monthly: string;
    price_annual: string;
    per_month: string;
    description: string;
    features: string[];
    cta: string;
  };
  pro: {
    name: string;
    badge: string;
    price_monthly: string;
    price_annual: string;
    per_month: string;
    description: string;
    features: string[];
    cta: string;
  };
  enterprise: {
    name: string;
    price: string;
    from: string;
    per_month: string;
    description: string;
    features: string[];
    cta: string;
  };
}

interface Props {
  messages: PricingMessages;
  loginHref: string;
}

export function PricingSection({ messages: m, loginHref }: Props) {
  const [annual, setAnnual] = useState(false);

  return (
    <section
      id="pricing"
      data-testid="section-pricing"
      className="bg-gray-50 py-20 sm:py-28"
    >
      <div className="mx-auto max-w-7xl px-4 sm:px-6 lg:px-8">
        {/* Header */}
        <ScrollReveal direction="fade-up">
        <div className="mx-auto max-w-2xl text-center">
          <h2 className="text-2xl font-bold tracking-tight text-gray-900 sm:text-3xl lg:text-4xl">
            {m.title}
          </h2>
          <p className="mt-4 text-lg text-gray-600">{m.subtitle}</p>

          {/* Toggle */}
          <div className="mt-8 inline-flex items-center gap-3 rounded-full border border-gray-200 bg-white p-1 shadow-sm">
            <button
              type="button"
              data-testid="pricing-toggle-monthly"
              onClick={() => setAnnual(false)}
              aria-pressed={!annual}
              className={`rounded-full px-5 py-2 text-sm font-medium transition-colors ${
                !annual
                  ? "bg-emerald-700 text-white shadow"
                  : "text-gray-600 hover:text-gray-900"
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
                  : "text-gray-600 hover:text-gray-900"
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
        <div className="mt-12 grid gap-8 lg:grid-cols-3">
          {/* Starter */}
          <ScrollReveal direction="fade-up" delay={0}>
          <Card
            data-testid="pricing-plan-starter"
            className="flex flex-col border border-gray-200 shadow-sm transition-all duration-300 hover:shadow-lg hover:-translate-y-1"
          >
            <CardHeader className="p-6 pb-0">
              <p className="text-sm font-semibold uppercase tracking-wide text-gray-500">
                {m.starter.name}
              </p>
              <div className="mt-3 flex items-end gap-1">
                <span className="text-4xl font-extrabold text-gray-900">
                  {annual ? m.starter.price_annual : m.starter.price_monthly}
                </span>
                <span className="mb-1 text-sm text-gray-500">
                  {m.starter.per_month}
                </span>
              </div>
              <p className="mt-2 text-sm text-gray-600">{m.starter.description}</p>
            </CardHeader>
            <CardContent className="flex flex-1 flex-col p-6">
              <ul className="flex-1 space-y-3">
                {m.starter.features.map((feature, i) => (
                  <li key={i} className="flex items-start gap-3">
                    <Check
                      className="mt-0.5 h-4 w-4 shrink-0 text-emerald-600"
                      aria-hidden="true"
                    />
                    <span className="text-sm text-gray-700">{feature}</span>
                  </li>
                ))}
              </ul>
              <Link href={loginHref} className="mt-8 block" data-testid="pricing-cta-starter">
                <Button
                  variant="outline"
                  className="w-full border-emerald-700 font-semibold text-emerald-700 hover:bg-emerald-50"
                  data-testid="btn-pricing-starter"
                >
                  {m.starter.cta}
                </Button>
              </Link>
            </CardContent>
          </Card>
          </ScrollReveal>

          {/* Pro — highlighted */}
          <ScrollReveal direction="fade-up" delay={150}>
          <Card
            data-testid="pricing-plan-pro"
            className="relative flex flex-col border-2 border-emerald-700 shadow-lg transition-all duration-300 hover:shadow-xl hover:-translate-y-1"
          >
            <div className="absolute -top-3.5 left-1/2 -translate-x-1/2">
              <Badge className="bg-emerald-700 px-4 py-1 text-xs font-semibold text-white">
                {m.pro.badge}
              </Badge>
            </div>
            <CardHeader className="p-6 pb-0 pt-8">
              <p className="text-sm font-semibold uppercase tracking-wide text-emerald-700">
                {m.pro.name}
              </p>
              <div className="mt-3 flex items-end gap-1">
                <span className="text-4xl font-extrabold text-gray-900">
                  {annual ? m.pro.price_annual : m.pro.price_monthly}
                </span>
                <span className="mb-1 text-sm text-gray-500">
                  {m.pro.per_month}
                </span>
              </div>
              <p className="mt-2 text-sm text-gray-600">{m.pro.description}</p>
            </CardHeader>
            <CardContent className="flex flex-1 flex-col p-6">
              <ul className="flex-1 space-y-3">
                {m.pro.features.map((feature, i) => (
                  <li key={i} className="flex items-start gap-3">
                    <Check
                      className="mt-0.5 h-4 w-4 shrink-0 text-emerald-600"
                      aria-hidden="true"
                    />
                    <span className="text-sm text-gray-700">{feature}</span>
                  </li>
                ))}
              </ul>
              <Link href={loginHref} className="mt-8 block" data-testid="pricing-cta-pro">
                <Button
                  className="w-full bg-emerald-700 font-semibold text-white hover:bg-emerald-800"
                  data-testid="btn-pricing-pro"
                >
                  {m.pro.cta}
                </Button>
              </Link>
            </CardContent>
          </Card>
          </ScrollReveal>

          {/* Enterprise */}
          <ScrollReveal direction="fade-up" delay={300}>
          <Card
            data-testid="pricing-plan-enterprise"
            className="flex flex-col border border-gray-200 shadow-sm transition-all duration-300 hover:shadow-lg hover:-translate-y-1"
          >
            <CardHeader className="p-6 pb-0">
              <p className="text-sm font-semibold uppercase tracking-wide text-gray-500">
                {m.enterprise.name}
              </p>
              <div className="mt-3 flex items-end gap-1">
                <span className="text-sm font-medium text-gray-500">
                  {m.enterprise.from}
                </span>
                <span className="text-4xl font-extrabold text-gray-900">
                  {m.enterprise.price}
                </span>
                <span className="mb-1 text-sm text-gray-500">
                  {m.enterprise.per_month}
                </span>
              </div>
              <p className="mt-2 text-sm text-gray-600">
                {m.enterprise.description}
              </p>
            </CardHeader>
            <CardContent className="flex flex-1 flex-col p-6">
              <ul className="flex-1 space-y-3">
                {m.enterprise.features.map((feature, i) => (
                  <li key={i} className="flex items-start gap-3">
                    <Check
                      className="mt-0.5 h-4 w-4 shrink-0 text-emerald-600"
                      aria-hidden="true"
                    />
                    <span className="text-sm text-gray-700">{feature}</span>
                  </li>
                ))}
              </ul>
              <a
                href="mailto:hello@vetolib.ae?subject=Enterprise%20Sales"
                className="mt-8 block"
                data-testid="pricing-cta-enterprise"
              >
                <Button
                  variant="outline"
                  className="w-full border-gray-300 font-semibold text-gray-700 hover:bg-gray-50"
                  data-testid="btn-pricing-enterprise"
                >
                  {m.enterprise.cta}
                </Button>
              </a>
            </CardContent>
          </Card>
          </ScrollReveal>
        </div>

        {/* Footer notes */}
        <p className="mt-8 text-center text-sm text-gray-500">
          {m.trial_note}{" "}
          <span className="text-gray-500">{m.vat_note}</span>
        </p>
      </div>
    </section>
  );
}
