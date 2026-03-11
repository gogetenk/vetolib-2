import { apiGet } from './client'

// ─── Types ────────────────────────────────────────────────────────────────────

export interface BookingSlot {
  /** ISO 8601 datetime in Asia/Dubai timezone offset */
  startsAt: string
  endsAt: string
  vetId: string
  vetName: string
  available: boolean
}

export interface BookingDay {
  date: string // YYYY-MM-DD in Asia/Dubai
  slots: BookingSlot[]
  /** true if the clinic is closed this day */
  closed: boolean
}

export interface SlotSuggestionDto {
  vetId: string
  vetName: string
  startsAt: string
  endsAt: string
  score: number
}

export interface SuggestSlotsParams {
  /** YYYY-MM-DD — search window start */
  from: string
  /** YYYY-MM-DD — search window end */
  to: string
  /** optional vet preference */
  vetId?: string
  /** reason / chief complaint for context */
  reason?: string
  /** duration in minutes (default 30) */
  durationMinutes?: number
}

// ─── API functions ────────────────────────────────────────────────────────────

export async function getWeekSlots(params: {
  weekStart: string // YYYY-MM-DD (Sunday)
  vetId?: string
  durationMinutes?: number
}): Promise<BookingDay[]> {
  const searchParams = new URLSearchParams({ weekStart: params.weekStart })
  if (params.vetId) searchParams.set('vetId', params.vetId)
  if (params.durationMinutes) searchParams.set('durationMinutes', String(params.durationMinutes))
  return apiGet<BookingDay[]>(`/api/v1/booking/slots?${searchParams.toString()}`)
}

export async function suggestSlots(
  params: SuggestSlotsParams
): Promise<SlotSuggestionDto[]> {
  const searchParams = new URLSearchParams({ from: params.from, to: params.to })
  if (params.vetId) searchParams.set('vetId', params.vetId)
  if (params.reason) searchParams.set('reason', params.reason)
  if (params.durationMinutes) searchParams.set('durationMinutes', String(params.durationMinutes))
  return apiGet<SlotSuggestionDto[]>(`/api/v1/booking/suggest?${searchParams.toString()}`)
}
