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
    <div className="p-6 lg:p-8 space-y-6" data-testid="profile-page">
      <div>
        <h1 className="text-[22px] font-bold text-[#061e44] flex items-center gap-2">
          <span className="w-1 h-5 bg-[#303ef5] rounded-full"></span>
          Profile
        </h1>
        <p className="text-[13px] text-muted-foreground mt-1 ml-3">
          View your account information.
        </p>
      </div>

      <Card className="bg-white border-border/80 rounded-xl shadow-sm" data-testid="profile-card">
        <CardHeader>
          <CardTitle className="text-[15px] font-bold text-[#061e44]">Account Details</CardTitle>
          <CardDescription className="text-[13px] text-muted-foreground">Your personal information and role within the clinic.</CardDescription>
        </CardHeader>
        <CardContent className="space-y-5">
          {profile ? (
            <>
              <div className="flex items-center gap-4" data-testid="profile-name">
                <div className="flex h-9 w-9 items-center justify-center rounded-xl bg-[#eef2fd]">
                  <User className="h-4 w-4 text-[#303ef5]" />
                </div>
                <div>
                  <p className="text-[12px] font-semibold text-muted-foreground uppercase tracking-wider">Name</p>
                  <p className="text-[14px] font-semibold text-[#061e44]">{profile.name}</p>
                </div>
              </div>

              <div className="flex items-center gap-4" data-testid="profile-email">
                <div className="flex h-9 w-9 items-center justify-center rounded-xl bg-[#eef2fd]">
                  <Mail className="h-4 w-4 text-[#303ef5]" />
                </div>
                <div>
                  <p className="text-[12px] font-semibold text-muted-foreground uppercase tracking-wider">Email</p>
                  <p className="text-[14px] font-semibold text-[#061e44]">{profile.email}</p>
                </div>
              </div>

              <div className="flex items-center gap-4" data-testid="profile-role">
                <div className="flex h-9 w-9 items-center justify-center rounded-xl bg-[#eef2fd]">
                  <Shield className="h-4 w-4 text-[#303ef5]" />
                </div>
                <div>
                  <p className="text-[12px] font-semibold text-muted-foreground uppercase tracking-wider">Role</p>
                  <p className="text-[14px] font-semibold text-[#061e44] capitalize">{role.toLowerCase()}</p>
                </div>
              </div>

              <div className="flex items-center gap-4" data-testid="profile-clinic">
                <div className="h-9 w-9" />
                <div>
                  <p className="text-[12px] font-semibold text-muted-foreground uppercase tracking-wider">Clinic</p>
                  <p className="text-[14px] font-semibold text-[#061e44]">{profile.clinicName}</p>
                </div>
              </div>
            </>
          ) : (
            <p className="text-[13px] text-muted-foreground" data-testid="profile-not-loaded">
              Unable to load profile information. Please log in again.
            </p>
          )}
        </CardContent>
      </Card>

      <Card className="bg-white border-border/80 rounded-xl shadow-sm" data-testid="profile-security-card">
        <CardHeader>
          <CardTitle className="text-[15px] font-bold text-[#061e44]">Security</CardTitle>
          <CardDescription className="text-[13px] text-muted-foreground">Manage your password and security settings.</CardDescription>
        </CardHeader>
        <CardContent>
          <Button variant="outline" data-testid="change-password-btn" className="rounded-xl h-10 px-5 font-semibold border-border/80 hover:bg-[#f4f6f9]">
            Change Password
          </Button>
        </CardContent>
      </Card>
    </div>
  )
}
