'use client'

import { useRef } from 'react'
import { useTranslations } from 'next-intl'
import { X, ImagePlus } from 'lucide-react'
import { Button } from '@/components/ui/button'

const MAX_PHOTOS = 3
const MAX_SIZE_BYTES = 5 * 1024 * 1024 // 5 MB
const ALLOWED_TYPES = ['image/jpeg', 'image/png']

export interface PhotoFile {
  file: File
  previewUrl: string
}

interface PhotoUploadProps {
  photos: PhotoFile[]
  onChange: (photos: PhotoFile[]) => void
  onError?: (error: string | null) => void
  error?: string
}

export function PhotoUpload({ photos, onChange, onError, error }: PhotoUploadProps) {
  const t = useTranslations('portal.new_message')
  const inputRef = useRef<HTMLInputElement>(null)

  function handleFiles(files: FileList | null) {
    if (!files) return

    const newPhotos: PhotoFile[] = []
    let validationError: string | null = null

    for (const file of Array.from(files)) {
      if (photos.length + newPhotos.length >= MAX_PHOTOS) {
        validationError = t('errors.too_many_photos')
        break
      }
      if (!ALLOWED_TYPES.includes(file.type)) {
        validationError = t('errors.invalid_type')
        continue
      }
      if (file.size > MAX_SIZE_BYTES) {
        validationError = t('errors.file_too_large')
        continue
      }
      newPhotos.push({
        file,
        previewUrl: URL.createObjectURL(file),
      })
    }

    onError?.(validationError)
    onChange([...photos, ...newPhotos])
  }

  function removePhoto(index: number) {
    const updated = photos.filter((_, i) => i !== index)
    // Revoke blob URL to free memory
    URL.revokeObjectURL(photos[index].previewUrl)
    onChange(updated)
  }

  return (
    <div className="flex flex-col gap-2" data-testid="photo-upload">
      <label className="text-sm font-medium text-gray-700">
        {t('photos_label')}
      </label>
      <p className="text-xs text-gray-500">{t('photos_hint')}</p>

      {/* Preview grid */}
      {photos.length > 0 && (
        <div className="flex flex-wrap gap-2" data-testid="photo-previews">
          {photos.map((photo, idx) => (
            <div key={idx} className="relative w-20 h-20 rounded-md overflow-hidden border border-gray-200">
              {/* eslint-disable-next-line @next/next/no-img-element */}
              <img
                src={photo.previewUrl}
                alt={photo.file.name}
                className="w-full h-full object-cover"
              />
              <button
                type="button"
                onClick={() => removePhoto(idx)}
                data-testid={`remove-photo-${idx}`}
                className="absolute top-0.5 right-0.5 bg-black/60 text-white rounded-full w-5 h-5 flex items-center justify-center hover:bg-black/80 transition-colors"
                aria-label={`Remove photo ${photo.file.name}`}
              >
                <X className="w-3 h-3" />
              </button>
            </div>
          ))}
        </div>
      )}

      {/* Add photo button */}
      {photos.length < MAX_PHOTOS && (
        <>
          <input
            ref={inputRef}
            type="file"
            accept="image/jpeg,image/png"
            multiple
            className="sr-only"
            data-testid="photo-file-input"
            onChange={(e) => handleFiles(e.target.files)}
            aria-label={t('add_photo')}
          />
          <Button
            type="button"
            variant="outline"
            size="sm"
            onClick={() => inputRef.current?.click()}
            data-testid="add-photo-btn"
            className="w-fit"
          >
            <ImagePlus className="h-4 w-4 me-1" />
            {t('add_photo')}
          </Button>
        </>
      )}

      {error && (
        <p className="text-xs text-red-600" data-testid="photo-error">
          {error}
        </p>
      )}
    </div>
  )
}
