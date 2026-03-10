/**
 * Shared API types used across multiple modules.
 * Import from here rather than defining locally.
 */

export interface PagedResult<T> {
  items: T[]
  totalCount: number
  page: number
  pageSize: number
}

export type Species = 'Dog' | 'Cat' | 'Bird' | 'Rabbit' | 'Horse' | 'Camel' | 'Exotic'

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

export interface PrescriptionPreflightResult {
  interactionAlerts: InteractionAlert[]
  stockAvailability: boolean
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
