import { apiGet, apiPost } from './client'

export interface ClinicSearchResult {
  id: string
  name: string
  address: string
  emirate: string
  phone: string
  rating: number
  reviewCount: number
}

export interface AskVetRequest {
  vetEmail: string
  ownerName: string
  petName: string
  message?: string
}

export interface AskVetResponse {
  success: boolean
}

const BASE = '/api/v1/pet-owners'

export function searchClinics(query: string): Promise<ClinicSearchResult[]> {
  const params = query ? `?q=${encodeURIComponent(query)}` : ''
  return apiGet<ClinicSearchResult[]>(`${BASE}/clinics${params}`)
}

export function askVetToJoin(data: AskVetRequest): Promise<AskVetResponse> {
  return apiPost<AskVetResponse>(`${BASE}/ask-vet`, data)
}
