import { clsx, type ClassValue } from "clsx";
import { twMerge } from "tailwind-merge";
import { format } from "date-fns";
import { TZDate } from "@date-fns/tz";

export function cn(...inputs: ClassValue[]) {
  return twMerge(clsx(inputs));
}

/**
 * Formats an amount in AED (United Arab Emirates Dirham).
 * Example: formatAED(1250) => "AED 1,250.00"
 */
export function formatAED(amount: number): string {
  return `AED ${amount.toLocaleString("en-AE", {
    minimumFractionDigits: 2,
    maximumFractionDigits: 2,
  })}`;
}

/**
 * Formats a date in the given timezone (defaults to Asia/Dubai).
 * Example: formatDate("2025-12-15T10:00:00Z") => "15 Dec 2025, 02:00 PM"
 */
export function formatDate(
  date: string | Date,
  timezone: string = "Asia/Dubai"
): string {
  const d = typeof date === "string" ? new Date(date) : date;
  const tzDate = new TZDate(d, timezone);
  return format(tzDate, "dd MMM yyyy, hh:mm a");
}
