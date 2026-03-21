"use client"

import { useRouter } from "next/navigation"
import { useEffect } from "react"
import { useRole } from "@/hooks/use-role"
import { WhatsAppSettingsPage } from "@/components/features/messaging/admin/WhatsAppSettingsPage"

export default function WhatsAppSettingsRoute() {
  const role = useRole()
  const router = useRouter()

  useEffect(() => {
    if (role && role.toUpperCase() !== "ADMIN") {
      router.replace("/dashboard")
    }
  }, [role, router])

  if (role.toUpperCase() !== "ADMIN") return null

  return <WhatsAppSettingsPage />
}
