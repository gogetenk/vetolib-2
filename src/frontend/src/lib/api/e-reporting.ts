import { apiGet, apiPost } from './client'

export type ReportStatus = 'DRAFT' | 'SUBMITTED' | 'ACCEPTED' | 'REJECTED'

export interface ReportLineItem {
  id: string
  description: string
  category: string
  quantity: number
  unitPrice: number
  vatAmount: number
  total: number
}

export interface ReportingPeriodDto {
  id: string
  month: number
  year: number
  status: ReportStatus
  totalRevenue: number
  totalVat: number
  invoiceCount: number
  submittedAt: string | null
  acceptedAt: string | null
  rejectedAt: string | null
  rejectionReason: string | null
  lineItems: ReportLineItem[]
  clinicId: string
  createdAt: string
}

export interface ReportingPeriodFilters {
  year?: number
  status?: ReportStatus
}

function buildReportingUrl(filters?: ReportingPeriodFilters): string {
  const params = new URLSearchParams()
  if (filters?.year) params.set('year', String(filters.year))
  if (filters?.status) params.set('status', filters.status)
  const qs = params.toString()
  return qs ? `/api/e-reporting?${qs}` : '/api/e-reporting'
}

export async function getReportingPeriods(filters?: ReportingPeriodFilters): Promise<ReportingPeriodDto[]> {
  return apiGet<ReportingPeriodDto[]>(buildReportingUrl(filters))
}

export async function getReportingPeriod(id: string): Promise<ReportingPeriodDto> {
  return apiGet<ReportingPeriodDto>(`/api/e-reporting/${id}`)
}

export async function submitReport(id: string): Promise<ReportingPeriodDto> {
  return apiPost<ReportingPeriodDto>(`/api/e-reporting/${id}/submit`, {})
}
