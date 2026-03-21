import { apiGet, apiPost, apiPatch, apiPut, apiDelete, apiPostFormData } from './client'
import type {
  ConversationDto,
  ConversationWithSuggestionsDto,
  MessageDto,
  ResponseTemplateDto,
  TriageStatsDto,
  MessagingHoursDto,
  SendReplyRequest,
  AddNoteRequest,
  ChangeStatusRequest,
  TransferRequest,
  RecategorizeRequest,
  CreateOutboundConversationRequest,
  CreateTemplateRequest,
  UpdateTemplateRequest,
  MessageCategory,
  ConversationStatus,
  WhatsAppConfigDto,
  UpdateWhatsAppConfigRequest,
  WhatsAppTestResult,
  UploadFilesResponse,
} from './messaging-types'
import type { PagedResult } from './types'

const BASE = '/api/v1/messaging'

// ─── Conversations ────────────────────────────────────────────────────────────

export interface ListConversationsParams {
  status?: ConversationStatus
  category?: MessageCategory
  assignedToUserId?: string
  fromDate?: string
  toDate?: string
  page?: number
  pageSize?: number
}

export function listConversations(
  params: ListConversationsParams = {}
): Promise<PagedResult<ConversationDto>> {
  const qs = new URLSearchParams()
  if (params.status) qs.set('status', params.status)
  if (params.category) qs.set('category', params.category)
  if (params.assignedToUserId) qs.set('assignedToUserId', params.assignedToUserId)
  if (params.fromDate) qs.set('fromDate', params.fromDate)
  if (params.toDate) qs.set('toDate', params.toDate)
  if (params.page) qs.set('page', String(params.page))
  if (params.pageSize) qs.set('pageSize', String(params.pageSize))
  const query = qs.toString()
  return apiGet<PagedResult<ConversationDto>>(
    `${BASE}/conversations${query ? `?${query}` : ''}`
  )
}

export function getConversation(id: string): Promise<ConversationWithSuggestionsDto> {
  return apiGet<ConversationWithSuggestionsDto>(`${BASE}/conversations/${id}`)
}

export function sendReply(id: string, body: SendReplyRequest): Promise<MessageDto> {
  return apiPost<MessageDto>(`${BASE}/conversations/${id}/reply`, body)
}

export function addNote(id: string, body: AddNoteRequest): Promise<MessageDto> {
  return apiPost<MessageDto>(`${BASE}/conversations/${id}/notes`, body)
}

export function changeStatus(id: string, body: ChangeStatusRequest): Promise<ConversationDto> {
  return apiPatch<ConversationDto>(`${BASE}/conversations/${id}/status`, body)
}

export function transferConversation(id: string, body: TransferRequest): Promise<ConversationDto> {
  return apiPatch<ConversationDto>(`${BASE}/conversations/${id}/transfer`, body)
}

export function recategorizeConversation(
  id: string,
  body: RecategorizeRequest
): Promise<ConversationDto> {
  return apiPatch<ConversationDto>(`${BASE}/conversations/${id}/category`, body)
}

export function markAsSpam(id: string): Promise<ConversationDto> {
  return apiPost<ConversationDto>(`${BASE}/conversations/${id}/spam`, {})
}

export function createOutboundConversation(
  body: CreateOutboundConversationRequest
): Promise<ConversationDto> {
  return apiPost<ConversationDto>(`${BASE}/conversations/outbound`, body)
}

export function getConversationSummary(id: string): Promise<{ summary: string }> {
  return apiGet<{ summary: string }>(`${BASE}/conversations/${id}/summary`)
}

// ─── File Upload ─────────────────────────────────────────────────────────────

export function uploadFiles(files: File[]): Promise<UploadFilesResponse> {
  const formData = new FormData()
  files.forEach((file) => formData.append('files', file))
  return apiPostFormData<UploadFilesResponse>(`${BASE}/upload`, formData)
}

// ─── Triage Statistics ────────────────────────────────────────────────────────

export function getTriageStats(): Promise<TriageStatsDto> {
  return apiGet<TriageStatsDto>(`${BASE}/stats`)
}

// ─── Templates ────────────────────────────────────────────────────────────────

export function listTemplates(): Promise<ResponseTemplateDto[]> {
  return apiGet<ResponseTemplateDto[]>(`${BASE}/templates`)
}

export function createTemplate(body: CreateTemplateRequest): Promise<ResponseTemplateDto> {
  return apiPost<ResponseTemplateDto>(`${BASE}/templates`, body)
}

export function updateTemplate(
  id: string,
  body: UpdateTemplateRequest
): Promise<ResponseTemplateDto> {
  return apiPut<ResponseTemplateDto>(`${BASE}/templates/${id}`, body)
}

export function deleteTemplate(id: string): Promise<void> {
  return apiDelete(`${BASE}/templates/${id}`)
}

// ─── Messaging Hours ──────────────────────────────────────────────────────────

export function getMessagingHours(): Promise<MessagingHoursDto[]> {
  return apiGet<MessagingHoursDto[]>(`${BASE}/hours`)
}

export function updateMessagingHours(hours: MessagingHoursDto[]): Promise<MessagingHoursDto[]> {
  return apiPatch<MessagingHoursDto[]>(`${BASE}/hours`, hours)
}

// ─── WhatsApp Configuration ─────────────────────────────────────────────────

export function getWhatsAppConfig(): Promise<WhatsAppConfigDto> {
  return apiGet<WhatsAppConfigDto>(`${BASE}/whatsapp/config`)
}

export function updateWhatsAppConfig(
  data: UpdateWhatsAppConfigRequest
): Promise<WhatsAppConfigDto> {
  return apiPut<WhatsAppConfigDto>(`${BASE}/whatsapp/config`, data)
}

export function testWhatsAppConnection(
  data: UpdateWhatsAppConfigRequest
): Promise<WhatsAppTestResult> {
  return apiPost<WhatsAppTestResult>(`${BASE}/whatsapp/test`, data)
}
