"use client"

import { useEffect, useState, useCallback } from "react"
import { useRouter } from "next/navigation"
import { Button } from "@/components/ui/button"
import { TeamTable } from "@/components/features/users/TeamTable"
import { InviteUserDialog } from "@/components/features/users/InviteUserDialog"
import { getUsers } from "@/lib/api/users"
import { useRole } from "@/hooks/use-role"
import type { UserDto, UserRole } from "@/lib/api/users"

function parseCurrentUserIdFromToken(): string | null {
  if (typeof window === "undefined") return null
  const token = localStorage.getItem("access_token")
  if (!token) return null
  try {
    const parts = token.split(".")
    const payload = JSON.parse(atob(parts[1].replace(/-/g, "+").replace(/_/g, "/"))) as Record<string, unknown>
    return (payload["sub"] as string) || null
  } catch {
    return null
  }
}

export default function TeamPage() {
  const role = useRole()
  const router = useRouter()
  const [users, setUsers] = useState<UserDto[]>([])
  const [isLoading, setIsLoading] = useState(true)
  const [inviteOpen, setInviteOpen] = useState(false)
  const [currentUserEmail, setCurrentUserEmail] = useState<string>("")

  // RBAC: redirect non-ADMIN
  useEffect(() => {
    if (role && role !== "VET" && role !== "ADMIN") {
      // Non-VET non-ADMIN: handled below
    }
    if (role && role !== "ADMIN") {
      // Use a short delay to let the role hydrate from localStorage
      const timer = setTimeout(() => {
        if (role !== "ADMIN") {
          router.replace("/appointments")
        }
      }, 500)
      return () => clearTimeout(timer)
    }
  }, [role, router])

  useEffect(() => {
    const email = parseCurrentUserIdFromToken()
    setCurrentUserEmail(email ?? "")
  }, [])

  const loadUsers = useCallback(async () => {
    try {
      setIsLoading(true)
      const data = await getUsers()
      setUsers(data)
    } catch {
      // silent — MSW or API error
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
          <h1 className="text-2xl font-bold tracking-tight">Team</h1>
          <p className="text-muted-foreground text-sm mt-1">
            Manage your clinic&apos;s team members
          </p>
        </div>
        <Button
          data-testid="invite-member-btn"
          onClick={() => setInviteOpen(true)}
        >
          Invite Member
        </Button>
      </div>

      {isLoading ? (
        <div data-testid="team-loading" className="text-sm text-muted-foreground">
          Loading team...
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
