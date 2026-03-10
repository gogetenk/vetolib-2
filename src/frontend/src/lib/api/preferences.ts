import { apiGet, apiPut, apiPost } from './client'

export type PreferenceSource = 'system_default' | 'clinic_default' | 'user_override'
export type PreferenceValueType = 'boolean' | 'string' | 'time'

export interface PreferenceItemDto {
  key: string
  label: string
  description?: string
  value: string
  defaultValue: string
  source: PreferenceSource
  valueType: PreferenceValueType
  disabled?: boolean
  disabledReason?: string
  adminOnly?: boolean
  options?: { value: string; label: string }[]
}

export interface PreferenceCategoryDto {
  key: string
  label: string
  description: string
  items: PreferenceItemDto[]
}

export interface UpdatePreferenceRequest {
  key: string
  value: string
}

export async function getPreferences(): Promise<PreferenceCategoryDto[]> {
  return apiGet<PreferenceCategoryDto[]>('/api/preferences')
}

export async function updatePreference(key: string, value: string): Promise<void> {
  return apiPut<void>(`/api/preferences/${key}`, { value })
}

export async function bulkUpdatePreferences(
  prefs: { key: string; value: string }[]
): Promise<void> {
  return apiPut<void>('/api/preferences/bulk', { preferences: prefs })
}

export async function revokeConsent(category: string): Promise<void> {
  return apiPost<void>('/api/preferences/consent/revoke', { category })
}

export async function getClinicDefaults(): Promise<PreferenceCategoryDto[]> {
  return apiGet<PreferenceCategoryDto[]>('/api/clinics/preferences')
}

export async function updateClinicDefaults(
  defaults: { key: string; value: string }[]
): Promise<void> {
  return apiPut<void>('/api/clinics/preferences', { preferences: defaults })
}
