import { apiGet, apiPost, apiPut } from './client'

// --- Enums / types ---

export type PregnancyStatus = 'Active' | 'Delivered' | 'Lost'
export type HeatCyclePhase = 'Proestrus' | 'Estrus' | 'Diestrus' | 'Anestrus'

// --- Litter ---

export interface LitterDto {
  id: string
  motherId: string
  motherName: string
  fatherId: string | null
  fatherName: string | null
  dateOfBirth: string
  species: string
  breed: string | null
  offspringCount: number
  notes: string | null
  clinicId: string
}

export interface OffspringDto {
  id: string
  litterId: string
  patientId: string | null
  name: string
  sex: 'Male' | 'Female' | 'Unknown'
  colorMarkings: string | null
  weightAtBirthKg: number | null
  status: 'Alive' | 'Deceased' | 'Adopted'
}

export interface CreateLitterRequest {
  motherId: string
  fatherId?: string | null
  dateOfBirth: string
  breed?: string | null
  notes?: string | null
}

export interface AddOffspringRequest {
  name: string
  sex: 'Male' | 'Female' | 'Unknown'
  colorMarkings?: string | null
  weightAtBirthKg?: number | null
}

// --- Lineage ---

export interface LineageDto {
  patientId: string
  patientName: string
  motherId: string | null
  motherName: string | null
  fatherId: string | null
  fatherName: string | null
}

export interface PedigreeNodeDto {
  id: string
  name: string
  species: string
  breed: string | null
  sex: 'Male' | 'Female' | 'Unknown'
  mother: PedigreeNodeDto | null
  father: PedigreeNodeDto | null
}

export interface SetLineageRequest {
  motherId?: string | null
  fatherId?: string | null
}

// --- Pregnancy ---

export interface PregnancyDto {
  id: string
  patientId: string
  patientName: string
  matingDate: string
  expectedDueDate: string
  actualDeliveryDate: string | null
  status: PregnancyStatus
  notes: string | null
  checks: PregnancyCheckDto[]
  clinicId: string
}

export interface PregnancyCheckDto {
  id: string
  pregnancyId: string
  checkDate: string
  notes: string
  result: string
  vetName: string
}

export interface CreatePregnancyRequest {
  patientId: string
  matingDate: string
  expectedDueDate: string
  notes?: string | null
}

export interface RecordDeliveryRequest {
  actualDeliveryDate: string
  notes?: string | null
}

export interface RecordLossRequest {
  lossDate: string
  notes?: string | null
}

export interface CreatePregnancyCheckRequest {
  checkDate: string
  notes: string
  result: string
}

export interface UpdatePregnancyCheckRequest {
  notes?: string
  result?: string
}

// --- Heat Cycle ---

export interface HeatCycleDto {
  id: string
  patientId: string
  patientName: string
  startDate: string
  endDate: string | null
  phase: HeatCyclePhase
  intensity: 'Low' | 'Medium' | 'High'
  notes: string | null
  recordedBy: string
  clinicId: string
}

export interface HeatCyclePredictionDto {
  patientId: string
  predictedNextStartDate: string
  averageCycleDays: number
  confidence: 'Low' | 'Medium' | 'High'
}

export interface CreateHeatCycleRequest {
  patientId: string
  startDate: string
  endDate?: string | null
  phase: HeatCyclePhase
  intensity: 'Low' | 'Medium' | 'High'
  notes?: string | null
}

// --- API functions ---

// Litters
export async function createLitter(data: CreateLitterRequest): Promise<LitterDto> {
  return apiPost<LitterDto>('/api/litters', data)
}

export async function getLitters(patientId?: string): Promise<LitterDto[]> {
  const query = patientId ? `?patientId=${patientId}` : ''
  return apiGet<LitterDto[]>(`/api/litters${query}`)
}

export async function getLitter(id: string): Promise<LitterDto> {
  return apiGet<LitterDto>(`/api/litters/${id}`)
}

export async function getLitterOffspring(litterId: string): Promise<OffspringDto[]> {
  return apiGet<OffspringDto[]>(`/api/litters/${litterId}/offspring`)
}

export async function addOffspring(litterId: string, data: AddOffspringRequest): Promise<OffspringDto> {
  return apiPost<OffspringDto>(`/api/litters/${litterId}/offspring`, data)
}

// Lineage
export async function getLineage(patientId: string): Promise<LineageDto> {
  return apiGet<LineageDto>(`/api/patients/${patientId}/lineage`)
}

export async function setLineage(patientId: string, data: SetLineageRequest): Promise<LineageDto> {
  return apiPut<LineageDto>(`/api/patients/${patientId}/lineage`, data)
}

export async function getPedigree(patientId: string): Promise<PedigreeNodeDto> {
  return apiGet<PedigreeNodeDto>(`/api/patients/${patientId}/pedigree`)
}

export async function getDescendants(patientId: string): Promise<PedigreeNodeDto[]> {
  return apiGet<PedigreeNodeDto[]>(`/api/patients/${patientId}/descendants`)
}

// Pregnancy
export async function createPregnancy(data: CreatePregnancyRequest): Promise<PregnancyDto> {
  return apiPost<PregnancyDto>('/api/pregnancies', data)
}

export async function getPregnancy(id: string): Promise<PregnancyDto> {
  return apiGet<PregnancyDto>(`/api/pregnancies/${id}`)
}

export async function getPregnanciesByPatient(patientId: string): Promise<PregnancyDto[]> {
  return apiGet<PregnancyDto[]>(`/api/pregnancies?patientId=${patientId}`)
}

export async function getActivePregnancies(): Promise<PregnancyDto[]> {
  return apiGet<PregnancyDto[]>('/api/pregnancies/active')
}

export async function recordDelivery(pregnancyId: string, data: RecordDeliveryRequest): Promise<PregnancyDto> {
  return apiPut<PregnancyDto>(`/api/pregnancies/${pregnancyId}/delivery`, data)
}

export async function recordLoss(pregnancyId: string, data: RecordLossRequest): Promise<PregnancyDto> {
  return apiPut<PregnancyDto>(`/api/pregnancies/${pregnancyId}/loss`, data)
}

export async function createPregnancyCheck(pregnancyId: string, data: CreatePregnancyCheckRequest): Promise<PregnancyCheckDto> {
  return apiPost<PregnancyCheckDto>(`/api/pregnancies/${pregnancyId}/checks`, data)
}

export async function updatePregnancyCheck(pregnancyId: string, checkId: string, data: UpdatePregnancyCheckRequest): Promise<PregnancyCheckDto> {
  return apiPut<PregnancyCheckDto>(`/api/pregnancies/${pregnancyId}/checks/${checkId}`, data)
}

// Heat Cycles
export async function createHeatCycle(data: CreateHeatCycleRequest): Promise<HeatCycleDto> {
  return apiPost<HeatCycleDto>('/api/heat-cycles', data)
}

export async function getHeatCycles(patientId?: string): Promise<HeatCycleDto[]> {
  const query = patientId ? `?patientId=${patientId}` : ''
  return apiGet<HeatCycleDto[]>(`/api/heat-cycles${query}`)
}

export async function getHeatCyclePrediction(patientId: string): Promise<HeatCyclePredictionDto> {
  return apiGet<HeatCyclePredictionDto>(`/api/heat-cycles/prediction?patientId=${patientId}`)
}
