"use client";

import Link from "next/link";
import { Button } from "@/components/ui/button";
import { trackEvent, AnalyticsEvents } from "@/lib/analytics";
import { ScrollReveal } from "./ScrollReveal";

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
        <ScrollReveal direction="fade-up">
          <h2 className="text-2xl font-extrabold tracking-tight text-white sm:text-3xl lg:text-4xl">
            {headline}
          </h2>
        </ScrollReveal>
        <ScrollReveal direction="fade-up" delay={150}>
          <p className="mt-6 text-lg leading-relaxed text-emerald-100">
            {subtitle}
          </p>
        </ScrollReveal>
        <ScrollReveal direction="fade-up" delay={300}>
          <div className="mt-8">
            <Link href={loginHref} data-testid="final-cta-button">
              <Button
                size="lg"
                className="bg-white px-10 text-base font-semibold text-emerald-700 transition-all duration-300 hover:bg-emerald-50 hover:shadow-lg hover:shadow-white/25 hover:scale-[1.02]"
                onClick={() => trackEvent(AnalyticsEvents.CTA_FINAL)}
                data-testid="btn-final-cta"
              >
                {cta}
              </Button>
            </Link>
          </div>
          <p className="mt-4 text-sm text-emerald-200">{reassurance}</p>
        </ScrollReveal>
      </div>
    </section>
  );
}
