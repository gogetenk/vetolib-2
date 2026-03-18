import { apiGet, apiPost } from './client'
import type { DrugCatalogEntryDto, DrugCategory } from './types'

export type { DrugCatalogEntryDto, DrugCategory }

export interface DrugCatalogFilters {
  search?: string
  category?: DrugCategory | 'all'
  species?: string
}

export interface CreateDrugRequest {
  innName: string
  displayName: string
  category: DrugCategory
  commonDosage: string
  requiresPrescription: boolean
  contraindicatedSpecies: { species: string; reason: string }[]
  dosageGuidelines: {
    species: string
    dosePerKg: number
    unit: string
    frequency: string
    maxDurationDays: number | null
  }[]
  interactionSeverity: string | null
}

/**
 * Fetch the full drug catalog with optional filters.
 */
export async function getDrugCatalog(filters?: DrugCatalogFilters): Promise<DrugCatalogEntryDto[]> {
  const params = new URLSearchParams()
  if (filters?.search?.trim()) params.set('search', filters.search.trim())
  if (filters?.category && filters.category !== 'all') params.set('category', filters.category)
  if (filters?.species && filters.species !== 'all') params.set('species', filters.species)
  const query = params.toString()
  return apiGet<DrugCatalogEntryDto[]>(`/api/medical-records/drugs/catalog${query ? `?${query}` : ''}`)
}

/**
 * Search the drug catalog by name (INN or display name).
 * Returns up to 15 results ranked by relevance.
 */
export async function searchDrugs(term: string): Promise<DrugCatalogEntryDto[]> {
  if (!term.trim()) return []
  const params = new URLSearchParams({ search: term.trim() })
  return apiGet<DrugCatalogEntryDto[]>(`/api/medical-records/drugs?${params}`)
}

/**
 * Fetch the full details of a single drug by its catalog ID.
 */
export async function getDrugById(id: string): Promise<DrugCatalogEntryDto> {
  return apiGet<DrugCatalogEntryDto>(`/api/medical-records/drugs/${id}`)
}

/**
 * Add a new drug to the catalog.
 */
export async function createDrug(data: CreateDrugRequest): Promise<DrugCatalogEntryDto> {
  return apiPost<DrugCatalogEntryDto>('/api/medical-records/drugs/catalog', data)
}
