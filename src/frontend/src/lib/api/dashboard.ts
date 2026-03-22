import { apiGet } from './client'

export type AppointmentStatus =
  | 'SCHEDULED'
  | 'CHECKED_IN'
  | 'IN_PROGRESS'
  | 'COMPLETED'
  | 'CANCELLED'

export type ConsultationType =
  | 'GENERAL'
  | 'VACCINATION'
  | 'SURGERY'
  | 'EMERGENCY'
  | 'FOLLOWUP'
  | 'GROOMING'

export type ActivityType = 'APPOINTMENT' | 'MEDICAL' | 'BILLING' | 'MESSAGE'

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
  consultationType?: ConsultationType
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

export interface RevenueByMonthDto {
  month: string // "2026-01"
  total: number
}

export interface PatientsBySpeciesDto {
  species: string
  count: number
}

export interface DashboardAnalyticsDto {
  revenueByMonth: RevenueByMonthDto[]
  patientsBySpecies: PatientsBySpeciesDto[]
  noShowRate: number
}

export async function getDashboardAnalytics(): Promise<DashboardAnalyticsDto> {
  return apiGet<DashboardAnalyticsDto>('/api/dashboard/analytics')
}

export interface AccumulatedValueDto {
  totalPatients: number
  totalMedicalRecords: number
  totalInvoices: number
  totalAppointments: number
  memberSince: string // ISO date string
}

export async function getAccumulatedValue(): Promise<AccumulatedValueDto> {
  return apiGet<AccumulatedValueDto>('/api/dashboard/accumulated-value')
}
