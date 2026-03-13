'use client'

import { useEffect, useState, useMemo } from 'react'
import { useTranslations, useLocale } from 'next-intl'
import Link from 'next/link'
import { toast } from 'sonner'
import { Search } from 'lucide-react'
import { Card, CardContent, CardHeader, CardTitle } from '@/components/ui/card'
import { Input } from '@/components/ui/input'
import { Badge } from '@/components/ui/badge'
import { Skeleton } from '@/components/ui/skeleton'
import { getAllMedicalRecords, type MedicalRecordDto } from '@/lib/api/medical-records'

function formatDate(dateStr: string): string {
  return new Date(dateStr).toLocaleDateString('en-AE', {
    timeZone: 'Asia/Dubai',
    day: '2-digit',
    month: 'short',
    year: 'numeric',
  })
}

export default function MedicalRecordsPage() {
  const t = useTranslations('medical_records')
  const locale = useLocale()
  const [records, setRecords] = useState<MedicalRecordDto[]>([])
  const [isLoading, setIsLoading] = useState(true)
  const [search, setSearch] = useState('')

  useEffect(() => {
    getAllMedicalRecords()
      .then(setRecords)
      .catch(() => toast.error(t('errors.load_failed')))
      .finally(() => setIsLoading(false))
  }, [t])

  const filtered = useMemo(() => {
    if (!search.trim()) return records
    const q = search.toLowerCase()
    return records.filter(
      (r) =>
        r.patientName.toLowerCase().includes(q) ||
        r.diagnosis.toLowerCase().includes(q) ||
        r.vetName.toLowerCase().includes(q) ||
        r.reason.toLowerCase().includes(q)
    )
  }, [records, search])

  return (
    <div className="space-y-6" data-testid="medical-records-page">
      <div className="flex items-center justify-between">
        <h1
          className="text-2xl font-bold"
          data-testid="medical-records-title"
        >
          {t('title')}
        </h1>
      </div>

      {/* Search */}
      <div className="relative">
        <Search className="absolute left-3 top-1/2 h-4 w-4 -translate-y-1/2 text-muted-foreground" />
        <Input
          placeholder={t('search_placeholder')}
          value={search}
          onChange={(e) => setSearch(e.target.value)}
          className="pl-10 transition-shadow duration-200 ease-in-out focus:ring-2 focus:ring-primary/20 focus:shadow-md"
          data-testid="medical-records-search"
        />
      </div>

      {isLoading ? (
        <div className="space-y-3">
          {[1, 2, 3, 4].map((i) => (
            <Skeleton key={i} className="h-24 w-full rounded-lg" style={{ animationDelay: `${(i - 1) * 100}ms` }} />
          ))}
        </div>
      ) : filtered.length === 0 ? (
        <Card>
          <CardContent className="py-8 text-center">
            <p className="text-sm text-muted-foreground animate-in fade-in duration-300" data-testid="medical-records-empty">
              {t('no_records')}
            </p>
          </CardContent>
        </Card>
      ) : (
        <div className="space-y-3 animate-in fade-in duration-300" data-testid="medical-records-list">
          {filtered.map((record) => (
            <Card key={record.id} data-testid={`medical-record-${record.id}`} className="transition-all duration-200 ease-in-out hover:-translate-y-0.5 hover:shadow-md">
              <CardHeader className="pb-2">
                <div className="flex items-start justify-between gap-2 flex-wrap">
                  <div>
                    <CardTitle className="text-sm font-semibold">
                      <Link
                        href={`/${locale}/patients/${record.patientId}`}
                        className="hover:underline"
                        data-testid={`record-patient-link-${record.id}`}
                      >
                        {record.patientName}
                      </Link>
                    </CardTitle>
                    <p className="text-xs text-muted-foreground mt-0.5">
                      {record.reason}
                    </p>
                  </div>
                  <Badge variant="outline" data-testid={`record-date-${record.id}`}>
                    {formatDate(record.visitDate)}
                  </Badge>
                </div>
              </CardHeader>
              <CardContent className="pt-0">
                <div className="grid grid-cols-2 sm:grid-cols-4 gap-3 text-xs mb-2">
                  <div>
                    <span className="text-muted-foreground block">{t('columns.vet')}</span>
                    <span className="font-medium">{record.vetName}</span>
                  </div>
                  <div>
                    <span className="text-muted-foreground block">{t('columns.weight')}</span>
                    <span className="font-medium">{record.weight} kg</span>
                  </div>
                  <div>
                    <span className="text-muted-foreground block">{t('columns.temperature')}</span>
                    <span className="font-medium">{record.temperature}&deg;C</span>
                  </div>
                  <div>
                    <span className="text-muted-foreground block">{t('columns.heart_rate')}</span>
                    <span className="font-medium">{record.heartRate} bpm</span>
                  </div>
                </div>
                <div className="text-sm">
                  <span className="font-medium">{t('columns.diagnosis')}: </span>
                  <span className="text-muted-foreground">{record.diagnosis}</span>
                </div>
              </CardContent>
            </Card>
          ))}
        </div>
      )}
    </div>
  )
}
