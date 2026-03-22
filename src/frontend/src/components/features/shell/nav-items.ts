import {
  CalendarDays,
  PawPrint,
  CreditCard,
  Package,
  MessageSquare,
} from "lucide-react";
import type { LucideIcon } from "lucide-react";

export type UserRole = "VET" | "RECEPTIONIST" | "ASSISTANT" | "ADMIN" | string;

export interface NavItem {
  href: string;
  labelKey: string;
  icon: LucideIcon;
  testId: string;
  roles?: UserRole[];
  badge?: string;
  badgeForRoles?: UserRole[];
  badgeLabelKey?: string;
}

/**
 * Main navigation items — shared between Header (desktop tabs) and Sidebar.
 * 5 items max. Dashboard and Settings are in the UserMenu dropdown.
 */
export function getMainNavItems(): NavItem[] {
  return [
    {
      href: "/appointments",
      labelKey: "appointments",
      icon: CalendarDays,
      testId: "nav-appointments",
    },
    {
      href: "/patients",
      labelKey: "patients",
      icon: PawPrint,
      testId: "nav-patients",
    },
    {
      href: "/messages",
      labelKey: "messages",
      icon: MessageSquare,
      testId: "nav-messages",
    },
    {
      href: "/billing",
      labelKey: "billing",
      icon: CreditCard,
      testId: "nav-billing",
    },
    {
      href: "/stock",
      labelKey: "stock",
      icon: Package,
      testId: "nav-stock",
      roles: ["VET", "ADMIN"],
    },
  ];
}
