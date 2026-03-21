'use client'

import { useState, useEffect, useCallback, useMemo } from 'react'
import Link from 'next/link'
import { format } from 'date-fns'
import { CalendarClock, Search } from 'lucide-react'
import {
  useReactTable,
  getCoreRowModel,
  flexRender,
  type ColumnDef,
} from '@tanstack/react-table'
import { Button } from '@/components/ui/button'
import { Input } from '@/components/ui/input'
import { Skeleton } from '@/components/ui/skeleton'
import { ErrorState } from '@/components/ui/error-state'
import {
  Select,
  SelectContent,
  SelectItem,
  SelectTrigger,
  SelectValue,
} from '@/components/ui/select'
import {
  Table,
  TableBody,
  TableCell,
  TableHead,
  TableHeader,
  TableRow,
} from '@/components/ui/table'
import { StatusBadge } from './StatusBadge'
import { EmptyState } from '@/components/features/onboarding/EmptyState'
import { LtrText } from '@/components/ui/ltr-text'
import { getAppointments } from '@/lib/api/appointments'
import type { AppointmentDto, AppointmentStatus } from '@/lib/api/appointments'
import { useTranslations, useLocale } from 'next-intl'

const SPECIES_ICONS: Record<string, string> = {
  Dog: '🐕',
  Cat: '🐈',
  Bird: '🦜',
  Rabbit: '🐇',
  Horse: '🐎',
  Exotic: '🦎',
}

const STATUS_KEYS: { value: AppointmentStatus | 'ALL'; key: string }[] = [
  { value: 'ALL', key: 'all_statuses' },
  { value: 'SCHEDULED', key: 'status.SCHEDULED' },
  { value: 'CHECKED_IN', key: 'status.CHECKED_IN' },
  { value: 'IN_PROGRESS', key: 'status.IN_PROGRESS' },
  { value: 'COMPLETED', key: 'status.COMPLETED' },
  { value: 'CANCELLED', key: 'status.CANCELLED' },
]

const PAGE_SIZE = 10

