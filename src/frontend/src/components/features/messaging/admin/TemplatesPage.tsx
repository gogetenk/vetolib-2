"use client"

import { useEffect, useState, useCallback } from "react"
import { useTranslations } from "next-intl"
import { Button } from "@/components/ui/button"
import {
  Table,
  TableBody,
  TableCell,
  TableHead,
  TableHeader,
  TableRow,
} from "@/components/ui/table"
import { Badge } from "@/components/ui/badge"
import { TemplateFormDialog } from "./TemplateFormDialog"
import { listTemplates, deleteTemplate } from "@/lib/api/messaging"
import type { ResponseTemplateDto } from "@/lib/api/messaging-types"
import {
  Dialog,
  DialogContent,
  DialogHeader,
  DialogTitle,
  DialogFooter,
} from "@/components/ui/dialog"
import { toast } from "sonner"
import { formatCategory } from "@/lib/format-category"

export function TemplatesPage() {
  const t = useTranslations("messaging_admin")
  const [templates, setTemplates] = useState<ResponseTemplateDto[]>([])
  const [isLoading, setIsLoading] = useState(true)
  const [formOpen, setFormOpen] = useState(false)
  const [editTemplate, setEditTemplate] = useState<ResponseTemplateDto | null>(null)
  const [deleteTarget, setDeleteTarget] = useState<ResponseTemplateDto | null>(null)
  const [isDeleting, setIsDeleting] = useState(false)

  const load = useCallback(async () => {
    try {
      setIsLoading(true)
      const data = await listTemplates()
      setTemplates(data)
    } catch {
      toast.error(t("templates.load_failed"))
    } finally {
      setIsLoading(false)
    }
  }, [t])

  useEffect(() => {
    load()
  }, [load])

  function handleAddClick() {
    setEditTemplate(null)
    setFormOpen(true)
  }

  function handleEditClick(tpl: ResponseTemplateDto) {
    setEditTemplate(tpl)
    setFormOpen(true)
  }

  function handleSaved(saved: ResponseTemplateDto) {
    setTemplates((prev) => {
      const idx = prev.findIndex((t) => t.id === saved.id)
      if (idx !== -1) {
        const next = [...prev]
        next[idx] = saved
        return next
      }
      return [...prev, saved]
    })
    setFormOpen(false)
    toast.success(t("templates.saved"))
  }

  async function handleDeleteConfirm() {
    if (!deleteTarget) return
    try {
      setIsDeleting(true)
      await deleteTemplate(deleteTarget.id)
      setTemplates((prev) => prev.filter((t) => t.id !== deleteTarget.id))
      toast.success(t("templates.deleted"))
      setDeleteTarget(null)
    } catch {
      toast.error(t("templates.delete_failed"))
    } finally {
      setIsDeleting(false)
    }
  }

  return (
    <div className="space-y-6" data-testid="templates-page">
      <div className="flex items-center justify-between">
        <div>
          <h2 className="text-xl font-semibold">{t("templates.title")}</h2>
          <p className="text-sm text-muted-foreground mt-1">{t("templates.subtitle")}</p>
        </div>
        <Button data-testid="add-template-btn" onClick={handleAddClick}>
          {t("templates.add")}
        </Button>
      </div>

      {isLoading ? (
        <div data-testid="templates-loading" className="text-sm text-muted-foreground">
          {t("common_loading")}
        </div>
      ) : templates.length === 0 ? (
        <div data-testid="templates-empty" className="text-sm text-muted-foreground">
          {t("templates.empty")}
        </div>
      ) : (
        <div className="rounded-md border" data-testid="templates-table">
          <Table>
            <TableHeader>
              <TableRow>
                <TableHead>{t("templates.col_name")}</TableHead>
                <TableHead>{t("templates.col_category")}</TableHead>
                <TableHead>{t("templates.col_updated")}</TableHead>
                <TableHead className="text-right">{t("templates.col_actions")}</TableHead>
              </TableRow>
            </TableHeader>
            <TableBody>
              {templates.map((tpl) => (
                <TableRow key={tpl.id} data-testid={`template-row-${tpl.id}`}>
                  <TableCell className="font-medium">{tpl.name}</TableCell>
                  <TableCell>
                    {tpl.category ? (
                      <Badge variant="secondary" data-testid={`template-category-${tpl.id}`}>
                        {formatCategory(tpl.category)}
                      </Badge>
                    ) : (
                      <span className="text-muted-foreground text-sm">—</span>
                    )}
                  </TableCell>
                  <TableCell className="text-sm text-muted-foreground">
                    {new Date(tpl.updatedAt).toLocaleDateString()}
                  </TableCell>
                  <TableCell className="text-right">
                    <div className="flex justify-end gap-2">
                      <Button
                        variant="outline"
                        size="sm"
                        data-testid={`edit-template-btn-${tpl.id}`}
                        onClick={() => handleEditClick(tpl)}
                      >
                        {t("templates.edit")}
                      </Button>
                      <Button
                        variant="destructive"
                        size="sm"
                        data-testid={`delete-template-btn-${tpl.id}`}
                        onClick={() => setDeleteTarget(tpl)}
                      >
                        {t("templates.delete")}
                      </Button>
                    </div>
                  </TableCell>
                </TableRow>
              ))}
            </TableBody>
          </Table>
        </div>
      )}

      <TemplateFormDialog
        open={formOpen}
        onOpenChange={setFormOpen}
        initialData={editTemplate}
        onSaved={handleSaved}
      />

      {/* Delete confirmation dialog */}
      <Dialog
        open={!!deleteTarget}
        onOpenChange={(open) => {
          if (!open) setDeleteTarget(null)
        }}
      >
        <DialogContent data-testid="delete-template-dialog">
          <DialogHeader>
            <DialogTitle>{t("templates.delete_confirm_title")}</DialogTitle>
          </DialogHeader>
          <p className="text-sm text-muted-foreground">
            {t("templates.delete_confirm_body", { name: deleteTarget?.name ?? "" })}
          </p>
          <DialogFooter>
            <Button
              variant="outline"
              data-testid="delete-template-cancel-btn"
              onClick={() => setDeleteTarget(null)}
              disabled={isDeleting}
            >
              {t("cancel")}
            </Button>
            <Button
              variant="destructive"
              data-testid="delete-template-confirm-btn"
              onClick={handleDeleteConfirm}
              disabled={isDeleting}
            >
              {isDeleting ? t("deleting") : t("templates.delete")}
            </Button>
          </DialogFooter>
        </DialogContent>
      </Dialog>
    </div>
  )
}
