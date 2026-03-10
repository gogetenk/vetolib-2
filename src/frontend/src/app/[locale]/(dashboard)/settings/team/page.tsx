"use client"

import { useEffect, useState, useCallback } from "react"
import { useRouter } from "next/navigation"
import { Button } from "@/components/ui/button"
import { TeamTable } from "@/components/features/users/TeamTable"
import { InviteUserDialog } from "@/components/features/users/InviteUserDialog"
import { getUsers } from "@/lib/api/users"
import { useRole } from "@/hooks/use-role"
import { useTranslations } from "next-intl"
import type { UserDto, UserRole } from "@/lib/api/users"
import { getStoredUser } from "@/lib/api/auth"

export default function TeamPage() {
  const t = useTranslations('team')
  const role = useRole()
  const router = useRouter()
  const [users, setUsers] = useState<UserDto[]>([])
  const [isLoading, setIsLoading] = useState(true)
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
      const data = await getUsers()
      setUsers(data)
    } catch {
      // silent
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
        <div data-testid="team-loading" className="text-sm text-muted-foreground">
          {t('loading')}
        </div>
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
