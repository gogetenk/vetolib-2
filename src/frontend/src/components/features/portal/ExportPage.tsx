'use client'

import { useState } from 'react'
import { useTranslations } from 'next-intl'
import { useRouter, useParams } from 'next/navigation'
import { Download, ArrowLeft } from 'lucide-react'
import { Button } from '@/components/ui/button'
import { exportConversations } from '@/lib/api/portal'

export function ExportPage() {
  const t = useTranslations('portal.export')
  const router = useRouter()
  const params = useParams<{ locale: string; clinicSlug: string }>()
  const [isDownloading, setIsDownloading] = useState(false)
  const [error, setError] = useState<string | null>(null)

  async function handleDownload() {
    setIsDownloading(true)
    setError(null)
    try {
      const blob = await exportConversations()
      const url = URL.createObjectURL(blob)
      const a = document.createElement('a')
      a.href = url
      a.download = 'my-conversations.txt'
      document.body.appendChild(a)
      a.click()
      document.body.removeChild(a)
      URL.revokeObjectURL(url)
    } catch {
      setError(t('error'))
    } finally {
      setIsDownloading(false)
    }
  }

  return (
    <div className="space-y-6" data-testid="export-page">
      {/* Back link */}
      <button
        type="button"
        onClick={() => router.push(`/${params.locale}/portal/${params.clinicSlug}`)}
        data-testid="export-back-btn"
        className="flex items-center gap-1 text-sm text-emerald-600 hover:text-emerald-800"
      >
        <ArrowLeft className="h-4 w-4" />
        {t('back')}
      </button>

      {/* Title + description */}
      <div>
        <h1 className="text-xl font-bold text-stone-900" data-testid="export-title">
          {t('title')}
        </h1>
        <p className="mt-1 text-sm text-stone-600" data-testid="export-description">
          {t('description')}
        </p>
      </div>

      {/* Error */}
      {error && (
        <p className="text-sm text-red-600 rounded-md bg-red-50 px-3 py-2 border border-red-200" data-testid="export-error">
          {error}
        </p>
      )}

      {/* Download button */}
      <Button
        onClick={handleDownload}
        disabled={isDownloading}
        data-testid="download-export-btn"
        className="bg-emerald-600 hover:bg-emerald-700 text-white"
      >
        <Download className="h-4 w-4 me-2" />
        {isDownloading ? t('downloading') : t('download')}
      </Button>
    </div>
  )
}
