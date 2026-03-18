export interface ConsultationColor {
  bg: string
  border: string
  text: string
  dot: string
  line: string
}

export const CONSULTATION_COLORS: Record<string, ConsultationColor> = {
  'General Checkup': { bg: 'bg-primary/10', border: 'border-s-primary', text: 'text-foreground', dot: 'bg-primary', line: 'bg-primary' },
  'Vaccination': { bg: 'bg-[#e8f6f0]', border: 'border-s-[#22c55e]', text: 'text-[#02020a]', dot: 'bg-[#22c55e]', line: 'bg-[#22c55e]' },
  'Surgery': { bg: 'bg-[#fef2f2]', border: 'border-s-[#ef4444]', text: 'text-[#440000]', dot: 'bg-[#ef4444]', line: 'bg-[#ef4444]' },
  'Emergency': { bg: 'bg-[#fff7ed]', border: 'border-s-[#f97316]', text: 'text-[#02020a]', dot: 'bg-[#f97316]', line: 'bg-[#f97316]' },
  'Dental': { bg: 'bg-[#fdf4ff]', border: 'border-s-[#d946ef]', text: 'text-[#02020a]', dot: 'bg-[#d946ef]', line: 'bg-[#d946ef]' },
  'Dermatology': { bg: 'bg-[#fdf2f8]', border: 'border-s-[#e879f9]', text: 'text-[#02020a]', dot: 'bg-[#e879f9]', line: 'bg-[#e879f9]' },
  'Follow-up': { bg: 'bg-[#f0fdfa]', border: 'border-s-[#14b8a6]', text: 'text-[#02020a]', dot: 'bg-[#14b8a6]', line: 'bg-[#14b8a6]' },
  'Grooming': { bg: 'bg-[#fefce8]', border: 'border-s-[#f59e0b]', text: 'text-[#02020a]', dot: 'bg-[#f59e0b]', line: 'bg-[#f59e0b]' },
  'Laboratory / Diagnostics': { bg: 'bg-[#f5f3ff]', border: 'border-s-[#a855f7]', text: 'text-[#02020a]', dot: 'bg-[#a855f7]', line: 'bg-[#a855f7]' },
  'Exotic Animal': { bg: 'bg-[#ecfdf5]', border: 'border-s-[#059669]', text: 'text-[#02020a]', dot: 'bg-[#059669]', line: 'bg-[#059669]' },
}

const FALLBACK_COLOR: ConsultationColor = {
  bg: 'bg-muted',
  border: 'border-s-[#9ca3af]',
  text: 'text-[#02020a]',
  dot: 'bg-[#9ca3af]',
  line: 'bg-[#9ca3af]',
}

export function getConsultationColor(consultationType: string): ConsultationColor {
  return CONSULTATION_COLORS[consultationType] ?? FALLBACK_COLOR
}
