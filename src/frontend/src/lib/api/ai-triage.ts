import { apiPost } from './client'

export type TriageSeverity = 'Emergency' | 'Normal' | 'Routine'

export interface TriageRequest {
  symptoms: string
  species?: string
}

export interface TriageResponse {
  triageId: string
  severity: TriageSeverity
  estimatedDurationMinutes: number
  recommendedSpecialty: string
  reasoning: string
  disclaimer: string
  confidence: number
}

export async function analyzeTriage(request: TriageRequest): Promise<TriageResponse> {
  return apiPost<TriageResponse>('/api/ai/triage', request)
}