export function AppointmentsTable() {
  const t = useTranslations('appointments')
  const tEmpty = useTranslations('onboarding.empty.appointments')
  const locale = useLocale()
  const [appointments, setAppointments] = useState<AppointmentDto[]>([])
  const [totalCount, setTotalCount] = useState(0)
  const [page, setPage] = useState(1)
  const [statusFilter, setStatusFilter] = useState<AppointmentStatus | 'ALL'>('ALL')
  const [dateFilter, setDateFilter] = useState<string>('')
  const [searchQuery, setSearchQuery] = useState('')
  const [isLoading, setIsLoading] = useState(true)
  const [error, setError] = useState<string | null>(null)

  const load = useCallback(async () => {
    setIsLoading(true)
    setError(null)
    try {
      const result = await getAppointments({
        status: statusFilter !== 'ALL' ? statusFilter : undefined,
        date: dateFilter || undefined,
        page,
        pageSize: PAGE_SIZE,
      })
      setAppointments(result.items)
      setTotalCount(result.totalCount)
    } catch {
      setError(t('error_load'))
    } finally {
      setIsLoading(false)
    }
  }, [statusFilter, dateFilter, page])

  useEffect(() => {
    load()
  }, [load])

  const filteredAppointments = useMemo(() => {
    if (!searchQuery.trim()) return appointments
    const q = searchQuery.toLowerCase()
    return appointments.filter(
      (a) =>
        a.patientName.toLowerCase().includes(q) ||
        a.ownerName.toLowerCase().includes(q)
    )
  }, [appointments, searchQuery])

  const columns: ColumnDef<AppointmentDto>[] = [
    {
      accessorKey: 'scheduledAt',
      header: t('columns.datetime'),
      cell: ({ getValue }) => {
        const val = getValue<string>()
        return (
          <span data-testid="cell-datetime" className="text-[13px] text-foreground">
            <LtrText>{format(new Date(val), 'dd MMM yyyy HH:mm')}</LtrText>
          </span>
        )
      },
    },
    {
      id: 'patient',
      header: t('columns.patient'),
      cell: ({ row }) => (
        <span data-testid="cell-patient" className="text-[13px] font-medium text-foreground">
          {SPECIES_ICONS[row.original.species] ?? '🐾'} {row.original.patientName}
        </span>
      ),
    },
    {
      accessorKey: 'ownerName',
      header: t('columns.owner'),
      cell: ({ getValue }) => (
        <span data-testid="cell-owner" className="text-[13px] text-foreground">{getValue<string>()}</span>
      ),
    },
    {
      accessorKey: 'vetName',
      header: t('columns.vet'),
      cell: ({ getValue }) => (
        <span data-testid="cell-vet" className="text-[13px] text-foreground">{getValue<string>()}</span>
      ),
    },
    {
      accessorKey: 'status',
      header: t('columns.status'),
      cell: ({ getValue }) => (
        <StatusBadge status={getValue<AppointmentStatus>()} />
      ),
    },
    {
      id: 'actions',
      header: t('columns.actions'),
      cell: ({ row }) => (
        <Link href={`/appointments/${row.original.id}`}>
          <Button
            variant="outline"
            size="sm"
            data-testid={`btn-view-${row.original.id}`}
            className="rounded-xl font-semibold border-border/80 hover:bg-muted text-[13px]"
          >
            {t('columns.view')}
          </Button>
        </Link>
      ),
    },
  ]

  const table = useReactTable({
    data: filteredAppointments,
    columns,
    getCoreRowModel: getCoreRowModel(),
    manualPagination: true,
    rowCount: totalCount,
  })

  const totalPages = Math.ceil(totalCount / PAGE_SIZE)

  return (
    <div className="space-y-4">
      {/* Filters */}
      <div className="flex flex-wrap gap-3 items-center">
        <div className="relative">
          <Search className="absolute left-2.5 top-2.5 h-4 w-4 text-muted-foreground" />
          <Input
            data-testid="appointment-search"
            placeholder={t('search_placeholder') ?? 'Search patient or owner...'}
            className="w-64 pl-9 rounded-xl border-border/80 text-[13px] focus:ring-2 focus:ring-primary/20 focus:border-primary/50"
            value={searchQuery}
            onChange={(e) => setSearchQuery(e.target.value)}
          />
        </div>
        <Select
          value={statusFilter}
          onValueChange={(val) => {
            setStatusFilter(val as AppointmentStatus | 'ALL')
            setPage(1)
          }}
        >
          <SelectTrigger className="w-48 rounded-xl border-border/80 text-[13px]" data-testid="status-filter" aria-label="Filter by status">
            <SelectValue placeholder={t('all_statuses')} />
          </SelectTrigger>
          <SelectContent>
            {STATUS_KEYS.map((opt) => (
              <SelectItem
                key={opt.value}
                value={opt.value}
                data-testid={`status-option-${opt.value.toLowerCase()}`}
              >
                {t(opt.key)}
              </SelectItem>
            ))}
          </SelectContent>
        </Select>

        <Input
          type="date"
          className="w-40 rounded-xl border-border/80 text-[13px] focus:ring-2 focus:ring-primary/20 focus:border-primary/50"
          data-testid="date-filter"
          aria-label="Filter by date"
          value={dateFilter}
          onChange={(e) => {
            setDateFilter(e.target.value)
            setPage(1)
          }}
        />

        {dateFilter && (
          <Button
            variant="ghost"
            size="sm"
            data-testid="btn-clear-date"
            onClick={() => setDateFilter('')}
          >
            {t('clear_date')}
          </Button>
        )}
      </div>

      {/* Mobile card layout */}
      <div className="md:hidden space-y-3" data-testid="appointments-cards">
        {isLoading ? (
          Array.from({ length: 3 }).map((_, i) => (
            <div key={i} className="rounded-xl border border-border/80 bg-white p-4 space-y-2">
              <Skeleton className="h-4 w-3/4" />
              <Skeleton className="h-3 w-1/2" />
              <Skeleton className="h-3 w-1/3" />
            </div>
          ))
        ) : error ? (
          <ErrorState
            data-testid="appointments-error-mobile"
            title={t('error_load')}
            description={error}
            onRetry={load}
          />
        ) : table.getRowModel().rows.length === 0 ? (
          <EmptyState
            icon={<CalendarClock className="h-16 w-16" />}
            title={tEmpty('title')}
            description={tEmpty('description')}
            primaryCta={{ label: tEmpty('cta'), href: `/${locale}/appointments/new` }}
            tip={tEmpty('tip')}
            data-testid-prefix="appointments"
          />
        ) : (
          table.getRowModel().rows.map((row) => (
            <Link
              key={row.id}
              href={`/${locale}/appointments/${row.original.id}`}
              data-testid={`appointment-card-${row.original.id}`}
              className="block rounded-xl border border-border/80 bg-white p-4 hover:shadow-md transition-all duration-200 min-h-[44px]"
            >
              <div className="flex items-start justify-between gap-2">
                <div className="min-w-0 flex-1">
                  <p className="font-semibold text-[14px] text-foreground truncate">
                    {SPECIES_ICONS[row.original.species] ?? '🐾'} {row.original.patientName}
                  </p>
                  <p className="text-[13px] text-muted-foreground mt-0.5">{row.original.ownerName}</p>
                </div>
                <StatusBadge status={row.original.status} />
              </div>
              <div className="mt-2 flex items-center gap-3 text-xs text-muted-foreground">
                <span><LtrText>{format(new Date(row.original.scheduledAt), 'dd MMM yyyy HH:mm')}</LtrText></span>
                <span>{row.original.vetName}</span>
              </div>
            </Link>
          ))
        )}
      </div>

      {/* Desktop table */}
      <div className="hidden md:block bg-white border border-border/80 rounded-xl shadow-sm overflow-hidden" data-testid="appointments-table">
        <Table>
          <TableHeader className="bg-muted">
            {table.getHeaderGroups().map((headerGroup) => (
              <TableRow key={headerGroup.id}>
                {headerGroup.headers.map((header) => (
                  <TableHead key={header.id} className="text-[11px] font-bold uppercase tracking-wider text-foreground">
                    {flexRender(header.column.columnDef.header, header.getContext())}
                  </TableHead>
                ))}
              </TableRow>
            ))}
          </TableHeader>
          <TableBody>
            {isLoading ? (
              Array.from({ length: 5 }).map((_, i) => (
                <TableRow key={i} data-testid={`appointment-skeleton-row-${i}`}>
                  {Array.from({ length: columns.length }).map((_, j) => (
                    <TableCell key={j}>
                      <Skeleton className="h-4 w-full" />
                    </TableCell>
                  ))}
                </TableRow>
              ))
            ) : error ? (
              <TableRow>
                <TableCell colSpan={columns.length} className="p-0">
                  <ErrorState
                    data-testid="appointments-error"
                    title={t('error_load')}
                    description={error}
                    onRetry={load}
                  />
                </TableCell>
              </TableRow>
            ) : table.getRowModel().rows.length === 0 ? (
              <TableRow>
                <TableCell colSpan={columns.length} className="p-0">
                  <EmptyState
                    icon={<CalendarClock className="h-16 w-16" />}
                    title={tEmpty('title')}
                    description={tEmpty('description')}
                    primaryCta={{ label: tEmpty('cta'), href: `/${locale}/appointments/new` }}
                    tip={tEmpty('tip')}
                    data-testid-prefix="appointments"
                  />
                </TableCell>
              </TableRow>
            ) : (
              table.getRowModel().rows.map((row) => (
                <TableRow key={row.id} data-testid={`appointment-row-${row.original.id}`} className="hover:bg-muted/50 border-border/30 transition-colors">
                  {row.getVisibleCells().map((cell) => (
                    <TableCell key={cell.id}>
                      {flexRender(cell.column.columnDef.cell, cell.getContext())}
                    </TableCell>
                  ))}
                </TableRow>
              ))
            )}
          </TableBody>
        </Table>
      </div> {/* end hidden md:block */}

      {/* Pagination */}
      {totalPages > 1 && (
        <div className="flex items-center justify-between" data-testid="pagination">
          <p className="text-[13px] text-muted-foreground">
            {t('page_of', { page, total: totalPages, count: totalCount })}
          </p>
          <div className="flex gap-2">
            <Button
              variant="outline"
              size="sm"
              data-testid="btn-prev-page"
              onClick={() => setPage((p) => Math.max(1, p - 1))}
              disabled={page === 1}
              className="rounded-xl font-semibold border-border/80 hover:bg-muted"
            >
              {t('previous')}
            </Button>
            <Button
              variant="outline"
              size="sm"
              data-testid="btn-next-page"
              onClick={() => setPage((p) => Math.min(totalPages, p + 1))}
              disabled={page === totalPages}
              className="rounded-xl font-semibold border-border/80 hover:bg-muted"
            >
              {t('next')}
            </Button>
          </div>
        </div>
      )}
    </div>
  )
}
