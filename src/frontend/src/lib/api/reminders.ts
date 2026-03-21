import { apiGet, apiPut } from './client'

export type ReminderType = 'appointment' | 'vaccination' | 'follow_up'
export type ReminderLogStatus = 'sent' | 'failed' | 'pending'

export interface ReminderConfigDto {
  appointmentReminders: {
    enabled: boolean
    timingHours: number
  }
  vaccinationReminders: {
    enabled: boolean
  }
  followUpReminders: {
    enabled: boolean
    daysAfter: number
  }
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
