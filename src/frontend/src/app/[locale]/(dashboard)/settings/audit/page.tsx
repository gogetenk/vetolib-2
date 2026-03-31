"use client"

import { useEffect, useState, useCallback } from "react"
import { toast } from "sonner"
import { useTranslations } from "next-intl"
import { useRole } from "@/hooks/use-role"
import { usePathname, useRouter } from "next/navigation"
import { getAuditEntries } from "@/lib/api/audit"
import type { AuditEntryDto } from "@/lib/api/audit"
import { PageContainer } from "@/components/ui/page-container"
import { Button } from "@/components/ui/button"
import { Input } from "@/components/ui/input"
import { Skeleton } from "@/components/ui/skeleton"
import { Badge } from "@/components/ui/badge"
import {
  Table,
  TableBody,
  TableCell,
  TableHead,
  TableHeader,
  TableRow,
} from "@/components/ui/table"
import {
  Select,
  SelectTrigger,
  SelectValue,
  SelectContent,
  SelectItem,
} from "@/components/ui/select"
import { ChevronDown, ChevronRight, ChevronLeft, ChevronsLeft, ChevronsRight, Shield } from "lucide-react"

const PAGE_SIZE = 20
const ACTION_TYPES = ["CREATE", "UPDATE", "DELETE"] as const

function formatDate(iso: string, locale: string): string {
  return new Date(iso).toLocaleString(locale === "ar" ? "ar-AE" : locale === "fr" ? "fr-FR" : "en-AE", {
    year: "numeric",
    month: "short",
    day: "numeric",
    hour: "2-digit",
    minute: "2-digit",
    timeZone: "Asia/Dubai",
  })
}

function ActionBadge({ action, t }: { action: string; t: (key: string) => string }) {
  const variant = action === "CREATE" ? "default" : action === "DELETE" ? "destructive" : "secondary"
  return (
    <Badge variant={variant} data-testid={`audit-action-badge-${action.toLowerCase()}`}>
      {t(`actions.${action.toLowerCase()}`)}
    </Badge>
  )
}

function DiffView({ oldValues, newValues, t }: { oldValues: Record<string, unknown> | null; newValues: Record<string, unknown> | null; t: (key: string) => string }) {
  const allKeys = new Set([
    ...Object.keys(oldValues ?? {}),
    ...Object.keys(newValues ?? {}),
  ])

  if (allKeys.size === 0) {
    return <p className="text-[12px] text-muted-foreground italic">{t("no_details")}</p>
  }

  return (
    <div className="grid gap-1" data-testid="audit-diff-view">
      <div className="grid grid-cols-3 gap-2 text-[11px] font-semibold text-muted-foreground uppercase tracking-wide pb-1 border-b border-border/50">
        <span>{t("field")}</span>
        <span>{t("old_value")}</span>
        <span>{t("new_value")}</span>
      </div>
      {Array.from(allKeys).map((key) => {
        const oldVal = oldValues?.[key]
        const newVal = newValues?.[key]
        return (
          <div key={key} className="grid grid-cols-3 gap-2 text-[12px] py-0.5">
            <span className="font-medium text-foreground">{key}</span>
            <span className="text-red-500/80 line-through">
              {oldVal !== undefined && oldVal !== null ? String(oldVal) : "-"}
            </span>
            <span className="text-green-600">
              {newVal !== undefined && newVal !== null ? String(newVal) : "-"}
            </span>
          </div>
        )
      })}
    </div>
  )
}

