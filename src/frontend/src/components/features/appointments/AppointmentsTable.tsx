'use client'

import { useState, useEffect, useCallback } from 'react'
import Link from 'next/link'
import { format } from 'date-fns'
import { CalendarClock } from 'lucide-react'
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
import { getAppointments } from '@/lib/api/appointments'
import type { AppointmentDto, AppointmentStatus } from '@/lib/api/appointments'
import { useTranslations } from 'next-intl'

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
  const [appointments, setAppointments] = useState<AppointmentDto[]>([])
  const [totalCount, setTotalCount] = useState(0)
  const [page, setPage] = useState(1)
  const [statusFilter, setStatusFilter] = useState<AppointmentStatus | 'ALL'>('ALL')
  const [dateFilter, setDateFilter] = useState<string>('')
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

  const columns: ColumnDef<AppointmentDto>[] = [
    {
      accessorKey: 'scheduledAt',
      header: t('columns.datetime'),
      cell: ({ getValue }) => {
        const val = getValue<string>()
        return (
          <span data-testid="cell-datetime">
            {format(new Date(val), 'dd MMM yyyy HH:mm')}
          </span>
        )
      },
    },
    {
      id: 'patient',
      header: t('columns.patient'),
      cell: ({ row }) => (
        <span data-testid="cell-patient">
          {SPECIES_ICONS[row.original.species] ?? '🐾'} {row.original.patientName}
        </span>
      ),
    },
    {
      accessorKey: 'ownerName',
      header: t('columns.owner'),
      cell: ({ getValue }) => (
        <span data-testid="cell-owner">{getValue<string>()}</span>
      ),
    },
    {
      accessorKey: 'vetName',
      header: t('columns.vet'),
      cell: ({ getValue }) => (
        <span data-testid="cell-vet">{getValue<string>()}</span>
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
          >
            {t('columns.view')}
          </Button>
        </Link>
      ),
    },
  ]

  const table = useReactTable({
    data: appointments,
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
        <Select
          value={statusFilter}
          onValueChange={(val) => {
            setStatusFilter(val as AppointmentStatus | 'ALL')
            setPage(1)
          }}
        >
          <SelectTrigger className="w-48" data-testid="status-filter" aria-label="Filter by status">
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
          className="w-40"
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

      {/* Table */}
      <div className="rounded-md border" data-testid="appointments-table">
        <Table>
          <TableHeader>
            {table.getHeaderGroups().map((headerGroup) => (
              <TableRow key={headerGroup.id}>
                {headerGroup.headers.map((header) => (
                  <TableHead key={header.id}>
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
                    primaryCta={{ label: tEmpty('cta'), href: '/appointments/new' }}
                    tip={tEmpty('tip')}
                    data-testid-prefix="appointments"
                  />
                </TableCell>
              </TableRow>
            ) : (
              table.getRowModel().rows.map((row) => (
                <TableRow key={row.id} data-testid={`appointment-row-${row.original.id}`}>
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
      </div>

      {/* Pagination */}
      {totalPages > 1 && (
        <div className="flex items-center justify-between" data-testid="pagination">
          <p className="text-sm text-muted-foreground">
            {t('page_of', { page, total: totalPages, count: totalCount })}
          </p>
          <div className="flex gap-2">
            <Button
              variant="outline"
              size="sm"
              data-testid="btn-prev-page"
              onClick={() => setPage((p) => Math.max(1, p - 1))}
              disabled={page === 1}
            >
              {t('previous')}
            </Button>
            <Button
              variant="outline"
              size="sm"
              data-testid="btn-next-page"
              onClick={() => setPage((p) => Math.min(totalPages, p + 1))}
              disabled={page === totalPages}
            >
              {t('next')}
            </Button>
          </div>
        </div>
      )}
    </div>
  )
}
