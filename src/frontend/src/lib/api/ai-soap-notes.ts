import { apiPost } from './client'

export interface SoapNotesRequest {
  reason: string
  anamnesis: string
  species?: string
  weight?: number
  temperature?: number
  heartRate?: number
}

export interface SoapNotesResponse {
  subjective: string
  objective: string
  assessment: string
  plan: string
  disclaimer: string
}

export async function generateSoapNotes(request: SoapNotesRequest): Promise<SoapNotesResponse> {
  return apiPost<SoapNotesResponse>('/api/v1/ai/soap-notes', request)
}
