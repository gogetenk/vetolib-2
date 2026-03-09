import { apiGet, apiPost, apiPatch, apiDelete, apiGetBlob } from './client'

export type InvoiceStatus = 'DRAFT' | 'SENT' | 'PAID' | 'CANCELLED'

export interface InvoiceLineItem {
  id: string
  description: string
  quantity: number
  unitPrice: number
  subtotal: number
}

export interface InvoiceDto {
  id: string
  invoiceNumber: string
  patientId: string
  patientName: string
  ownerName: string
  ownerPhone: string
  appointmentId: string | null
  status: InvoiceStatus
  items: InvoiceLineItem[]
  subtotal: number
  vatRate: number
  vatAmount: number
  total: number
  notes: string | null
  createdAt: string
  paidAt: string | null
  dueDate: string | null
  clinicId: string
}

export interface PagedResult<T> {
  items: T[]
  totalCount: number
  page: number
  pageSize: number
}

export interface InvoiceFilters {
  status?: InvoiceStatus
  dateFrom?: string
  dateTo?: string
  page?: number
  pageSize?: number
}

export interface CreateInvoiceLineItem {
  description: string
  quantity: number
  unitPrice: number
}

export interface CreateInvoiceRequest {
  patientId: string
  patientName: string
  ownerName: string
  ownerPhone: string
  appointmentId?: string | null
  items: CreateInvoiceLineItem[]
  notes?: string | null
}

function buildInvoiceUrl(filters?: InvoiceFilters): string {
  const params = new URLSearchParams()
  if (filters?.status) params.set('status', filters.status)
  if (filters?.dateFrom) params.set('dateFrom', filters.dateFrom)
  if (filters?.dateTo) params.set('dateTo', filters.dateTo)
  if (filters?.page) params.set('page', String(filters.page))
  if (filters?.pageSize) params.set('pageSize', String(filters.pageSize))
  const qs = params.toString()
  return qs ? `/api/invoices?${qs}` : '/api/invoices'
}

export async function getInvoices(filters?: InvoiceFilters): Promise<PagedResult<InvoiceDto>> {
  return apiGet<PagedResult<InvoiceDto>>(buildInvoiceUrl(filters))
}

export async function getInvoice(id: string): Promise<InvoiceDto> {
  return apiGet<InvoiceDto>(`/api/invoices/${id}`)
}

export async function createInvoice(data: CreateInvoiceRequest): Promise<InvoiceDto> {
  return apiPost<InvoiceDto>('/api/invoices', data)
}

export async function sendInvoice(id: string): Promise<InvoiceDto> {
  return apiPatch<InvoiceDto>(`/api/invoices/${id}/send`, {})
}

export async function markAsPaid(id: string, paidAt?: string): Promise<InvoiceDto> {
  return apiPatch<InvoiceDto>(`/api/invoices/${id}/pay`, paidAt ? { paidAt } : {})
}

export async function cancelInvoice(id: string): Promise<InvoiceDto> {
  return apiPatch<InvoiceDto>(`/api/invoices/${id}/cancel`, {})
}

export async function deleteInvoice(id: string): Promise<void> {
  return apiDelete(`/api/invoices/${id}`)
}

export async function downloadInvoicePdf(id: string): Promise<Blob> {
  return apiGetBlob(`/api/invoices/${id}/pdf`)
}
