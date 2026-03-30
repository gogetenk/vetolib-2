'use client'

import { useState } from 'react'
import {
  Dialog,
  DialogContent,
  DialogHeader,
  DialogTitle,
  DialogFooter,
} from '@/components/ui/dialog'
import { Button } from '@/components/ui/button'
import { Input } from '@/components/ui/input'
import { Label } from '@/components/ui/label'
import { Textarea } from '@/components/ui/textarea'
import type { CreateLitterRequest } from '@/lib/api/breeding'

interface RegisterLitterDialogProps {
  open: boolean
  onOpenChange: (open: boolean) => void
  motherId: string
  species: string
  onSubmit: (data: CreateLitterRequest) => Promise<void>
}

function todayDateString(): string {
  const now = new Date()
  return `${now.getFullYear()}-${String(now.getMonth() + 1).padStart(2, '0')}-${String(now.getDate()).padStart(2, '0')}`
}

export function RegisterLitterDialog({
  open,
  onOpenChange,
  motherId,
  onSubmit,
}: RegisterLitterDialogProps) {
  const [birthDate, setBirthDate] = useState(todayDateString())
  const [fatherPatientId, setFatherPatientId] = useState('')
  const [externalFatherName, setExternalFatherName] = useState('')
  const [bornCount, setBornCount] = useState('')
  const [aliveCount, setAliveCount] = useState('')
  const [notes, setNotes] = useState('')
  const [isSubmitting, setIsSubmitting] = useState(false)

  const resetForm = () => {
    setBirthDate(todayDateString())
    setFatherPatientId('')
    setExternalFatherName('')
    setBornCount('')
    setAliveCount('')
    setNotes('')
  }

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault()
    setIsSubmitting(true)
    try {
      await onSubmit({
        motherPatientId: motherId,
        fatherPatientId: fatherPatientId.trim() || null,
        externalFatherName: externalFatherName.trim() || null,
        birthDate,
        bornCount: parseInt(bornCount, 10),
        aliveCount: parseInt(aliveCount, 10),
        notes: notes.trim() || null,
      })
      resetForm()
    } finally {
      setIsSubmitting(false)
    }
  }

  const handleOpenChange = (newOpen: boolean) => {
    if (!newOpen) {
      resetForm()
    }
    onOpenChange(newOpen)
  }

  return (
    <Dialog open={open} onOpenChange={handleOpenChange}>
      <DialogContent className="sm:max-w-md">
        <DialogHeader>
          <DialogTitle>Register Litter</DialogTitle>
        </DialogHeader>
        <form onSubmit={handleSubmit} className="space-y-4">
          <div className="space-y-1.5">
            <Label htmlFor="litter-dob">Date of Birth</Label>
            <Input
              id="litter-dob"
              type="date"
              value={birthDate}
              onChange={(e) => setBirthDate(e.target.value)}
              data-testid="litter-dob-input"
              required
            />
          </div>
          <div className="grid grid-cols-2 gap-3">
            <div className="space-y-1.5">
              <Label htmlFor="litter-born-count">Born Count</Label>
              <Input
                id="litter-born-count"
                type="number"
                min={0}
                value={bornCount}
                onChange={(e) => setBornCount(e.target.value)}
                data-testid="litter-born-count-input"
                required
              />
            </div>
            <div className="space-y-1.5">
              <Label htmlFor="litter-alive-count">Alive Count</Label>
              <Input
                id="litter-alive-count"
                type="number"
                min={0}
                value={aliveCount}
                onChange={(e) => setAliveCount(e.target.value)}
                data-testid="litter-alive-count-input"
                required
              />
            </div>
          </div>
          <div className="space-y-1.5">
            <Label htmlFor="litter-father">
              Father Patient ID <span className="text-muted-foreground font-normal">(optional)</span>
            </Label>
            <Input
              id="litter-father"
              placeholder="e.g. pat-0000-0000-0000-000000000001"
              value={fatherPatientId}
              onChange={(e) => setFatherPatientId(e.target.value)}
              data-testid="litter-father-input"
            />
          </div>
          <div className="space-y-1.5">
            <Label htmlFor="litter-external-father">
              External Father Name <span className="text-muted-foreground font-normal">(optional)</span>
            </Label>
            <Input
              id="litter-external-father"
              placeholder="e.g. Champion Rex"
              value={externalFatherName}
              onChange={(e) => setExternalFatherName(e.target.value)}
              data-testid="litter-external-father-input"
            />
          </div>
          <div className="space-y-1.5">
            <Label htmlFor="litter-notes">
              Notes <span className="text-muted-foreground font-normal">(optional)</span>
            </Label>
            <Textarea
              id="litter-notes"
              placeholder="Any notes about this litter..."
              value={notes}
              onChange={(e) => setNotes(e.target.value)}
              rows={3}
              data-testid="litter-notes-input"
            />
          </div>
          <DialogFooter>
            <Button
              type="submit"
              disabled={isSubmitting}
              data-testid="litter-submit-btn"
              className="w-full sm:w-auto"
            >
              {isSubmitting ? 'Saving...' : 'Register Litter'}
            </Button>
          </DialogFooter>
        </form>
      </DialogContent>
    </Dialog>
  )
}
