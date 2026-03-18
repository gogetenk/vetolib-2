'use client'

import { useCallback, useRef, useState } from 'react'
import { Upload, Download, X, CheckCircle, AlertCircle } from 'lucide-react'
import { Button } from '@/components/ui/button'
import {
  Dialog,
  DialogContent,
  DialogHeader,
  DialogTitle,
} from '@/components/ui/dialog'
import { useTranslations } from 'next-intl'
import { importPatientsCsv, downloadImportTemplate } from '@/lib/api/patients'
import type { ImportReportDto } from '@/lib/api/patients'
import { trackEvent, AnalyticsEvents } from '@/lib/analytics'

interface CsvImportDialogProps {
  open: boolean
  onOpenChange: (open: boolean) => void
  onImported?: () => void
}

interface PreviewRow {
  [key: string]: string
}

export function CsvImportDialog({ open, onOpenChange, onImported }: CsvImportDialogProps) {
  const t = useTranslations('patients.import')
  const [isDragOver, setIsDragOver] = useState(false)
  const [selectedFile, setSelectedFile] = useState<File | null>(null)
  const [preview, setPreview] = useState<PreviewRow[]>([])
  const [previewHeaders, setPreviewHeaders] = useState<string[]>([])
  const [isImporting, setIsImporting] = useState(false)
  const [report, setReport] = useState<ImportReportDto | null>(null)
  const [error, setError] = useState<string | null>(null)
  const fileInputRef = useRef<HTMLInputElement>(null)

  const parsePreview = useCallback((file: File) => {
    const reader = new FileReader()
    reader.onload = (e) => {
      const text = e.target?.result as string
      const lines = text.split('\n').filter(l => l.trim())
      if (lines.length === 0) return

      const headers = lines[0].split(',').map(h => h.trim().replace(/\r$/, ''))
      const rows = lines.slice(1, 6).map(line => {
        const cells = line.split(',').map(c => c.trim().replace(/\r$/, ''))
        const row: PreviewRow = {}
        headers.forEach((h, i) => {
          row[h] = cells[i] ?? ''
        })
        return row
      })

      setPreviewHeaders(headers)
      setPreview(rows)
    }
    reader.readAsText(file)
  }, [])

  const handleFileSelect = useCallback((file: File) => {
    if (!file.name.endsWith('.csv')) {
      setError(t('error_not_csv'))
      return
    }
    setSelectedFile(file)
    setReport(null)
    setError(null)
    parsePreview(file)
  }, [parsePreview, t])

  const handleDrop = useCallback((e: React.DragEvent) => {
    e.preventDefault()
    setIsDragOver(false)
    const file = e.dataTransfer.files[0]
    if (file) handleFileSelect(file)
  }, [handleFileSelect])

  const handleDragOver = useCallback((e: React.DragEvent) => {
    e.preventDefault()
    setIsDragOver(true)
  }, [])

  const handleDragLeave = useCallback(() => {
    setIsDragOver(false)
  }, [])

  const handleFileInputChange = useCallback((e: React.ChangeEvent<HTMLInputElement>) => {
    const file = e.target.files?.[0]
    if (file) handleFileSelect(file)
  }, [handleFileSelect])

  const handleImport = useCallback(async () => {
    if (!selectedFile) return
    setIsImporting(true)
    setError(null)
    try {
      const result = await importPatientsCsv(selectedFile)
      setReport(result)
      trackEvent(AnalyticsEvents.PATIENT_CSV_IMPORTED, {
        row_count: String(result.imported + result.skipped),
        success_count: String(result.imported),
        error_count: String(result.errors.length),
      })
      if (result.imported > 0) {
        onImported?.()
      }
    } catch {
      setError(t('error_import_failed'))
    } finally {
      setIsImporting(false)
    }
  }, [selectedFile, t, onImported])

  const handleDownloadTemplate = useCallback(async () => {
    try {
      await downloadImportTemplate()
    } catch {
      // silently fail
    }
  }, [])

  const handleClose = useCallback(() => {
    setSelectedFile(null)
    setPreview([])
    setPreviewHeaders([])
    setReport(null)
    setError(null)
    onOpenChange(false)
  }, [onOpenChange])

  return (
    <Dialog open={open} onOpenChange={handleClose}>
      <DialogContent className="max-w-2xl rounded-xl" data-testid="csv-import-dialog">
        <DialogHeader>
          <DialogTitle className="text-[18px] font-bold text-foreground" data-testid="csv-import-title">{t('title')}</DialogTitle>
        </DialogHeader>

        <div className="space-y-4">
          {/* Download template */}
          <div className="flex items-center justify-between rounded-xl border border-border/80 bg-muted p-3">
            <p className="text-[13px] text-muted-foreground">{t('template_hint')}</p>
            <Button
              variant="outline"
              size="sm"
              onClick={handleDownloadTemplate}
              data-testid="download-template-btn"
              className="rounded-xl border-border/80 text-[12px] font-semibold hover:bg-white"
            >
              <Download className="h-4 w-4 me-1" />
              {t('download_template')}
            </Button>
          </div>

          {/* Drag-and-drop zone */}
          {!report && (
            <div
              data-testid="csv-dropzone"
              className={`relative flex flex-col items-center justify-center rounded-xl border-2 border-dashed p-8 text-center transition-colors cursor-pointer ${
                isDragOver
                  ? 'border-primary bg-primary/5'
                  : 'border-border/60 hover:border-primary/50'
              }`}
              onDrop={handleDrop}
              onDragOver={handleDragOver}
              onDragLeave={handleDragLeave}
              onClick={() => fileInputRef.current?.click()}
            >
              <input
                ref={fileInputRef}
                type="file"
                accept=".csv"
                className="hidden"
                onChange={handleFileInputChange}
                data-testid="csv-file-input"
              />
              <Upload className="mb-2 h-8 w-8 text-muted-foreground" />
              {selectedFile ? (
                <div className="flex items-center gap-2">
                  <span className="font-medium text-sm" data-testid="selected-filename">
                    {selectedFile.name}
                  </span>
                  <button
                    type="button"
                    onClick={(e) => {
                      e.stopPropagation()
                      setSelectedFile(null)
                      setPreview([])
                      setPreviewHeaders([])
                    }}
                    data-testid="clear-file-btn"
                  >
                    <X className="h-4 w-4 text-muted-foreground hover:text-destructive" />
                  </button>
                </div>
              ) : (
                <>
                  <p className="text-sm font-medium">{t('drop_hint')}</p>
                  <p className="text-xs text-muted-foreground mt-1">{t('csv_only')}</p>
                </>
              )}
            </div>
          )}

          {/* Error */}
          {error && (
            <div
              data-testid="csv-import-error"
              className="flex items-center gap-2 rounded-xl border border-destructive/50 bg-destructive/10 p-3 text-sm text-destructive"
            >
              <AlertCircle className="h-4 w-4 shrink-0" />
              {error}
            </div>
          )}

          {/* Preview table */}
          {preview.length > 0 && !report && (
            <div data-testid="csv-preview">
              <p className="mb-2 text-sm font-medium">{t('preview_label')}</p>
              <div className="overflow-x-auto rounded-xl border border-border/80">
                <table className="w-full text-xs">
                  <thead>
                    <tr className="bg-muted">
                      {previewHeaders.map(h => (
                        <th key={h} className="px-3 py-2 text-left text-[11px] font-bold uppercase tracking-wider text-foreground">{h}</th>
                      ))}
                    </tr>
                  </thead>
                  <tbody>
                    {preview.map((row, i) => (
                      <tr key={i} className="border-t border-border/30 hover:bg-muted/50">
                        {previewHeaders.map(h => (
                          <td key={h} className="px-3 py-2 text-[12px] text-muted-foreground">{row[h]}</td>
                        ))}
                      </tr>
                    ))}
                  </tbody>
                </table>
              </div>
              <p className="mt-1 text-xs text-muted-foreground">{t('preview_note')}</p>
            </div>
          )}

          {/* Import report */}
          {report && (
            <div data-testid="import-report" className="rounded-xl border border-border/80 p-4 space-y-3">
              <div className="flex items-center gap-2">
                <CheckCircle className="h-5 w-5 text-green-500" />
                <span className="font-medium text-sm">{t('import_complete')}</span>
              </div>
              <div className="grid grid-cols-2 gap-4 text-sm">
                <div data-testid="report-imported" className="rounded-xl bg-green-50 p-3 text-center dark:bg-green-950/20">
                  <p className="text-2xl font-bold text-green-600">{report.imported}</p>
                  <p className="text-xs text-muted-foreground">{t('imported')}</p>
                </div>
                <div data-testid="report-skipped" className="rounded-xl bg-amber-50 p-3 text-center dark:bg-amber-950/20">
                  <p className="text-2xl font-bold text-amber-600">{report.skipped}</p>
                  <p className="text-xs text-muted-foreground">{t('skipped')}</p>
                </div>
              </div>
              {report.errors.length > 0 && (
                <div data-testid="report-errors" className="space-y-1">
                  <p className="text-xs font-medium text-muted-foreground">{t('errors_title')}</p>
                  <ul className="space-y-1 max-h-32 overflow-y-auto">
                    {report.errors.map((err, i) => (
                      <li key={i} className="text-xs text-destructive flex items-start gap-1">
                        <AlertCircle className="h-3 w-3 shrink-0 mt-0.5" />
                        {err}
                      </li>
                    ))}
                  </ul>
                </div>
              )}
            </div>
          )}

          {/* Actions */}
          <div className="flex justify-end gap-2">
            <Button variant="outline" onClick={handleClose} data-testid="csv-import-cancel" className="rounded-xl font-semibold border-border/80 hover:bg-muted">
              {report ? t('close') : t('cancel')}
            </Button>
            {!report && (
              <Button
                onClick={handleImport}
                disabled={!selectedFile || isImporting}
                data-testid="csv-import-submit"
                className="bg-primary hover:bg-primary/90 text-primary-foreground font-semibold rounded-xl shadow-sm"
              >
                {isImporting ? (
                  <>
                    <span className="h-4 w-4 me-2 animate-spin rounded-full border-2 border-current border-t-transparent" />
                    {t('importing')}
                  </>
                ) : (
                  t('import')
                )}
              </Button>
            )}
          </div>
        </div>
      </DialogContent>
    </Dialog>
  )
}
