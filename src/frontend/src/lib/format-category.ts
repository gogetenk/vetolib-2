import type { MessageCategory } from '@/lib/api/messaging-types'

/**
 * Maps PascalCase enum values to human-readable labels.
 */
const CATEGORY_LABELS: Record<MessageCategory, string> = {
  MedicalUrgency: 'Medical Urgency',
  PostOperativeFollowUp: 'Post-Operative Follow-Up',
  MedicalQuestion: 'Medical Question',
  AppointmentRequest: 'Appointment Request',
  Administrative: 'Administrative',
  Feedback: 'Feedback',
  Other: 'Other',
}

export function formatCategory(category: MessageCategory | null | undefined): string {
  if (!category) return ''
  return CATEGORY_LABELS[category] ?? category
}
