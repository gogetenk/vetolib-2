"use client"

import { useState } from "react"
import { toast } from "sonner"
import {
  Dialog,
  DialogContent,
  DialogDescription,
  DialogFooter,
  DialogHeader,
  DialogTitle,
} from "@/components/ui/dialog"
import { Button } from "@/components/ui/button"
import { Label } from "@/components/ui/label"
import {
  Select,
  SelectContent,
  SelectItem,
  SelectTrigger,
  SelectValue,
} from "@/components/ui/select"
import { changeUserRole } from "@/lib/api/users"
import type { UserDto, UserRole } from "@/lib/api/users"
import { trackEvent, AnalyticsEvents } from "@/lib/analytics"
import { useTranslations } from "next-intl"

const ASSIGNABLE_ROLE_VALUES: Exclude<UserRole, "ADMIN">[] = ["VET", "ASSISTANT", "RECEPTIONIST"]

interface ChangeRoleDialogProps {
  user: UserDto
  open: boolean
  onOpenChange: (open: boolean) => void
  onRoleChanged: (newRole: UserRole) => void
}

export function ChangeRoleDialog({
  user,
  open,
  onOpenChange,
  onRoleChanged,
}: ChangeRoleDialogProps) {
  const t = useTranslations('team.change_role')
  const tRoles = useTranslations('team.roles')
  const [selectedRole, setSelectedRole] = useState<string | null>(user.role === "ADMIN" ? "VET" : user.role)
  const [isSubmitting, setIsSubmitting] = useState(false)

  async function handleSubmit() {
    if (!selectedRole || isSubmitting) return
    const role = selectedRole as Exclude<UserRole, "ADMIN">
    setIsSubmitting(true)
    const fromRole = user.role
    try {
      await changeUserRole(user.id, { role })
      trackEvent(AnalyticsEvents.USER_ROLE_CHANGED, {
        from_role: fromRole,
        to_role: role,
      })
      toast.success(t('success', { fullName: user.fullName, role }))
      onRoleChanged(role as UserRole)
    } catch {
      toast.error(t('error'))
    } finally {
      setIsSubmitting(false)
    }
  }

  return (
    <Dialog open={open} onOpenChange={onOpenChange}>
      <DialogContent className="rounded-2xl" data-testid="change-role-dialog">
        <DialogHeader>
          <DialogTitle className="text-[18px] font-bold text-foreground">{t('title')}</DialogTitle>
          <DialogDescription>
            {t('description', { fullName: user.fullName })}
          </DialogDescription>
        </DialogHeader>

        <div className="space-y-4 py-2">
          <div className="space-y-2">
            <Label htmlFor="role-select">{t('label')}</Label>
            <Select
              value={selectedRole}
              onValueChange={(val) => setSelectedRole(val)}
            >
              <SelectTrigger
                id="role-select"
                data-testid="change-role-select"
                className="rounded-xl border-border/80 text-[13px]"
              >
                <SelectValue placeholder={t('placeholder')} />
              </SelectTrigger>
              <SelectContent>
                {ASSIGNABLE_ROLE_VALUES.map((value) => (
                  <SelectItem
                    key={value}
                    value={value}
                    data-testid={`role-option-${value}`}
                  >
                    {tRoles(value.toLowerCase() as 'vet' | 'assistant' | 'receptionist')}
                  </SelectItem>
                ))}
              </SelectContent>
            </Select>
          </div>
        </div>

        <DialogFooter>
          <Button
            variant="outline"
            data-testid="change-role-cancel-btn"
            onClick={() => onOpenChange(false)}
            disabled={isSubmitting}
            className="rounded-xl font-semibold border-border/80 hover:bg-muted"
          >
            {t('cancel')}
          </Button>
          <Button
            data-testid="change-role-confirm-btn"
            onClick={handleSubmit}
            disabled={isSubmitting}
            className="bg-primary hover:bg-primary/90 text-primary-foreground font-semibold rounded-xl shadow-sm"
          >
            {isSubmitting ? t('saving') : t('save')}
          </Button>
        </DialogFooter>
      </DialogContent>
    </Dialog>
  )
}
