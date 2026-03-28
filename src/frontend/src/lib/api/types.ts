/**
 * Shared API types used across multiple modules.
 * Import from here rather than defining locally.
 */

// ─── Onboarding ───────────────────────────────────────────────────────────────

export interface OnboardingStepDto {
  id: string
  title: string
  completed: boolean
  order: number
}

export interface OnboardingProgressDto {
  totalSteps: number
  completedSteps: number
  percentComplete: number
}

export interface OnboardingStateDto {
  welcomeBannerVisible: boolean
  checklistVisible: boolean
  progress: OnboardingProgressDto
  steps: OnboardingStepDto[]
}

export interface PagedResult<T> {
  items: T[]
  totalCount: number
  page: number
  pageSize: number
}

export type Species = 'Dog' | 'Cat' | 'Bird' | 'Rabbit' | 'Horse' | 'Camel' | 'Exotic' | 'Falcon' | 'Reptile'

// ─── Drug Catalog ────────────────────────────────────────────────────────────

export type DrugCategory =
  | 'Antibiotic'
  | 'Antiparasitic'
  | 'AntiInflammatory'
  | 'Analgesic'
  | 'Vaccine'
  | 'Antifungal'
  | 'Cardiac'
  | 'Dermatological'
  | 'Ophthalmic'
  | 'Hormonal'
  | 'Other'

export type InteractionSeverity = 'Low' | 'Moderate' | 'High' | 'Contraindicated'

export interface SpeciesContraindicationDto {
  species: Species
  reason: string
}

export interface DosageGuidelineDto {
  species: Species
  dosePerKg: number
  unit: string
  frequency: string
  maxDurationDays: number | null
}

export interface DrugCatalogEntryDto {
  id: string
  innName: string
  displayName: string
  category: DrugCategory
  commonDosage: string
  contraindicatedSpecies: SpeciesContraindicationDto[]
  dosageGuidelines: DosageGuidelineDto[]
  interactionSeverity: InteractionSeverity | null
  requiresPrescription: boolean
}

// ─── Prescription Preflight ───────────────────────────────────────────────────

export type AlertSeverity = 'Critical' | 'Moderate' | 'Info'
export type AlertType = 'SpeciesContraindication' | 'DrugInteraction' | 'DosageOutOfRange' | 'StockWarning'

export interface InteractionAlert {
  severity: AlertSeverity
  type: AlertType
  message: string
  alternativeDrugIds: string[]
}

export interface SafeAlternative {
  id: string
  displayName: string
  innName: string
  commonDosage: string
}

export interface DosageRange {
  minDose: number
  maxDose: number
  unit: string
  dosePerKg: number
  recommendedDose?: number
}

export interface StockAlternativeDto {
  stockItemId: string
  name: string
  drugCatalogEntryId: string | null
  quantity: number
  unit: string
}

export interface StockAvailabilityResult {
  available: boolean
  quantity: number
  unit: string
  isLowStock: boolean
  isExpiringSoon: boolean
  alternatives: StockAlternativeDto[]
}

export interface PrescriptionPreflightResult {
  interactionAlerts: InteractionAlert[]
  stockAvailability: boolean | StockAvailabilityResult
  safeAlternatives: SafeAlternative[]
  dosageRange: DosageRange | null
}

export interface PreflightRequest {
  patientId: string
  drugCatalogEntryId: string
  dosageAmount?: number
  patientWeightKg?: number
  patientSpecies?: Species
}
