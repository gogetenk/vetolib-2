'use client'

import { useState } from 'react'
import { useTranslations } from 'next-intl'
import { ChevronDown, ChevronUp, Sparkles } from 'lucide-react'
import { cn } from '@/lib/utils'

interface ConversationSummaryProps {
  summary: string | null
  isLoading: boolean
}

export function ConversationSummary({ summary, isLoading }: ConversationSummaryProps) {
  const t = useTranslations('messaging')
  const [isOpen, setIsOpen] = useState(true)

  return (
    <div
      className="border border-[#303ef5]/20 rounded-xl bg-[#eef2fd] mb-4"
      data-testid="ai-summary-card"
    >
      <button
        type="button"
        className="w-full flex items-center justify-between px-4 py-3 text-left"
        onClick={() => setIsOpen((p) => !p)}
        data-testid="ai-summary-toggle"
        aria-expanded={isOpen}
      >
        <div className="flex items-center gap-2">
          <Sparkles className="h-4 w-4 text-[#303ef5]" aria-hidden />
          <span className="text-[13px] font-bold text-[#303ef5]">{t('ai_summary')}</span>
        </div>
        {isOpen ? (
          <ChevronUp className="h-4 w-4 text-[#303ef5]" />
        ) : (
          <ChevronDown className="h-4 w-4 text-[#303ef5]" />
        )}
      </button>

      <div
        className={cn('overflow-hidden transition-all duration-200', isOpen ? 'max-h-96' : 'max-h-0')}
        aria-hidden={!isOpen}
      >
        <div className="px-4 pb-4 text-[13px] text-[#061e44]">
          {isLoading ? (
            <p className="italic text-[#303ef5]/70" data-testid="ai-summary-loading">
              {t('loading_summary')}
            </p>
          ) : summary ? (
            <p data-testid="ai-summary-text">{summary}</p>
          ) : (
            <p className="italic text-[#303ef5]/70" data-testid="ai-summary-unavailable">
              {t('summary_unavailable')}
            </p>
          )}
        </div>
      </div>
    </div>
  )
}
