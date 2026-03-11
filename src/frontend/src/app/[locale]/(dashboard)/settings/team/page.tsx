"use client"

import { useEffect, useState, useCallback } from "react"
import { useRouter } from "next/navigation"
import { Users } from "lucide-react"
import { Button } from "@/components/ui/button"
import { Skeleton } from "@/components/ui/skeleton"
import { TeamTable } from "@/components/features/users/TeamTable"
import { InviteUserDialog } from "@/components/features/users/InviteUserDialog"
import { EmptyState } from "@/components/features/onboarding/EmptyState"
import { ErrorState } from "@/components/ui/error-state"
import { getUsers } from "@/lib/api/users"
import { useRole } from "@/hooks/use-role"
import { useTranslations } from "next-intl"
import type { UserDto, UserRole } from "@/lib/api/users"
import { getStoredUser } from "@/lib/api/auth"

export default function TeamPage() {
  const t = useTranslations('team')
  const tEmpty = useTranslations('onboarding.empty.team')
  const role = useRole()
  const router = useRouter()
  const [users, setUsers] = useState<UserDto[]>([])
  const [isLoading, setIsLoading] = useState(true)
  const [error, setError] = useState<string | null>(null)
  const [inviteOpen, setInviteOpen] = useState(false)
  const [currentUserEmail, setCurrentUserEmail] = useState<string>("")

  useEffect(() => {
    if (role && role !== "VET" && role !== "ADMIN") {
      // Non-VET non-ADMIN: handled below
    }
    if (role && role !== "ADMIN") {
      const timer = setTimeout(() => {
        if (role !== "ADMIN") {
          router.replace("/appointments")
        }
      }, 500)
      return () => clearTimeout(timer)
    }
  }, [role, router])

  useEffect(() => {
    const user = getStoredUser()
    setCurrentUserEmail(user?.email ?? "")
  }, [])

  const loadUsers = useCallback(async () => {
    try {
      setIsLoading(true)
      setError(null)
      const data = await getUsers()
      setUsers(data)
    } catch {
      setError('Failed to load team members')
    } finally {
      setIsLoading(false)
    }
  }, [])

  useEffect(() => {
    if (role === "ADMIN") {
      loadUsers()
    }
  }, [role, loadUsers])

  function handleRoleChanged(userId: string, newRole: UserRole) {
    setUsers((prev) =>
      prev.map((u) => (u.id === userId ? { ...u, role: newRole } : u))
    )
  }

  function handleUserDeactivated(userId: string) {
    setUsers((prev) => prev.filter((u) => u.id !== userId))
  }

  function handleUserInvited(user: UserDto) {
    setUsers((prev) => [...prev, user])
  }

  if (role !== "ADMIN") {
    return null
  }

  return (
    <div className="space-y-6" data-testid="team-page">
      <div className="flex items-center justify-between">
        <div>
          <h1 className="text-2xl font-bold tracking-tight">{t('title')}</h1>
          <p className="text-muted-foreground text-sm mt-1">
            {t('subtitle')}
          </p>
        </div>
        <Button
          data-testid="invite-member-btn"
          onClick={() => setInviteOpen(true)}
        >
          {t('invite_member')}
        </Button>
      </div>

      {isLoading ? (
        <div data-testid="team-loading" className="space-y-3">
          {[1, 2, 3].map((i) => (
            <Skeleton key={i} className="h-12 w-full rounded-lg" />
          ))}
        </div>
      ) : error ? (
        <ErrorState
          data-testid="team-error"
          title="Failed to load team members"
          description={error}
          onRetry={loadUsers}
        />
      ) : users.length <= 1 ? (
        <EmptyState
          icon={<Users className="h-16 w-16" />}
          title={tEmpty('title')}
          description={tEmpty('description')}
          primaryCta={{
            label: tEmpty('cta'),
            onClick: () => setInviteOpen(true),
          }}
          tip={tEmpty('tip')}
          data-testid-prefix="team"
        />
      ) : (
        <TeamTable
          users={users}
          currentUserEmail={currentUserEmail}
          isAdmin={true}
          onRoleChanged={handleRoleChanged}
          onUserDeactivated={handleUserDeactivated}
        />
      )}

      <InviteUserDialog
        open={inviteOpen}
        onOpenChange={setInviteOpen}
        onUserInvited={handleUserInvited}
      />
    </div>
  )
}
