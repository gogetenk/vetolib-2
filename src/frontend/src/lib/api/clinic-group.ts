import { apiGet, apiPost, apiPut, apiDelete } from './client'
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

export interface ClinicGroupDetail {
  id: string
  name: string
  clinics: ClinicGroupClinicDto[]
}

export interface ClinicGroupClinicDto {
  id: string
  name: string
  address: string
  totalPatients: number
  monthlyRevenue: number
}

export interface ClinicGroupStats {
  totalPatients: number
  totalRevenue: number
  clinicCount: number
}

export interface ClinicSearchResult {
  id: string
  name: string
  address: string
}

// GET /api/v1/clinic-groups/{id}/clinics
export async function getClinicGroupClinics(groupId: string): Promise<ClinicGroupResponse> {
  return apiGet<ClinicGroupResponse>(`/api/v1/clinic-groups/${groupId}/clinics`)
}

// GET /api/v1/clinic-groups/{id}
export async function getClinicGroupDetail(groupId: string): Promise<ClinicGroupDetail> {
  return apiGet<ClinicGroupDetail>(`/api/v1/clinic-groups/${groupId}`)
}

// GET /api/v1/clinic-groups/{id}/stats
export async function getClinicGroupStats(groupId: string): Promise<ClinicGroupStats> {
  return apiGet<ClinicGroupStats>(`/api/v1/clinic-groups/${groupId}/stats`)
}

// PUT /api/v1/clinic-groups/{id}
export async function updateClinicGroup(groupId: string, data: { name: string }): Promise<void> {
  await apiPut(`/api/v1/clinic-groups/${groupId}`, data)
}

// POST /api/v1/clinic-groups/{id}/clinics
export async function addClinicToGroup(groupId: string, clinicId: string): Promise<void> {
  await apiPost(`/api/v1/clinic-groups/${groupId}/clinics`, { clinicId })
}

// DELETE /api/v1/clinic-groups/{id}/clinics/{clinicId}
export async function removeClinicFromGroup(groupId: string, clinicId: string): Promise<void> {
  await apiDelete(`/api/v1/clinic-groups/${groupId}/clinics/${clinicId}`)
}

// GET /api/v1/clinics/search?q={query}
export async function searchClinics(query: string): Promise<ClinicSearchResult[]> {
  return apiGet<ClinicSearchResult[]>(`/api/v1/clinics/search?q=${encodeURIComponent(query)}`)
}

// POST /api/v1/auth/switch-clinic
export async function switchClinic(clinicId: string): Promise<SwitchClinicResponse> {
  const response = await apiPost<SwitchClinicResponse>('/api/v1/auth/switch-clinic', { clinicId })
  storeTokens(response)
  return response
}
