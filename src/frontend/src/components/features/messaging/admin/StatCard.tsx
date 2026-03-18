import { Card, CardContent, CardHeader, CardTitle } from "@/components/ui/card"
import { cn } from "@/lib/utils"

interface StatCardProps {
  title: string
  value: string
  description?: string
  className?: string
  testId?: string
}

export function StatCard({ title, value, description, className, testId }: StatCardProps) {
  return (
    <Card className={cn("bg-white border-border/80 rounded-xl shadow-sm", className)} data-testid={testId ?? "stat-card"}>
      <CardHeader className="pb-2">
        <CardTitle className="text-[13px] font-semibold text-muted-foreground">
          {title}
        </CardTitle>
      </CardHeader>
      <CardContent>
        <div className="text-2xl font-bold text-[#061e44]" data-testid={`${testId ?? "stat-card"}-value`}>
          {value}
        </div>
        {description && (
          <p className="text-[12px] text-muted-foreground mt-1">{description}</p>
        )}
      </CardContent>
    </Card>
  )
}
