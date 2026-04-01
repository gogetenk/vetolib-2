'use client'

import { useEffect, useState, useCallback } from 'react'
import { ClipboardList, Plus, Trash2 } from 'lucide-react'
import { Button } from '@/components/ui/button'
import { Badge } from '@/components/ui/badge'
import { Card, CardContent, CardHeader, CardTitle } from '@/components/ui/card'
import {
  Table,
  TableBody,
  TableCell,
  TableHead,
  TableHeader,
  TableRow,
} from '@/components/ui/table'
import { EmptyState } from '@/components/features/onboarding/EmptyState'
import { ErrorState } from '@/components/ui/error-state'
import { Skeleton } from '@/components/ui/skeleton'
import { cn } from '@/lib/utils'
import { formatDate } from '@/lib/utils'
import { getWaitlistEntries, deleteWaitlistEntry } from '@/lib/api/waitlist'
import type { WaitlistEntryDto, WaitlistPriority } from '@/lib/api/waitlist'
import { useTranslations } from 'next-intl'
import { AddWaitlistDialog } from './AddWaitlistDialog'

const PRIORITY_STYLES: Record<WaitlistPriority, string> = {
  LOW: 'bg-muted text-muted-foreground',
  NORMAL: 'bg-primary/10 text-primary',
  HIGH: 'bg-amber-50 text-amber-700',
  URGENT: 'bg-destructive/10 text-destructive',
}

function PriorityBadge({ priority }: { priority: WaitlistPriority }) {
  const t = useTranslations('waitlist.priority')
  const style = PRIORITY_STYLES[priority] ?? 'bg-muted text-muted-foreground'
  return (
    <Badge
      className={cn('text-[10px] font-bold uppercase tracking-wider border-0', style)}
      data-testid={`waitlist-priority-${priority.toLowerCase()}`}
    >
      {t(priority)}
    </Badge>
  )
}

function TableSkeleton() {
  return (
    <div className="space-y-3" data-testid="waitlist-skeleton">
      {Array.from({ length: 4 }).map((_, i) => (
        <Skeleton key={i} className="h-12 w-full rounded-lg" />
      ))}
    </div>
  )
}

export function WaitlistTable() {
  const t = useTranslations('waitlist')
  const tEmpty = useTranslations('waitlist.empty')

  const [entries, setEntries] = useState<WaitlistEntryDto[]>([])
  const [loading, setLoading] = useState(true)
  const [error, setError] = useState<string | null>(null)
  const [dialogOpen, setDialogOpen] = useState(false)

  const loadEntries = useCallback(async () => {
    setLoading(true)
    setError(null)
    try {
      const data = await getWaitlistEntries()
      setEntries(data)
    } catch {
      setError(t('failed_to_load'))
    } finally {
      setLoading(false)
    }
  }, [t])

  useEffect(() => {
    loadEntries()
  }, [loadEntries])

  const handleRemove = useCallback(async (id: string) => {
    try {
      await deleteWaitlistEntry(id)
      setEntries((prev) => prev.filter((e) => e.id !== id))
    } catch {
      setError(t('failed_to_delete'))
    }
  }, [t])

  const handleAdded = useCallback(() => {
    setDialogOpen(false)
    loadEntries()
  }, [loadEntries])

  if (loading) {
    return (
      <Card>
        <CardHeader>
          <CardTitle>{t('title')}</CardTitle>
        </CardHeader>
        <CardContent>
          <TableSkeleton />
        </CardContent>
      </Card>
    )
  }

  if (error) {
    return (
      <ErrorState
        data-testid="waitlist-error"
        title={t('error_title')}
        description={error}
        onRetry={loadEntries}
      />
    )
  }

  if (entries.length === 0) {
    return (
      <>
        <EmptyState
          icon={<ClipboardList className="h-12 w-12 text-muted-foreground" />}
          title={tEmpty('title')}
          description={tEmpty('description')}
          primaryCta={{
            label: tEmpty('cta'),
            onClick: () => setDialogOpen(true),
            'data-testid': 'waitlist-add-empty',
          }}
          data-testid-prefix="waitlist"
        />
        <AddWaitlistDialog
          open={dialogOpen}
          onOpenChange={setDialogOpen}
          onAdded={handleAdded}
        />
      </>
    )
  }

  return (
    <>
      <Card data-testid="waitlist-card">
        <CardHeader className="flex flex-row items-center justify-between">
          <CardTitle className="text-lg font-semibold">{t('title')}</CardTitle>
          <Button
            size="sm"
            data-testid="waitlist-add-button"
            onClick={() => setDialogOpen(true)}
          >
            <Plus className="h-4 w-4 ltr:mr-1 rtl:ml-1" />
            {t('add')}
          </Button>
        </CardHeader>
        <CardContent>
          <Table data-testid="waitlist-table">
            <TableHeader>
              <TableRow>
                <TableHead>{t('col_patient')}</TableHead>
                <TableHead>{t('col_reason')}</TableHead>
                <TableHead>{t('col_preferred_time')}</TableHead>
                <TableHead>{t('col_priority')}</TableHead>
                <TableHead>{t('col_date_added')}</TableHead>
                <TableHead className="w-[60px]" />
              </TableRow>
            </TableHeader>
            <TableBody>
              {entries.map((entry) => (
                <TableRow key={entry.id} data-testid={`waitlist-row-${entry.id}`}>
                  <TableCell>
                    <div>
                      <span className="font-medium" data-testid={`waitlist-patient-${entry.id}`}>
                        {entry.patientName}
                      </span>
                      <p className="text-xs text-muted-foreground">{entry.ownerName}</p>
                    </div>
                  </TableCell>
                  <TableCell data-testid={`waitlist-reason-${entry.id}`}>
                    {entry.reason}
                  </TableCell>
                  <TableCell data-testid={`waitlist-time-${entry.id}`}>
                    {entry.preferredDay} &middot; {entry.preferredTime}
                  </TableCell>
                  <TableCell>
                    <PriorityBadge priority={entry.priority} />
                  </TableCell>
                  <TableCell className="text-muted-foreground text-sm">
                    {formatDate(entry.createdAt)}
                  </TableCell>
                  <TableCell>
                    <Button
                      variant="ghost"
                      size="icon"
                      data-testid={`waitlist-remove-${entry.id}`}
                      onClick={() => handleRemove(entry.id)}
                      aria-label={t('remove')}
                    >
                      <Trash2 className="h-4 w-4 text-destructive" />
                    </Button>
                  </TableCell>
                </TableRow>
              ))}
            </TableBody>
          </Table>
        </CardContent>
      </Card>
      <AddWaitlistDialog
        open={dialogOpen}
        onOpenChange={setDialogOpen}
        onAdded={handleAdded}
      />
    </>
  )
}
