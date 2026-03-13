export interface ConsultationColor {
  bg: string
  border: string
  text: string
}

export const CONSULTATION_COLORS: Record<string, ConsultationColor> = {
  'General Checkup': { bg: 'bg-blue-100', border: 'border-l-blue-500', text: 'text-blue-700' },
  'Vaccination': { bg: 'bg-green-100', border: 'border-l-green-500', text: 'text-green-700' },
  'Surgery': { bg: 'bg-red-100', border: 'border-l-red-500', text: 'text-red-700' },
  'Emergency': { bg: 'bg-orange-100', border: 'border-l-orange-500', text: 'text-orange-700' },
  'Dental': { bg: 'bg-purple-100', border: 'border-l-purple-500', text: 'text-purple-700' },
  'Dermatology': { bg: 'bg-pink-100', border: 'border-l-pink-500', text: 'text-pink-700' },
  'Follow-up': { bg: 'bg-teal-100', border: 'border-l-teal-500', text: 'text-teal-700' },
  'Grooming': { bg: 'bg-amber-100', border: 'border-l-amber-500', text: 'text-amber-700' },
  'Laboratory / Diagnostics': { bg: 'bg-indigo-100', border: 'border-l-indigo-500', text: 'text-indigo-700' },
  'Exotic Animal': { bg: 'bg-emerald-100', border: 'border-l-emerald-600', text: 'text-emerald-700' },
}

const FALLBACK_COLOR: ConsultationColor = {
  bg: 'bg-stone-100',
  border: 'border-l-gray-400',
  text: 'text-stone-700',
}

export function getConsultationColor(consultationType: string): ConsultationColor {
  return CONSULTATION_COLORS[consultationType] ?? FALLBACK_COLOR
}
