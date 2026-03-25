'use client'

import { Syringe, Stethoscope, Scissors, AlertTriangle, FileText } from 'lucide-react'
import type { MedicalRecordDto } from '@/lib/api/medical-records'
import { useTranslations } from 'next-intl'

function formatDateTime(dateStr: string): string {
  return new Date(dateStr).toLocaleDateString('en-AE', {
    timeZone: 'Asia/Dubai',
    day: '2-digit',
    month: 'short',
    year: 'numeric',
  })
}

type RecordType = 'vaccination' | 'surgery' | 'emergency' | 'checkup' | 'other'

function inferRecordType(reason: string): RecordType {
  const lower = reason.toLowerCase()
  if (lower.includes('vaccin') || lower.includes('immuniz')) return 'vaccination'
  if (lower.includes('surg') || lower.includes('operat') || lower.includes('spay') || lower.includes('neuter')) return 'surgery'
  if (lower.includes('emerg') || lower.includes('injur') || lower.includes('trauma') || lower.includes('wound') || lower.includes('accident')) return 'emergency'
  if (lower.includes('check') || lower.includes('routine') || lower.includes('exam') || lower.includes('annual') || lower.includes('wellness')) return 'checkup'
  return 'other'
}

function getAccentColor(type: RecordType): string {
  switch (type) {
    case 'vaccination': return 'bg-success'
    case 'surgery': return 'bg-rose-400'
    case 'emergency': return 'bg-amber-400'
    case 'checkup': return 'bg-blue-400'
    default: return 'bg-border'
  }
}

function RecordTypeIcon({ type, className }: { type: RecordType; className?: string }) {
  const cls = className ?? 'h-4 w-4'
  switch (type) {
    case 'vaccination': return <Syringe className={cls} data-testid="record-icon-vaccination" />
    case 'surgery': return <Scissors className={cls} data-testid="record-icon-surgery" />
    case 'emergency': return <AlertTriangle className={cls} data-testid="record-icon-emergency" />
    case 'checkup': return <Stethoscope className={cls} data-testid="record-icon-checkup" />
    default: return <FileText className={cls} data-testid="record-icon-other" />
  }
}

interface MedicalRecordsListProps {
  records: MedicalRecordDto[]
  isLoading?: boolean
}

function MedicalRecordRow({ record }: { record: MedicalRecordDto }) {
  const recordType = inferRecordType(record.reason)
  const accentColor = getAccentColor(recordType)

  return (
    <div
      data-testid={`medical-record-${record.id}`}
      className="relative border-b border-border/30 last:border-b-0 hover:bg-muted/50 transition-colors"
    >
      {/* Accent bar */}
      <div className={`absolute inset-y-0 start-0 w-1 ${accentColor}`} />

      {/* Main row: 4 equal columns */}
      <div className="grid grid-cols-4 gap-4 items-start ps-5 pe-4 py-3">
        {/* Reason + vet */}
        <div>
          <div className="flex items-center gap-1.5">
            <RecordTypeIcon type={recordType} className="h-4 w-4 text-muted-foreground shrink-0" />
            <p
              className="font-semibold text-[13px] text-foreground truncate"
              data-testid={`record-reason-${record.id}`}
            >
              {record.reason}
            </p>
          </div>
          <p
            className="text-[12px] text-muted-foreground mt-0.5 ps-[22px]"
            data-testid={`record-vet-${record.id}`}
          >
            {record.vetName}
          </p>
        </div>

        {/* Date */}
        <span
          className="text-[13px] text-muted-foreground"
          data-testid={`record-date-${record.id}`}
        >
          {formatDateTime(record.visitDate)}
        </span>

        {/* Vitals */}
        <div className="text-[12px] space-y-0.5" data-testid={`record-vitals-${record.id}`}>
          <span data-testid={`record-weight-${record.id}`} className="text-foreground font-semibold block">{record.weight} kg</span>
          <span data-testid={`record-temp-${record.id}`} className="text-foreground font-semibold block">{record.temperature}&deg;C</span>
          <span data-testid={`record-hr-${record.id}`} className="text-foreground font-semibold block">{record.heartRate} bpm</span>
        </div>

        {/* Diagnosis + treatment */}
        <div className="text-[13px] space-y-1">
          <p data-testid={`record-diagnosis-${record.id}`} className="text-muted-foreground truncate">{record.diagnosis}</p>
          <p data-testid={`record-treatment-${record.id}`} className="text-muted-foreground truncate">{record.treatment}</p>
          {record.prescription && (
            <p data-testid={`record-prescription-${record.id}`} className="text-muted-foreground truncate">{record.prescription}</p>
          )}
        </div>
      </div>
    </div>
  )
}

export function MedicalRecordsList({ records, isLoading }: MedicalRecordsListProps) {
  const t = useTranslations('medical_records.list')

  if (isLoading) {
    return (
      <div data-testid="medical-records-loading" className="space-y-3">
        {[1, 2, 3].map((i) => (
          <div key={i} className="h-32 rounded-xl bg-muted animate-pulse" style={{ animationDelay: `${(i - 1) * 100}ms` }} />
        ))}
      </div>
    )
  }

  if (records.length === 0) {
    return (
      <p
        className="text-muted-foreground text-[13px] py-8 text-center animate-in fade-in duration-300"
        data-testid="medical-records-empty"
      >
        {t('empty')}
      </p>
    )
  }

  return (
    <div data-testid="medical-records-list" className="bg-white border border-border/80 rounded-xl shadow-sm overflow-hidden w-full animate-in fade-in duration-300">
      {/* Header row */}
      <div className="grid grid-cols-4 gap-4 text-[11px] font-bold text-foreground uppercase tracking-wider px-4 ps-5 py-3 bg-muted border-b border-border/50">
        <span>{t('columns.reason')}</span>
        <span>{t('columns.date')}</span>
        <span>{t('columns.vitals')}</span>
        <span>{t('columns.diagnosis')}</span>
      </div>
      {records.map((record) => (
        <MedicalRecordRow key={record.id} record={record} />
      ))}
    </div>
  )
}
