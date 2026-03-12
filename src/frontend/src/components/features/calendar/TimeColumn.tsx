'use client'

const START_HOUR = 7
const END_HOUR = 21

export function TimeColumn() {
  const hours: number[] = []
  for (let h = START_HOUR; h <= END_HOUR; h++) {
    hours.push(h)
  }

  return (
    <div className="flex-shrink-0 w-16 border-e border-border" data-testid="calendar-time-column">
      {/* Header spacer to align with day headers */}
      <div className="h-12 border-b border-border" />
      {hours.map((hour) => (
        <div key={hour} className="h-16 relative border-b border-border/50">
          <span className="absolute -top-2.5 end-2 text-xs text-muted-foreground">
            {String(hour).padStart(2, '0')}:00
          </span>
        </div>
      ))}
    </div>
  )
}

export { START_HOUR, END_HOUR }
