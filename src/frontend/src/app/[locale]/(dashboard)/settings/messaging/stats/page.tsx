"use client"

import { useRouter } from "next/navigation"
import { useEffect } from "react"
import { useRole } from "@/hooks/use-role"
import { TriageStatsPage } from "@/components/features/messaging/admin/TriageStatsPage"

export default function TriageStatsSettingsPage() {
  const role = useRole()
  const router = useRouter()

  useEffect(() => {
    if (role && role !== "ADMIN") {
      router.replace("/dashboard")
    }
  }, [role, router])

  if (role !== "ADMIN") return null

  return <TriageStatsPage />
}
