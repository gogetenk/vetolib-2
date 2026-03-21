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
      <DialogContent className="rounded-2xl" data-testid="invite-user-dialog">
        <DialogHeader>
          <DialogTitle className="text-[18px] font-bold text-foreground">Invite Team Member</DialogTitle>
          <DialogDescription>
            Send an invitation to a new team member. They will receive a temporary password.
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
                Invitation sent successfully!
              </p>
              <p className="text-xs text-amber-700">
                Share this password securely — it won&apos;t be shown again.
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
                  {copied ? "Copied!" : "Copy"}
                </Button>
              </div>
            </div>
            <DialogFooter>
              <Button
                data-testid="invite-done-btn"
                onClick={handleClose}
                className="bg-primary hover:bg-primary/90 text-primary-foreground font-semibold rounded-xl shadow-sm"
              >
                Done
              </Button>
            </DialogFooter>
          </div>
        ) : (
          // Invite form
          <form onSubmit={handleSubmit(onSubmit)} noValidate>
            <div className="space-y-4 py-2">
              <div className="space-y-2">
                <Label htmlFor="invite-email">
                  Email <span className="text-destructive">*</span>
                </Label>
                <Input
                  id="invite-email"
                  type="email"
                  placeholder="colleague@desertpaws.ae"
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
                  Full Name <span className="text-destructive">*</span>
                </Label>
                <Input
                  id="invite-fullname"
                  type="text"
                  placeholder="Dr. Fatima Al-Zaabi"
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
                  Role <span className="text-destructive">*</span>
                </Label>
                <Select onValueChange={(val) => setValue("role", val as "VET" | "ASSISTANT" | "RECEPTIONIST")}>
                  <SelectTrigger
                    id="invite-role"
                    data-testid="invite-role-select"
                    aria-invalid={!!errors.role}
                    className="rounded-xl border-border/80 text-[13px]"
                  >
                    <SelectValue placeholder="Select a role" />
                  </SelectTrigger>
                  <SelectContent>
                    <SelectItem value="VET" data-testid="invite-role-vet">Vet</SelectItem>
                    <SelectItem value="ASSISTANT" data-testid="invite-role-assistant">Assistant</SelectItem>
                    <SelectItem value="RECEPTIONIST" data-testid="invite-role-receptionist">Receptionist</SelectItem>
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
                Cancel
              </Button>
              <Button
                type="submit"
                data-testid="invite-submit-btn"
                disabled={isSubmitting}
                className="bg-primary hover:bg-primary/90 text-primary-foreground font-semibold rounded-xl shadow-sm"
              >
                {isSubmitting ? "Sending..." : "Send Invite"}
              </Button>
            </DialogFooter>
          </form>
        )}
      </DialogContent>
    </Dialog>
  )
}
