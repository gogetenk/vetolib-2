'use client'

import { useCallback, useEffect, useRef, useState } from 'react'
import { useTranslations } from 'next-intl'
import { Send, StickyNote, Paperclip, Loader2, Check } from 'lucide-react'
import { Button } from '@/components/ui/button'
import { Textarea } from '@/components/ui/textarea'
import { cn } from '@/lib/utils'
import { uploadFiles } from '@/lib/api/messaging'
import { AttachmentPreviewList } from './AttachmentPreview'
import type { AttachmentFile } from './AttachmentPreview'

const MAX_CHARS = 2000
const MAX_FILES = 5
const MAX_FILE_SIZE_BYTES = 10 * 1024 * 1024 // 10 MB
const ACCEPTED_TYPES = '.pdf,.jpg,.jpeg,.png'
const ACCEPTED_MIME_TYPES = ['application/pdf', 'image/jpeg', 'image/png']

interface ReplyComposerProps {
  prefillText: string
  onPrefillConsumed: () => void
  isSending: boolean
  canAddNote: boolean
  canAttachToRecord: boolean
  onSendReply: (body: string, attachmentIds?: string[]) => Promise<void>
  onAddNote: (body: string) => Promise<void>
}

export function ReplyComposer({
  prefillText,
  onPrefillConsumed,
  isSending,
  canAddNote,
  canAttachToRecord,
  onSendReply,
  onAddNote,
}: ReplyComposerProps) {
  const t = useTranslations('messaging')
  const [text, setText] = useState('')
  const [sendSuccess, setSendSuccess] = useState(false)
  const [attachments, setAttachments] = useState<AttachmentFile[]>([])
  const [fileError, setFileError] = useState<string | null>(null)
  const textareaRef = useRef<HTMLTextAreaElement>(null)
  const fileInputRef = useRef<HTMLInputElement>(null)
  const sendSuccessTimeout = useRef<ReturnType<typeof setTimeout> | null>(null)
  const previewUrlsRef = useRef<Set<string>>(new Set())

  // Clean up send success timeout and preview URLs on unmount
  useEffect(() => {
    const timeoutRef = sendSuccessTimeout
    const urlsRef = previewUrlsRef
    return () => {
      if (timeoutRef.current) clearTimeout(timeoutRef.current)
      // Revoke all tracked object URLs to avoid memory leaks
      urlsRef.current.forEach((url) => URL.revokeObjectURL(url))
      urlsRef.current.clear()
    }
  }, [])

  // When an AI suggestion is selected, prefill the textarea.
  // Use a ref to track consumed prefill and queueMicrotask to avoid synchronous setState in effect.
  const lastConsumedPrefill = useRef('')
  useEffect(() => {
    if (prefillText && prefillText !== lastConsumedPrefill.current) {
      lastConsumedPrefill.current = prefillText
      queueMicrotask(() => {
        setText(prefillText)
        onPrefillConsumed()
        textareaRef.current?.focus()
      })
    }
  }, [prefillText, onPrefillConsumed])

  const charCount = text.length
  const isOverLimit = charCount > MAX_CHARS
  const isEmpty = text.trim().length === 0 && attachments.length === 0
  const hasUploadingFiles = attachments.some((a) => a.isUploading)

  const handleFileSelect = useCallback(
    (e: React.ChangeEvent<HTMLInputElement>) => {
      setFileError(null)
      const selectedFiles = Array.from(e.target.files ?? [])
      if (selectedFiles.length === 0) return

      // Validate total count
      const totalFiles = attachments.length + selectedFiles.length
      if (totalFiles > MAX_FILES) {
        setFileError(t('file_error_max_count', { max: MAX_FILES }))
        // Reset file input
        if (fileInputRef.current) fileInputRef.current.value = ''
        return
      }

      // Validate each file
      const validFiles: File[] = []
      for (const file of selectedFiles) {
        if (!ACCEPTED_MIME_TYPES.includes(file.type)) {
          setFileError(t('file_error_type'))
          if (fileInputRef.current) fileInputRef.current.value = ''
          return
        }
        if (file.size > MAX_FILE_SIZE_BYTES) {
          setFileError(t('file_error_size', { max: '10 MB' }))
          if (fileInputRef.current) fileInputRef.current.value = ''
          return
        }
        validFiles.push(file)
      }

      // Create attachment entries
      const newAttachments: AttachmentFile[] = validFiles.map((file) => {
        const isImage = file.type.startsWith('image/')
        const previewUrl = isImage ? URL.createObjectURL(file) : null
        if (previewUrl) previewUrlsRef.current.add(previewUrl)
        return {
          file,
          id: crypto.randomUUID(),
          previewUrl,
          isUploading: false,
          uploadProgress: 0,
          uploadedId: null,
          error: null,
        }
      })

      setAttachments((prev) => [...prev, ...newAttachments])

      // Reset file input so the same file can be selected again
      if (fileInputRef.current) fileInputRef.current.value = ''
    },
    [attachments.length, t]
  )

  const handleRemoveAttachment = useCallback((id: string) => {
    setAttachments((prev) => {
      const att = prev.find((a) => a.id === id)
      if (att?.previewUrl) {
        URL.revokeObjectURL(att.previewUrl)
        previewUrlsRef.current.delete(att.previewUrl)
      }
      return prev.filter((a) => a.id !== id)
    })
    setFileError(null)
  }, [])

  const handleSend = async () => {
    if (isEmpty || isOverLimit || isSending || hasUploadingFiles) return

    // Upload files first if any
    let attachmentIds: string[] | undefined
    if (attachments.length > 0) {
      // Mark all as uploading
      setAttachments((prev) =>
        prev.map((a) => ({ ...a, isUploading: true, uploadProgress: 30 }))
      )

      try {
        const filesToUpload = attachments.map((a) => a.file)
        const uploadedIds = await uploadFiles(filesToUpload)
        attachmentIds = uploadedIds

        // Mark as done
        setAttachments((prev) =>
          prev.map((a, i) => ({
            ...a,
            isUploading: false,
            uploadProgress: 100,
            uploadedId: uploadedIds[i] ?? null,
          }))
        )
      } catch {
        // Mark upload error
        setAttachments((prev) =>
          prev.map((a) => ({
            ...a,
            isUploading: false,
            uploadProgress: 0,
            error: t('file_upload_failed'),
          }))
        )
        return
      }
    }

    await onSendReply(text.trim(), attachmentIds)
    setText('')
    // Clean up preview URLs
    attachments.forEach((att) => {
      if (att.previewUrl) {
        URL.revokeObjectURL(att.previewUrl)
        previewUrlsRef.current.delete(att.previewUrl)
      }
    })
    setAttachments([])
    setFileError(null)
    setSendSuccess(true)
    if (sendSuccessTimeout.current) clearTimeout(sendSuccessTimeout.current)
    sendSuccessTimeout.current = setTimeout(() => setSendSuccess(false), 1500)
  }

  const handleAddNote = async () => {
    if (text.trim().length === 0 || isOverLimit || isSending || !canAddNote) return
    await onAddNote(text.trim())
    setText('')
  }

  const handleKeyDown = (e: React.KeyboardEvent<HTMLTextAreaElement>) => {
    if (e.key === 'Enter' && (e.ctrlKey || e.metaKey) && !isEmpty && !isOverLimit) {
      e.preventDefault()
      handleSend()
    }
  }

  const handlePaperclipClick = () => {
    fileInputRef.current?.click()
  }

  return (
    <div className="p-3" data-testid="reply-composer">
      <Textarea
        ref={textareaRef}
        data-testid="reply-textarea"
        value={text}
        onChange={(e) => setText(e.target.value)}
        onKeyDown={handleKeyDown}
        placeholder={t('reply_placeholder')}
        className={cn(
          'resize-none min-h-[80px] max-h-[200px] rounded-xl border-border/80 text-[13px] focus:ring-2 focus:ring-primary/20 focus:border-primary/50 transition-shadow',
          isOverLimit && 'border-red-400 focus-visible:ring-red-400'
        )}
        disabled={isSending}
        aria-label={t('reply_placeholder')}
        maxLength={MAX_CHARS + 100}
      />

      {/* Attachment previews */}
      <AttachmentPreviewList
        attachments={attachments}
        onRemove={handleRemoveAttachment}
      />

      {/* File validation error */}
      {fileError && (
        <p
          className="text-xs text-red-500 mt-1 animate-slide-up-fade"
          role="alert"
          data-testid="file-error"
        >
          {fileError}
        </p>
      )}

      {/* Hidden file input */}
      <input
        ref={fileInputRef}
        type="file"
        accept={ACCEPTED_TYPES}
        multiple
        className="hidden"
        onChange={handleFileSelect}
        data-testid="file-input"
        aria-label={t('attach_files')}
      />

      <div className="flex items-center justify-between mt-2 gap-2">
        {/* Character counter */}
        <span
          data-testid="char-counter"
          className={cn(
            'text-xs tabular-nums',
            isOverLimit ? 'text-red-500 font-semibold' : 'text-muted-foreground'
          )}
          aria-live="polite"
        >
          {charCount}/{MAX_CHARS}
        </span>

        <div className="flex items-center gap-2">
          {/* File attachment button */}
          <Button
            type="button"
            variant="ghost"
            size="sm"
            data-testid="attach-file-btn"
            disabled={isSending || attachments.length >= MAX_FILES}
            onClick={handlePaperclipClick}
            aria-label={t('attach_files')}
            title={t('attach_files')}
          >
            <Paperclip className="h-4 w-4" />
            {attachments.length > 0 && (
              <span className="text-xs text-muted-foreground ms-1">
                {attachments.length}/{MAX_FILES}
              </span>
            )}
          </Button>

          {/* Attach to medical record — VetOrAdmin only */}
          {canAttachToRecord && (
            <Button
              type="button"
              variant="ghost"
              size="sm"
              data-testid="add-to-record-btn"
              disabled={isSending || (text.trim().length === 0)}
              aria-label={t('attach_to_record')}
              title={t('attach_to_record')}
            >
              <StickyNote className="h-4 w-4" />
              <span className="sr-only">{t('attach_to_record')}</span>
            </Button>
          )}

          {/* Add internal note — VetOrAdmin only */}
          {canAddNote && (
            <Button
              type="button"
              variant="outline"
              size="sm"
              data-testid="add-note-btn"
              disabled={isSending || text.trim().length === 0 || isOverLimit}
              onClick={handleAddNote}
              aria-label={t('add_note')}
              className="rounded-xl text-[12px] font-semibold border-border/80 hover:bg-muted"
            >
              <StickyNote className="h-4 w-4 me-1.5" aria-hidden />
              {t('add_note')}
            </Button>
          )}

          <Button
            type="button"
            size="sm"
            data-testid="send-reply-btn"
            disabled={isSending || isEmpty || isOverLimit || hasUploadingFiles}
            onClick={handleSend}
            className="bg-primary hover:bg-primary/90 text-primary-foreground font-semibold rounded-xl shadow-sm"
          >
            {isSending ? (
              <Loader2 className="h-4 w-4 me-1.5 animate-spin" aria-hidden />
            ) : sendSuccess ? (
              <Check className="h-4 w-4 me-1.5 animate-success-check text-green-200" aria-hidden />
            ) : (
              <Send className="h-4 w-4 me-1.5" aria-hidden />
            )}
            {isSending ? t('sending') : sendSuccess ? t('send_reply') : t('send_reply')}
          </Button>
        </div>
      </div>

      {isOverLimit && (
        <p className="text-xs text-red-500 mt-1 animate-slide-up-fade" role="alert" data-testid="char-limit-warning">
          {t('char_limit_exceeded')}
        </p>
      )}
    </div>
  )
}
