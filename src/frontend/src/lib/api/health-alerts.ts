import { apiGet, apiPatch, apiPost } from './client'

export type AlertSeverity = 'High' | 'Medium' | 'Low'
export type AlertStatus = 'Active' | 'Acknowledged' | 'Dismissed'

export interface HealthAlertDto {
  id: string
  patientId: string
  patientName: string
  breed: string
  species: string
  age: string
  ownerName: string
  title: string
  description: string
  severity: AlertSeverity
  status: AlertStatus
  dismissReason?: string
  createdAt: string
}

export interface AppointmentPreFillDto {
  patientId: string
  patientName: string
  ownerName: string
  reason: string
  suggestedDate: string
  suggestedDurationMinutes: number
}

export async function getHealthAlerts(): Promise<HealthAlertDto[]> {
  return apiGet<HealthAlertDto[]>('/api/v1/ai/health-alerts')
}

export async function getPatientHealthAlerts(patientId: string): Promise<HealthAlertDto[]> {
  return apiGet<HealthAlertDto[]>(`/api/v1/ai/health-alerts/patient/${patientId}`)
}

export async function dismissAlert(id: string, reason: string): Promise<void> {
  await apiPatch<void>(`/api/v1/ai/health-alerts/${id}/dismiss`, { reason })
}

export async function acknowledgeAlert(id: string): Promise<void> {
  await apiPatch<void>(`/api/v1/ai/health-alerts/${id}/acknowledge`, {})
}

export async function convertAlertToAppointment(id: string): Promise<AppointmentPreFillDto> {
  return apiPost<AppointmentPreFillDto>(`/api/v1/ai/health-alerts/${id}/convert-to-appointment`, {})
}
