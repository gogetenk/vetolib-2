import { apiGet, apiPost } from './client'
import type { PagedResult } from './patients'

export interface MedicalRecordDto {
  id: string
  patientId: string
  patientName: string
  vetName: string
  visitDate: string
  reason: string
  anamnesis: string
  weight: number
  temperature: number
  heartRate: number
  diagnosis: string
  treatment: string
  prescription: string | null
  nextVisitDate: string | null
  clinicId: string
}

export interface CreateMedicalRecordRequest {
  reason: string
  anamnesis: string
  weight: number
  temperature: number
  heartRate: number
  diagnosis: string
  treatment: string
  prescription?: string
  nextVisitDate?: string
}

export async function getPatientMedicalRecords(
  patientId: string
): Promise<PagedResult<MedicalRecordDto>> {
  return apiGet<PagedResult<MedicalRecordDto>>(`/api/patients/${patientId}/medical-records`)
}

export async function createMedicalRecord(
  patientId: string,
  data: CreateMedicalRecordRequest
): Promise<MedicalRecordDto> {
  return apiPost<MedicalRecordDto>(`/api/patients/${patientId}/medical-records`, data)
}
