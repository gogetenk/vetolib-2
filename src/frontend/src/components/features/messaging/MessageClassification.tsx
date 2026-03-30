'use client'

import { useState, useCallback } from 'react'
import { useTranslations } from 'next-intl'
import { ThumbsUp, ThumbsDown, MoreHorizontal, AlertTriangle } from 'lucide-react'
import { Badge } from '@/components/ui/badge'
import {
  Select,
  SelectContent,
  SelectItem,
  SelectTrigger,
  SelectValue,
} from '@/components/ui/select'
import { cn } from '@/lib/utils'
import type {
  MessageClassificationDto,
  ClassificationUrgency,
  ClassificationCategory,
} from '@/lib/api/messaging-types'
import { classifyMessage, sendClassificationFeedback } from '@/lib/api/messaging'

// ─── Urgency badge colors ─────────────────────────────────────────────────────

const URGENCY_STYLES: Record<ClassificationUrgency, string> = {
  Critical: 'bg-red-100 text-red-800 border-red-200',
  High: 'bg-orange-100 text-orange-800 border-orange-200',
  Normal: 'bg-muted text-muted-foreground border-border',
  Low: 'bg-green-100 text-green-800 border-green-200',
}

// ─── Classification override dropdown ─────────────────────────────────────────

const URGENCY_OPTIONS: ClassificationUrgency[] = ['Critical', 'High', 'Normal', 'Low']
const CATEGORY_OPTIONS: ClassificationCategory[] = [
  'MedicalUrgency',
  'PostOperativeFollowUp',
  'MedicalQuestion',
  'AppointmentRequest',
  'Administrative',
  'Feedback',
  'Other',
]

interface MessageClassificationProps {
  messageId: string
  conversationId: string
  classification: MessageClassificationDto
  align: 'left' | 'right'
  onClassificationUpdate?: (updated: MessageClassificationDto) => void
}

