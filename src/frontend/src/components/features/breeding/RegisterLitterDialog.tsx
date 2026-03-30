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
  const [dateOfBirth, setDateOfBirth] = useState(todayDateString())
  const [fatherId, setFatherId] = useState('')
  const [breed, setBreed] = useState('')
  const [notes, setNotes] = useState('')
  const [isSubmitting, setIsSubmitting] = useState(false)

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault()
    setIsSubmitting(true)
    try {
      await onSubmit({
        motherId,
        fatherId: fatherId.trim() || null,
        dateOfBirth,
        breed: breed.trim() || null,
        notes: notes.trim() || null,
      })
      setDateOfBirth(todayDateString())
      setFatherId('')
      setBreed('')
      setNotes('')
    } finally {
      setIsSubmitting(false)
    }
  }

  const handleOpenChange = (newOpen: boolean) => {
    if (!newOpen) {
      setDateOfBirth(todayDateString())
      setFatherId('')
      setBreed('')
      setNotes('')
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
              value={dateOfBirth}
              onChange={(e) => setDateOfBirth(e.target.value)}
              data-testid="litter-dob-input"
              required
            />
          </div>
          <div className="space-y-1.5">
            <Label htmlFor="litter-father">
              Father ID <span className="text-muted-foreground font-normal">(optional)</span>
            </Label>
            <Input
              id="litter-father"
              placeholder="e.g. pat-0000-0000-0000-000000000001"
              value={fatherId}
              onChange={(e) => setFatherId(e.target.value)}
              data-testid="litter-father-input"
            />
          </div>
          <div className="space-y-1.5">
            <Label htmlFor="litter-breed">
              Breed <span className="text-muted-foreground font-normal">(optional)</span>
            </Label>
            <Input
              id="litter-breed"
              placeholder="e.g. Golden Labrador Mix"
              value={breed}
              onChange={(e) => setBreed(e.target.value)}
              data-testid="litter-breed-input"
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
