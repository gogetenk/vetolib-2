'use client'

import { useEffect, useState, useCallback } from 'react'
import Link from 'next/link'
import { Plus } from 'lucide-react'
import { Input } from '@/components/ui/input'
import { Button } from '@/components/ui/button'
import { PatientCard } from '@/components/features/patients/PatientCard'
import { getPatients } from '@/lib/api/patients'
import type { PatientDto } from '@/lib/api/patients'
import { useRole } from '@/hooks/use-role'

export default function PatientsPage() {
  const [patients, setPatients] = useState<PatientDto[]>([])
  const [searchQuery, setSearchQuery] = useState('')
  const [isLoading, setIsLoading] = useState(true)
  const role = useRole()

  const canWrite = role === 'VET' || role === 'ADMIN'

  const fetchPatients = useCallback(async (search?: string) => {
    setIsLoading(true)
    try {
      const result = await getPatients({ search: search || undefined })
      setPatients(result.items)
    } catch {
      setPatients([])
    } finally {
      setIsLoading(false)
    }
  }, [])

  useEffect(() => {
    fetchPatients()
  }, [fetchPatients])

  useEffect(() => {
    const timer = setTimeout(() => {
      fetchPatients(searchQuery)
    }, 300)
    return () => clearTimeout(timer)
  }, [searchQuery, fetchPatients])

  return (
    <div className="space-y-6" data-testid="patients-page">
      <div className="flex items-center justify-between">
        <h1 className="text-2xl font-bold" data-testid="patients-title">
          Patients
        </h1>
        {canWrite && (
          <Button
            render={<Link href="/patients/new" />}
            data-testid="add-patient-btn"
          >
            <Plus className="h-4 w-4 mr-1" />
            Add Patient
          </Button>
        )}
      </div>

      <div>
        <Input
          type="search"
          placeholder="Search by name or owner..."
          value={searchQuery}
          onChange={(e) => setSearchQuery(e.target.value)}
          data-testid="search-input"
          className="max-w-sm"
        />
      </div>

      {isLoading ? (
        <div
          data-testid="patients-loading"
          className="grid grid-cols-1 gap-4 md:grid-cols-2 lg:grid-cols-3"
        >
          {[1, 2, 3, 4].map((i) => (
            <div key={i} className="h-48 rounded-lg bg-muted animate-pulse" />
          ))}
        </div>
      ) : patients.length === 0 ? (
        <p
          className="text-muted-foreground text-sm py-12 text-center"
          data-testid="patients-empty"
        >
          No patients found.
        </p>
      ) : (
        <div
          data-testid="patients-table"
          className="grid grid-cols-1 gap-4 md:grid-cols-2 lg:grid-cols-3"
        >
          {patients.map((patient) => (
            <PatientCard key={patient.id} patient={patient} />
          ))}
        </div>
      )}
    </div>
  )
}
