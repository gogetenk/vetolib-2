'use client'

import { useTranslations } from 'next-intl'
import { Sparkles, AlertCircle } from 'lucide-react'
import type { AiSuggestion } from '@/lib/api/messaging-types'

interface AiSuggestionsPanelProps {
  suggestions: AiSuggestion[]
  onSelect: (text: string) => void
}

export function AiSuggestionsPanel({ suggestions, onSelect }: AiSuggestionsPanelProps) {
  const t = useTranslations('messaging')

  return (
    <div
      className="p-4"
      data-testid="ai-suggestions-panel"
    >
      <div className="flex items-center gap-2 mb-3">
        <Sparkles className="h-4 w-4 text-[#6366f1]" aria-hidden />
        <h3 className="text-[13px] font-bold text-[#061e44]">
          {t('ai_suggestions_title')}
        </h3>
      </div>

      <div className="space-y-2" data-testid="ai-suggestions-list">
        {suggestions.map((suggestion, index) => (
          <button
            key={index}
            type="button"
            data-testid={`ai-suggestion-${index}`}
            className="w-full text-left rounded-xl border border-[#6366f1]/20 bg-[#6366f1]/5 px-3 py-2.5 text-[13px] text-[#061e44] hover:bg-[#6366f1]/10 hover:border-[#6366f1]/30 transition-colors focus:outline-none focus:ring-2 focus:ring-[#6366f1]/30"
            dir={suggestion.language === 'ar' ? 'rtl' : 'ltr'}
            onClick={() => onSelect(suggestion.text)}
          >
            {suggestion.text}
          </button>
        ))}
      </div>

      {/* AI disclaimer — constant, never LLM-generated */}
      <div
        className="mt-3 flex items-start gap-2 rounded-md bg-amber-50 border border-amber-200 px-3 py-2"
        data-testid="ai-disclaimer"
        role="note"
      >
        <AlertCircle className="h-3.5 w-3.5 text-amber-600 mt-0.5 flex-shrink-0" aria-hidden />
        <p className="text-xs text-amber-700 leading-snug">
          {t('ai_disclaimer')}
        </p>
      </div>
    </div>
  )
}
