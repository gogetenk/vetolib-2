"use client"

import { useMemo } from "react"
import { User, Mail, Shield } from "lucide-react"
import { Card, CardHeader, CardTitle, CardDescription, CardContent } from "@/components/ui/card"
import { Button } from "@/components/ui/button"
import { getStoredUser } from "@/lib/api/auth"
import { useRole } from "@/hooks/use-role"

interface ProfileInfo {
  name: string
  email: string
  clinicName: string
}

export default function ProfilePage() {
  const role = useRole()
  const profile = useMemo<ProfileInfo | null>(() => {
    const user = getStoredUser()
    if (user) {
      return {
        name: user.name,
        email: user.email,
        clinicName: user.clinicName,
      }
    }
    return null
  }, [])

  return (
    <div className="space-y-6" data-testid="profile-page">
      <div>
        <h1 className="text-2xl font-bold tracking-tight">Profile</h1>
        <p className="text-sm text-muted-foreground mt-1">
          View your account information.
        </p>
      </div>

      <Card data-testid="profile-card">
        <CardHeader>
          <CardTitle>Account Details</CardTitle>
          <CardDescription>Your personal information and role within the clinic.</CardDescription>
        </CardHeader>
        <CardContent className="space-y-4">
          {profile ? (
            <>
              <div className="flex items-center gap-3" data-testid="profile-name">
                <User className="h-5 w-5 text-muted-foreground" />
                <div>
                  <p className="text-sm font-medium text-muted-foreground">Name</p>
                  <p className="text-base">{profile.name}</p>
                </div>
              </div>

              <div className="flex items-center gap-3" data-testid="profile-email">
                <Mail className="h-5 w-5 text-muted-foreground" />
                <div>
                  <p className="text-sm font-medium text-muted-foreground">Email</p>
                  <p className="text-base">{profile.email}</p>
                </div>
              </div>

              <div className="flex items-center gap-3" data-testid="profile-role">
                <Shield className="h-5 w-5 text-muted-foreground" />
                <div>
                  <p className="text-sm font-medium text-muted-foreground">Role</p>
                  <p className="text-base capitalize">{role.toLowerCase()}</p>
                </div>
              </div>

              <div className="flex items-center gap-3" data-testid="profile-clinic">
                <div className="h-5 w-5" />
                <div>
                  <p className="text-sm font-medium text-muted-foreground">Clinic</p>
                  <p className="text-base">{profile.clinicName}</p>
                </div>
              </div>
            </>
          ) : (
            <p className="text-sm text-muted-foreground" data-testid="profile-not-loaded">
              Unable to load profile information. Please log in again.
            </p>
          )}
        </CardContent>
      </Card>

      <Card data-testid="profile-security-card">
        <CardHeader>
          <CardTitle>Security</CardTitle>
          <CardDescription>Manage your password and security settings.</CardDescription>
        </CardHeader>
        <CardContent>
          <Button variant="outline" data-testid="change-password-btn">
            Change Password
          </Button>
        </CardContent>
      </Card>
    </div>
  )
}
