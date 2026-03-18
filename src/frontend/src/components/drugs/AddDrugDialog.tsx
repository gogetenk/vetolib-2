'use client'

import { useState } from 'react'
import { useTranslations } from 'next-intl'
import {
  Dialog,
  DialogContent,
  DialogHeader,
  DialogTitle,
  DialogDescription,
  DialogFooter,
} from '@/components/ui/dialog'
import { Button } from '@/components/ui/button'
import { Input } from '@/components/ui/input'
import { Label } from '@/components/ui/label'
import {
  Select,
  SelectContent,
  SelectItem,
  SelectTrigger,
  SelectValue,
} from '@/components/ui/select'
import { Switch } from '@/components/ui/switch'
import type { CreateDrugRequest, DrugCategory } from '@/lib/api/drugs'

interface AddDrugDialogProps {
  open: boolean
  onOpenChange: (open: boolean) => void
  onSubmit: (data: CreateDrugRequest) => Promise<void>
}

const CATEGORIES: DrugCategory[] = [
  'Antibiotic',
  'Antiparasitic',
  'AntiInflammatory',
  'Analgesic',
  'Vaccine',
  'Antifungal',
  'Cardiac',
  'Dermatological',
  'Ophthalmic',
  'Hormonal',
  'Other',
]

export function AddDrugDialog({ open, onOpenChange, onSubmit }: AddDrugDialogProps) {
  const t = useTranslations('drugs')
  const [isSubmitting, setIsSubmitting] = useState(false)
  const [innName, setInnName] = useState('')
  const [displayName, setDisplayName] = useState('')
  const [category, setCategory] = useState<DrugCategory>('Antibiotic')
  const [commonDosage, setCommonDosage] = useState('')
  const [requiresPrescription, setRequiresPrescription] = useState(true)

  function resetForm() {
    setInnName('')
    setDisplayName('')
    setCategory('Antibiotic')
    setCommonDosage('')
    setRequiresPrescription(true)
  }

  async function handleSubmit(e: React.FormEvent) {
    e.preventDefault()
    if (!innName.trim() || !displayName.trim() || !commonDosage.trim()) return

    setIsSubmitting(true)
    try {
      await onSubmit({
        innName: innName.trim(),
        displayName: displayName.trim(),
        category,
        commonDosage: commonDosage.trim(),
        requiresPrescription,
        contraindicatedSpecies: [],
        dosageGuidelines: [],
        interactionSeverity: null,
      })
      resetForm()
    } finally {
      setIsSubmitting(false)
    }
  }

  return (
    <Dialog
      open={open}
      onOpenChange={(isOpen) => {
        if (!isOpen) resetForm()
        onOpenChange(isOpen)
      }}
    >
      <DialogContent className="sm:max-w-md" data-testid="add-drug-dialog">
        <DialogHeader>
          <DialogTitle data-testid="add-drug-dialog-title">{t('form.add_title')}</DialogTitle>
          <DialogDescription>{t('form.add_description')}</DialogDescription>
        </DialogHeader>

        <form onSubmit={handleSubmit} className="space-y-4" data-testid="add-drug-form">
          <div className="space-y-2">
            <Label htmlFor="drug-inn-name">{t('form.inn_name')}</Label>
            <Input
              id="drug-inn-name"
              data-testid="input-drug-inn-name"
              placeholder={t('form.inn_name_placeholder')}
              value={innName}
              onChange={(e) => setInnName(e.target.value)}
              required
              className="rounded-xl"
            />
          </div>

          <div className="space-y-2">
            <Label htmlFor="drug-display-name">{t('form.display_name')}</Label>
            <Input
              id="drug-display-name"
              data-testid="input-drug-display-name"
              placeholder={t('form.display_name_placeholder')}
              value={displayName}
              onChange={(e) => setDisplayName(e.target.value)}
              required
              className="rounded-xl"
            />
          </div>

          <div className="space-y-2">
            <Label>{t('form.category')}</Label>
            <Select value={category} onValueChange={(v) => { if (v) setCategory(v as DrugCategory) }}>
              <SelectTrigger className="rounded-xl" data-testid="select-drug-category">
                <SelectValue />
              </SelectTrigger>
              <SelectContent>
                {CATEGORIES.map(cat => (
                  <SelectItem
                    key={cat}
                    value={cat}
                    data-testid={`select-drug-category-${cat.toLowerCase()}`}
                  >
                    {t(`categories.${cat}`)}
                  </SelectItem>
                ))}
              </SelectContent>
            </Select>
          </div>

          <div className="space-y-2">
            <Label htmlFor="drug-common-dosage">{t('form.common_dosage')}</Label>
            <Input
              id="drug-common-dosage"
              data-testid="input-drug-common-dosage"
              placeholder={t('form.common_dosage_placeholder')}
              value={commonDosage}
              onChange={(e) => setCommonDosage(e.target.value)}
              required
              className="rounded-xl"
            />
          </div>

          <div className="flex items-center justify-between">
            <Label htmlFor="drug-requires-prescription">{t('form.requires_prescription')}</Label>
            <Switch
              id="drug-requires-prescription"
              data-testid="switch-drug-requires-prescription"
              checked={requiresPrescription}
              onCheckedChange={setRequiresPrescription}
            />
          </div>

          <DialogFooter>
            <Button
              type="button"
              variant="outline"
              onClick={() => onOpenChange(false)}
              data-testid="btn-cancel-add-drug"
              className="rounded-xl"
            >
              {t('form.cancel')}
            </Button>
            <Button
              type="submit"
              disabled={isSubmitting || !innName.trim() || !displayName.trim() || !commonDosage.trim()}
              data-testid="btn-submit-add-drug"
              className="bg-emerald-600 hover:bg-emerald-700 text-white font-semibold rounded-xl"
            >
              {isSubmitting ? t('form.submitting') : t('form.submit')}
            </Button>
          </DialogFooter>
        </form>
      </DialogContent>
    </Dialog>
  )
}
