"use client"

import { useEffect, useState, useCallback } from "react"
import { Building2, Search, Trash2, Plus, Users, DollarSign, MapPin } from "lucide-react"
import { toast } from "sonner"
import { useTranslations } from "next-intl"
import { PageContainer } from "@/components/ui/page-container"
import { Button } from "@/components/ui/button"
import { Input } from "@/components/ui/input"
import { Skeleton } from "@/components/ui/skeleton"
import { Card, CardContent, CardHeader, CardTitle } from "@/components/ui/card"
import {
  Table,
  TableBody,
  TableCell,
  TableHead,
  TableHeader,
  TableRow,
} from "@/components/ui/table"
import {
  Dialog,
  DialogContent,
  DialogDescription,
  DialogFooter,
  DialogHeader,
  DialogTitle,
} from "@/components/ui/dialog"
import { ErrorState } from "@/components/ui/error-state"
import { useRole } from "@/hooks/use-role"
import {
  getClinicGroupDetail,
  getClinicGroupStats,
  updateClinicGroup,
  addClinicToGroup,
  removeClinicFromGroup,
  searchClinics,
  type ClinicGroupDetail,
  type ClinicGroupStats,
  type ClinicSearchResult,
  type ClinicGroupClinicDto,
} from "@/lib/api/clinic-group"

function parseJwtClaim(key: string): string {
  if (typeof window === "undefined") return ""
  const token = localStorage.getItem("access_token")
  if (!token) return ""
  try {
    const base64 = token.split(".")[1]
    const json = atob(base64.replace(/-/g, "+").replace(/_/g, "/"))
    const payload = JSON.parse(json) as Record<string, unknown>
    return (payload[key] as string) ?? ""
  } catch {
    return ""
  }
}

