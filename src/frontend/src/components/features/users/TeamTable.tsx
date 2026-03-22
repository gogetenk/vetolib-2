"use client"

import { useState } from "react"
import { toast } from "sonner"
import {
  Table,
  TableBody,
  TableCell,
  TableHead,
  TableHeader,
  TableRow,
} from "@/components/ui/table"
import { Button } from "@/components/ui/button"
import { ChangeRoleDialog } from "./ChangeRoleDialog"
import { deactivateUser } from "@/lib/api/users"
import type { UserDto, UserRole } from "@/lib/api/users"
import { cn } from "@/lib/utils"
import { Pencil } from "lucide-react"

interface TeamTableProps {
  users: UserDto[]
  /** Email of the currently authenticated user — used to disable self-deactivation */
  currentUserEmail: string
  isAdmin: boolean
  onRoleChanged: (userId: string, newRole: UserRole) => void
  onUserDeactivated: (userId: string) => void
}

function roleBadgeClass(role: UserRole): string {
  switch (role) {
    case "ADMIN":
      return "bg-red-50 text-red-500"
    case "VET":
      return "bg-primary/10 text-primary"
    case "ASSISTANT":
      return "bg-success/10 text-success"
    case "RECEPTIONIST":
      return "bg-orange-50 text-orange-500"
    default:
      return "bg-muted text-muted-foreground"
  }
}

function getInitials(name: string): string {
  return name
    .split(" ")
    .map((part) => part[0])
    .join("")
    .toUpperCase()
    .slice(0, 2);
}

export function TeamTable({
  users,
  currentUserEmail,
  isAdmin,
  onRoleChanged,
  onUserDeactivated,
}: TeamTableProps) {
  const [changeRoleUser, setChangeRoleUser] = useState<UserDto | null>(null)
  const [deactivatingId, setDeactivatingId] = useState<string | null>(null)

  async function handleDeactivate(user: UserDto) {
    if (deactivatingId) return
    setDeactivatingId(user.id)
    try {
      await deactivateUser(user.id)
      toast.success(`${user.fullName} has been deactivated`)
      onUserDeactivated(user.id)
    } catch {
      toast.error("Failed to deactivate user")
    } finally {
      setDeactivatingId(null)
    }
  }

  return (
    <>
      <div className="bg-white border border-border/80 rounded-xl shadow-sm overflow-hidden">
        <Table data-testid="team-table">
          <TableHeader>
            <TableRow className="bg-muted hover:bg-muted border-b border-border/50">
              <TableHead className="h-12 px-6 text-[11px] font-bold text-foreground uppercase tracking-wider">Collaborateurs</TableHead>
              <TableHead className="h-12 px-6 text-[11px] font-bold text-foreground uppercase tracking-wider">Niveau de visibilité</TableHead>
              <TableHead className="h-12 px-6 text-[11px] font-bold text-foreground uppercase tracking-wider text-center">Statut</TableHead>
              {isAdmin && <TableHead className="h-12 px-6 text-[11px] font-bold text-foreground uppercase tracking-wider text-right w-24"></TableHead>}
            </TableRow>
          </TableHeader>
          <TableBody>
            {users.map((user) => (
              <TableRow key={user.id} data-testid={`user-row-${user.id}`} className="hover:bg-muted/50 border-border/30 transition-colors">
                <TableCell className="px-6 py-4" data-testid={`user-name-${user.id}`}>
                  <div className="flex items-center gap-4">
                    <div className={cn("flex h-10 w-10 shrink-0 items-center justify-center rounded-full text-sm font-bold", roleBadgeClass(user.role))}>
                      {getInitials(user.fullName)}
                    </div>
                    <div className="flex flex-col">
                      <span className="text-[14px] font-semibold text-foreground">{user.fullName}</span>
                      <span className="text-[12px] text-muted-foreground">{user.role.toLowerCase().replace('_', ' ')}</span>
                    </div>
                  </div>
                </TableCell>
                <TableCell className="px-6 py-4 text-[13px] text-muted-foreground font-medium">
                  Tous détails
                </TableCell>
                <TableCell className="px-6 py-4 text-center">
                  <button
                    data-testid={`user-status-toggle-${user.id}`}
                    onClick={() => isAdmin && handleDeactivate(user)}
                    disabled={!isAdmin || user.email === currentUserEmail || deactivatingId === user.id}
                    className={cn(
                      "relative inline-flex h-5 w-9 shrink-0 cursor-pointer items-center justify-center rounded-full transition-colors duration-200 ease-in-out focus:outline-none focus-visible:ring-2 focus-visible:ring-primary focus-visible:ring-offset-2",
                      user.isActive ? "bg-primary" : "bg-muted-foreground/30",
                      (!isAdmin || user.email === currentUserEmail) && "cursor-not-allowed opacity-50"
                    )}
                    role="switch"
                    aria-checked={user.isActive}
                  >
                    <span
                      aria-hidden="true"
                      className={cn(
                        "pointer-events-none inline-block h-4 w-4 transform rounded-full bg-white shadow ring-0 transition duration-200 ease-in-out",
                        user.isActive ? "translate-x-2" : "-translate-x-2"
                      )}
                    />
                  </button>
                </TableCell>
                {isAdmin && (
                  <TableCell className="px-6 py-4 text-right">
                    <Button
                      variant="ghost"
                      size="icon-sm"
                      className="rounded-full text-muted-foreground hover:text-primary hover:bg-primary/5"
                      data-testid={`change-role-btn-${user.id}`}
                      onClick={() => setChangeRoleUser(user)}
                    >
                      <Pencil className="size-4" />
                    </Button>
                  </TableCell>
                )}
              </TableRow>
            ))}
          </TableBody>
        </Table>
      </div>

      {changeRoleUser && (
        <ChangeRoleDialog
          user={changeRoleUser}
          open={!!changeRoleUser}
          onOpenChange={(open) => {
            if (!open) setChangeRoleUser(null)
          }}
          onRoleChanged={(newRole) => {
            onRoleChanged(changeRoleUser.id, newRole)
            setChangeRoleUser(null)
          }}
        />
      )}
    </>
  )
}
