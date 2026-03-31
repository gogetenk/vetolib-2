import { apiGet, apiPut } from './client'

export type DayOfWeek =
  | 'Sunday'
  | 'Monday'
  | 'Tuesday'
  | 'Wednesday'
  | 'Thursday'
  | 'Friday'
  | 'Saturday'

export interface WorkingHoursDayDto {
  dayOfWeek: DayOfWeek
  isOpen: boolean
  openTime: string | null
  closeTime: string | null
  breakStartTime: string | null
  breakEndTime: string | null
}

export interface WorkingHoursDto {
  days: WorkingHoursDayDto[]
}

export interface UpdateWorkingHoursRequest {
  days: WorkingHoursDayDto[]
}

export async function getWorkingHours(): Promise<WorkingHoursDto> {
  return apiGet<WorkingHoursDto>('/api/v1/preferences/working-hours')
}

export async function updateWorkingHours(
  request: UpdateWorkingHoursRequest
): Promise<void> {
  return apiPut<void>('/api/v1/preferences/working-hours', request)
}
