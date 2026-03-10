import { apiGet } from './client'
import type { DrugCatalogEntryDto } from './types'

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
