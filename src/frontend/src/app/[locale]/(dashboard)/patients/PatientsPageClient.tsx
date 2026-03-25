'use client'

import { useEffect, useState, useCallback } from 'react'
import Link from 'next/link'
import { Plus, Upload, ClipboardList, ChevronLeft, ChevronRight } from 'lucide-react'
import { Input } from '@/components/ui/input'
import { Button } from '@/components/ui/button'
import { PatientCard } from '@/components/features/patients/PatientCard'
import { CsvImportDialog } from '@/components/features/patients/CsvImportDialog'
import { EmptyState } from '@/components/features/onboarding/EmptyState'
import { ErrorState } from '@/components/ui/error-state'
import { getPatients } from '@/lib/api/patients'
import type { PatientDto } from '@/lib/api/patients'
import { useRole } from '@/hooks/use-role'
import { useTranslations } from 'next-intl'
import { PageContainer } from "@/components/ui/page-container"
import { trackEvent, AnalyticsEvents } from '@/lib/analytics'

export default function PatientsPageClient() {
  const t = useTranslations('patients')
  const tEmpty = useTranslations('onboarding.empty.patients')
  const [patients, setPatients] = useState<PatientDto[]>([])
  const [searchQuery, setSearchQuery] = useState('')
  const [isLoading, setIsLoading] = useState(true)
  const [error, setError] = useState<string | null>(null)
  const [showImportDialog, setShowImportDialog] = useState(false)
  const [currentPage, setCurrentPage] = useState(1)
  const [totalCount, setTotalCount] = useState(0)
  const pageSize = 12
  const role = useRole()

  const canWrite = role === 'VET' || role === 'ADMIN'

  const fetchPatients = useCallback(async (search?: string, page = 1): Promise<number> => {
    setIsLoading(true)
    setError(null)
    try {
      const result = await getPatients({ search: search || undefined, page, pageSize })
      setPatients(result.items)
      setTotalCount(result.totalCount)
      setCurrentPage(result.page)
      return result.items.length
    } catch {
      setError(t('errors.load_failed'))
      setPatients([])
      setTotalCount(0)
      return 0
    } finally {
      setIsLoading(false)
    }
  }, [])

  useEffect(() => {
    fetchPatients()
  }, [fetchPatients])

  useEffect(() => {
    const timer = setTimeout(async () => {
      setCurrentPage(1)
      if (searchQuery.length > 0) {
        const count = await fetchPatients(searchQuery, 1)
        trackEvent(AnalyticsEvents.PATIENT_SEARCHED, {
          query_length: String(searchQuery.length),
          has_results: String(count > 0),
        })
      } else {
        fetchPatients(searchQuery, 1)
      }
    }, 300)
    return () => clearTimeout(timer)
  }, [searchQuery, fetchPatients])

  return (
    <PageContainer data-testid="patients-page">
      <div className="flex items-center justify-between">
        <div>
          <h1 className="text-[22px] font-bold text-foreground flex items-center gap-2" data-testid="patients-title">
            <span className="w-1 h-5 bg-primary rounded-full"></span>
            {t('title')}
          </h1>
        </div>
        {canWrite && (
          <div className="flex items-center gap-3">
            <Button
              variant="outline"
              onClick={() => setShowImportDialog(true)}
              data-testid="import-csv-btn"
              className="rounded-xl h-10 px-5 font-semibold border-border/80 hover:bg-muted"
            >
              <Upload className="h-4 w-4 me-1.5" />
              {t('import_csv')}
            </Button>
            <Link href="patients/new">
              <Button
                data-testid="add-patient-btn"
                className="bg-primary hover:bg-primary/90 text-white font-semibold rounded-xl h-10 px-5 shadow-sm"
              >
                <Plus className="h-4 w-4 me-1.5" />
                {t('add_patient')}
              </Button>
            </Link>
          </div>
        )}
      </div>

      <div className="space-y-2">
        <Input
          type="search"
          placeholder={t('search_placeholder')}
          aria-label="Search patients"
          value={searchQuery}
          onChange={(e) => setSearchQuery(e.target.value)}
          data-testid="search-input"
          className="w-full bg-white border-border/80 rounded-xl h-11 transition-shadow duration-200 ease-in-out focus:ring-2 focus:ring-primary/20 focus:border-primary/50 focus:shadow-md"
        />
        {!isLoading && !error && totalCount > 0 && (
          <p className="text-[13px] text-muted-foreground font-medium" data-testid="patients-count">
            {totalCount} {totalCount === 1 ? 'patient' : 'patients'}
          </p>
        )}
      </div>

      {isLoading ? (
        <div
          data-testid="patients-loading"
          className="grid grid-cols-1 gap-6 md:grid-cols-2 lg:grid-cols-3"
        >
          {[1, 2, 3, 4].map((i) => (
            <div
              key={i}
              className="h-48 rounded-xl bg-muted animate-pulse"
              style={{ animationDelay: `${(i - 1) * 100}ms` }}
            />
          ))}
        </div>
      ) : error ? (
        <ErrorState
          data-testid="patients-error"
          title={t('errors.load_failed')}
          description={error}
          onRetry={() => fetchPatients(searchQuery)}
        />
      ) : patients.length === 0 ? (
        <EmptyState
          icon={<ClipboardList className="h-16 w-16" />}
          title={tEmpty('title')}
          description={tEmpty('description')}
          primaryCta={{ label: tEmpty('cta'), href: 'patients/new' }}
          secondaryCta={
            canWrite
              ? {
                  label: tEmpty('cta_import'),
                  onClick: () => setShowImportDialog(true),
                  'data-testid': 'empty-state-cta-import',
                }
              : undefined
          }
          tip={tEmpty('tip')}
          data-testid-prefix="patients"
        />
      ) : (
        <div
          data-testid="patients-table"
          className="grid grid-cols-1 gap-6 md:grid-cols-2 lg:grid-cols-3 animate-in fade-in duration-300"
        >
          {patients.map((patient) => (
            <PatientCard key={patient.id} patient={patient} />
          ))}
        </div>
      )}

      {/* Pagination */}
      {!isLoading && !error && totalCount > pageSize && (() => {
        const totalPages = Math.ceil(totalCount / pageSize)
        return (
          <div className="flex items-center justify-center gap-2 pt-2" data-testid="patients-pagination">
            <Button
              variant="outline"
              size="sm"
              disabled={currentPage <= 1}
              onClick={() => fetchPatients(searchQuery, currentPage - 1)}
              data-testid="pagination-prev"
              className="rounded-xl h-9 px-3"
            >
              <ChevronLeft className="h-4 w-4 me-1" />
              Previous
            </Button>
            <span className="text-[13px] text-muted-foreground font-medium px-3" data-testid="pagination-info">
              Page {currentPage} of {totalPages}
            </span>
            <Button
              variant="outline"
              size="sm"
              disabled={currentPage >= totalPages}
              onClick={() => fetchPatients(searchQuery, currentPage + 1)}
              data-testid="pagination-next"
              className="rounded-xl h-9 px-3"
            >
              Next
              <ChevronRight className="h-4 w-4 ms-1" />
            </Button>
          </div>
        )
      })()}

      {canWrite && (
        <CsvImportDialog
          open={showImportDialog}
          onOpenChange={setShowImportDialog}
          onImported={() => fetchPatients(searchQuery)}
        />
      )}
    </PageContainer>
  )
}
