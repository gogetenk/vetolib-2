'use client'

import { useEffect, useState, useCallback } from 'react'
import Link from 'next/link'
import { Plus, Upload } from 'lucide-react'
import { Input } from '@/components/ui/input'
import { Button } from '@/components/ui/button'
import { PatientCard } from '@/components/features/patients/PatientCard'
import { CsvImportDialog } from '@/components/features/patients/CsvImportDialog'
import { getPatients } from '@/lib/api/patients'
import type { PatientDto } from '@/lib/api/patients'
import { useRole } from '@/hooks/use-role'
import { useTranslations } from 'next-intl'
import { trackEvent, AnalyticsEvents } from '@/lib/analytics'

export default function PatientsPage() {
  const t = useTranslations('patients')
  const [patients, setPatients] = useState<PatientDto[]>([])
  const [searchQuery, setSearchQuery] = useState('')
  const [isLoading, setIsLoading] = useState(true)
  const [showImportDialog, setShowImportDialog] = useState(false)
  const role = useRole()

  const canWrite = role === 'VET' || role === 'ADMIN'

  const fetchPatients = useCallback(async (search?: string) => {
    setIsLoading(true)
    try {
      const result = await getPatients({ search: search || undefined })
      setPatients(result.items)
    } catch {
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
    <div className="space-y-6" data-testid="patients-page">
      <div className="flex items-center justify-between">
        <h1 className="text-2xl font-bold" data-testid="patients-title">
          {t('title')}
        </h1>
        {canWrite && (
          <div className="flex items-center gap-2">
            <Button
              variant="outline"
              onClick={() => setShowImportDialog(true)}
              data-testid="import-csv-btn"
            >
              <Upload className="h-4 w-4 me-1" />
              {t('import_csv')}
            </Button>
            <Button
              render={<Link href="patients/new" />}
              data-testid="add-patient-btn"
            >
              <Plus className="h-4 w-4 me-1" />
              {t('add_patient')}
            </Button>
          </div>
        )}
      </div>

      <div>
        <Input
          type="search"
          placeholder={t('search_placeholder')}
          value={searchQuery}
          onChange={(e) => setSearchQuery(e.target.value)}
          data-testid="search-input"
          className="max-w-sm"
        />
      </div>

      {isLoading ? (
        <div
          data-testid="patients-loading"
          className="grid grid-cols-1 gap-4 md:grid-cols-2 lg:grid-cols-3"
        >
          {[1, 2, 3, 4].map((i) => (
            <div key={i} className="h-48 rounded-lg bg-muted animate-pulse" />
          ))}
        </div>
      ) : patients.length === 0 ? (
        <p
          className="text-muted-foreground text-sm py-12 text-center"
          data-testid="patients-empty"
        >
          {t('no_patients')}
        </p>
      ) : (
        <div
          data-testid="patients-table"
          className="grid grid-cols-1 gap-4 md:grid-cols-2 lg:grid-cols-3"
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
