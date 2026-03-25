"use client"

import { useState } from "react"
import { useForm } from "react-hook-form"
import { z } from "zod"
import { zodResolver } from "@hookform/resolvers/zod"
import {
  Dialog,
  DialogContent,
  DialogDescription,
  DialogFooter,
  DialogHeader,
  DialogTitle,
} from "@/components/ui/dialog"
import { Button } from "@/components/ui/button"
import { Input } from "@/components/ui/input"
import { Label } from "@/components/ui/label"
import {
  Select,
  SelectContent,
  SelectItem,
  SelectTrigger,
  SelectValue,
} from "@/components/ui/select"
import { inviteUser } from "@/lib/api/users"
import type { UserDto } from "@/lib/api/users"
import { trackEvent, AnalyticsEvents } from "@/lib/analytics"
import { useTranslations } from "next-intl"

const inviteSchema = z.object({
  email: z.string().email("Please enter a valid email address"),
  fullName: z.string().min(2, "Full name must be at least 2 characters"),
  role: z.enum(["VET", "ASSISTANT", "RECEPTIONIST"], {
    error: "Please select a role",
  }),
})

type InviteFormValues = z.infer<typeof inviteSchema>

interface InviteUserDialogProps {
  open: boolean
  onOpenChange: (open: boolean) => void
  onUserInvited: (user: UserDto) => void
}

export function InviteUserDialog({
  open,
  onOpenChange,
  onUserInvited,
}: InviteUserDialogProps) {
  const t = useTranslations('team.invite')
  const tRoles = useTranslations('team.roles')
  const [temporaryPassword, setTemporaryPassword] = useState<string | null>(null)
  const [copied, setCopied] = useState(false)

  const {
    register,
    handleSubmit,
    setValue,
    reset,
    formState: { errors, isSubmitting },
  } = useForm<InviteFormValues>({
    resolver: zodResolver(inviteSchema),
  })

  async function onSubmit(data: InviteFormValues) {
    const result = await inviteUser(data)
    trackEvent(AnalyticsEvents.USER_INVITED, {
      role: data.role,
    })
    setTemporaryPassword(result.temporaryPassword)
    onUserInvited(result.user)
  }

  function handleCopy() {
    if (temporaryPassword) {
      navigator.clipboard.writeText(temporaryPassword)
      setCopied(true)
      setTimeout(() => setCopied(false), 2000)
    }
  }

  function handleClose() {
    if (!temporaryPassword) {
      onOpenChange(false)
    } else {
      // Reset state after showing the password
      setTemporaryPassword(null)
      setCopied(false)
      reset()
      onOpenChange(false)
    }
  }

  return (
    <Dialog open={open} onOpenChange={handleClose}>
      <DialogContent className="rounded-xl" data-testid="invite-user-dialog">
        <DialogHeader>
          <DialogTitle className="text-[18px] font-bold text-foreground">{t('title')}</DialogTitle>
          <DialogDescription>
            {t('description')}
          </DialogDescription>
        </DialogHeader>

        {temporaryPassword ? (
          // Success state — show temporary password
          <div className="space-y-4">
            <div
              data-testid="temp-password-alert"
              className="rounded-xl border border-amber-200 bg-amber-50 p-4 space-y-3"
            >
              <p className="text-sm font-semibold text-amber-800">
                {t('success_title')}
              </p>
              <p className="text-xs text-amber-700">
                {t('success_password_hint')}
              </p>
              <div className="flex items-center gap-2">
                <code
                  data-testid="temp-password-value"
                  className="flex-1 rounded bg-white border border-amber-200 px-3 py-2 text-sm font-mono text-amber-900"
                >
                  {temporaryPassword}
                </code>
                <Button
                  variant="outline"
                  size="sm"
                  data-testid="copy-password-btn"
                  onClick={handleCopy}
                  className="rounded-xl font-semibold border-border/80"
                >
                  {copied ? t('copied') : t('copy')}
                </Button>
              </div>
            </div>
            <DialogFooter>
              <Button
                data-testid="invite-done-btn"
                onClick={handleClose}
                className="bg-primary hover:bg-primary/90 text-primary-foreground font-semibold rounded-xl shadow-sm"
              >
                {t('done')}
              </Button>
            </DialogFooter>
          </div>
        ) : (
          // Invite form
          <form onSubmit={handleSubmit(onSubmit)} noValidate>
            <div className="space-y-4 py-2">
              <div className="space-y-2">
                <Label htmlFor="invite-email">
                  {t('email_label')} <span className="text-destructive">*</span>
                </Label>
                <Input
                  id="invite-email"
                  type="email"
                  placeholder={t('email_placeholder')}
                  data-testid="invite-email-input"
                  aria-invalid={!!errors.email}
                  className="rounded-xl border-border/80 text-[13px]"
                  {...register("email")}
                />
                {errors.email && (
                  <p className="text-sm text-destructive" data-testid="invite-email-error">
                    {errors.email.message}
                  </p>
                )}
              </div>

              <div className="space-y-2">
                <Label htmlFor="invite-fullname">
                  {t('fullname_label')} <span className="text-destructive">*</span>
                </Label>
                <Input
                  id="invite-fullname"
                  type="text"
                  placeholder={t('fullname_placeholder')}
                  data-testid="invite-fullname-input"
                  aria-invalid={!!errors.fullName}
                  className="rounded-xl border-border/80 text-[13px]"
                  {...register("fullName")}
                />
                {errors.fullName && (
                  <p className="text-sm text-destructive" data-testid="invite-fullname-error">
                    {errors.fullName.message}
                  </p>
                )}
              </div>

              <div className="space-y-2">
                <Label htmlFor="invite-role">
                  {t('role_label')} <span className="text-destructive">*</span>
                </Label>
                <Select onValueChange={(val) => setValue("role", val as "VET" | "ASSISTANT" | "RECEPTIONIST")}>
                  <SelectTrigger
                    id="invite-role"
                    data-testid="invite-role-select"
                    aria-invalid={!!errors.role}
                    className="rounded-xl border-border/80 text-[13px]"
                  >
                    <SelectValue placeholder={t('role_placeholder')} />
                  </SelectTrigger>
                  <SelectContent>
                    <SelectItem value="VET" data-testid="invite-role-vet">{tRoles('vet')}</SelectItem>
                    <SelectItem value="ASSISTANT" data-testid="invite-role-assistant">{tRoles('assistant')}</SelectItem>
                    <SelectItem value="RECEPTIONIST" data-testid="invite-role-receptionist">{tRoles('receptionist')}</SelectItem>
                  </SelectContent>
                </Select>
                {errors.role && (
                  <p className="text-sm text-destructive" data-testid="invite-role-error">
                    {errors.role.message}
                  </p>
                )}
              </div>
            </div>

            <DialogFooter className="mt-4">
              <Button
                type="button"
                variant="outline"
                data-testid="invite-cancel-btn"
                onClick={handleClose}
                disabled={isSubmitting}
                className="rounded-xl font-semibold border-border/80 hover:bg-muted"
              >
                {t('cancel')}
              </Button>
              <Button
                type="submit"
                data-testid="invite-submit-btn"
                disabled={isSubmitting}
                className="bg-primary hover:bg-primary/90 text-primary-foreground font-semibold rounded-xl shadow-sm"
              >
                {isSubmitting ? t('submitting') : t('submit')}
              </Button>
            </DialogFooter>
          </form>
        )}
      </DialogContent>
    </Dialog>
  )
}
