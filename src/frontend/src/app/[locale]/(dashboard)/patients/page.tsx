'use client'

import { useEffect, useState, useCallback } from 'react'
import Link from 'next/link'
import { Plus, Upload, ClipboardList } from 'lucide-react'
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
import { trackEvent, AnalyticsEvents } from '@/lib/analytics'

export default function PatientsPage() {
  const t = useTranslations('patients')
  const tEmpty = useTranslations('onboarding.empty.patients')
  const [patients, setPatients] = useState<PatientDto[]>([])
  const [searchQuery, setSearchQuery] = useState('')
  const [isLoading, setIsLoading] = useState(true)
  const [error, setError] = useState<string | null>(null)
  const [showImportDialog, setShowImportDialog] = useState(false)
  const role = useRole()

  const canWrite = role === 'VET' || role === 'ADMIN'

  const fetchPatients = useCallback(async (search?: string) => {
    setIsLoading(true)
    setError(null)
    try {
      const result = await getPatients({ search: search || undefined })
      setPatients(result.items)
    } catch {
      setError('Failed to load patients')
      setPatients([])
    } finally {
      setIsLoading(false)
    }
  }, [])

  useEffect(() => {
    fetchPatients()
  }, [fetchPatients])

  useEffect(() => {
    const timer = setTimeout(async () => {
      if (searchQuery.length > 0) {
        await fetchPatients(searchQuery)
        // Track after results are loaded (patients state updated async, use local ref)
        // We track optimistically here; has_results is determined after fetch
        trackEvent(AnalyticsEvents.PATIENT_SEARCHED, {
          query_length: String(searchQuery.length),
          has_results: String(patients.length > 0),
        })
      } else {
        fetchPatients(searchQuery)
      }
    }, 300)
    return () => clearTimeout(timer)
  // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [searchQuery, fetchPatients])

  return (
    <div className="p-6 lg:p-8 space-y-6" data-testid="patients-page">
      <div className="flex items-center justify-between">
        <div>
          <h1 className="text-[22px] font-bold text-[#061e44] flex items-center gap-2" data-testid="patients-title">
            <span className="w-1 h-5 bg-[#303ef5] rounded-full"></span>
            {t('title')}
          </h1>
        </div>
        {canWrite && (
          <div className="flex items-center gap-3">
            <Button
              variant="outline"
              onClick={() => setShowImportDialog(true)}
              data-testid="import-csv-btn"
              className="rounded-xl h-10 px-5 font-semibold border-border/80 hover:bg-[#f4f6f9]"
            >
              <Upload className="h-4 w-4 me-1.5" />
              {t('import_csv')}
            </Button>
            <Link href="patients/new">
              <Button
                data-testid="add-patient-btn"
                className="bg-[#303ef5] hover:bg-[#2530c4] text-white font-semibold rounded-xl h-10 px-5 shadow-sm"
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
          value={searchQuery}
          onChange={(e) => setSearchQuery(e.target.value)}
          data-testid="search-input"
          className="w-full bg-white border-border/80 rounded-xl h-11 transition-shadow duration-200 ease-in-out focus:ring-2 focus:ring-[#303ef5]/20 focus:border-[#303ef5]/50 focus:shadow-md"
        />
        {!isLoading && !error && patients.length > 0 && (
          <p className="text-[13px] text-muted-foreground font-medium" data-testid="patients-count">
            {patients.length} {patients.length === 1 ? 'patient' : 'patients'}
          </p>
        )}
      </div>

      {isLoading ? (
        <div
          data-testid="patients-loading"
          className="grid grid-cols-1 gap-4 md:grid-cols-2 lg:grid-cols-3"
        >
          {[1, 2, 3, 4].map((i) => (
            <div
              key={i}
              className="h-48 rounded-lg bg-muted animate-pulse"
              style={{ animationDelay: `${(i - 1) * 100}ms` }}
            />
          ))}
        </div>
      ) : error ? (
        <ErrorState
          data-testid="patients-error"
          title="Failed to load patients"
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
          className="grid grid-cols-1 gap-4 md:grid-cols-2 lg:grid-cols-3 animate-in fade-in duration-300"
        >
          {patients.map((patient) => (
            <PatientCard key={patient.id} patient={patient} />
          ))}
        </div>
      )}

      {canWrite && (
        <CsvImportDialog
          open={showImportDialog}
          onOpenChange={setShowImportDialog}
          onImported={() => fetchPatients(searchQuery)}
        />
      )}
    </div>
  )
}