export function MessageClassificationBadges({
  messageId,
  conversationId,
  classification,
  align,
  onClassificationUpdate,
}: MessageClassificationProps) {
  const t = useTranslations('messaging.classification')
  const tCat = useTranslations('messaging.category')
  const [showOverride, setShowOverride] = useState(false)
  const [feedbackGiven, setFeedbackGiven] = useState<'up' | 'down' | null>(null)
  const [isSubmitting, setIsSubmitting] = useState(false)

  const [overrideUrgency, setOverrideUrgency] = useState<ClassificationUrgency>(classification.urgency)
  const [overrideCategory, setOverrideCategory] = useState<ClassificationCategory>(classification.category)

  const needsReview = classification.confidence < 0.7

  const handleFeedback = useCallback(async (isAccurate: boolean) => {
    if (feedbackGiven) return
    try {
      await sendClassificationFeedback(conversationId, messageId, { isAccurate })
      setFeedbackGiven(isAccurate ? 'up' : 'down')
    } catch {
      // silently fail for feedback
    }
  }, [conversationId, messageId, feedbackGiven])

  const handleOverrideSubmit = useCallback(async () => {
    setIsSubmitting(true)
    try {
      const updated = await classifyMessage(conversationId, messageId, {
        urgency: overrideUrgency,
        category: overrideCategory,
      })
      if (updated.classification && onClassificationUpdate) {
        onClassificationUpdate(updated.classification)
      }
      setShowOverride(false)
    } catch {
      // silently fail
    } finally {
      setIsSubmitting(false)
    }
  }, [conversationId, messageId, overrideUrgency, overrideCategory, onClassificationUpdate])

  return (
    <div
      className={cn('flex flex-wrap items-center gap-1 mt-1', align === 'right' ? 'justify-end' : 'justify-start')}
      data-testid={`classification-${messageId}`}
    >
      {/* Urgency badge */}
      <Badge
        variant="outline"
        className={cn('text-[10px] h-4 px-1.5 border', URGENCY_STYLES[classification.urgency])}
        data-testid={`classification-urgency-${messageId}`}
      >
        {t(`urgency.${classification.urgency}`)}
      </Badge>

      {/* Category badge */}
      <Badge
        variant="outline"
        className="text-[10px] h-4 px-1.5 border-border text-muted-foreground"
        data-testid={`classification-category-${messageId}`}
      >
        {tCat(classification.category)}
      </Badge>

      {/* Needs review badge */}
      {needsReview && (
        <Badge
          variant="outline"
          className="text-[10px] h-4 px-1.5 border-amber-300 bg-amber-50 text-amber-700"
          data-testid={`classification-needs-review-${messageId}`}
        >
          <AlertTriangle className="h-2.5 w-2.5 me-0.5" aria-hidden />
          {t('needs_review')}
        </Badge>
      )}

      {/* Feedback buttons */}
      <div className="flex items-center gap-0.5 ms-1">
        <button
          type="button"
          className={cn(
            'p-0.5 rounded transition-colors',
            feedbackGiven === 'up'
              ? 'text-green-600'
              : 'text-muted-foreground/50 hover:text-green-600'
          )}
          onClick={() => handleFeedback(true)}
          disabled={feedbackGiven !== null}
          data-testid={`classification-feedback-up-${messageId}`}
          aria-label={t('feedback_accurate')}
        >
          <ThumbsUp className="h-3 w-3" />
        </button>
        <button
          type="button"
          className={cn(
            'p-0.5 rounded transition-colors',
            feedbackGiven === 'down'
              ? 'text-red-600'
              : 'text-muted-foreground/50 hover:text-red-600'
          )}
          onClick={() => handleFeedback(false)}
          disabled={feedbackGiven !== null}
          data-testid={`classification-feedback-down-${messageId}`}
          aria-label={t('feedback_inaccurate')}
        >
          <ThumbsDown className="h-3 w-3" />
        </button>
      </div>

      {/* Override button */}
      <div className="relative">
        <button
          type="button"
          className="p-0.5 rounded text-muted-foreground/50 hover:text-foreground transition-colors"
          onClick={() => setShowOverride(!showOverride)}
          data-testid={`classification-override-btn-${messageId}`}
          aria-label={t('override')}
        >
          <MoreHorizontal className="h-3 w-3" />
        </button>

        {/* Override dropdown */}
        {showOverride && (
          <div
            className={cn(
              'absolute z-50 mt-1 w-56 bg-card border border-border rounded-lg shadow-lg p-3 space-y-3',
              align === 'right' ? 'right-0' : 'left-0'
            )}
            data-testid={`classification-override-panel-${messageId}`}
          >
            <div>
              <label className="text-[11px] font-medium text-muted-foreground block mb-1">
                {t('urgency_label')}
              </label>
              <div data-testid={`classification-override-urgency-${messageId}`}>
                <Select
                  value={overrideUrgency}
                  onValueChange={(v) => setOverrideUrgency(v as ClassificationUrgency)}
                >
                  <SelectTrigger className="w-full text-xs" size="sm">
                    <SelectValue>{t(`urgency.${overrideUrgency}`)}</SelectValue>
                  </SelectTrigger>
                  <SelectContent>
                    {URGENCY_OPTIONS.map((u) => (
                      <SelectItem key={u} value={u}>{t(`urgency.${u}`)}</SelectItem>
                    ))}
                  </SelectContent>
                </Select>
              </div>
            </div>

            <div>
              <label className="text-[11px] font-medium text-muted-foreground block mb-1">
                {t('category_label')}
              </label>
              <div data-testid={`classification-override-category-${messageId}`}>
                <Select
                  value={overrideCategory}
                  onValueChange={(v) => setOverrideCategory(v as ClassificationCategory)}
                >
                  <SelectTrigger className="w-full text-xs" size="sm">
                    <SelectValue>{tCat(overrideCategory)}</SelectValue>
                  </SelectTrigger>
                  <SelectContent>
                    {CATEGORY_OPTIONS.map((c) => (
                      <SelectItem key={c} value={c}>{tCat(c)}</SelectItem>
                    ))}
                  </SelectContent>
                </Select>
              </div>
            </div>

            <div className="flex justify-end gap-2">
              <button
                type="button"
                className="text-[11px] px-2 py-1 rounded text-muted-foreground hover:bg-muted transition-colors"
                onClick={() => setShowOverride(false)}
                data-testid={`classification-override-cancel-${messageId}`}
              >
                {t('cancel')}
              </button>
              <button
                type="button"
                className="text-[11px] px-2 py-1 rounded bg-primary text-white hover:bg-primary/90 transition-colors disabled:opacity-50"
                onClick={handleOverrideSubmit}
                disabled={isSubmitting}
                data-testid={`classification-override-submit-${messageId}`}
              >
                {isSubmitting ? t('saving') : t('save')}
              </button>
            </div>
          </div>
        )}
      </div>

      {/* Overridden indicator */}
      {classification.overriddenByUserId && (
        <span
          className="text-[9px] text-muted-foreground/60 italic"
          data-testid={`classification-overridden-${messageId}`}
        >
          {t('overridden')}
        </span>
      )}
    </div>
  )
}
