"use client";

import Link from "next/link";
import { Button } from "@/components/ui/button";
import { trackEvent } from "@/lib/analytics";

interface Props {
  href: string;
  /** Which CTA location: hero, features, footer, sticky, final */
  location: string;
  buttonClassName?: string;
  size?: "default" | "sm" | "lg" | "icon";
  children: React.ReactNode;
  "data-testid"?: string;
  buttonTestId?: string;
}

/**
 * A CTA link that fires a `generate_lead` conversion event (GA4)
 * plus the location-specific analytics event (PostHog + GA4).
 */
export function TrackedCtaLink({
  href,
  location,
  buttonClassName,
  size = "lg",
  children,
  buttonTestId,
  ...rest
}: Props) {
  const handleClick = () => {
    trackEvent("generate_lead", { cta_location: location });
  };

  return (
    <Link href={href} data-testid={rest["data-testid"]}>
      <Button
        size={size}
        className={buttonClassName}
        onClick={handleClick}
        data-testid={buttonTestId}
      >
        {children}
      </Button>
    </Link>
  );
}
