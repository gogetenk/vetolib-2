"use client";

import Link from "next/link";
import { Button } from "@/components/ui/button";
import { trackEvent, AnalyticsEvents } from "@/lib/analytics";

interface Props {
  loginHref: string;
  headline: string;
  subtitle: string;
  cta: string;
  reassurance: string;
}

export function FinalCtaSection({
  loginHref,
  headline,
  subtitle,
  cta,
  reassurance,
}: Props) {
  return (
    <section
      data-testid="section-final-cta"
      className="bg-emerald-700 py-20 sm:py-28"
    >
      <div className="mx-auto max-w-3xl px-4 text-center sm:px-6 lg:px-8">
        <h2 className="text-2xl font-extrabold tracking-tight text-white sm:text-3xl lg:text-4xl">
          {headline}
        </h2>
        <p className="mt-6 text-lg leading-relaxed text-emerald-100">
          {subtitle}
        </p>
        <div className="mt-8">
          <Link href={loginHref} data-testid="final-cta-button">
            <Button
              size="lg"
              className="bg-white px-10 text-base font-semibold text-emerald-700 hover:bg-emerald-50"
              onClick={() => trackEvent(AnalyticsEvents.CTA_FINAL)}
            >
              {cta}
            </Button>
          </Link>
        </div>
        <p className="mt-4 text-sm text-emerald-200">{reassurance}</p>
      </div>
    </section>
  );
}
