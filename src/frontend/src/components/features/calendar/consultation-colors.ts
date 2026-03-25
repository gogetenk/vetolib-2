export interface ConsultationColor {
  bg: string
  border: string
  text: string
  dot: string
  line: string
}

export const CONSULTATION_COLORS: Record<string, ConsultationColor> = {
  'General Checkup': { bg: 'bg-primary/10', border: 'border-s-primary', text: 'text-foreground', dot: 'bg-primary', line: 'bg-primary' },
  'Vaccination': { bg: 'bg-green-500/10', border: 'border-s-green-500', text: 'text-foreground', dot: 'bg-green-500', line: 'bg-green-500' },
  'Surgery': { bg: 'bg-destructive/10', border: 'border-s-destructive', text: 'text-destructive-foreground', dot: 'bg-destructive', line: 'bg-destructive' },
  'Emergency': { bg: 'bg-orange-500/10', border: 'border-s-orange-500', text: 'text-foreground', dot: 'bg-orange-500', line: 'bg-orange-500' },
  'Dental': { bg: 'bg-fuchsia-500/10', border: 'border-s-fuchsia-500', text: 'text-foreground', dot: 'bg-fuchsia-500', line: 'bg-fuchsia-500' },
  'Dermatology': { bg: 'bg-fuchsia-400/10', border: 'border-s-fuchsia-400', text: 'text-foreground', dot: 'bg-fuchsia-400', line: 'bg-fuchsia-400' },
  'Follow-up': { bg: 'bg-teal-500/10', border: 'border-s-teal-500', text: 'text-foreground', dot: 'bg-teal-500', line: 'bg-teal-500' },
  'Grooming': { bg: 'bg-amber-500/10', border: 'border-s-amber-500', text: 'text-foreground', dot: 'bg-amber-500', line: 'bg-amber-500' },
  'Laboratory / Diagnostics': { bg: 'bg-purple-500/10', border: 'border-s-purple-500', text: 'text-foreground', dot: 'bg-purple-500', line: 'bg-purple-500' },
  'Exotic Animal': { bg: 'bg-emerald-600/10', border: 'border-s-emerald-600', text: 'text-foreground', dot: 'bg-emerald-600', line: 'bg-emerald-600' },
}

const FALLBACK_COLOR: ConsultationColor = {
  bg: 'bg-muted',
  border: 'border-s-muted-foreground',
  text: 'text-foreground',
  dot: 'bg-muted-foreground',
  line: 'bg-muted-foreground',
}

export function getConsultationColor(consultationType: string): ConsultationColor {
  return CONSULTATION_COLORS[consultationType] ?? FALLBACK_COLOR
}
