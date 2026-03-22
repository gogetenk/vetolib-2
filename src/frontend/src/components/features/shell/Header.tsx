"use client";

import React, { useState } from "react";
import { Menu, PawPrint, Settings } from "lucide-react";
import Link from "next/link";
import { usePathname } from "next/navigation";
import { Button } from "@/components/ui/button";
import {
  Sheet,
  SheetContent,
  SheetHeader,
  SheetTitle,
} from "@/components/ui/sheet";
import { useRole } from "@/hooks/use-role";
import { useLocale, useTranslations } from "next-intl";
import { UserMenu } from "./UserMenu";
import { MobileSidebarContent } from "./Sidebar";
import { cn } from "@/lib/utils";
import { useMessagingSseContext } from "@/components/features/messaging/MessagingSseProvider";
import { ClinicSwitcher } from "./ClinicSwitcher";

export function Header() {
  const [mobileOpen, setMobileOpen] = useState(false);
  const role = useRole();
  const locale = useLocale();
  const pathname = usePathname();
  const { unreadCount } = useMessagingSseContext();
  const t = useTranslations("nav");

  const navItems = [
    { href: "/dashboard", label: t("dashboard") },
    { href: "/appointments", label: t("appointments") },
    { href: "/messages", label: t("messages"), badge: unreadCount > 0 ? unreadCount : undefined },
{ href: "/patients", label: t("patients") },
    { href: "/billing", label: t("billing") },
    { href: "/stock", label: t("stock") },
  ];

  return (
    <header
      data-testid="dashboard-header"
      className="flex h-16 items-center justify-between bg-white px-4 sm:px-6 border-b border-border/80 shadow-sm sticky top-0 z-50"
    >
      <div className="flex items-center gap-6 h-full">
        {/* Logo WEDA-like */}
        <Link
          href={`/${locale}/appointments`}
          data-testid="header-logo"
          className="flex items-center justify-center font-bold text-2xl text-foreground tracking-tighter"
        >
          <PawPrint className="h-6 w-6 mr-1" />
          <span>Veto</span>
        </Link>

        {/* Clinic Switcher — visible only for multi-clinic groups */}
        <ClinicSwitcher />

        {/* Desktop Navigation Tabs */}
        <nav className="hidden lg:flex items-center gap-1 h-full ml-4">
          {navItems.map((item) => {
            const isActive = pathname.startsWith(`/${locale}${item.href}`);
            return (
              <Link
                key={item.href}
                href={`/${locale}${item.href}`}
                aria-current={isActive ? "page" : undefined}
                className={cn(
                  "relative flex items-center h-full px-4 text-[13px] font-semibold transition-colors hover:text-primary",
                  isActive ? "text-primary" : "text-muted-foreground"
                )}
              >
                {item.label}
                {item.badge && (
                  <span className="ml-1.5 flex h-5 min-w-5 items-center justify-center rounded-full bg-primary text-[10px] font-bold text-primary-foreground px-1">
                    {item.badge}
                  </span>
                )}
                {/* Active indicator bar at bottom */}
                {isActive && (
                  <span className="absolute bottom-0 left-0 right-0 h-0.5 bg-primary rounded-t-full" />
                )}
              </Link>
            );
          })}
        </nav>
      </div>

      <div className="flex items-center gap-4">
        {/* Hamburger — mobile only */}
        <Button
          variant="ghost"
          size="icon"
          data-testid="mobile-menu-trigger"
          className="lg:hidden"
          aria-label="Open navigation menu"
          onClick={() => setMobileOpen(true)}
        >
          <Menu className="h-5 w-5" />
        </Button>

        {/* Settings shortcut — desktop only */}
        <Link
          href={`/${locale}/settings`}
          data-testid="header-settings-link"
          className="hidden lg:flex"
        >
          <Button
            variant="ghost"
            size="icon"
            aria-label="Settings"
            className="text-muted-foreground hover:text-foreground"
          >
            <Settings className="h-5 w-5" />
          </Button>
        </Link>

        {/* User menu */}
        <UserMenu />
      </div>

      {/* Mobile sidebar sheet */}
      <Sheet open={mobileOpen} onOpenChange={setMobileOpen}>
        <SheetContent side="left" data-testid="mobile-sidebar-sheet" className="p-0">
          <SheetHeader className="p-4 border-b">
            <SheetTitle className="flex items-center gap-2 text-foreground">
              <PawPrint className="h-5 w-5" />
              Vetara
            </SheetTitle>
          </SheetHeader>
          <div className="flex-1 overflow-y-auto">
            <MobileSidebarContent
              role={role}
              onItemClick={() => setMobileOpen(false)}
            />
          </div>
        </SheetContent>
      </Sheet>
    </header>
  );
}
