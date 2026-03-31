import { apiGet, apiPut } from './client'

export type ReminderType = 'appointment' | 'vaccination' | 'follow_up'
export type ReminderLogStatus = 'sent' | 'failed' | 'pending'
export type ReminderChannel = 'Email' | 'WhatsApp' | 'Both'

export interface ReminderConfigDto {
  appointment24hEnabled: boolean
  vaccinationDueEnabled: boolean
  followUpEnabled: boolean
  appointment24hLeadTimeHours: number
  vaccinationDueLeadTimeDays: number
  preferredReminderChannel: ReminderChannel
}

export interface ReminderLogDto {
  id: string
  sentAt: string
  type: ReminderType
  patientName: string
  ownerName: string
  status: ReminderLogStatus
  errorMessage?: string
}

export async function getReminderConfig(): Promise<ReminderConfigDto> {
  return apiGet<ReminderConfigDto>('/api/v1/notifications/reminders/config')
}

export async function updateReminderConfig(config: ReminderConfigDto): Promise<void> {
  return apiPut<void>('/api/v1/notifications/reminders/config', config)
}

export async function getReminderLogs(): Promise<ReminderLogDto[]> {
  return apiGet<ReminderLogDto[]>('/api/v1/notifications/reminders/logs')
}
