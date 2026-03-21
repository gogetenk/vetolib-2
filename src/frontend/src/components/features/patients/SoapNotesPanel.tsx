'use client'

import { useState } from 'react'
import { useTranslations } from 'next-intl'
import { Button } from '@/components/ui/button'
import { Textarea } from '@/components/ui/textarea'
import { Label } from '@/components/ui/label'
import { Badge } from '@/components/ui/badge'
import { Card, CardContent, CardHeader, CardTitle } from '@/components/ui/card'
import { generateSoapNotes } from '@/lib/api/ai-soap-notes'
import type { SoapNotesRequest, SoapNotesResponse } from '@/lib/api/ai-soap-notes'

// ── Types ──────────────────────────────────────────────────────────────────────

export interface SoapNotesValues {
  subjective: string
  objective: string
  assessment: string
  plan: string
}

interface SoapNotesPanelProps {
  /** Data to send to the AI endpoint */
  requestData: SoapNotesRequest
  /** Called when user accepts the SOAP notes */
  onAccept: (values: SoapNotesValues) => void
  /** Whether the panel should be visible */
  open: boolean
}

// ── Component ──────────────────────────────────────────────────────────────────

export function SoapNotesPanel({ requestData, onAccept, open }: SoapNotesPanelProps) {
  const t = useTranslations('soap_notes')
  const [loading, setLoading] = useState(false)
  const [error, setError] = useState<string | null>(null)
  const [generated, setGenerated] = useState(false)
  const [disclaimer, setDisclaimer] = useState<string | null>(null)
  const [values, setValues] = useState<SoapNotesValues>({
    subjective: '',
    objective: '',
    assessment: '',
    plan: '',
  })

  if (!open) return null

  const handleGenerate = async () => {
    setLoading(true)
    setError(null)
    try {
      const response: SoapNotesResponse = await generateSoapNotes(requestData)
      setValues({
        subjective: response.subjective,
        objective: response.objective,
        assessment: response.assessment,
        plan: response.plan,
      })
      setDisclaimer(response.disclaimer)
      setGenerated(true)
    } catch {
      setError(t('error'))
    } finally {
      setLoading(false)
    }
  }

  const handleFieldChange = (field: keyof SoapNotesValues, value: string) => {
    setValues(prev => ({ ...prev, [field]: value }))
  }

  const handleAccept = () => {
    onAccept(values)
  }

  const sections: { key: keyof SoapNotesValues; label: string; rows: number }[] = [
    { key: 'subjective', label: t('subjective'), rows: 4 },
    { key: 'objective', label: t('objective'), rows: 4 },
    { key: 'assessment', label: t('assessment'), rows: 3 },
    { key: 'plan', label: t('plan'), rows: 4 },
  ]

  return (
    <Card
      className="bg-blue-50/50 border-blue-200/60 rounded-xl shadow-sm"
      data-testid="soap-notes-panel"
    >
      <CardHeader className="pb-3">
        <div className="flex items-center gap-2">
          <CardTitle className="text-[15px] font-bold text-foreground">
            {t('title')}
          </CardTitle>
          {generated && (
            <Badge
              variant="secondary"
              className="bg-blue-100 text-blue-700 text-[11px]"
              data-testid="soap-ai-badge"
            >
              {t('ai_badge')}
            </Badge>
          )}
        </div>
      </CardHeader>
      <CardContent className="space-y-4">
        {!generated && !loading && (
          <div className="text-center py-4">
            <p className="text-sm text-muted-foreground mb-3">
              {t('description')}
            </p>
            <Button
              type="button"
              onClick={handleGenerate}
              disabled={loading}
              data-testid="generate-soap-btn"
              className="bg-blue-600 hover:bg-blue-700 text-white font-semibold rounded-xl shadow-sm"
            >
              {t('generate')}
            </Button>
          </div>
        )}

        {loading && (
          <div className="flex items-center justify-center py-8" data-testid="soap-loading">
            <div className="flex items-center gap-3">
              <div className="h-5 w-5 animate-spin rounded-full border-2 border-blue-600 border-t-transparent" />
              <span className="text-sm text-muted-foreground">{t('generating')}</span>
            </div>
          </div>
        )}

        {error && (
          <p
            className="text-sm text-destructive text-center py-2"
            data-testid="soap-error"
            role="alert"
          >
            {error}
          </p>
        )}

        {generated && !loading && (
          <>
            {sections.map(({ key, label, rows }) => (
              <div key={key} className="space-y-1.5">
                <Label
                  htmlFor={`soap-${key}`}
                  className="text-[13px] font-semibold text-foreground"
                >
                  {label}
                </Label>
                <Textarea
                  id={`soap-${key}`}
                  rows={rows}
                  value={values[key]}
                  onChange={(e) => handleFieldChange(key, e.target.value)}
                  data-testid={`soap-${key}`}
                  className="rounded-xl border-blue-200/80 bg-white text-[13px] focus:ring-2 focus:ring-blue-500/20 focus:border-blue-500/50"
                />
              </div>
            ))}

            {disclaimer && (
              <p
                className="text-xs text-muted-foreground italic bg-amber-50 border border-amber-200/60 rounded-lg px-3 py-2"
                data-testid="soap-disclaimer"
              >
                {disclaimer}
              </p>
            )}

            <div className="flex gap-2 justify-end pt-2">
              <Button
                type="button"
                variant="outline"
                onClick={handleGenerate}
                disabled={loading}
                data-testid="regenerate-soap-btn"
                className="rounded-xl font-semibold border-blue-200 hover:bg-blue-50 text-blue-700"
              >
                {t('regenerate')}
              </Button>
              <Button
                type="button"
                onClick={handleAccept}
                data-testid="accept-soap-btn"
                className="bg-blue-600 hover:bg-blue-700 text-white font-semibold rounded-xl shadow-sm"
              >
                {t('accept')}
              </Button>
            </div>
          </>
        )}
      </CardContent>
    </Card>
  )
}
