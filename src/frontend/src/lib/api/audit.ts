import { apiGet } from './client'

export interface AuditEntryDto {
  id: string
  timestamp: string
  action: string
  entityType: string
  entityId: string
  changedBy: string
  oldValues: Record<string, unknown> | null
  newValues: Record<string, unknown> | null
}

export interface AuditPagedResult {
  items: AuditEntryDto[]
  totalCount: number
  page: number
  pageSize: number
}

export interface AuditQueryParams {
  page?: number
  pageSize?: number
  startDate?: string
  endDate?: string
  user?: string
  action?: string
}

export async function getAuditEntries(params: AuditQueryParams = {}): Promise<AuditPagedResult> {
  const searchParams = new URLSearchParams()
  if (params.page) searchParams.set('page', String(params.page))
  if (params.pageSize) searchParams.set('pageSize', String(params.pageSize))
  if (params.startDate) searchParams.set('startDate', params.startDate)
  if (params.endDate) searchParams.set('endDate', params.endDate)
  if (params.user) searchParams.set('user', params.user)
  if (params.action) searchParams.set('action', params.action)

  const query = searchParams.toString()
  return apiGet<AuditPagedResult>(`/api/v1/audit${query ? `?${query}` : ''}`)
}
