'use client'

import { useState, useEffect } from 'react'
import { useRouter } from 'next/navigation'
import { useTranslations } from 'next-intl'
import { Loader2 } from 'lucide-react'
import { Button } from '@/components/ui/button'
import { Card, CardContent, CardHeader, CardTitle } from '@/components/ui/card'
import { Input } from '@/components/ui/input'
import { Label } from '@/components/ui/label'
import { Textarea } from '@/components/ui/textarea'
import { LtrText } from '@/components/ui/ltr-text'
import { formatAED } from '@/lib/utils'
import { createInvoice } from '@/lib/api/billing'
import type { CreateInvoiceLineItem } from '@/lib/api/billing'
import { getPatients } from '@/lib/api/patients'
import type { PatientDto } from '@/lib/api/patients'
import { trackEvent, AnalyticsEvents, bucketAed } from '@/lib/analytics'
import { toast } from 'sonner'

interface LineItem {
  description: string
  quantity: number
  unitPrice: number
}

interface PatientOption {
  id: string
  name: string
  ownerName: string
  ownerPhone: string
}

export function InvoiceForm() {
  const router = useRouter()
  const t = useTranslations('billing.form')
  const [patients, setPatients] = useState<PatientOption[]>([])
  const [selectedPatientId, setSelectedPatientId] = useState('')
  const [notes, setNotes] = useState('')
  const [items, setItems] = useState<LineItem[]>([
    { description: '', quantity: 1, unitPrice: 0 },
  ])
  const [submitting, setSubmitting] = useState(false)
  const [error, setError] = useState<string | null>(null)
  const [patientSearch, setPatientSearch] = useState('')

  useEffect(() => {
    getPatients({ pageSize: 100 })
      .then((result) => {
        setPatients(
          result.items.map((p: PatientDto) => ({
            id: p.id,
            name: p.name,
            ownerName: p.ownerName,
            ownerPhone: p.ownerPhone,
          }))
        )
      })
      .catch(() => {
        toast.error('Failed to load patients')
      })
  }, [])

  const selectedPatient = patients.find((p) => p.id === selectedPatientId)

  const filteredPatients = patients.filter((p) =>
    p.name.toLowerCase().includes(patientSearch.toLowerCase()) ||
    p.ownerName.toLowerCase().includes(patientSearch.toLowerCase())
  )

  const subtotal = items.reduce((sum, item) => sum + item.quantity * item.unitPrice, 0)
  const vatAmount = Math.round(subtotal * 0.05 * 100) / 100
  const total = Math.round((subtotal + vatAmount) * 100) / 100

  function addItem() {
    setItems((prev) => [...prev, { description: '', quantity: 1, unitPrice: 0 }])
  }

  function removeItem(index: number) {
    setItems((prev) => prev.filter((_, i) => i !== index))
  }

  function updateItem(index: number, field: keyof LineItem, value: string | number) {
    setItems((prev) =>
      prev.map((item, i) =>
        i === index ? { ...item, [field]: field === 'description' ? value : Number(value) } : item
      )
    )
  }

  async function handleSubmit(e: React.FormEvent) {
    e.preventDefault()
    if (!selectedPatient) {
      setError(t('errors.select_patient'))
      return
    }
    if (items.length === 0 || items.some((item) => !item.description)) {
      setError(t('errors.add_item_description'))
      return
    }

    setSubmitting(true)
    setError(null)

    try {
      const invoice = await createInvoice({
        patientId: selectedPatient.id,
        patientName: selectedPatient.name,
        ownerName: selectedPatient.ownerName,
        ownerPhone: selectedPatient.ownerPhone,
        items: items as CreateInvoiceLineItem[],
        notes: notes || null,
      })
      trackEvent(AnalyticsEvents.INVOICE_CREATED, {
        item_count: String(items.length),
        total_aed: bucketAed(total),
      })
      router.push(`/billing/${invoice.id}`)
    } catch {
      setError(t('errors.create_failed'))
      setSubmitting(false)
    }
  }

  return (
    <form onSubmit={handleSubmit} data-testid="invoice-form">
      <div className="space-y-6">
        {/* Patient Selection */}
        <Card className="border-border/80 shadow-sm">
          <CardHeader>
            <CardTitle className="text-[15px] font-bold text-foreground">{t('patient_section')}</CardTitle>
          </CardHeader>
          <CardContent className="space-y-3">
            <div>
              <Label htmlFor="patient-search" className="text-[13px] font-semibold text-muted-foreground">{t('search_patient')}</Label>
              <Input
                id="patient-search"
                data-testid="patient-search-input"
                placeholder={t('search_patient_placeholder')}
                className="rounded-xl border-border/80 text-[13px] focus:ring-2 focus:ring-primary/20 focus:border-primary/50 mt-1.5"
                value={patientSearch}
                onChange={(e) => setPatientSearch(e.target.value)}
              />
              {patientSearch && !selectedPatient && (
                <div
                  className="mt-1 rounded-xl border border-border/80 bg-white shadow-lg overflow-hidden"
                  data-testid="patient-dropdown"
                >
                  {filteredPatients.map((p) => (
                    <button
                      key={p.id}
                      type="button"
                      data-testid={`patient-option-${p.id}`}
                      className="w-full px-4 py-2.5 text-start text-[13px] hover:bg-muted transition-colors duration-150 border-b border-border/20 last:border-b-0"
                      onClick={() => {
                        setSelectedPatientId(p.id)
                        setPatientSearch(p.name)
                      }}
                    >
                      <span className="font-semibold text-foreground">{p.name}</span>{' '}
                      <span className="text-muted-foreground">— {p.ownerName}</span>
                    </button>
                  ))}
                  {filteredPatients.length === 0 && (
                    <p className="px-4 py-2.5 text-[13px] text-muted-foreground">{t('no_patients_found')}</p>
                  )}
                </div>
              )}
            </div>
            {selectedPatient && (
              <div className="rounded-xl bg-muted border border-border/50 px-4 py-3 text-[13px]" data-testid="selected-patient">
                <p className="font-semibold text-foreground">{selectedPatient.name}</p>
                <p className="text-muted-foreground mt-0.5">
                  {t('owner_label')}: {selectedPatient.ownerName} — <LtrText>{selectedPatient.ownerPhone}</LtrText>
                </p>
                <button
                  type="button"
                  data-testid="clear-patient-btn"
                  className="text-[12px] text-primary hover:text-primary/90 font-semibold mt-1.5 transition-colors"
                  onClick={() => {
                    setSelectedPatientId('')
                    setPatientSearch('')
                  }}
                >
                  {t('change_patient')}
                </button>
              </div>
            )}
          </CardContent>
        </Card>

        {/* Line Items */}
        <Card className="border-border/80 shadow-sm">
          <CardHeader>
            <div className="flex items-center justify-between">
              <CardTitle className="text-[15px] font-bold text-foreground">{t('items_section')}</CardTitle>
              <Button
                type="button"
                variant="outline"
                size="sm"
                data-testid="add-item-btn"
                onClick={addItem}
                className="rounded-xl font-semibold border-border/80 hover:bg-muted text-[13px]"
              >
                {t('add_item')}
              </Button>
            </div>
          </CardHeader>
          <CardContent className="space-y-3">
            {items.map((item, index) => (
              <div key={index} className="grid grid-cols-12 gap-2 items-end" data-testid={`line-item-${index}`}>
                <div className="col-span-5">
                  {index === 0 && <Label className="text-[13px] font-semibold text-muted-foreground">{t('description')}</Label>}
                  <Input
                    data-testid={`item-description-${index}`}
                    placeholder={t('description_placeholder')}
                    aria-label={t('item_description_aria', { index: index + 1 })}
                    className="rounded-xl border-border/80 text-[13px] focus:ring-2 focus:ring-primary/20 focus:border-primary/50"
                    value={item.description}
                    onChange={(e) => updateItem(index, 'description', e.target.value)}
                    required
                  />
                </div>
                <div className="col-span-2">
                  {index === 0 && <Label className="text-[13px] font-semibold text-muted-foreground">{t('qty')}</Label>}
                  <Input
                    data-testid={`item-quantity-${index}`}
                    type="number"
                    min={1}
                    aria-label={t('item_quantity_aria', { index: index + 1 })}
                    className="rounded-xl border-border/80 text-[13px] focus:ring-2 focus:ring-primary/20 focus:border-primary/50"
                    value={item.quantity}
                    onChange={(e) => updateItem(index, 'quantity', e.target.value)}
                    required
                  />
                </div>
                <div className="col-span-3">
                  {index === 0 && <Label className="text-[13px] font-semibold text-muted-foreground">{t('unit_price')}</Label>}
                  <Input
                    data-testid={`item-unit-price-${index}`}
                    type="number"
                    min={0}
                    step={0.01}
                    aria-label={t('item_unit_price_aria', { index: index + 1 })}
                    className="rounded-xl border-border/80 text-[13px] focus:ring-2 focus:ring-primary/20 focus:border-primary/50"
                    value={item.unitPrice}
                    onChange={(e) => updateItem(index, 'unitPrice', e.target.value)}
                    required
                  />
                </div>
                <div className="col-span-1 text-end text-[13px] font-semibold text-foreground" data-testid={`item-subtotal-${index}`}>
                  <LtrText>{formatAED(item.quantity * item.unitPrice)}</LtrText>
                </div>
                <div className="col-span-1 flex justify-end">
                  {items.length > 1 && (
                    <button
                      type="button"
                      data-testid={`remove-item-${index}`}
                      aria-label={t('item_remove_aria', { index: index + 1 })}
                      className="text-destructive hover:text-destructive/80 text-lg leading-none transition-colors w-8 h-8 rounded-lg flex items-center justify-center hover:bg-destructive/10"
                      onClick={() => removeItem(index)}
                    >
                      <span aria-hidden="true">×</span>
                    </button>
                  )}
                </div>
              </div>
            ))}

            {/* Totals */}
            <div className="mt-4 rounded-xl bg-muted border border-border/50 px-4 py-3 space-y-1.5 text-[13px]" data-testid="invoice-totals">
              <div className="flex justify-between">
                <span className="text-muted-foreground">{t('subtotal_excl_vat')}</span>
                <LtrText data-testid="form-subtotal" className="font-semibold text-foreground">{formatAED(subtotal)}</LtrText>
              </div>
              <div className="flex justify-between">
                <span className="text-muted-foreground">{t('vat_percent')}</span>
                <LtrText data-testid="form-vat" className="text-muted-foreground">{formatAED(vatAmount)}</LtrText>
              </div>
              <div className="flex justify-between font-bold text-[15px] text-foreground border-t border-border/50 pt-1.5">
                <span>{t('total_aed')}</span>
                <LtrText data-testid="form-total">{formatAED(total)}</LtrText>
              </div>
            </div>
          </CardContent>
        </Card>

        {/* Notes */}
        <Card className="border-border/80 shadow-sm">
          <CardHeader>
            <CardTitle className="text-[15px] font-bold text-foreground">{t('notes_section')}</CardTitle>
          </CardHeader>
          <CardContent>
            <Textarea
              data-testid="invoice-notes"
              className="resize-y rounded-xl border-border/80 text-[13px] focus:ring-2 focus:ring-primary/20 focus:border-primary/50 min-h-[100px]"
              placeholder={t('notes_placeholder')}
              value={notes}
              onChange={(e) => setNotes(e.target.value)}
            />
          </CardContent>
        </Card>

        {error && (
          <p className="text-[13px] text-destructive font-medium animate-slide-up-fade" data-testid="form-error">
            {error}
          </p>
        )}

        <div className="sticky bottom-0 bg-background/95 backdrop-blur-sm py-4 border-t border-border/50 flex gap-3 justify-end">
          <Button
            type="button"
            variant="outline"
            data-testid="cancel-form-btn"
            onClick={() => router.push('/billing')}
            className="rounded-xl h-10 px-5 font-semibold border-border/80 hover:bg-muted"
          >
            {t('cancel')}
          </Button>
          <Button
            type="submit"
            data-testid="submit-invoice-btn"
            disabled={submitting}
            className="bg-primary hover:bg-primary/90 text-primary-foreground font-semibold rounded-xl h-10 px-6 shadow-sm"
          >
            {submitting && <Loader2 className="h-4 w-4 me-1.5 animate-spin" aria-hidden />}
            {submitting ? t('creating') : t('create_invoice')}
          </Button>
        </div>
      </div>
    </form>
  )
}
