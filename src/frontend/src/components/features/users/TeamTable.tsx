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
import { Badge } from "@/components/ui/badge"
import { Button } from "@/components/ui/button"
import { ChangeRoleDialog } from "./ChangeRoleDialog"
import { deactivateUser } from "@/lib/api/users"
import type { UserDto, UserRole } from "@/lib/api/users"

interface TeamTableProps {
  users: UserDto[]
  /** Email of the currently authenticated user — used to disable self-deactivation */
  currentUserEmail: string
  isAdmin: boolean
  onRoleChanged: (userId: string, newRole: UserRole) => void
  onUserDeactivated: (userId: string) => void
}

function roleBadgeVariant(role: UserRole): string {
  switch (role) {
    case "ADMIN":
      return "destructive"
    case "VET":
      return "default"
    case "ASSISTANT":
      return "secondary"
    case "RECEPTIONIST":
      return "outline"
    default:
      return "secondary"
  }
}

function roleBadgeClass(role: UserRole): string {
  switch (role) {
    case "ADMIN":
      return "bg-red-100 text-red-700 border-red-200"
    case "VET":
      return "bg-blue-100 text-blue-700 border-blue-200"
    case "ASSISTANT":
      return "bg-green-100 text-green-700 border-green-200"
    case "RECEPTIONIST":
      return "bg-orange-100 text-orange-700 border-orange-200"
    default:
      return ""
  }
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
      <Table data-testid="team-table">
        <TableHeader>
          <TableRow>
            <TableHead>Name</TableHead>
            <TableHead>Email</TableHead>
            <TableHead>Role</TableHead>
            <TableHead>Status</TableHead>
            {isAdmin && <TableHead className="text-right">Actions</TableHead>}
          </TableRow>
        </TableHeader>
        <TableBody>
          {users.map((user) => (
            <TableRow key={user.id} data-testid={`user-row-${user.id}`}>
              <TableCell data-testid={`user-name-${user.id}`}>
                {user.fullName}
              </TableCell>
              <TableCell data-testid={`user-email-${user.id}`}>
                {user.email}
              </TableCell>
              <TableCell>
                <Badge
                  data-testid={`user-role-badge-${user.id}`}
                  variant="outline"
                  className={roleBadgeClass(user.role)}
                >
                  {user.role}
                </Badge>
              </TableCell>
              <TableCell>
                <Badge
                  data-testid={`user-status-${user.id}`}
                  variant={user.isActive ? "default" : "secondary"}
                  className={
                    user.isActive
                      ? "bg-green-100 text-green-700 border-green-200"
                      : "bg-gray-100 text-gray-500 border-gray-200"
                  }
                >
                  {user.isActive ? "Active" : "Inactive"}
                </Badge>
              </TableCell>
              {isAdmin && (
                <TableCell className="text-right space-x-2">
                  <Button
                    variant="outline"
                    size="sm"
                    data-testid={`change-role-btn-${user.id}`}
                    onClick={() => setChangeRoleUser(user)}
                  >
                    Change Role
                  </Button>
                  <Button
                    variant="destructive"
                    size="sm"
                    data-testid={`deactivate-btn-${user.id}`}
                    disabled={
                      user.email === currentUserEmail ||
                      deactivatingId === user.id
                    }
                    onClick={() => handleDeactivate(user)}
                  >
                    {deactivatingId === user.id ? "Deactivating..." : "Deactivate"}
                  </Button>
                </TableCell>
              )}
            </TableRow>
          ))}
        </TableBody>
      </Table>

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
