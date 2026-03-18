'use client'

import { Syringe, Stethoscope, Scissors, AlertTriangle, FileText } from 'lucide-react'
import type { MedicalRecordDto } from '@/lib/api/medical-records'
import { Card, CardContent } from '@/components/ui/card'

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

function getRecordAccent(type: RecordType): string {
  switch (type) {
    case 'vaccination': return 'border-emerald-400'
    case 'surgery': return 'border-rose-400'
    case 'emergency': return 'border-amber-400'
    case 'checkup': return 'border-blue-400'
    default: return 'border-stone-300'
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

function MedicalRecordItem({ record }: { record: MedicalRecordDto }) {
  const recordType = inferRecordType(record.reason)
  const accentClass = getRecordAccent(recordType)

  return (
    <Card
      data-testid={`medical-record-${record.id}`}
      className={`mb-3 bg-white border border-border/80 rounded-xl shadow-sm transition-all duration-200 ease-in-out hover:-translate-y-0.5 hover:shadow-md border-s-4 ${accentClass}`}
    >
      <CardContent className="p-4">
        {/* Title row with icon + date below */}
        <div className="flex items-start justify-between gap-2">
          <div>
            <div className="flex items-center gap-1.5">
              <RecordTypeIcon type={recordType} className="h-4 w-4 text-muted-foreground" />
              <p
                className="font-semibold text-[14px] text-[#061e44]"
                data-testid={`record-reason-${record.id}`}
              >
                {record.reason}
              </p>
            </div>
            <p
              className="text-[12px] text-muted-foreground mt-0.5"
              data-testid={`record-date-${record.id}`}
            >
              {formatDateTime(record.visitDate)}
            </p>
            <p
              className="text-[12px] text-muted-foreground"
              data-testid={`record-vet-${record.id}`}
            >
              {record.vetName}
            </p>
          </div>
        </div>

        {/* Vitals row (S8) */}
        <div
          className="bg-[#f4f6f9] rounded-xl px-3 py-2 mt-3 grid grid-cols-3 gap-3"
          data-testid={`record-vitals-${record.id}`}
        >
          <div data-testid={`record-weight-${record.id}`}>
            <span className="text-[11px] font-bold uppercase tracking-wider text-muted-foreground block">Weight</span>
            <span className="text-[13px] font-semibold text-[#061e44]">{record.weight} kg</span>
          </div>
          <div data-testid={`record-temp-${record.id}`}>
            <span className="text-[11px] font-bold uppercase tracking-wider text-muted-foreground block">Temperature</span>
            <span className="text-[13px] font-semibold text-[#061e44]">{record.temperature}&deg;C</span>
          </div>
          <div data-testid={`record-hr-${record.id}`}>
            <span className="text-[11px] font-bold uppercase tracking-wider text-muted-foreground block">Heart Rate</span>
            <span className="text-[13px] font-semibold text-[#061e44]">{record.heartRate} bpm</span>
          </div>
        </div>

        {/* Diagnosis + treatment */}
        <div className="space-y-1.5 text-[13px] mt-3">
          <div data-testid={`record-diagnosis-${record.id}`}>
            <span className="font-semibold text-[#061e44]">Diagnosis: </span>
            <span className="text-muted-foreground">{record.diagnosis}</span>
          </div>
          <div data-testid={`record-treatment-${record.id}`}>
            <span className="font-semibold text-[#061e44]">Treatment: </span>
            <span className="text-muted-foreground">{record.treatment}</span>
          </div>
          {record.prescription && (
            <div data-testid={`record-prescription-${record.id}`}>
              <span className="font-semibold text-[#061e44]">Prescription: </span>
              <span className="text-muted-foreground">{record.prescription}</span>
            </div>
          )}
        </div>
      </CardContent>
    </Card>
  )
}

export function MedicalRecordsList({ records, isLoading }: MedicalRecordsListProps) {
  if (isLoading) {
    return (
      <div data-testid="medical-records-loading" className="space-y-3">
        {[1, 2, 3].map((i) => (
          <div key={i} className="h-32 rounded-lg bg-muted animate-pulse" style={{ animationDelay: `${(i - 1) * 100}ms` }} />
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
        No medical records found.
      </p>
    )
  }

  return (
    <div data-testid="medical-records-list" className="animate-in fade-in duration-300">
      {records.map((record) => (
        <MedicalRecordItem key={record.id} record={record} />
      ))}
    </div>
  )
}
