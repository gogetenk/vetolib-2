'use client'

import { useEffect, useState, useCallback } from 'react'
import { Card, CardContent, CardHeader, CardTitle } from '@/components/ui/card'
import { Skeleton } from '@/components/ui/skeleton'
import { getAccumulatedValue, type AccumulatedValueDto } from '@/lib/api/dashboard'
import { ErrorState } from '@/components/ui/error-state'
import { useTranslations } from 'next-intl'
import { Users, FileText, Receipt, CalendarCheck } from 'lucide-react'

export function AccumulatedValueCard() {
  const t = useTranslations('dashboard.accumulated_value')
  const [data, setData] = useState<AccumulatedValueDto | null>(null)
  const [loading, setLoading] = useState(true)
  const [error, setError] = useState<string | null>(null)

  const load = useCallback(async () => {
    try {
      const result = await getAccumulatedValue()
      setData(result)
      setError(null)
    } catch {
      setError('Failed to load accumulated value')
    } finally {
      setLoading(false)
    }
  }, [])

  useEffect(() => {
    load()
  }, [load])

  if (error) {
    return (
      <ErrorState
        data-testid="accumulated-value-error"
        title="Failed to load clinic data"
        description="Could not retrieve your Vetara data."
        onRetry={load}
      />
    )
  }

  function formatMemberSince(isoDate: string): string {
    const date = new Date(isoDate)
    return date.toLocaleDateString('en-AE', {
      month: 'long',
      year: 'numeric',
      timeZone: 'Asia/Dubai',
    })
  }

  const counters = [
    {
      key: 'patients' as const,
      label: t('patients'),
      value: data?.totalPatients ?? 0,
      icon: Users,
      color: 'text-primary',
      bg: 'bg-primary/10',
    },
    {
      key: 'medical_records' as const,
      label: t('medical_records'),
      value: data?.totalMedicalRecords ?? 0,
      icon: FileText,
      color: 'text-blue-500',
      bg: 'bg-blue-50',
    },
    {
      key: 'invoices' as const,
      label: t('invoices'),
      value: data?.totalInvoices ?? 0,
      icon: Receipt,
      color: 'text-amber-500',
      bg: 'bg-amber-50',
    },
    {
      key: 'appointments' as const,
      label: t('appointments'),
      value: data?.totalAppointments ?? 0,
      icon: CalendarCheck,
      color: 'text-green-500',
      bg: 'bg-green-50',
    },
  ]

  return (
    <Card
      className="border-border/80 shadow-sm"
      data-testid="accumulated-value-card"
    >
      <CardHeader className="pb-3">
        <CardTitle
          className="text-[15px] font-semibold text-foreground"
          data-testid="accumulated-value-title"
        >
          {t('title')}
        </CardTitle>
        {loading ? (
          <Skeleton className="h-4 w-40" />
        ) : data ? (
          <p
            className="text-[12px] text-muted-foreground"
            data-testid="accumulated-value-member-since"
          >
            {t('member_since', { date: formatMemberSince(data.memberSince) })}
          </p>
        ) : null}
      </CardHeader>
      <CardContent>
        <div
          className="grid grid-cols-2 gap-4 sm:grid-cols-4"
          data-testid="accumulated-value-counters"
        >
          {counters.map((counter) => (
            <div
              key={counter.key}
              className="flex flex-col items-center gap-1.5 rounded-lg border border-border/50 p-3"
              data-testid={`accumulated-value-${counter.key}`}
            >
              <div
                className={`flex h-8 w-8 items-center justify-center rounded-lg ${counter.bg}`}
              >
                <counter.icon
                  className={`h-4 w-4 ${counter.color}`}
                  aria-hidden="true"
                />
              </div>
              {loading ? (
                <Skeleton className="h-7 w-12" />
              ) : (
                <p
                  className="text-xl font-bold text-foreground"
                  data-testid={`accumulated-value-${counter.key}-value`}
                >
                  {counter.value.toLocaleString()}
                </p>
              )}
              <p className="text-[11px] text-muted-foreground text-center font-medium">
                {counter.label}
              </p>
            </div>
          ))}
        </div>
      </CardContent>
    </Card>
  )
}
