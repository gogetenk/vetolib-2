import { apiGet, apiPost, apiPatch, apiPostFormData, apiGetBlob } from './client'

export type Species = 'Dog' | 'Cat' | 'Bird' | 'Rabbit' | 'Horse' | 'Camel' | 'Exotic' | 'Falcon' | 'Reptile'

export interface PatientDto {
  id: string
  name: string
  species: Species
  breed: string
  dateOfBirth: string
  ageYears: number
  gender: 'Male' | 'Female' | 'Unknown'
  weightKg: number | null
  ownerName: string
  ownerPhone: string
  ownerEmail: string
  clinicId: string
  lastVisitDate: string | null
  nextAppointmentDate: string | null
}

export interface CreatePatientRequest {
  name: string
  species: Species
  breed?: string
  dateOfBirth: string
  gender: 'Male' | 'Female' | 'Unknown'
  weightKg?: number | null
  ownerName: string
  ownerPhone: string
  ownerEmail?: string
}

export interface PagedResult<T> {
  items: T[]
  totalCount: number
  page: number
  pageSize: number
}

export interface PatientFilters {
  search?: string
  page?: number
  pageSize?: number
}

export interface VaccinationDto {
  id: string
  name: string
  administeredDate: string
  nextDueDate: string
  vetName: string
}

export interface PrescriptionDto {
  id: string
  medication: string
  dosage: string
  duration: string
  prescribedDate: string
  status: 'active' | 'completed'
  vetName: string
}

export async function getPatients(
  filters?: PatientFilters
): Promise<PagedResult<PatientDto>> {
  const params = new URLSearchParams()
  if (filters?.search) params.set('search', filters.search)
  if (filters?.page) params.set('page', String(filters.page))
  if (filters?.pageSize) params.set('pageSize', String(filters.pageSize))
  const query = params.toString()
  return apiGet<PagedResult<PatientDto>>(`/api/patients${query ? `?${query}` : ''}`)
}

export async function getPatient(id: string): Promise<PatientDto> {
  return apiGet<PatientDto>(`/api/patients/${id}`)
}

export async function getPatientVaccinations(patientId: string): Promise<VaccinationDto[]> {
  return apiGet<VaccinationDto[]>(`/api/patients/${patientId}/vaccinations`)
}

export async function getPatientPrescriptions(patientId: string): Promise<PrescriptionDto[]> {
  return apiGet<PrescriptionDto[]>(`/api/patients/${patientId}/prescriptions`)
}

export async function createPatient(data: CreatePatientRequest): Promise<PatientDto> {
  return apiPost<PatientDto>('/api/patients', data)
}

export async function updatePatient(id: string, data: Partial<CreatePatientRequest>): Promise<PatientDto> {
  return apiPatch<PatientDto>(`/api/patients/${id}`, data)
}

export interface ImportReportDto {
  imported: number
  skipped: number
  errors: string[]
}

export async function importPatientsCsv(file: File): Promise<ImportReportDto> {
  const formData = new FormData()
  formData.append('file', file)
  return apiPostFormData<ImportReportDto>('/api/patients/import', formData)
}

export async function downloadImportTemplate(): Promise<Blob> {
  return apiGetBlob('/api/patients/import/template')
}
