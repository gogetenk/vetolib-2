'use client'

import { useTranslations } from 'next-intl'
import { X, FileText, Loader2 } from 'lucide-react'
import { cn } from '@/lib/utils'

export interface AttachmentFile {
  file: File
  id: string
  previewUrl: string | null
  isUploading: boolean
  uploadProgress: number
  uploadedId: string | null
  error: string | null
}

function formatFileSize(bytes: number): string {
  if (bytes < 1024) return `${bytes} B`
  if (bytes < 1024 * 1024) return `${(bytes / 1024).toFixed(1)} KB`
  return `${(bytes / (1024 * 1024)).toFixed(1)} MB`
}

interface AttachmentPreviewItemProps {
  attachment: AttachmentFile
  onRemove: (id: string) => void
}

function AttachmentPreviewItem({ attachment, onRemove }: AttachmentPreviewItemProps) {
  const t = useTranslations('messaging')
  const isImage = attachment.file.type.startsWith('image/')

  return (
    <div
      className={cn(
        'relative flex items-center gap-2 rounded-lg border p-2 bg-muted/50',
        attachment.error && 'border-red-300 bg-red-50'
      )}
      data-testid={`attachment-preview-${attachment.id}`}
    >
      {/* Thumbnail or icon */}
      <div className="flex-shrink-0 w-10 h-10 rounded overflow-hidden flex items-center justify-center bg-muted">
        {isImage && attachment.previewUrl ? (
          // eslint-disable-next-line @next/next/no-img-element
          <img
            src={attachment.previewUrl}
            alt={attachment.file.name}
            className="w-10 h-10 object-cover"
            data-testid={`attachment-thumb-${attachment.id}`}
          />
        ) : (
          <FileText className="h-5 w-5 text-muted-foreground" aria-hidden />
        )}
      </div>

      {/* File info */}
      <div className="flex-1 min-w-0">
        <p
          className="text-xs font-medium truncate text-foreground"
          data-testid={`attachment-name-${attachment.id}`}
          title={attachment.file.name}
        >
          {attachment.file.name}
        </p>
        <p className="text-[11px] text-muted-foreground">
          {formatFileSize(attachment.file.size)}
        </p>
        {attachment.error && (
          <p className="text-[11px] text-red-500" data-testid={`attachment-error-${attachment.id}`}>
            {attachment.error}
          </p>
        )}
      </div>

      {/* Progress bar */}
      {attachment.isUploading && (
        <div className="absolute bottom-0 left-0 right-0 h-1 bg-muted rounded-b-lg overflow-hidden">
          <div
            className="h-full bg-primary transition-all duration-300"
            style={{ width: `${attachment.uploadProgress}%` }}
            data-testid={`attachment-progress-${attachment.id}`}
            role="progressbar"
            aria-valuenow={attachment.uploadProgress}
            aria-valuemin={0}
            aria-valuemax={100}
          />
        </div>
      )}

      {/* Upload spinner or remove button */}
      {attachment.isUploading ? (
        <Loader2
          className="h-4 w-4 animate-spin text-muted-foreground flex-shrink-0"
          aria-label={t('uploading')}
        />
      ) : (
        <button
          type="button"
          onClick={() => onRemove(attachment.id)}
          className="flex-shrink-0 p-0.5 rounded hover:bg-muted-foreground/10 transition-colors"
          data-testid={`attachment-remove-${attachment.id}`}
          aria-label={t('remove_attachment')}
        >
          <X className="h-3.5 w-3.5 text-muted-foreground" />
        </button>
      )}
    </div>
  )
}

interface AttachmentPreviewListProps {
  attachments: AttachmentFile[]
  onRemove: (id: string) => void
}

export function AttachmentPreviewList({ attachments, onRemove }: AttachmentPreviewListProps) {
  if (attachments.length === 0) return null

  return (
    <div className="flex flex-wrap gap-2 mt-2" data-testid="attachment-preview-list">
      {attachments.map((att) => (
        <AttachmentPreviewItem key={att.id} attachment={att} onRemove={onRemove} />
      ))}
    </div>
  )
}
