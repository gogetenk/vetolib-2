import { apiGet, apiPost, apiPatch } from './client'

export type AppointmentStatus =
  | 'Scheduled'
  | 'CheckedIn'
  | 'InProgress'
  | 'Completed'
  | 'Cancelled'
  | 'NoShow'

export type AppointmentAction = 'CHECK_IN' | 'START' | 'COMPLETE' | 'CANCEL'

export type BookingSource = 'Staff' | 'OwnerPortal'

export type Species = 'Dog' | 'Cat' | 'Bird' | 'Rabbit' | 'Horse' | 'Exotic' | 'Falcon' | 'Reptile'

export interface AppointmentDto {
  id: string
  clinicId: string
  veterinarianId: string
  veterinarianName: string
  animalId: string
  animalName: string
  ownerName: string
  date: string            // DateOnly "YYYY-MM-DD"
  startTime: string       // TimeOnly "HH:mm:ss"
  durationMinutes: number
  endTime: string         // TimeOnly "HH:mm:ss"
  status: AppointmentStatus
  reason: string | null
  source: BookingSource
  rescheduleCount: number
  originalAppointmentId: string | null
}

/** Build a JS Date from the backend date + startTime fields */
export function appointmentToDate(dto: Pick<AppointmentDto, 'date' | 'startTime'>): Date {
  return new Date(`${dto.date}T${dto.startTime}`)
}

export interface PagedResult<T> {
  items: T[]
  totalCount: number
  page: number
  pageSize: number
}

export interface AppointmentFilters {
  status?: AppointmentStatus
  veterinarianId?: string
  date?: string
  page?: number
  pageSize?: number
}

export interface CreateAppointmentRequest {
  animalId: string
  veterinarianId: string
  date: string
  startTime: string
  durationMinutes: number
  reason?: string
  source: BookingSource
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
  if (filters?.veterinarianId) params.set('veterinarianId', filters.veterinarianId)
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
