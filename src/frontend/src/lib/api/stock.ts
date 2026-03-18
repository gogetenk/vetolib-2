import { apiGet, apiPost, apiPatch } from './client'

export type StockCategory = 'Medication' | 'Vaccine' | 'Supply'
export type MovementType = 'IN' | 'OUT' | 'ADJUSTMENT'
export type FullMovementType = 'INCOMING' | 'OUTGOING' | 'ADJUSTMENT' | 'LOSS' | 'RETURN'

export interface StockItemDto {
  id: string
  name: string
  category: StockCategory
  quantity: number
  unit: string
  threshold: number
  expiryDate: string | null
  clinicId: string
  isLowStock: boolean
  isExpiringSoon: boolean
}

export interface StockAlertDto {
  id: string
  name: string
  category: StockCategory
  quantity: number
  threshold: number
  expiryDate: string | null
  alertType: 'low-stock' | 'expiring-soon'
}

export interface CreateStockItemRequest {
  name: string
  category: StockCategory
  quantity: number
  unit: string
  threshold: number
  expiryDate?: string
}

export interface UpdateStockItemRequest {
  name?: string
  category?: StockCategory
  quantity?: number
  unit?: string
  threshold?: number
  expiryDate?: string | null
}

export interface CreateStockMovementRequest {
  type: MovementType
  quantity: number
  reason?: string
}

export interface StockMovementDto {
  id: string
  stockItemId: string
  type: MovementType
  quantity: number
  reason: string | null
  newQuantity: number
  createdAt: string
}

export interface StockMovementHistoryDto {
  id: string
  stockItemId: string
  stockItemName: string
  type: FullMovementType
  quantity: number
  previousQuantity: number
  newQuantity: number
  reason: string | null
  performedBy: string
  patientName: string | null
  createdAt: string
}

export interface StockMovementFilters {
  type?: FullMovementType
  stockItemId?: string
  dateFrom?: string
  dateTo?: string
  search?: string
}

export interface StockFilters {
  category?: StockCategory
  status?: 'low-stock' | 'expiring-soon'
}

export async function getStockItems(filters?: StockFilters): Promise<StockItemDto[]> {
  const params = new URLSearchParams()
  if (filters?.category) params.set('category', filters.category)
  if (filters?.status) params.set('status', filters.status)
  const query = params.toString()
  return apiGet<StockItemDto[]>(`/api/v1/stock${query ? `?${query}` : ''}`)
}

export async function getStockAlerts(): Promise<StockAlertDto[]> {
  return apiGet<StockAlertDto[]>('/api/v1/stock/alerts')
}

export async function createStockItem(data: CreateStockItemRequest): Promise<StockItemDto> {
  return apiPost<StockItemDto>('/api/v1/stock', data)
}

export async function updateStockItem(id: string, data: UpdateStockItemRequest): Promise<StockItemDto> {
  return apiPatch<StockItemDto>(`/api/v1/stock/${id}`, data)
}

export async function getStockMovements(filters?: StockMovementFilters): Promise<StockMovementHistoryDto[]> {
  const params = new URLSearchParams()
  if (filters?.type) params.set('type', filters.type)
  if (filters?.stockItemId) params.set('stockItemId', filters.stockItemId)
  if (filters?.dateFrom) params.set('dateFrom', filters.dateFrom)
  if (filters?.dateTo) params.set('dateTo', filters.dateTo)
  if (filters?.search) params.set('search', filters.search)
  const query = params.toString()
  return apiGet<StockMovementHistoryDto[]>(`/api/v1/stock/movements${query ? `?${query}` : ''}`)
}

export async function createStockMovement(
  stockItemId: string,
  data: CreateStockMovementRequest
): Promise<StockMovementDto> {
  return apiPost<StockMovementDto>(`/api/v1/stock/${stockItemId}/movements`, data)
}
