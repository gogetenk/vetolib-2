import { posthog, isPostHogAvailable } from '@/lib/posthog'

declare global {
  interface Window {
    gtag?: (command: string, event: string, params?: Record<string, string>) => void;
  }
}

export function trackEvent(
  name: string,
  properties?: Record<string, string>
): void {
  if (isPostHogAvailable()) {
    posthog.capture(name, properties)
  }
}

/**
 * Returns a bucketed range string for an AED amount.
 * Used to avoid sending exact monetary amounts as PII.
 */
export function bucketAed(amount: number): string {
  if (amount < 100) return "0-100"
  if (amount < 500) return "100-500"
  if (amount < 1000) return "500-1000"
  return "1000+"
}

export const AnalyticsEvents = {
  // Landing page events (also tracked via gtag in landing components)
  CTA_HERO: "cta_click_hero",
  CTA_DEMO: "cta_click_demo",
  CTA_PRICING: "cta_click_pricing",
  CTA_FINAL: "cta_click_final",
  CTA_STICKY: "cta_click_sticky",
  CTA_FOOTER: "cta_click_footer",
  EXIT_INTENT_SHOWN: "exit_intent_shown",
  EXIT_INTENT_EMAIL: "exit_intent_email_captured",
  SCROLL_DEPTH: "scroll_depth",
  FAQ_EXPAND: "faq_expand",
  LANGUAGE_SWITCH: "language_switch",
  PRICING_TOGGLE: "pricing_toggle",

  // P1 — Appointments
  APPOINTMENT_FORM_OPENED: "appointment_form_opened",
  APPOINTMENT_CREATED: "appointment_created",
  APPOINTMENT_STATUS_CHANGED: "appointment_status_changed",

  // P1 — Patients
  PATIENT_CREATED: "patient_created",
  PATIENT_SEARCHED: "patient_searched",

  // P1 — Medical Records
  MEDICAL_RECORD_ADDED: "medical_record_added",

  // P1 — Billing
  INVOICE_CREATED: "invoice_created",
  INVOICE_STATUS_CHANGED: "invoice_status_changed",

  // P1 — Dashboard
  DASHBOARD_VIEWED: "dashboard_viewed",

  // P1 — Errors
  API_ERROR_DISPLAYED: "api_error_displayed",

  // P2 — Patients
  PATIENT_CSV_IMPORTED: "patient_csv_imported",

  // P2 — Billing
  INVOICE_PDF_DOWNLOADED: "invoice_pdf_downloaded",

  // P2 — Team Management
  USER_INVITED: "user_invited",
  USER_ROLE_CHANGED: "user_role_changed",

  // P2 — Auth
  PASSWORD_CHANGED: "password_changed",
  FORM_VALIDATION_ERROR: "form_validation_error",
  SESSION_EXPIRED: "session_expired",

  // P2 — Analytics
  ANALYTICS_SECTION_VIEWED: "analytics_section_viewed",

  // AI Triage
  AI_TRIAGE_ACCEPTED: "ai_triage_accepted",
  AI_TRIAGE_OVERRIDDEN: "ai_triage_overridden",
} as const;
