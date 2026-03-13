'use client'

import type { MedicalRecordDto } from '@/lib/api/medical-records'
import { Card, CardContent } from '@/components/ui/card'
import { Badge } from '@/components/ui/badge'
import { Separator } from '@/components/ui/separator'

function formatDateTime(dateStr: string): string {
  return new Date(dateStr).toLocaleDateString('en-AE', {
    timeZone: 'Asia/Dubai',
    day: '2-digit',
    month: 'short',
    year: 'numeric',
  })
}

interface MedicalRecordsListProps {
  records: MedicalRecordDto[]
  isLoading?: boolean
}

function MedicalRecordItem({ record }: { record: MedicalRecordDto }) {
  return (
    <Card
      data-testid={`medical-record-${record.id}`}
      className="mb-3 transition-all duration-200 ease-in-out hover:-translate-y-0.5 hover:shadow-md"
    >
      <CardContent className="p-4">
        <div className="flex items-start justify-between gap-2 flex-wrap">
          <div>
            <p
              className="font-semibold text-sm"
              data-testid={`record-reason-${record.id}`}
            >
              {record.reason}
            </p>
            <p
              className="text-xs text-muted-foreground mt-0.5"
              data-testid={`record-vet-${record.id}`}
            >
              {record.vetName}
            </p>
          </div>
          <Badge
            variant="outline"
            data-testid={`record-date-${record.id}`}
          >
            {formatDateTime(record.visitDate)}
          </Badge>
        </div>

        <Separator className="my-3" />

        <div className="grid grid-cols-3 gap-3 text-xs mb-3">
          <div data-testid={`record-weight-${record.id}`}>
            <span className="text-muted-foreground block">Weight</span>
            <span className="font-medium">{record.weight} kg</span>
          </div>
          <div data-testid={`record-temp-${record.id}`}>
            <span className="text-muted-foreground block">Temperature</span>
            <span className="font-medium">{record.temperature}°C</span>
          </div>
          <div data-testid={`record-hr-${record.id}`}>
            <span className="text-muted-foreground block">Heart Rate</span>
            <span className="font-medium">{record.heartRate} bpm</span>
          </div>
        </div>

        <div className="space-y-2 text-sm">
          <div data-testid={`record-diagnosis-${record.id}`}>
            <span className="font-medium">Diagnosis: </span>
            <span className="text-muted-foreground">{record.diagnosis}</span>
          </div>
          <div data-testid={`record-treatment-${record.id}`}>
            <span className="font-medium">Treatment: </span>
            <span className="text-muted-foreground">{record.treatment}</span>
          </div>
          {record.prescription && (
            <div data-testid={`record-prescription-${record.id}`}>
              <span className="font-medium">Prescription: </span>
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
        className="text-muted-foreground text-sm py-8 text-center animate-in fade-in duration-300"
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
