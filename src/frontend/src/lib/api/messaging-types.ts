/**
 * Messaging module TypeScript types.
 * These match the Vetolib.Messaging.Contracts DTOs.
 */

// ─── Enums ──────────────────────────────────────────────────────────────────

export type MessageCategory =
  | 'MedicalUrgency'
  | 'PostOperativeFollowUp'
  | 'MedicalQuestion'
  | 'AppointmentRequest'
  | 'Administrative'
  | 'Feedback'
  | 'Other'

export type ConversationStatus = 'Open' | 'InProgress' | 'Resolved' | 'Closed'

export type ConversationPriority = 'Critical' | 'High' | 'Normal' | 'Low'

export type MessageSender = 'Owner' | 'Vet' | 'Staff' | 'System'

// ─── Core entities ───────────────────────────────────────────────────────────

export interface MessageAttachmentDto {
  id: string
  fileName: string
  contentType: string
  fileSizeBytes: number
  url: string
}

export interface MessageDto {
  id: string
  conversationId: string
  sender: MessageSender
  senderUserId: string | null
  senderName: string | null
  body: string
  isInternalNote: boolean
  sentAt: string
  attachments: MessageAttachmentDto[]
}

export interface ConversationDto {
  id: string
  clinicId: string
  ownerId: string
  ownerName: string
  ownerEmail: string
  patientId: string | null
  patientName: string | null
  subject: string
  category: MessageCategory
  priority: ConversationPriority
  status: ConversationStatus
  assignedToUserId: string | null
  assignedToUserName: string | null
  assignedToRole: string | null
  aiTriageConfidence: number | null
  isTriageUncertain: boolean
  isSpam: boolean
  unreadCount: number
  lastMessageAt: string
  createdAt: string
  messages?: MessageDto[]
}

export interface AiSuggestion {
  text: string
  language: 'en' | 'ar'
}

export interface ConversationWithSuggestionsDto extends ConversationDto {
  aiSuggestions: AiSuggestion[]
  aiSummary: string | null
}

// ─── Response Templates ──────────────────────────────────────────────────────

export interface ResponseTemplateDto {
  id: string
  clinicId: string
  name: string
  contentEn: string
  contentAr: string
  category: MessageCategory | null
  createdAt: string
  updatedAt: string
}

// ─── Triage Statistics ───────────────────────────────────────────────────────

export interface CategoryStat {
  category: MessageCategory
  count: number
  percentage: number
}

export interface DailyVolumeStat {
  date: string
  count: number
}

export interface TriageStatsDto {
  averageFirstResponseMinutes: number
  totalConversations: number
  openConversations: number
  byCategory: CategoryStat[]
  aiAccuracyPercent: number
  conversionToAppointmentPercent: number
  dailyVolume: DailyVolumeStat[]
}

// ─── Messaging Hours ─────────────────────────────────────────────────────────

export interface MessagingHoursDto {
  dayOfWeek: number // 0 = Sunday ... 6 = Saturday
  openTime: string  // "HH:mm"
  closeTime: string // "HH:mm"
  isClosed: boolean
}

// ─── Patient Context ─────────────────────────────────────────────────────────

export interface PatientContextDto {
  patientId: string
  patientName: string
  species: string
  breed: string
  ageYears: number
  lastExaminationDate: string | null
  currentPrescriptions: string[]
  knownAllergies: string[]
  vaccinationHistory: string[]
  outstandingInvoicesAed: number
}

// ─── Request types ───────────────────────────────────────────────────────────

export interface UploadedAttachmentDto {
  id: string
  fileName: string
  contentType: string
  fileSizeBytes: number
  url: string
}

export interface UploadFilesResponse {
  attachments: UploadedAttachmentDto[]
}

export interface SendReplyRequest {
  body: string
  attachmentIds?: string[]
}

export interface AddNoteRequest {
  body: string
}

export interface ChangeStatusRequest {
  status: ConversationStatus
}

export interface TransferRequest {
  toRole: string
  toUserId?: string | null
}

export interface RecategorizeRequest {
  category: MessageCategory
}

export interface CreateOutboundConversationRequest {
  ownerId: string
  ownerName: string
  patientId: string | null
  subject: string
  body: string
}

export interface CreateTemplateRequest {
  name: string
  contentEn: string
  contentAr: string
  category: MessageCategory | null
}

export interface UpdateTemplateRequest {
  name: string
  contentEn: string
  contentAr: string
  category: MessageCategory | null
}

// ─── Portal types ────────────────────────────────────────────────────────────

export interface PortalPetDto {
  id: string
  name: string
  species: string
  breed: string
  ageYears: number
}

export interface PortalConversationDto {
  id: string
  subject: string
  category: MessageCategory
  status: ConversationStatus
  lastMessageAt: string
  createdAt: string
  unreadByOwnerCount: number
  messages?: PortalMessageDto[]
}

export interface PortalMessageDto {
  id: string
  sender: MessageSender
  senderName: string | null
  body: string
  sentAt: string
  attachments: MessageAttachmentDto[]
}

export interface CreatePortalConversationRequest {
  petId: string | null
  subject: string
  category: MessageCategory
  body: string
}

export interface SendPortalMessageRequest {
  body: string
}

export interface ConsentRequest {
  consentVersion: string
}

export interface ConsentResponse {
  acceptedAt: string
  version: string
}

// ─── WhatsApp Configuration ─────────────────────────────────────────────────

export interface WhatsAppConfigDto {
  enabled: boolean
  businessAccountId: string
  phoneNumberId: string
  accessToken: string
  optInCount: number
}

export interface UpdateWhatsAppConfigRequest {
  enabled: boolean
  businessAccountId: string
  phoneNumberId: string
  accessToken: string
}

export interface WhatsAppTestResult {
  success: boolean
  message: string
}
