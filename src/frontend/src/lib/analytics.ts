declare global {
  interface Window {
    gtag?: (command: string, event: string, params?: Record<string, string>) => void;
  }
}

export function trackEvent(
  name: string,
  properties?: Record<string, string>
): void {
  if (typeof window !== "undefined" && typeof window.gtag === "function") {
    window.gtag("event", name, properties);
  }
}

export const AnalyticsEvents = {
  CTA_HERO: "cta_click_hero",
  CTA_DEMO: "cta_click_demo",
  CTA_PRICING: "cta_click_pricing",
  CTA_FINAL: "cta_click_final",
  SCROLL_DEPTH: "scroll_depth",
  FAQ_EXPAND: "faq_expand",
  LANGUAGE_SWITCH: "language_switch",
  PRICING_TOGGLE: "pricing_toggle",
} as const;
