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
import type { CreateHeatCycleRequest } from '@/lib/api/breeding'

interface RecordHeatCycleDialogProps {
  open: boolean
  onOpenChange: (open: boolean) => void
  patientId: string
  onSubmit: (data: CreateHeatCycleRequest) => Promise<void>
}

function todayDateString(): string {
  const now = new Date()
  return `${now.getFullYear()}-${String(now.getMonth() + 1).padStart(2, '0')}-${String(now.getDate()).padStart(2, '0')}`
}

export function RecordHeatCycleDialog({
  open,
  onOpenChange,
  patientId,
  onSubmit,
}: RecordHeatCycleDialogProps) {
  const [startDate, setStartDate] = useState(todayDateString())
  const [endDate, setEndDate] = useState('')
  const [notes, setNotes] = useState('')
  const [isSubmitting, setIsSubmitting] = useState(false)

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault()
    if (!startDate) return
    setIsSubmitting(true)
    try {
      await onSubmit({
        patientId,
        startDate,
        endDate: endDate || null,
        notes: notes.trim() || null,
      })
      setStartDate(todayDateString())
      setEndDate('')
      setNotes('')
    } finally {
      setIsSubmitting(false)
    }
  }

  const handleOpenChange = (newOpen: boolean) => {
    if (!newOpen) {
      setStartDate(todayDateString())
      setEndDate('')
      setNotes('')
    }
    onOpenChange(newOpen)
  }

  return (
    <Dialog open={open} onOpenChange={handleOpenChange}>
      <DialogContent className="sm:max-w-md">
        <DialogHeader>
          <DialogTitle>Record Heat Cycle</DialogTitle>
        </DialogHeader>
        <form onSubmit={handleSubmit} className="space-y-4">
          <div className="space-y-1.5">
            <Label htmlFor="heat-start-date">Start Date</Label>
            <Input
              id="heat-start-date"
              type="date"
              value={startDate}
              onChange={(e) => setStartDate(e.target.value)}
              data-testid="heat-start-date-input"
              required
            />
          </div>
          <div className="space-y-1.5">
            <Label htmlFor="heat-end-date">
              End Date <span className="text-muted-foreground font-normal">(optional)</span>
            </Label>
            <Input
              id="heat-end-date"
              type="date"
              value={endDate}
              onChange={(e) => setEndDate(e.target.value)}
              data-testid="heat-end-date-input"
            />
          </div>
          <div className="space-y-1.5">
            <Label htmlFor="heat-notes">
              Notes <span className="text-muted-foreground font-normal">(optional)</span>
            </Label>
            <Textarea
              id="heat-notes"
              placeholder="Any observations..."
              value={notes}
              onChange={(e) => setNotes(e.target.value)}
              rows={3}
              data-testid="heat-notes-input"
            />
          </div>
          <DialogFooter>
            <Button
              type="submit"
              disabled={isSubmitting}
              data-testid="heat-cycle-submit-btn"
              className="w-full sm:w-auto"
            >
              {isSubmitting ? 'Saving...' : 'Record Heat Cycle'}
            </Button>
          </DialogFooter>
        </form>
      </DialogContent>
    </Dialog>
  )
}
