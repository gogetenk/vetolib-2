'use client'

import { useEffect, useState, useCallback } from 'react'
import { useRouter } from 'next/navigation'
import { Plus, ArrowLeft } from 'lucide-react'
import { toast } from 'sonner'
import { useTranslations, useLocale } from 'next-intl'
import { Button } from '@/components/ui/button'
import { DrugCatalogTable } from '@/components/drugs/DrugCatalogTable'
import { AddDrugDialog } from '@/components/drugs/AddDrugDialog'
import { getDrugCatalog, createDrug } from '@/lib/api/drugs'
import type { DrugCatalogEntryDto, CreateDrugRequest } from '@/lib/api/drugs'

export default function DrugCatalogPage() {
  const t = useTranslations('drugs')
  const router = useRouter()
  const locale = useLocale()

  const [drugs, setDrugs] = useState<DrugCatalogEntryDto[]>([])
  const [isLoading, setIsLoading] = useState(true)
  const [showAddDialog, setShowAddDialog] = useState(false)

  // Filter state
  const [searchQuery, setSearchQuery] = useState('')
  const [categoryFilter, setCategoryFilter] = useState('all')
  const [speciesFilter, setSpeciesFilter] = useState('all')

  const fetchDrugs = useCallback(async () => {
    setIsLoading(true)
    try {
      const data = await getDrugCatalog({
        search: searchQuery || undefined,
        category: categoryFilter === 'all' ? undefined : categoryFilter as DrugCatalogEntryDto['category'],
        species: speciesFilter === 'all' ? undefined : speciesFilter,
      })
      setDrugs(data)
    } catch {
      toast.error(t('errors.load_failed'))
    } finally {
      setIsLoading(false)
    }
  }, [searchQuery, categoryFilter, speciesFilter, t])

  useEffect(() => {
    fetchDrugs()
  }, [fetchDrugs])

  const handleAddDrug = async (data: CreateDrugRequest) => {
    try {
      await createDrug(data)
      toast.success(t('success.drug_added'))
      setShowAddDialog(false)
      await fetchDrugs()
    } catch {
      toast.error(t('errors.save_failed'))
    }
  }

  const handleViewDrug = (drug: DrugCatalogEntryDto) => {
    router.push(`/${locale}/stock/drugs/${drug.id}`)
  }

  return (
    <div className="p-6 lg:p-8 space-y-6" data-testid="drug-catalog-page">
      <div className="flex items-center justify-between">
        <div className="flex items-center gap-3">
          <Button
            variant="outline"
            size="sm"
            onClick={() => router.push(`/${locale}/stock`)}
            data-testid="btn-back-to-stock"
            className="rounded-xl border-border/80 hover:bg-[#f4f6f9]"
          >
            <ArrowLeft className="h-4 w-4" />
          </Button>
          <h1 className="text-[22px] font-bold text-[#061e44] flex items-center gap-2" data-testid="drug-catalog-title">
            <span className="w-1 h-5 bg-emerald-500 rounded-full"></span>
            {t('title')}
          </h1>
        </div>
        <Button
          onClick={() => setShowAddDialog(true)}
          data-testid="btn-add-drug"
          className="bg-emerald-600 hover:bg-emerald-700 text-white font-semibold rounded-xl h-10 px-5 shadow-sm"
        >
          <Plus className="me-1.5 h-4 w-4" />
          {t('add_drug')}
        </Button>
      </div>

      {/* Stats bar */}
      <div className="flex gap-4" data-testid="drug-catalog-stats">
        <div className="bg-white border border-border/80 rounded-xl px-4 py-3 shadow-sm">
          <p className="text-[11px] font-bold uppercase tracking-wider text-muted-foreground">{t('stats.total')}</p>
          <p className="text-lg font-bold text-[#061e44] tabular-nums" data-testid="stat-total-drugs">{drugs.length}</p>
        </div>
        <div className="bg-white border border-border/80 rounded-xl px-4 py-3 shadow-sm">
          <p className="text-[11px] font-bold uppercase tracking-wider text-muted-foreground">{t('stats.prescription')}</p>
          <p className="text-lg font-bold text-amber-700 tabular-nums" data-testid="stat-rx-drugs">
            {drugs.filter(d => d.requiresPrescription).length}
          </p>
        </div>
        <div className="bg-white border border-border/80 rounded-xl px-4 py-3 shadow-sm">
          <p className="text-[11px] font-bold uppercase tracking-wider text-muted-foreground">{t('stats.otc')}</p>
          <p className="text-lg font-bold text-emerald-700 tabular-nums" data-testid="stat-otc-drugs">
            {drugs.filter(d => !d.requiresPrescription).length}
          </p>
        </div>
      </div>

      {/* Table */}
      {isLoading ? (
        <div className="space-y-2" data-testid="drug-catalog-loading">
          {[1, 2, 3, 4, 5].map(i => (
            <div
              key={i}
              className="h-12 rounded-md bg-muted animate-pulse"
              style={{ animationDelay: `${(i - 1) * 100}ms` }}
            />
          ))}
        </div>
      ) : (
        <DrugCatalogTable
          drugs={drugs}
          onView={handleViewDrug}
          searchQuery={searchQuery}
          onSearchChange={setSearchQuery}
          categoryFilter={categoryFilter}
          onCategoryFilterChange={setCategoryFilter}
          speciesFilter={speciesFilter}
          onSpeciesFilterChange={setSpeciesFilter}
        />
      )}

      {/* Add drug dialog */}
      <AddDrugDialog
        open={showAddDialog}
        onOpenChange={setShowAddDialog}
        onSubmit={handleAddDrug}
      />
    </div>
  )
}
