"use client"

import { useEffect, useState } from "react"
import { useTranslations } from "next-intl"
import {
  Dialog,
  DialogContent,
  DialogHeader,
  DialogTitle,
  DialogFooter,
} from "@/components/ui/dialog"
import { Button } from "@/components/ui/button"
import { Input } from "@/components/ui/input"
import { Label } from "@/components/ui/label"
import { Textarea } from "@/components/ui/textarea"
import {
  Select,
  SelectContent,
  SelectItem,
  SelectTrigger,
  SelectValue,
} from "@/components/ui/select"
import { createTemplate, updateTemplate } from "@/lib/api/messaging"
import type { ResponseTemplateDto, MessageCategory } from "@/lib/api/messaging-types"
import { toast } from "sonner"

const CATEGORIES: MessageCategory[] = [
  "MedicalUrgency",
  "PostOperativeFollowUp",
  "MedicalQuestion",
  "AppointmentRequest",
  "Administrative",
  "Feedback",
  "Other",
]

interface Props {
  open: boolean
  onOpenChange: (open: boolean) => void
  initialData: ResponseTemplateDto | null
  onSaved: (saved: ResponseTemplateDto) => void
}

export function TemplateFormDialog({ open, onOpenChange, initialData, onSaved }: Props) {
  const t = useTranslations("messaging_admin")

  const [name, setName] = useState("")
  const [category, setCategory] = useState<MessageCategory | "" | "none">("")
  const [contentEn, setContentEn] = useState("")
  const [contentAr, setContentAr] = useState("")
  const [isSaving, setIsSaving] = useState(false)
  const [errors, setErrors] = useState<Record<string, string>>({})

  useEffect(() => {
    if (open) {
      setName(initialData?.name ?? "")
      setCategory(initialData?.category ?? "")
      setContentEn(initialData?.contentEn ?? "")
      setContentAr(initialData?.contentAr ?? "")
      setErrors({})
    }
  }, [open, initialData])

  function validate() {
    const errs: Record<string, string> = {}
    if (!name.trim()) errs.name = t("templates.form.name_required")
    if (!contentEn.trim()) errs.contentEn = t("templates.form.content_en_required")
    if (!contentAr.trim()) errs.contentAr = t("templates.form.content_ar_required")
    return errs
  }

  async function handleSave() {
    const errs = validate()
    if (Object.keys(errs).length > 0) {
      setErrors(errs)
      return
    }
    try {
      setIsSaving(true)
      const payload = {
        name: name.trim(),
        contentEn: contentEn.trim(),
        contentAr: contentAr.trim(),
        category: category === "" || category === "none" ? null : (category as MessageCategory),
      }
      let saved: ResponseTemplateDto
      if (initialData) {
        saved = await updateTemplate(initialData.id, payload)
      } else {
        saved = await createTemplate(payload)
      }
      onSaved(saved)
    } catch {
      toast.error(t("templates.save_failed"))
    } finally {
      setIsSaving(false)
    }
  }

  const isEdit = !!initialData

  return (
    <Dialog open={open} onOpenChange={onOpenChange}>
      <DialogContent className="max-w-2xl rounded-2xl" data-testid="template-form-dialog">
        <DialogHeader>
          <DialogTitle className="text-[18px] font-bold text-foreground">
            {isEdit ? t("templates.form.edit_title") : t("templates.form.add_title")}
          </DialogTitle>
        </DialogHeader>

        <div className="space-y-4 py-2">
          {/* Name */}
          <div className="space-y-1">
            <Label htmlFor="template-name" className="text-[13px] font-semibold text-foreground">{t("templates.form.name")}</Label>
            <Input
              id="template-name"
              data-testid="template-name-input"
              value={name}
              onChange={(e) => setName(e.target.value)}
              placeholder={t("templates.form.name_placeholder")}
              className="rounded-xl border-border/80 text-[13px] focus:ring-2 focus:ring-primary/20 focus:border-primary/50"
            />
            {errors.name && (
              <p className="text-xs text-destructive" data-testid="template-name-error">
                {errors.name}
              </p>
            )}
          </div>

          {/* Category */}
          <div className="space-y-1">
            <Label htmlFor="template-category" className="text-[13px] font-semibold text-foreground">{t("templates.form.category")}</Label>
            <Select
              value={category}
              onValueChange={(val) => setCategory(val as MessageCategory | "" | "none")}
            >
              <SelectTrigger
                id="template-category"
                data-testid="template-category-select"
                className="rounded-xl border-border/80 text-[13px]"
              >
                <SelectValue placeholder={t("templates.form.category_placeholder")} />
              </SelectTrigger>
              <SelectContent>
                <SelectItem value="none">{t("templates.form.no_category")}</SelectItem>
                {CATEGORIES.map((cat) => (
                  <SelectItem key={cat} value={cat} data-testid={`category-option-${cat}`}>
                    {cat}
                  </SelectItem>
                ))}
              </SelectContent>
            </Select>
          </div>

          {/* Content EN */}
          <div className="space-y-1">
            <Label htmlFor="template-content-en" className="text-[13px] font-semibold text-foreground">{t("templates.form.content_en")}</Label>
            <Textarea
              id="template-content-en"
              data-testid="template-content-en-input"
              rows={4}
              value={contentEn}
              onChange={(e) => setContentEn(e.target.value)}
              placeholder={t("templates.form.content_en_placeholder")}
              dir="ltr"
              className="rounded-xl border-border/80 text-[13px] focus:ring-2 focus:ring-primary/20 focus:border-primary/50"
            />
            {errors.contentEn && (
              <p className="text-xs text-destructive" data-testid="template-content-en-error">
                {errors.contentEn}
              </p>
            )}
          </div>

          {/* Content AR */}
          <div className="space-y-1">
            <Label htmlFor="template-content-ar" className="text-[13px] font-semibold text-foreground">{t("templates.form.content_ar")}</Label>
            <Textarea
              id="template-content-ar"
              data-testid="template-content-ar-input"
              rows={4}
              value={contentAr}
              onChange={(e) => setContentAr(e.target.value)}
              placeholder={t("templates.form.content_ar_placeholder")}
              dir="rtl"
              className="font-arabic rounded-xl border-border/80 text-[13px] focus:ring-2 focus:ring-primary/20 focus:border-primary/50"
            />
            {errors.contentAr && (
              <p className="text-xs text-destructive" data-testid="template-content-ar-error">
                {errors.contentAr}
              </p>
            )}
          </div>
        </div>

        <DialogFooter>
          <Button
            variant="outline"
            data-testid="template-form-cancel-btn"
            onClick={() => onOpenChange(false)}
            disabled={isSaving}
            className="rounded-xl font-semibold border-border/80 hover:bg-muted"
          >
            {t("cancel")}
          </Button>
          <Button
            data-testid="template-form-save-btn"
            onClick={handleSave}
            disabled={isSaving}
            className="bg-primary hover:bg-primary/90 text-primary-foreground font-semibold rounded-xl shadow-sm"
          >
            {isSaving ? t("saving") : t("save")}
          </Button>
        </DialogFooter>
      </DialogContent>
    </Dialog>
  )
}
