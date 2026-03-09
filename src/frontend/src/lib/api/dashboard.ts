import { apiGet } from './client'

export type AppointmentStatus =
  | 'SCHEDULED'
  | 'CHECKED_IN'
  | 'IN_PROGRESS'
  | 'COMPLETED'
  | 'CANCELLED'

export type ActivityType = 'APPOINTMENT' | 'MEDICAL' | 'BILLING'

export interface DashboardStatsDto {
  appointmentsToday: number
  pendingCheckin: number
  unpaidInvoicesAed: number
  totalPatients: number
}

export interface TodayAppointmentDto {
  id: string
  patientName: string
  species: string
  ownerName: string
  vetName: string
  vetId: string
  status: AppointmentStatus
  scheduledAt: string
}

export interface ActivityDto {
  id: string
  type: ActivityType
  message: string
  occurredAt: string
  relatedId: string | null
}

export async function getDashboardStats(): Promise<DashboardStatsDto> {
  return apiGet<DashboardStatsDto>('/api/dashboard/stats')
}

export async function getTodayAppointments(): Promise<TodayAppointmentDto[]> {
  return apiGet<TodayAppointmentDto[]>('/api/dashboard/today-appointments')
}

export async function getRecentActivity(): Promise<ActivityDto[]> {
  return apiGet<ActivityDto[]>('/api/dashboard/recent-activity')
}
