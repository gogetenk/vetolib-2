'use client'

import { useState, useEffect } from 'react'
import { useTranslations } from 'next-intl'
import { useRouter, useParams } from 'next/navigation'
import { Button } from '@/components/ui/button'
import { Input } from '@/components/ui/input'
import { PetSelector } from './PetSelector'
import { CategorySelector } from './CategorySelector'
import { PhotoUpload, type PhotoFile } from './PhotoUpload'
import { listPortalPets, createPortalConversation } from '@/lib/api/portal'
import { ApiError } from '@/lib/api/client'
import type { PortalPetDto, MessageCategory } from '@/lib/api/messaging-types'
import { CheckCircle } from 'lucide-react'

const MAX_CHARS = 2000

export function NewMessageForm() {
  const t = useTranslations('portal.new_message')
  const router = useRouter()
  const params = useParams<{ locale: string; clinicSlug: string }>()

  const [pets, setPets] = useState<PortalPetDto[]>([])
  const [selectedPetId, setSelectedPetId] = useState<string | null>(null)
  const [category, setCategory] = useState<MessageCategory | null>(null)
  const [subject, setSubject] = useState('')
  const [messageBody, setMessageBody] = useState('')
  const [photos, setPhotos] = useState<PhotoFile[]>([])
  const [photoError, setPhotoError] = useState<string | null>(null)
  const [categoryError, setCategoryError] = useState<string | null>(null)
  const [subjectError, setSubjectError] = useState<string | null>(null)
  const [messageError, setMessageError] = useState<string | null>(null)
  const [submitError, setSubmitError] = useState<string | null>(null)
  const [isSubmitting, setIsSubmitting] = useState(false)
  const [success, setSuccess] = useState(false)

  useEffect(() => {
    listPortalPets().then(setPets).catch(() => setPets([]))
  }, [])

  const charsRemaining = MAX_CHARS - messageBody.length
  const isOverLimit = charsRemaining < 0

  function handlePhotosChange(newPhotos: PhotoFile[]) {
    setPhotos(newPhotos)
  }

  function handlePhotoError(err: string | null) {
    setPhotoError(err)
  }

  function validate(): boolean {
    let valid = true
    if (!subject.trim()) {
      setSubjectError(t('errors.subject_required'))
      valid = false
    } else {
      setSubjectError(null)
    }
    if (!category) {
      setCategoryError(t('errors.category_required'))
      valid = false
    } else {
      setCategoryError(null)
    }
    if (!messageBody.trim()) {
      setMessageError(t('errors.message_required'))
      valid = false
    } else {
      setMessageError(null)
    }
    return valid
  }

  async function handleSend() {
    if (!validate()) return
    setIsSubmitting(true)
    setSubmitError(null)
    try {
      await createPortalConversation({
        petId: selectedPetId,
        subject: subject.trim(),
        category: category!,
        body: messageBody,
      })
      setSuccess(true)
    } catch (err) {
      if (err instanceof ApiError && err.status === 429) {
        setSubmitError(t('errors.too_many_messages'))
      } else {
        setSubmitError(t('errors.send_failed'))
      }
    } finally {
      setIsSubmitting(false)
    }
  }

  if (success) {
    return (
      <div className="flex flex-col items-center text-center gap-4 py-12" data-testid="message-success">
        <CheckCircle className="w-14 h-14 text-primary" />
        <h2 className="text-lg font-semibold text-foreground" data-testid="success-title">
          {t('success_title')}
        </h2>
        <p className="text-sm text-muted-foreground max-w-sm" data-testid="success-body">
          {t('success_body')}
        </p>
        <Button
          onClick={() => router.push(`/${params.locale}/portal/${params.clinicSlug}`)}
          data-testid="back-to-conversations-btn"
          className="bg-primary hover:bg-primary/90 text-white"
        >
          {t('back_to_conversations')}
        </Button>
      </div>
    )
  }

  return (
    <div className="space-y-5" data-testid="new-message-form">
      <h1 className="text-xl font-bold text-foreground" data-testid="new-message-title">
        {t('title')}
      </h1>

      {/* Pet selector */}
      <PetSelector
        pets={pets}
        value={selectedPetId}
        onChange={setSelectedPetId}
      />

      {/* Category selector */}
      <CategorySelector
        value={category}
        onChange={setCategory}
        error={categoryError ?? undefined}
      />

      {/* Subject */}
      <div className="flex flex-col gap-1">
        <label htmlFor="message-subject" className="text-sm font-medium text-foreground">
          {t('subject_label')}
        </label>
        <Input
          id="message-subject"
          data-testid="message-subject"
          value={subject}
          onChange={(e) => {
            setSubject(e.target.value)
            if (subjectError) setSubjectError(null)
          }}
          placeholder={t('subject_placeholder')}
          className={subjectError ? 'border-red-500' : ''}
        />
        {subjectError && (
          <p className="text-xs text-red-600" data-testid="subject-error">
            {subjectError}
          </p>
        )}
      </div>

      {/* Message textarea */}
      <div className="flex flex-col gap-1">
        <label htmlFor="message-body" className="text-sm font-medium text-foreground">
          {t('message_label')}
        </label>
        <textarea
          id="message-body"
          data-testid="message-input"
          value={messageBody}
          onChange={(e) => {
            setMessageBody(e.target.value)
            if (messageError) setMessageError(null)
          }}
          placeholder={t('message_placeholder')}
          rows={6}
          className={`block w-full rounded-md border px-3 py-2 text-sm shadow-sm focus:outline-none focus:ring-1 resize-none ${
            isOverLimit
              ? 'border-red-500 focus:border-red-500 focus:ring-red-500'
              : messageError
              ? 'border-red-500 focus:border-red-500 focus:ring-red-500'
              : 'border-border/80 focus:border-primary focus:ring-primary'
          }`}
        />
        <div className="flex justify-between items-center">
          {messageError && (
            <p className="text-xs text-red-600" data-testid="message-error">
              {messageError}
            </p>
          )}
          <p
            className={`text-xs ms-auto ${isOverLimit ? 'text-red-600 font-semibold' : 'text-muted-foreground'}`}
            data-testid="char-counter"
          >
            {isOverLimit
              ? t('characters_over', { count: Math.abs(charsRemaining) })
              : t('characters_remaining', { count: charsRemaining })}
          </p>
        </div>
      </div>

      {/* Photo upload */}
      <PhotoUpload
        photos={photos}
        onChange={handlePhotosChange}
        onError={handlePhotoError}
        error={photoError ?? undefined}
      />

      {/* Submit error */}
      {submitError && (
        <p className="text-sm text-red-600 rounded-md bg-red-50 px-3 py-2 border border-red-200" data-testid="submit-error">
          {submitError}
        </p>
      )}

      {/* Send button */}
      <Button
        onClick={handleSend}
        disabled={isSubmitting || isOverLimit}
        data-testid="send-message-btn"
        className="w-full bg-primary hover:bg-primary/90 text-white disabled:opacity-50"
      >
        {isSubmitting ? t('sending') : t('send')}
      </Button>
    </div>
  )
}
