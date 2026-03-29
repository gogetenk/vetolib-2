import { apiGet, apiPost } from './client'

export interface WeightEntryDto {
  id: string
  patientId: string
  weightKg: number
  recordedAt: string
  note: string | null
  recordedBy: string
}

export interface WeightCurvePointDto {
  date: string
  weightKg: number
}

export interface CreateWeightRequest {
  weightKg: number
  recordedAt: string
  note?: string | null
}

export async function getPatientWeights(patientId: string): Promise<WeightEntryDto[]> {
  return apiGet<WeightEntryDto[]>(`/api/patients/${patientId}/weights`)
}

export async function getPatientWeightCurve(patientId: string): Promise<WeightCurvePointDto[]> {
  return apiGet<WeightCurvePointDto[]>(`/api/patients/${patientId}/weights/curve`)
}

export async function addPatientWeight(
  patientId: string,
  data: CreateWeightRequest
): Promise<WeightEntryDto> {
  return apiPost<WeightEntryDto>(`/api/patients/${patientId}/weights`, data)
}
