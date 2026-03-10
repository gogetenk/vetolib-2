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