export default function ClinicGroupPage() {
  const t = useTranslations("clinic_group")
  const role = useRole()

  const [group, setGroup] = useState<ClinicGroupDetail | null>(null)
  const [stats, setStats] = useState<ClinicGroupStats | null>(null)
  const [isLoading, setIsLoading] = useState(true)
  const [error, setError] = useState<string | null>(null)

  // Group name editing
  const [editName, setEditName] = useState("")
  const [isSavingName, setIsSavingName] = useState(false)

  // Add clinic dialog
  const [addDialogOpen, setAddDialogOpen] = useState(false)
  const [searchQuery, setSearchQuery] = useState("")
  const [searchResults, setSearchResults] = useState<ClinicSearchResult[]>([])
  const [isSearching, setIsSearching] = useState(false)
  const [addingClinicId, setAddingClinicId] = useState<string | null>(null)

  // Remove clinic dialog
  const [removeDialogOpen, setRemoveDialogOpen] = useState(false)
  const [clinicToRemove, setClinicToRemove] = useState<ClinicGroupClinicDto | null>(null)
  const [isRemoving, setIsRemoving] = useState(false)

  const groupId = parseJwtClaim("clinic_group_id")

  const loadData = useCallback(async () => {
    if (!groupId) return
    try {
      setIsLoading(true)
      setError(null)
      const [groupData, statsData] = await Promise.all([
        getClinicGroupDetail(groupId),
        getClinicGroupStats(groupId),
      ])
      setGroup(groupData)
      setStats(statsData)
      setEditName(groupData.name)
    } catch {
      setError(t("errors.load_failed"))
    } finally {
      setIsLoading(false)
    }
  }, [groupId, t])

  useEffect(() => {
    if (role === "ADMIN") {
      loadData()
    }
  }, [role, loadData])

  const handleSaveName = async () => {
    if (!groupId || !editName.trim() || editName === group?.name) return
    try {
      setIsSavingName(true)
      await updateClinicGroup(groupId, { name: editName.trim() })
      setGroup((prev) => prev ? { ...prev, name: editName.trim() } : prev)
      toast.success(t("name_updated"))
    } catch {
      toast.error(t("errors.update_failed"))
    } finally {
      setIsSavingName(false)
    }
  }

  const handleSearch = async (query: string) => {
    setSearchQuery(query)
    if (query.length < 2) {
      setSearchResults([])
      return
    }
    try {
      setIsSearching(true)
      const results = await searchClinics(query)
      setSearchResults(results)
    } catch {
      toast.error(t("errors.search_failed"))
    } finally {
      setIsSearching(false)
    }
  }

  const handleAddClinic = async (clinic: ClinicSearchResult) => {
    if (!groupId) return
    try {
      setAddingClinicId(clinic.id)
      await addClinicToGroup(groupId, clinic.id)
      toast.success(t("added_success", { clinicName: clinic.name }))
      setAddDialogOpen(false)
      setSearchQuery("")
      setSearchResults([])
      await loadData()
    } catch {
      toast.error(t("errors.add_failed"))
    } finally {
      setAddingClinicId(null)
    }
  }

  const handleRemoveClinic = async () => {
    if (!groupId || !clinicToRemove) return
    try {
      setIsRemoving(true)
      await removeClinicFromGroup(groupId, clinicToRemove.id)
      toast.success(t("removed_success", { clinicName: clinicToRemove.name }))
      setRemoveDialogOpen(false)
      setClinicToRemove(null)
      await loadData()
    } catch {
      toast.error(t("errors.remove_failed"))
    } finally {
      setIsRemoving(false)
    }
  }

  const openRemoveDialog = (clinic: ClinicGroupClinicDto) => {
    setClinicToRemove(clinic)
    setRemoveDialogOpen(true)
  }

  if (role !== "ADMIN") {
    return (
      <PageContainer data-testid="clinic-group-page">
        <div className="flex flex-col items-center justify-center py-16 text-center gap-4" data-testid="clinic-group-access-denied">
          <Building2 className="h-16 w-16 text-muted-foreground" />
          <h2 className="text-lg font-semibold">{t("access_denied")}</h2>
          <p className="text-sm text-muted-foreground">{t("admin_only")}</p>
        </div>
      </PageContainer>
    )
  }

  return (
    <PageContainer data-testid="clinic-group-page">
      {/* Page header */}
      <div className="flex items-center justify-between">
        <div>
          <h1 className="text-[22px] font-bold text-foreground flex items-center gap-2">
            <span className="w-1 h-5 bg-primary rounded-full"></span>
            {t("title")}
          </h1>
          <p className="text-[13px] text-muted-foreground mt-1 ms-3">
            {t("subtitle")}
          </p>
        </div>
        <Button
          data-testid="add-clinic-btn"
          onClick={() => setAddDialogOpen(true)}
          className="bg-primary hover:bg-primary/90 text-primary-foreground font-semibold rounded-xl h-11 px-6 shadow-sm"
        >
          <Plus className="h-4 w-4 me-2" />
          {t("add_clinic")}
        </Button>
      </div>

      {isLoading ? (
        <div data-testid="clinic-group-loading" className="space-y-4">
          <Skeleton className="h-12 w-full rounded-xl" />
          <div className="grid grid-cols-1 md:grid-cols-3 gap-4">
            {[1, 2, 3].map((i) => (
              <Skeleton key={i} className="h-24 rounded-xl" />
            ))}
          </div>
          {[1, 2, 3].map((i) => (
            <Skeleton key={i} className="h-12 w-full rounded-xl" />
          ))}
        </div>
      ) : error ? (
        <ErrorState
          data-testid="clinic-group-error"
          title={t("errors.load_failed")}
          description={error}
          onRetry={loadData}
        />
      ) : (
        <>
          {/* Group name section */}
          <Card data-testid="clinic-group-name-card">
            <CardHeader>
              <CardTitle className="text-[15px]">{t("group_name_label")}</CardTitle>
            </CardHeader>
            <CardContent>
              <div className="flex items-center gap-3">
                <Input
                  data-testid="group-name-input"
                  value={editName}
                  onChange={(e) => setEditName(e.target.value)}
                  className="max-w-md"
                />
                <Button
                  data-testid="save-group-name-btn"
                  onClick={handleSaveName}
                  disabled={isSavingName || editName === group?.name || !editName.trim()}
                  size="sm"
                >
                  {isSavingName ? t("saving") : t("save")}
                </Button>
              </div>
            </CardContent>
          </Card>

          {/* Stats summary */}
          {stats && (
            <div data-testid="clinic-group-stats" className="grid grid-cols-1 md:grid-cols-3 gap-4">
              <Card data-testid="stat-total-patients">
                <CardContent className="pt-6">
                  <div className="flex items-center gap-3">
                    <div className="flex h-10 w-10 items-center justify-center rounded-xl bg-blue-100 dark:bg-blue-900/30">
                      <Users className="h-5 w-5 text-blue-600 dark:text-blue-400" />
                    </div>
                    <div>
                      <p className="text-[13px] text-muted-foreground">{t("total_patients")}</p>
                      <p className="text-xl font-bold">{stats.totalPatients.toLocaleString()}</p>
                    </div>
                  </div>
                </CardContent>
              </Card>
              <Card data-testid="stat-total-revenue">
                <CardContent className="pt-6">
                  <div className="flex items-center gap-3">
                    <div className="flex h-10 w-10 items-center justify-center rounded-xl bg-green-100 dark:bg-green-900/30">
                      <DollarSign className="h-5 w-5 text-green-600 dark:text-green-400" />
                    </div>
                    <div>
                      <p className="text-[13px] text-muted-foreground">{t("total_revenue")}</p>
                      <p className="text-xl font-bold">{stats.totalRevenue.toLocaleString()} AED</p>
                    </div>
                  </div>
                </CardContent>
              </Card>
              <Card data-testid="stat-clinic-count">
                <CardContent className="pt-6">
                  <div className="flex items-center gap-3">
                    <div className="flex h-10 w-10 items-center justify-center rounded-xl bg-purple-100 dark:bg-purple-900/30">
                      <Building2 className="h-5 w-5 text-purple-600 dark:text-purple-400" />
                    </div>
                    <div>
                      <p className="text-[13px] text-muted-foreground">{t("clinic_count")}</p>
                      <p className="text-xl font-bold">{stats.clinicCount}</p>
                    </div>
                  </div>
                </CardContent>
              </Card>
            </div>
          )}

          {/* Clinics table */}
          <Card data-testid="clinic-group-clinics-card">
            <CardHeader>
              <CardTitle className="text-[15px]">{t("clinics_title")}</CardTitle>
            </CardHeader>
            <CardContent>
              {group && group.clinics.length > 0 ? (
                <Table data-testid="clinics-table">
                  <TableHeader>
                    <TableRow>
                      <TableHead>{t("clinic_name")}</TableHead>
                      <TableHead>{t("address")}</TableHead>
                      <TableHead className="text-right">{t("patients")}</TableHead>
                      <TableHead className="text-right">{t("revenue")}</TableHead>
                      <TableHead className="text-right">{t("actions")}</TableHead>
                    </TableRow>
                  </TableHeader>
                  <TableBody>
                    {group.clinics.map((clinic) => (
                      <TableRow key={clinic.id} data-testid={`clinic-row-${clinic.id}`}>
                        <TableCell className="font-medium">{clinic.name}</TableCell>
                        <TableCell className="text-muted-foreground">
                          <span className="flex items-center gap-1">
                            <MapPin className="h-3.5 w-3.5" />
                            {clinic.address}
                          </span>
                        </TableCell>
                        <TableCell className="text-right">{clinic.totalPatients.toLocaleString()}</TableCell>
                        <TableCell className="text-right">{clinic.monthlyRevenue.toLocaleString()} AED</TableCell>
                        <TableCell className="text-right">
                          <Button
                            data-testid={`remove-clinic-btn-${clinic.id}`}
                            variant="ghost"
                            size="sm"
                            onClick={() => openRemoveDialog(clinic)}
                            className="text-destructive hover:text-destructive hover:bg-destructive/10"
                          >
                            <Trash2 className="h-4 w-4 me-1" />
                            {t("remove")}
                          </Button>
                        </TableCell>
                      </TableRow>
                    ))}
                  </TableBody>
                </Table>
              ) : (
                <p className="text-sm text-muted-foreground py-8 text-center" data-testid="no-clinics-message">
                  {t("search_no_results")}
                </p>
              )}
            </CardContent>
          </Card>
        </>
      )}

      {/* Add Clinic Dialog */}
      <Dialog open={addDialogOpen} onOpenChange={setAddDialogOpen}>
        <DialogContent data-testid="add-clinic-dialog">
          <DialogHeader>
            <DialogTitle>{t("add_clinic")}</DialogTitle>
          </DialogHeader>
          <div className="space-y-4">
            <div className="relative">
              <Search className="absolute start-3 top-1/2 -translate-y-1/2 h-4 w-4 text-muted-foreground" />
              <Input
                data-testid="clinic-search-input"
                placeholder={t("search_placeholder")}
                value={searchQuery}
                onChange={(e) => handleSearch(e.target.value)}
                className="ps-9"
              />
            </div>
            <div className="max-h-64 overflow-y-auto space-y-2" data-testid="clinic-search-results">
              {isSearching ? (
                <div className="space-y-2">
                  {[1, 2].map((i) => (
                    <Skeleton key={i} className="h-14 w-full rounded-lg" />
                  ))}
                </div>
              ) : searchResults.length > 0 ? (
                searchResults.map((clinic) => (
                  <div
                    key={clinic.id}
                    className="flex items-center justify-between p-3 rounded-lg border"
                    data-testid={`search-result-${clinic.id}`}
                  >
                    <div>
                      <p className="text-sm font-medium">{clinic.name}</p>
                      <p className="text-xs text-muted-foreground">{clinic.address}</p>
                    </div>
                    <Button
                      data-testid={`add-clinic-confirm-btn-${clinic.id}`}
                      size="sm"
                      onClick={() => handleAddClinic(clinic)}
                      disabled={addingClinicId === clinic.id}
                    >
                      <Plus className="h-3.5 w-3.5 me-1" />
                      {t("add")}
                    </Button>
                  </div>
                ))
              ) : searchQuery.length >= 2 ? (
                <p className="text-sm text-muted-foreground text-center py-4" data-testid="no-search-results">
                  {t("search_no_results")}
                </p>
              ) : null}
            </div>
          </div>
        </DialogContent>
      </Dialog>

      {/* Remove Clinic Confirmation Dialog */}
      <Dialog open={removeDialogOpen} onOpenChange={setRemoveDialogOpen}>
        <DialogContent data-testid="remove-clinic-dialog">
          <DialogHeader>
            <DialogTitle>{t("remove_confirm_title")}</DialogTitle>
            <DialogDescription>
              {clinicToRemove
                ? t("remove_confirm_description", { clinicName: clinicToRemove.name })
                : ""}
            </DialogDescription>
          </DialogHeader>
          <DialogFooter>
            <Button
              data-testid="remove-clinic-cancel-btn"
              variant="outline"
              onClick={() => setRemoveDialogOpen(false)}
            >
              {t("cancel")}
            </Button>
            <Button
              data-testid="remove-clinic-confirm-btn"
              variant="destructive"
              onClick={handleRemoveClinic}
              disabled={isRemoving}
            >
              {t("remove_confirm")}
            </Button>
          </DialogFooter>
        </DialogContent>
      </Dialog>
    </PageContainer>
  )
}
