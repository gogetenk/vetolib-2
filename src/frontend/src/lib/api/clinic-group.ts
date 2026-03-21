import { apiGet, apiPost } from './client'
import { storeTokens } from './auth'

export interface ClinicSummary {
  id: string
  name: string
  address: string
}

export interface ClinicGroupResponse {
  clinics: ClinicSummary[]
}

export interface SwitchClinicResponse {
  accessToken: string
  refreshToken: string
  expiresIn: number
}

// GET /api/v1/clinic-groups/{id}/clinics
export async function getClinicGroupClinics(groupId: string): Promise<ClinicGroupResponse> {
  return apiGet<ClinicGroupResponse>(`/api/v1/clinic-groups/${groupId}/clinics`)
}

// POST /api/v1/auth/switch-clinic
export async function switchClinic(clinicId: string): Promise<SwitchClinicResponse> {
  const response = await apiPost<SwitchClinicResponse>('/api/v1/auth/switch-clinic', { clinicId })
  storeTokens(response)
  return response
}
