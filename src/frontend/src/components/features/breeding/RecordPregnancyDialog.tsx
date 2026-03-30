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
import type { CreatePregnancyRequest } from '@/lib/api/breeding'

interface RecordPregnancyDialogProps {
  open: boolean
  onOpenChange: (open: boolean) => void
  patientId: string
  onSubmit: (data: CreatePregnancyRequest) => Promise<void>
}

function todayDateString(): string {
  const now = new Date()
  return `${now.getFullYear()}-${String(now.getMonth() + 1).padStart(2, '0')}-${String(now.getDate()).padStart(2, '0')}`
}

export function RecordPregnancyDialog({
  open,
  onOpenChange,
  patientId,
  onSubmit,
}: RecordPregnancyDialogProps) {
  const [matingDate, setMatingDate] = useState(todayDateString())
  const [expectedDueDate, setExpectedDueDate] = useState('')
  const [notes, setNotes] = useState('')
  const [isSubmitting, setIsSubmitting] = useState(false)

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault()
    if (!matingDate || !expectedDueDate) return
    setIsSubmitting(true)
    try {
      await onSubmit({
        patientId,
        matingDate,
        expectedDueDate,
        notes: notes.trim() || null,
      })
      setMatingDate(todayDateString())
      setExpectedDueDate('')
      setNotes('')
    } finally {
      setIsSubmitting(false)
    }
  }

  const handleOpenChange = (newOpen: boolean) => {
    if (!newOpen) {
      setMatingDate(todayDateString())
      setExpectedDueDate('')
      setNotes('')
    }
    onOpenChange(newOpen)
  }

  return (
    <Dialog open={open} onOpenChange={handleOpenChange}>
      <DialogContent className="sm:max-w-md">
        <DialogHeader>
          <DialogTitle>Record Pregnancy</DialogTitle>
        </DialogHeader>
        <form onSubmit={handleSubmit} className="space-y-4">
          <div className="space-y-1.5">
            <Label htmlFor="pregnancy-mating-date">Mating Date</Label>
            <Input
              id="pregnancy-mating-date"
              type="date"
              value={matingDate}
              onChange={(e) => setMatingDate(e.target.value)}
              data-testid="pregnancy-mating-date-input"
              required
            />
          </div>
          <div className="space-y-1.5">
            <Label htmlFor="pregnancy-due-date">Expected Due Date</Label>
            <Input
              id="pregnancy-due-date"
              type="date"
              value={expectedDueDate}
              onChange={(e) => setExpectedDueDate(e.target.value)}
              data-testid="pregnancy-due-date-input"
              required
            />
          </div>
          <div className="space-y-1.5">
            <Label htmlFor="pregnancy-notes">
              Notes <span className="text-muted-foreground font-normal">(optional)</span>
            </Label>
            <Textarea
              id="pregnancy-notes"
              placeholder="Any notes about this pregnancy..."
              value={notes}
              onChange={(e) => setNotes(e.target.value)}
              rows={3}
              data-testid="pregnancy-notes-input"
            />
          </div>
          <DialogFooter>
            <Button
              type="submit"
              disabled={isSubmitting || !expectedDueDate}
              data-testid="pregnancy-submit-btn"
              className="w-full sm:w-auto"
            >
              {isSubmitting ? 'Saving...' : 'Record Pregnancy'}
            </Button>
          </DialogFooter>
        </form>
      </DialogContent>
    </Dialog>
  )
}
