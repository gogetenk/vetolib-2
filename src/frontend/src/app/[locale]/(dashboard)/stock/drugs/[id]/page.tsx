'use client'

import { useEffect, useState } from 'react'
import { useParams, useRouter } from 'next/navigation'
import { ArrowLeft } from 'lucide-react'
import { toast } from 'sonner'
import { useTranslations, useLocale } from 'next-intl'
import { Button } from '@/components/ui/button'
import { DrugDetailCard } from '@/components/drugs/DrugDetailCard'
import { getDrugById } from '@/lib/api/drugs'
import type { DrugCatalogEntryDto } from '@/lib/api/drugs'

export default function DrugDetailPage() {
  const t = useTranslations('drugs')
  const router = useRouter()
  const locale = useLocale()
  const params = useParams<{ id: string }>()
  const drugId = params.id

  const [drug, setDrug] = useState<DrugCatalogEntryDto | null>(null)
  const [isLoading, setIsLoading] = useState(true)

  useEffect(() => {
    async function fetchDrug() {
      setIsLoading(true)
      try {
        const data = await getDrugById(drugId)
        setDrug(data)
      } catch {
        toast.error(t('errors.load_failed'))
      } finally {
        setIsLoading(false)
      }
    }
    fetchDrug()
  }, [drugId, t])

  return (
    <div className="p-6 lg:p-8 space-y-6" data-testid="drug-detail-page">
      <div className="flex items-center gap-3">
        <Button
          variant="outline"
          size="sm"
          onClick={() => router.push(`/${locale}/stock/drugs`)}
          data-testid="btn-back-to-catalog"
          className="rounded-xl border-border/80 hover:bg-[#f4f6f9]"
        >
          <ArrowLeft className="h-4 w-4" />
        </Button>
        <h1 className="text-[22px] font-bold text-[#061e44] flex items-center gap-2" data-testid="drug-detail-title">
          <span className="w-1 h-5 bg-[#303ef5] rounded-full"></span>
          {drug ? drug.displayName : t('detail.loading')}
        </h1>
      </div>

      {isLoading ? (
        <div className="space-y-4" data-testid="drug-detail-loading">
          <div className="h-24 rounded-xl bg-muted animate-pulse" />
          <div className="h-16 rounded-xl bg-muted animate-pulse" style={{ animationDelay: '100ms' }} />
          <div className="h-40 rounded-xl bg-muted animate-pulse" style={{ animationDelay: '200ms' }} />
        </div>
      ) : drug ? (
        <DrugDetailCard drug={drug} />
      ) : (
        <p
          className="py-12 text-center text-sm text-muted-foreground"
          data-testid="drug-detail-not-found"
        >
          {t('errors.not_found')}
        </p>
      )}
    </div>
  )
}
