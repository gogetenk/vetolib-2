"use client";

import React, { useState } from "react";
import { Menu, PawPrint, ChevronDown } from "lucide-react";
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
import { useLocale } from "next-intl";
import { UserMenu } from "./UserMenu";
import { MobileSidebarContent } from "./Sidebar";
import { cn } from "@/lib/utils";
import { useMessagingSseContext } from "@/components/features/messaging/MessagingSseProvider";

function parseClinicName(token: string): string {
  try {
    const base64 = token.split(".")[1];
    const json = atob(base64.replace(/-/g, "+").replace(/_/g, "/"));
    const payload = JSON.parse(json) as Record<string, unknown>;
    return (payload["clinicName"] as string) || "";
  } catch {
    return "";
  }
}

function getInitialClinicName(): string {
  if (typeof window === "undefined") return "";
  const token = localStorage.getItem("access_token");
  return token ? parseClinicName(token) : "Desert Paws Clinic"; // Fallback for dev
}

export function Header() {
  const [mobileOpen, setMobileOpen] = useState(false);
  const role = useRole();
  const locale = useLocale();
  const pathname = usePathname();
  const [clinicName] = useState<string>(getInitialClinicName);
  const { unreadCount } = useMessagingSseContext();

  const navItems = [
    { href: "/appointments", label: "Agenda" },
    { href: "/messages", label: "Messagerie", badge: unreadCount > 0 ? unreadCount : undefined },
    { href: "/medical-records", label: "Dossier et consultation" },
    { href: "/patients", label: "Patients" },
    { href: "/billing", label: "Comptabilité" },
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
          className="flex items-center justify-center font-bold text-2xl text-[#061e44] tracking-tighter"
        >
          <PawPrint className="h-6 w-6 mr-1" />
          <span>Veto</span>
        </Link>

        {/* Structure Selector Button */}
        <button className="hidden lg:flex items-center gap-2 px-3 py-1.5 rounded-full border border-border/80 bg-white hover:bg-[#f4f6f9] transition-colors text-[13px] font-semibold text-[#061e44]">
          <span className="max-w-[150px] truncate">{clinicName}</span>
          <ChevronDown className="h-4 w-4 text-muted-foreground" />
        </button>

        {/* Desktop Navigation Tabs */}
        <nav className="hidden lg:flex items-center gap-1 h-full ml-4">
          {navItems.map((item) => {
            const isActive = pathname.startsWith(`/${locale}${item.href}`);
            return (
              <Link
                key={item.href}
                href={`/${locale}${item.href}`}
                className={cn(
                  "relative flex items-center h-full px-4 text-[13px] font-semibold transition-colors hover:text-[#303ef5]",
                  isActive ? "text-[#303ef5]" : "text-muted-foreground"
                )}
              >
                {item.label}
                {item.badge && (
                  <span className="ml-1.5 flex h-5 min-w-5 items-center justify-center rounded-full bg-[#303ef5] text-[10px] font-bold text-white px-1">
                    {item.badge}
                  </span>
                )}
                {/* Active indicator bar at bottom */}
                {isActive && (
                  <span className="absolute bottom-0 left-0 right-0 h-0.5 bg-[#303ef5] rounded-t-full" />
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
          onClick={() => setMobileOpen(true)}
        >
          <Menu className="h-5 w-5" />
        </Button>

        {/* User menu */}
        <UserMenu />
      </div>

      {/* Mobile sidebar sheet */}
      <Sheet open={mobileOpen} onOpenChange={setMobileOpen}>
        <SheetContent side="left" data-testid="mobile-sidebar-sheet" className="p-0">
          <SheetHeader className="p-4 border-b">
            <SheetTitle className="flex items-center gap-2 text-foreground">
              <PawPrint className="h-5 w-5" />
              Vetolib
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
