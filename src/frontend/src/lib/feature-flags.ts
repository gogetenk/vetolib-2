/**
 * Feature flags tied to subscription plans.
 *
 * For MVP, all features are unlocked (the subscription check is a stub).
 * The mapping exists so that when billing is wired, we can enforce plan limits
 * without touching every component individually.
 */

export type SubscriptionPlan = "starter" | "professional" | "enterprise";

export type FeatureKey =
  | "scheduling"
  | "medical_records"
  | "email_reminders"
  | "client_portal"
  | "bilingual_interface"
  | "soap_notes"
  | "whatsapp_reminders"
  | "ai_health_alerts"
  | "ai_triage"
  | "ai_soap_scribe"
  | "stock_management"
  | "breeding_module"
  | "analytics_dashboard"
  | "recurring_appointments"
  | "qr_checkin"
  | "multi_clinic"
  | "api_access"
  | "custom_roles"
  | "priority_support"
  | "dedicated_account_manager"
  | "custom_integrations";

/**
 * Maps each feature to the minimum plan required to access it.
 */
export const FEATURE_PLAN_MAP: Record<FeatureKey, SubscriptionPlan> = {
  // Starter (Free) features
  scheduling: "starter",
  medical_records: "starter",
  email_reminders: "starter",
  client_portal: "starter",
  bilingual_interface: "starter",
  soap_notes: "starter",

  // Professional (149 AED/mo) features
  whatsapp_reminders: "professional",
  ai_health_alerts: "professional",
  ai_triage: "professional",
  ai_soap_scribe: "professional",
  stock_management: "professional",
  breeding_module: "professional",
  analytics_dashboard: "professional",
  recurring_appointments: "professional",
  qr_checkin: "professional",

  // Enterprise (Custom) features
  multi_clinic: "enterprise",
  api_access: "enterprise",
  custom_roles: "enterprise",
  priority_support: "enterprise",
  dedicated_account_manager: "enterprise",
  custom_integrations: "enterprise",
};

const PLAN_RANK: Record<SubscriptionPlan, number> = {
  starter: 0,
  professional: 1,
  enterprise: 2,
};

/**
 * Check if a feature is available for a given subscription plan.
 * Higher-tier plans include all features from lower tiers.
 */
export function hasFeatureAccess(
  currentPlan: SubscriptionPlan,
  feature: FeatureKey,
): boolean {
  const requiredPlan = FEATURE_PLAN_MAP[feature];
  return PLAN_RANK[currentPlan] >= PLAN_RANK[requiredPlan];
}

/**
 * Stub: returns the current clinic's subscription plan.
 *
 * For MVP, always returns "enterprise" so all features are unlocked.
 * When billing is wired, this will read from the clinic's subscription state.
 */
export function getCurrentPlan(): SubscriptionPlan {
  // TODO: Wire to actual billing/subscription state
  return "enterprise";
}

/**
 * Convenience: check if the current clinic can access a feature.
 * Uses the stub getCurrentPlan() under the hood.
 */
export function isFeatureEnabled(feature: FeatureKey): boolean {
  return hasFeatureAccess(getCurrentPlan(), feature);
}

/**
 * Plan metadata for display purposes (e.g., upgrade prompts).
 */
export const PLAN_METADATA: Record<
  SubscriptionPlan,
  { label: string; priceAed: string | null }
> = {
  starter: { label: "Starter", priceAed: null },
  professional: { label: "Professional", priceAed: "149" },
  enterprise: { label: "Enterprise", priceAed: null },
};

/**
 * Patient limits per plan. null = unlimited.
 */
export const PLAN_PATIENT_LIMITS: Record<SubscriptionPlan, number | null> = {
  starter: 50,
  professional: null,
  enterprise: null,
};

/**
 * Vet (practitioner) limits per plan. null = unlimited.
 */
export const PLAN_VET_LIMITS: Record<SubscriptionPlan, number | null> = {
  starter: 1,
  professional: 5,
  enterprise: null,
};