export default function AuditTrailPage() {
  const t = useTranslations("audit_trail")
  const role = useRole()
  const pathname = usePathname()
  const router = useRouter()

  const [entries, setEntries] = useState<AuditEntryDto[]>([])
  const [totalCount, setTotalCount] = useState(0)
  const [page, setPage] = useState(1)
  const [isLoading, setIsLoading] = useState(true)
  const [expandedRow, setExpandedRow] = useState<string | null>(null)

  // Filters
  const [startDate, setStartDate] = useState("")
  const [endDate, setEndDate] = useState("")
  const [userFilter, setUserFilter] = useState("")
  const [actionFilter, setActionFilter] = useState("")

  const isAdmin = role === "ADMIN"
  const totalPages = Math.max(1, Math.ceil(totalCount / PAGE_SIZE))

  // Detect locale from pathname
  const locale = pathname.split("/")[1] || "en"

  const loadEntries = useCallback(async () => {
    setIsLoading(true)
    try {
      const result = await getAuditEntries({
        page,
        pageSize: PAGE_SIZE,
        startDate: startDate || undefined,
        endDate: endDate || undefined,
        user: userFilter || undefined,
        action: actionFilter && actionFilter !== "ALL" ? actionFilter : undefined,
      })
      setEntries(result.items)
      setTotalCount(result.totalCount)
    } catch {
      toast.error(t("errors.load_failed"))
    } finally {
      setIsLoading(false)
    }
  }, [page, startDate, endDate, userFilter, actionFilter, t])

  useEffect(() => {
    loadEntries()
  }, [loadEntries])

  // Redirect non-admins
  if (!isAdmin) {
    return (
      <PageContainer data-testid="audit-trail-page">
        <div className="flex flex-col items-center justify-center py-16 gap-4" data-testid="audit-access-denied">
          <Shield className="h-12 w-12 text-muted-foreground" />
          <h2 className="text-lg font-semibold text-foreground">{t("access_denied")}</h2>
          <p className="text-[13px] text-muted-foreground">{t("admin_only")}</p>
          <Button
            variant="outline"
            onClick={() => router.back()}
            data-testid="audit-go-back-btn"
          >
            {t("go_back")}
          </Button>
        </div>
      </PageContainer>
    )
  }

  const handleApplyFilters = () => {
    setPage(1)
    // loadEntries will be triggered by the useEffect dependency change
  }

  const handleClearFilters = () => {
    setStartDate("")
    setEndDate("")
    setUserFilter("")
    setActionFilter("")
    setPage(1)
  }

  const toggleRow = (id: string) => {
    setExpandedRow((prev) => (prev === id ? null : id))
  }

  return (
    <PageContainer data-testid="audit-trail-page">
      <div>
        <h1 className="text-[22px] font-bold text-foreground flex items-center gap-2">
          <span className="w-1 h-5 bg-primary rounded-full"></span>
          {t("title")}
        </h1>
        <p className="text-[13px] text-muted-foreground mt-1 ms-3">
          {t("subtitle")}
        </p>
      </div>

      {/* Filters */}
      <div className="bg-card border border-border/80 rounded-xl shadow-sm p-4 space-y-3" data-testid="audit-filters">
        <h3 className="text-[13px] font-bold text-foreground">{t("filters")}</h3>
        <div className="grid grid-cols-1 gap-3 sm:grid-cols-2 lg:grid-cols-4">
          <div>
            <label htmlFor="audit-start-date" className="text-[12px] text-muted-foreground mb-1 block">
              {t("start_date")}
            </label>
            <Input
              id="audit-start-date"
              type="date"
              value={startDate}
              onChange={(e) => setStartDate(e.target.value)}
              data-testid="audit-filter-start-date"
            />
          </div>
          <div>
            <label htmlFor="audit-end-date" className="text-[12px] text-muted-foreground mb-1 block">
              {t("end_date")}
            </label>
            <Input
              id="audit-end-date"
              type="date"
              value={endDate}
              onChange={(e) => setEndDate(e.target.value)}
              data-testid="audit-filter-end-date"
            />
          </div>
          <div>
            <label htmlFor="audit-user-filter" className="text-[12px] text-muted-foreground mb-1 block">
              {t("user")}
            </label>
            <Input
              id="audit-user-filter"
              type="text"
              placeholder={t("user_placeholder")}
              value={userFilter}
              onChange={(e) => setUserFilter(e.target.value)}
              data-testid="audit-filter-user"
            />
          </div>
          <div>
            <label htmlFor="audit-action-filter" className="text-[12px] text-muted-foreground mb-1 block">
              {t("action")}
            </label>
            <Select value={actionFilter} onValueChange={(v) => setActionFilter(v ?? "")}>
              <SelectTrigger id="audit-action-filter" data-testid="audit-filter-action">
                <SelectValue placeholder={t("all_actions")} />
              </SelectTrigger>
              <SelectContent>
                <SelectItem value="ALL">{t("all_actions")}</SelectItem>
                {ACTION_TYPES.map((a) => (
                  <SelectItem key={a} value={a} data-testid={`audit-filter-action-${a.toLowerCase()}`}>
                    {t(`actions.${a.toLowerCase()}`)}
                  </SelectItem>
                ))}
              </SelectContent>
            </Select>
          </div>
        </div>
        <div className="flex gap-2 pt-1">
          <Button
            size="sm"
            onClick={handleApplyFilters}
            data-testid="audit-apply-filters-btn"
            className="rounded-xl h-9 px-4 font-semibold"
          >
            {t("apply_filters")}
          </Button>
          <Button
            size="sm"
            variant="outline"
            onClick={handleClearFilters}
            data-testid="audit-clear-filters-btn"
            className="rounded-xl h-9 px-4 font-semibold"
          >
            {t("clear_filters")}
          </Button>
        </div>
      </div>

      {/* Table */}
      {isLoading ? (
        <div className="space-y-2" data-testid="audit-loading">
          {[...Array(5)].map((_, i) => (
            <Skeleton key={i} className="h-12 w-full rounded-xl" />
          ))}
        </div>
      ) : entries.length === 0 ? (
        <div className="flex flex-col items-center justify-center py-12 gap-2" data-testid="audit-empty-state">
          <Shield className="h-8 w-8 text-muted-foreground" />
          <p className="text-[13px] text-muted-foreground">{t("no_entries")}</p>
        </div>
      ) : (
        <>
          <div className="border border-border/80 rounded-xl overflow-hidden shadow-sm" data-testid="audit-table">
            <Table>
              <TableHeader>
                <TableRow>
                  <TableHead className="w-8"></TableHead>
                  <TableHead>{t("columns.date")}</TableHead>
                  <TableHead>{t("columns.user")}</TableHead>
                  <TableHead>{t("columns.action")}</TableHead>
                  <TableHead>{t("columns.entity")}</TableHead>
                  <TableHead>{t("columns.details")}</TableHead>
                </TableRow>
              </TableHeader>
              <TableBody>
                {entries.map((entry) => {
                  const isExpanded = expandedRow === entry.id
                  return (
                    <TableRow
                      key={entry.id}
                      data-testid={`audit-row-${entry.id}`}
                      className="cursor-pointer"
                      onClick={() => toggleRow(entry.id)}
                    >
                      <TableCell>
                        <button
                          type="button"
                          data-testid={`audit-expand-btn-${entry.id}`}
                          className="p-0.5 rounded hover:bg-muted transition-colors"
                          aria-label={isExpanded ? t("collapse") : t("expand")}
                        >
                          {isExpanded ? (
                            <ChevronDown className="h-4 w-4 text-muted-foreground" />
                          ) : (
                            <ChevronRight className="h-4 w-4 text-muted-foreground" />
                          )}
                        </button>
                      </TableCell>
                      <TableCell className="text-[12px]" data-testid={`audit-date-${entry.id}`}>
                        {formatDate(entry.timestamp, locale)}
                      </TableCell>
                      <TableCell className="text-[12px] font-medium" data-testid={`audit-user-${entry.id}`}>
                        {entry.changedBy}
                      </TableCell>
                      <TableCell>
                        <ActionBadge action={entry.action} t={t} />
                      </TableCell>
                      <TableCell className="text-[12px]" data-testid={`audit-entity-${entry.id}`}>
                        <span className="font-medium">{entry.entityType}</span>
                        <span className="text-muted-foreground ms-1 text-[11px]">({entry.entityId})</span>
                      </TableCell>
                      <TableCell className="text-[12px] text-muted-foreground">
                        {isExpanded ? t("click_to_collapse") : t("click_to_expand")}
                      </TableCell>
                    </TableRow>
                  )
                })}
              </TableBody>
            </Table>

            {/* Expanded details rendered outside table for layout */}
            {expandedRow && (
              <div
                className="border-t border-border/50 bg-muted/30 p-4"
                data-testid={`audit-details-${expandedRow}`}
              >
                <DiffView
                  oldValues={entries.find((e) => e.id === expandedRow)?.oldValues ?? null}
                  newValues={entries.find((e) => e.id === expandedRow)?.newValues ?? null}
                  t={t}
                />
              </div>
            )}
          </div>

          {/* Pagination */}
          <div className="flex items-center justify-between" data-testid="audit-pagination">
            <p className="text-[12px] text-muted-foreground">
              {t("showing", { from: (page - 1) * PAGE_SIZE + 1, to: Math.min(page * PAGE_SIZE, totalCount), total: totalCount })}
            </p>
            <div className="flex items-center gap-1">
              <Button
                variant="outline"
                size="sm"
                disabled={page <= 1}
                onClick={() => setPage(1)}
                data-testid="audit-first-page-btn"
                className="h-8 w-8 p-0"
              >
                <ChevronsLeft className="h-4 w-4" />
              </Button>
              <Button
                variant="outline"
                size="sm"
                disabled={page <= 1}
                onClick={() => setPage((p) => Math.max(1, p - 1))}
                data-testid="audit-prev-page-btn"
                className="h-8 w-8 p-0"
              >
                <ChevronLeft className="h-4 w-4" />
              </Button>
              <span className="text-[12px] text-muted-foreground px-2" data-testid="audit-page-indicator">
                {t("page_of", { page, total: totalPages })}
              </span>
              <Button
                variant="outline"
                size="sm"
                disabled={page >= totalPages}
                onClick={() => setPage((p) => Math.min(totalPages, p + 1))}
                data-testid="audit-next-page-btn"
                className="h-8 w-8 p-0"
              >
                <ChevronRight className="h-4 w-4" />
              </Button>
              <Button
                variant="outline"
                size="sm"
                disabled={page >= totalPages}
                onClick={() => setPage(totalPages)}
                data-testid="audit-last-page-btn"
                className="h-8 w-8 p-0"
              >
                <ChevronsRight className="h-4 w-4" />
              </Button>
            </div>
          </div>
        </>
      )}
    </PageContainer>
  )
}
