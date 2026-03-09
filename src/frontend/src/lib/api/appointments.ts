import { apiGet, apiPost, apiPatch } from './client'

export type AppointmentStatus =
  | 'SCHEDULED'
  | 'CHECKED_IN'
  | 'IN_PROGRESS'
  | 'COMPLETED'
  | 'CANCELLED'

export type AppointmentAction = 'CHECK_IN' | 'START' | 'COMPLETE' | 'CANCEL'

export type Species = 'Dog' | 'Cat' | 'Bird' | 'Rabbit' | 'Horse' | 'Exotic'

export interface AppointmentDto {
  id: string
  patientName: string
  species: Species
  ownerName: string
  ownerPhone: string
  vetId: string
  vetName: string
  status: AppointmentStatus
  scheduledAt: string
  reason: string
  notes?: string
  cancellationReason?: string
  clinicId: string
}

export interface PagedResult<T> {
  items: T[]
  totalCount: number
  page: number
  pageSize: number
}

export interface AppointmentFilters {
  status?: AppointmentStatus
  vetId?: string
  date?: string
  page?: number
  pageSize?: number
}

export interface CreateAppointmentRequest {
  patientName: string
  species: Species
  ownerName: string
  ownerPhone: string
  vetId: string
  scheduledAt: string
  reason: string
  notes?: string
}

export interface VetDto {
  id: string
  name: string
}

export async function getVets(): Promise<VetDto[]> {
  return apiGet<VetDto[]>('/api/vets')
}

export async function getAppointments(
  filters?: AppointmentFilters
): Promise<PagedResult<AppointmentDto>> {
  const params = new URLSearchParams()
  if (filters?.status) params.set('status', filters.status)
  if (filters?.vetId) params.set('vetId', filters.vetId)
  if (filters?.date) params.set('date', filters.date)
  if (filters?.page) params.set('page', String(filters.page))
  if (filters?.pageSize) params.set('pageSize', String(filters.pageSize))
  const query = params.toString()
  return apiGet<PagedResult<AppointmentDto>>(`/api/appointments${query ? `?${query}` : ''}`)
}

export async function getAppointment(id: string): Promise<AppointmentDto> {
  return apiGet<AppointmentDto>(`/api/appointments/${id}`)
}

export async function createAppointment(
  data: CreateAppointmentRequest
): Promise<AppointmentDto> {
  return apiPost<AppointmentDto>('/api/appointments', data)
}

export async function transitionAppointment(
  id: string,
  action: AppointmentAction
): Promise<AppointmentDto> {
  return apiPatch<AppointmentDto>(`/api/appointments/${id}/transition`, { action })
}

export async function cancelAppointment(
  id: string,
  reason: string
): Promise<AppointmentDto> {
  return apiPatch<AppointmentDto>(`/api/appointments/${id}/transition`, {
    action: 'CANCEL',
    reason,
  })
}
