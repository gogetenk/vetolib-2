import { apiGet, apiPost, apiDelete } from './client'

export type WaitlistPriority = 'LOW' | 'NORMAL' | 'HIGH' | 'URGENT'

export interface WaitlistEntryDto {
  id: string
  patientId: string
  patientName: string
  ownerName: string
  reason: string
  preferredDay: string
  preferredTime: string
  priority: WaitlistPriority
  createdAt: string
}

export interface CreateWaitlistEntryRequest {
  patientId: string
  reason: string
  preferredDay: string
  preferredTime: string
  priority: WaitlistPriority
}

export async function getWaitlistEntries(): Promise<WaitlistEntryDto[]> {
  return apiGet<WaitlistEntryDto[]>('/api/v1/waitlist')
}

export async function createWaitlistEntry(entry: CreateWaitlistEntryRequest): Promise<WaitlistEntryDto> {
  return apiPost<WaitlistEntryDto>('/api/v1/waitlist', entry)
}

export async function deleteWaitlistEntry(id: string): Promise<void> {
  return apiDelete(`/api/v1/waitlist/${id}`)
}
