'use client'

import { useState } from 'react'
import { useTranslations } from 'next-intl'
import {
  MoreHorizontal,
  CheckCircle,
  Archive,
  AlertTriangle,
  CalendarPlus,
  ArrowRightLeft,
  RotateCcw,
  UserCog,
} from 'lucide-react'
import { Button } from '@/components/ui/button'
import {
  Select,
  SelectContent,
  SelectItem,
  SelectTrigger,
  SelectValue,
} from '@/components/ui/select'
import type { ConversationWithSuggestionsDto, ConversationStatus } from '@/lib/api/messaging-types'

interface ConversationActionsProps {
  conversation: ConversationWithSuggestionsDto
  role: string | null
  onStatusChange: (status: ConversationStatus) => Promise<void>
  onTransfer: (toRole: string, toUserId?: string | null) => Promise<void>
  onMarkSpam: () => Promise<void>
  onConvertToAppointment: () => void
}

const ROLES_FOR_TRANSFER = ['VET', 'RECEPTIONIST', 'ADMIN']

export function ConversationActions({
  conversation,
  role,
  onStatusChange,
  onTransfer,
  onMarkSpam,
  onConvertToAppointment,
}: ConversationActionsProps) {
  const t = useTranslations('messaging')
  const [open, setOpen] = useState(false)
  const [transferOpen, setTransferOpen] = useState(false)
  const [selectedRole, setSelectedRole] = useState('')
  const [isTransferring, setIsTransferring] = useState(false)

  const isResolved = conversation.status === 'Resolved'
  const isClosed = conversation.status === 'Closed'
  const isAdmin = role === 'ADMIN'

  const handleTransferConfirm = async () => {
    if (!selectedRole) return
    setIsTransferring(true)
    try {
      await onTransfer(selectedRole, null)
      setTransferOpen(false)
      setSelectedRole('')
    } finally {
      setIsTransferring(false)
    }
  }

  return (
    <>
      <div className="relative flex-shrink-0" data-testid="conversation-actions">
        <Button
          variant="ghost"
          size="sm"
          data-testid="conversation-actions-trigger"
          onClick={() => setOpen((v) => !v)}
          aria-label={t('actions')}
          aria-expanded={open}
          aria-haspopup="menu"
        >
          <MoreHorizontal className="h-4 w-4" />
        </Button>

        {open && (
          <>
            {/* Backdrop */}
            <div
              className="fixed inset-0 z-10"
              onClick={() => setOpen(false)}
              aria-hidden
            />
            {/* Dropdown */}
            <div
              className="absolute end-0 top-full mt-1 z-20 min-w-[200px] rounded-xl border border-border/80 bg-card shadow-xl py-1.5"
              role="menu"
              data-testid="conversation-actions-menu"
            >
              {/* Transfer */}
              <button
                type="button"
                role="menuitem"
                data-testid="transfer-btn"
                className="flex w-full items-center gap-2 px-3 py-2 text-[13px] font-medium text-foreground hover:bg-muted transition-colors"
                onClick={() => { setTransferOpen(true); setOpen(false) }}
              >
                <ArrowRightLeft className="h-4 w-4 text-primary" />
                {t('action_transfer')}
              </button>

              {/* Convert to appointment */}
              <button
                type="button"
                role="menuitem"
                data-testid="convert-appointment-btn"
                className="flex w-full items-center gap-2 px-3 py-2 text-[13px] font-medium text-foreground hover:bg-muted transition-colors"
                onClick={() => { onConvertToAppointment(); setOpen(false) }}
              >
                <CalendarPlus className="h-4 w-4 text-primary" />
                {t('action_convert_appointment')}
              </button>

              {/* Resolve */}
              {!isResolved && !isClosed && (
                <button
                  type="button"
                  role="menuitem"
                  data-testid="resolve-btn"
                  className="flex w-full items-center gap-2 px-3 py-2 text-[13px] font-medium text-foreground hover:bg-muted transition-colors"
                  onClick={() => { void onStatusChange('Resolved'); setOpen(false) }}
                >
                  <CheckCircle className="h-4 w-4 text-green-500" />
                  {t('action_resolve')}
                </button>
              )}

              {/* Close */}
              {!isClosed && (
                <button
                  type="button"
                  role="menuitem"
                  data-testid="close-btn"
                  className="flex w-full items-center gap-2 px-3 py-2 text-[13px] font-medium text-foreground hover:bg-muted transition-colors"
                  onClick={() => { void onStatusChange('Closed'); setOpen(false) }}
                >
                  <Archive className="h-4 w-4 text-muted-foreground" />
                  {t('action_close')}
                </button>
              )}

              {/* Reopen */}
              {(isResolved || isClosed) && (
                <button
                  type="button"
                  role="menuitem"
                  data-testid="reopen-btn"
                  className="flex w-full items-center gap-2 px-3 py-2 text-[13px] font-medium text-foreground hover:bg-muted transition-colors"
                  onClick={() => { void onStatusChange('Open'); setOpen(false) }}
                >
                  <RotateCcw className="h-4 w-4 text-primary" />
                  {t('action_reopen')}
                </button>
              )}

              {/* Mark as spam */}
              <button
                type="button"
                role="menuitem"
                data-testid="mark-spam-btn"
                className="flex w-full items-center gap-2 px-3 py-2 text-[13px] font-medium text-destructive hover:bg-red-50 transition-colors"
                onClick={() => { void onMarkSpam(); setOpen(false) }}
              >
                <AlertTriangle className="h-4 w-4" />
                {t('action_mark_spam')}
              </button>

              {/* Reassign — Admin only */}
              {isAdmin && (
                <button
                  type="button"
                  role="menuitem"
                  data-testid="reassign-btn"
                  className="flex w-full items-center gap-2 px-3 py-2 text-[13px] font-medium text-foreground hover:bg-muted transition-colors"
                  onClick={() => { setTransferOpen(true); setOpen(false) }}
                >
                  <UserCog className="h-4 w-4 text-muted-foreground" />
                  {t('action_reassign')}
                </button>
              )}
            </div>
          </>
        )}
      </div>

      {/* Transfer dialog — simple inline modal */}
      {transferOpen && (
        <div
          className="fixed inset-0 z-50 flex items-center justify-center bg-black/40"
          data-testid="transfer-dialog-overlay"
          onClick={() => setTransferOpen(false)}
        >
          <div
            className="bg-card rounded-xl shadow-xl p-6 w-full max-w-sm mx-4"
            data-testid="transfer-dialog"
            onClick={(e) => e.stopPropagation()}
            role="dialog"
            aria-modal="true"
            aria-labelledby="transfer-dialog-title"
          >
            <h2 id="transfer-dialog-title" className="text-[15px] font-bold text-foreground mb-4">
              {t('transfer_dialog_title')}
            </h2>
            <label className="text-[13px] font-semibold text-foreground block mb-2">
              {t('transfer_to_role')}
            </label>
            <div className="mb-4" data-testid="transfer-role-select">
              <Select
                value={selectedRole}
                onValueChange={(v) => setSelectedRole(v as string)}
              >
                <SelectTrigger className="w-full rounded-xl border-border/80 text-[13px]">
                  <SelectValue>{selectedRole || t('select_role')}</SelectValue>
                </SelectTrigger>
                <SelectContent>
                  {ROLES_FOR_TRANSFER.map((r) => (
                    <SelectItem
                      key={r}
                      value={r}
                      data-testid={`transfer-role-option-${r.toLowerCase()}`}
                    >
                      {r}
                    </SelectItem>
                  ))}
                </SelectContent>
              </Select>
            </div>
            <div className="flex justify-end gap-2">
              <Button
                type="button"
                variant="outline"
                size="sm"
                onClick={() => setTransferOpen(false)}
                data-testid="transfer-cancel-btn"
                disabled={isTransferring}
                className="rounded-xl font-semibold border-border/80 hover:bg-muted"
              >
                {t('cancel')}
              </Button>
              <Button
                type="button"
                size="sm"
                onClick={handleTransferConfirm}
                data-testid="transfer-confirm-btn"
                disabled={!selectedRole || isTransferring}
                className="bg-primary hover:bg-primary/90 text-primary-foreground font-semibold rounded-xl shadow-sm"
              >
                {isTransferring ? t('transferring') : t('action_transfer')}
              </Button>
            </div>
          </div>
        </div>
      )}
    </>
  )
}
