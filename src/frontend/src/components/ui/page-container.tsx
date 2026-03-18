import * as React from "react"

import { cn } from "@/lib/utils"

type PageContainerVariant = "full" | "default" | "narrow"

const variantClasses: Record<PageContainerVariant, string> = {
  full: "",
  default: "max-w-6xl mx-auto",
  narrow: "max-w-4xl mx-auto",
}

function PageContainer({
  className,
  variant = "full",
  ...props
}: React.ComponentProps<"div"> & { variant?: PageContainerVariant }) {
  return (
    <div
      data-testid="page-container"
      className={cn(
        "p-6 lg:p-8 space-y-6 animate-in fade-in duration-300",
        variantClasses[variant],
        className
      )}
      {...props}
    />
  )
}

export { PageContainer }
