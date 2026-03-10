"use client"

import { useRouter } from "next/navigation"
import { useEffect } from "react"
import { useRole } from "@/hooks/use-role"
import { MessagingHoursPage } from "@/components/features/messaging/admin/MessagingHoursPage"

export default function MessagingHoursSettingsPage() {
  const role = useRole()
  const router = useRouter()

  useEffect(() => {
    if (role && role !== "ADMIN") {
      router.replace("/dashboard")
    }
  }, [role, router])

  if (role !== "ADMIN") return null

  return <MessagingHoursPage />
}
