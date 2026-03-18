'use client'

const START_HOUR = 7
const END_HOUR = 21

export function TimeColumn() {
  const hours: number[] = []
  for (let h = START_HOUR; h <= END_HOUR; h++) {
    hours.push(h)
  }

  return (
    <div className="flex-shrink-0 w-16 border-e border-border/40 bg-white" data-testid="calendar-time-column">
      {/* Header spacer to align with day headers */}
      <div className="h-14 border-b border-border/40" />
      {hours.map((hour) => (
        <div key={hour} className="h-16 relative border-b border-border/20">
          <span className="absolute -top-2.5 end-3 text-[11px] font-semibold text-muted-foreground/70">
            {String(hour).padStart(2, '0')}:00
          </span>
        </div>
      ))}
    </div>
  )
}

export { START_HOUR, END_HOUR }
