"use client";

import { useEffect, useState } from "react";
import Link from "next/link";
import { Button } from "@/components/ui/button";
import { trackEvent, AnalyticsEvents } from "@/lib/analytics";

interface Props {
  signupHref: string;
  ctaLabel: string;
  tagline: string;
}

/**
 * Sticky CTA bar that appears when the user scrolls past the hero section.
 * Disappears when scrolling back up to avoid duplicating the hero CTA.
 */
export function StickyCtaBar({ signupHref, ctaLabel, tagline }: Props) {
  const [visible, setVisible] = useState(false);

  useEffect(() => {
    function handleScroll() {
      // Show after scrolling past ~600px (roughly past the hero)
      setVisible(window.scrollY > 600);
    }
    window.addEventListener("scroll", handleScroll, { passive: true });
    return () => window.removeEventListener("scroll", handleScroll);
  }, []);

  return (
    <div
      data-testid="sticky-cta-bar"
      className={`fixed top-0 left-0 right-0 z-[60] border-b border-primary/80 bg-primary transition-all duration-300 ${
        visible
          ? "translate-y-0 opacity-100"
          : "-translate-y-full opacity-0 pointer-events-none"
      }`}
    >
      <div className="mx-auto flex max-w-7xl items-center justify-between px-4 py-2.5 sm:px-6 lg:px-8">
        <p className="hidden text-sm font-medium text-primary-foreground sm:block">
          {tagline}
        </p>
        <Link href={signupHref} data-testid="sticky-cta-link">
          <Button
            size="sm"
            className="bg-white px-6 text-sm font-semibold text-primary transition-all duration-200 hover:bg-accent hover:shadow-md"
            onClick={() => {
              trackEvent(AnalyticsEvents.CTA_STICKY);
              trackEvent("generate_lead", { cta_location: "sticky" });
            }}
            data-testid="btn-sticky-cta"
          >
            {ctaLabel}
          </Button>
        </Link>
      </div>
    </div>
  );
}
