"use client";

import React from "react";
import Link from "next/link";
import { usePathname } from "next/navigation";
import {
  CalendarDays,
  LayoutDashboard,
  PawPrint,
  ClipboardList,
  CreditCard,
  Settings,
  User,
  Users,
  Package,
  MessageSquare,
} from "lucide-react";
import { cn } from "@/lib/utils";
import { Badge } from "@/components/ui/badge";
import { Separator } from "@/components/ui/separator";
import { useRole } from "@/hooks/use-role";
import { useMessagingSseContext } from "@/components/features/messaging/MessagingSseProvider";
import { useLocale, useTranslations } from "next-intl";

type UserRole = "VET" | "RECEPTIONIST" | "ASSISTANT" | "ADMIN" | string;

interface NavItem {
  href: string;
  label: string;
  icon: React.ReactNode;
  testId: string;
  roles?: UserRole[]; // undefined = all roles
  badge?: string;     // optional badge text
  badgeForRoles?: UserRole[]; // show badge only for these roles
}

function getMainNavItems(t: (key: string) => string): NavItem[] {
  return [
    {
      href: "/dashboard",
      label: t("dashboard"),
      icon: <LayoutDashboard className="h-5 w-5" />,
      testId: "nav-dashboard",
    },
    {
      href: "/appointments",
      label: t("appointments"),
      icon: <CalendarDays className="h-5 w-5" />,
      testId: "nav-appointments",
    },
    {
      href: "/patients",
      label: t("patients"),
      icon: <PawPrint className="h-5 w-5" />,
      testId: "nav-patients",
    },
    {
      href: "/medical-records",
      label: t("medical_records"),
      icon: <ClipboardList className="h-5 w-5" />,
      testId: "nav-medical-records",
      // RECEPTIONIST cannot see medical records
      roles: ["VET", "ASSISTANT"],
      badge: t("read_only"),
      // badge only shown for ASSISTANT role
      badgeForRoles: ["ASSISTANT"],
    },
    {
      href: "/billing",
      label: t("billing"),
      icon: <CreditCard className="h-5 w-5" />,
      testId: "nav-billing",
    },
    {
      href: "/messages",
      label: t("messages"),
      icon: <MessageSquare className="h-5 w-5" />,
      testId: "nav-messages",
    },
    {
      href: "/stock",
      label: t("stock"),
      icon: <Package className="h-5 w-5" />,
      testId: "nav-stock",
      roles: ["VET", "ADMIN"],
    },
    {
      href: "/settings/team",
      label: t("team"),
      icon: <Users className="h-5 w-5" />,
      testId: "nav-team",
      roles: ["ADMIN"],
    },
    {
      href: "/settings/messaging/templates",
      label: t("messaging_settings"),
      icon: <Settings className="h-5 w-5" />,
      testId: "nav-messaging-settings",
      roles: ["ADMIN"],
    },
  ];
}

function getBottomNavItems(t: (key: string) => string): NavItem[] {
  return [
    {
      href: "/settings",
      label: t("settings"),
      icon: <Settings className="h-5 w-5" />,
      testId: "nav-settings",
    },
    {
      href: "/profile",
      label: t("profile"),
      icon: <User className="h-5 w-5" />,
      testId: "nav-profile",
    },
  ];
}

interface SidebarNavItemProps {
  item: NavItem;
  isActive: boolean;
  role: UserRole;
  locale: string;
  onClick?: () => void;
  messagingUnreadCount?: number;
}

function SidebarNavItem({ item, isActive, role, locale, onClick, messagingUnreadCount }: SidebarNavItemProps) {
  const showBadge = item.badge && item.badgeForRoles?.includes(role);
  const isMessagesItem = item.href === "/messages";
  const localizedHref = `/${locale}${item.href}`;
  const showUnreadBadge = isMessagesItem && messagingUnreadCount != null && messagingUnreadCount > 0;

  return (
    <Link
      href={localizedHref}
      data-testid={item.testId}
      onClick={onClick}
      className={cn(
        "group/nav-item relative flex items-center gap-3 rounded-xl px-3 py-2.5 text-[13px] font-semibold",
        "transition-all duration-200 ease-in-out",
        "focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-[#303ef5]/30 focus-visible:ring-offset-1",
        isActive
          ? "bg-[#eef2fd] text-[#303ef5]"
          : "text-muted-foreground hover:bg-[#f4f6f9] hover:text-[#061e44]"
      )}
      aria-current={isActive ? "page" : undefined}
    >
      {/* Active indicator bar */}
      <span
        className={cn(
          "absolute ltr:left-0 rtl:right-0 top-1/2 -translate-y-1/2 h-6 w-[3px] rounded-full bg-[#303ef5]",
          "transition-all duration-200 ease-in-out",
          isActive
            ? "opacity-100 scale-y-100"
            : "opacity-0 scale-y-0"
        )}
        aria-hidden="true"
      />
      <span className={cn(
        "transition-transform duration-200 ease-in-out",
        "group-hover/nav-item:scale-110",
        isActive && "text-[#303ef5]"
      )}>
        {item.icon}
      </span>
      <span>{item.label}</span>
      {showUnreadBadge && (
        <Badge
          data-testid="nav-messages-unread-badge"
          className="ltr:ml-auto rtl:mr-auto text-[10px] font-bold bg-[#303ef5] text-white rounded-full h-5 min-w-5 flex items-center justify-center px-1 animate-pulse"
        >
          {messagingUnreadCount}
        </Badge>
      )}
      {!showUnreadBadge && showBadge && (
        <Badge
          data-testid={`${item.testId}-badge`}
          variant="secondary"
          className="ltr:ml-auto rtl:mr-auto text-xs"
        >
          {item.badge}
        </Badge>
      )}
    </Link>
  );
}

interface SidebarContentProps {
  role: UserRole;
  pathname: string;
  locale: string;
  onItemClick?: () => void;
}

function SidebarContent({ role, pathname, locale, onItemClick }: SidebarContentProps) {
  const { unreadCount } = useMessagingSseContext();
  const t = useTranslations("nav");

  const visibleMain = getMainNavItems(t).filter(
    (item) => !item.roles || item.roles.includes(role)
  );
  const visibleBottom = getBottomNavItems(t).filter(
    (item) => !item.roles || item.roles.includes(role)
  );

  return (
    <div className="flex h-full flex-col">
      {/* Main navigation */}
      <nav
        data-testid="sidebar-nav"
        className="flex flex-1 flex-col gap-1 p-4"
      >
        {visibleMain.map((item) => (
          <SidebarNavItem
            key={item.href}
            item={item}
            isActive={pathname.startsWith(`/${locale}${item.href}`)}
            role={role}
            locale={locale}
            onClick={onItemClick}
            messagingUnreadCount={unreadCount}
          />
        ))}
      </nav>

      {/* Bottom navigation */}
      <div>
        <Separator />
        <nav
          data-testid="sidebar-nav-bottom"
          className="flex flex-col gap-1 p-4"
        >
          {visibleBottom.map((item) => (
            <SidebarNavItem
              key={item.href}
              item={item}
              isActive={pathname.startsWith(`/${locale}${item.href}`)}
              role={role}
              locale={locale}
              onClick={onItemClick}
            />
          ))}
        </nav>
      </div>
    </div>
  );
}

export function Sidebar() {
  const pathname = usePathname();
  const role = useRole();
  const locale = useLocale();

  return (
    <aside
      data-testid="dashboard-sidebar"
      className="hidden w-64 bg-sidebar text-sidebar-foreground md:flex md:flex-col transition-all duration-200 ease-in-out"
    >
      <div className="flex h-16 items-center px-6">
        <Link
          href={`/${locale}/appointments`}
          data-testid="sidebar-logo"
          className="group/logo flex items-center gap-2 font-bold text-xl text-[#303ef5] transition-all duration-200 ease-in-out hover:opacity-80"
        >
          <PawPrint className="h-6 w-6 transition-transform duration-200 ease-in-out group-hover/logo:rotate-[-8deg] group-hover/logo:scale-110" />
          <span>Vetolib</span>
        </Link>
      </div>
      <div className="flex-1 overflow-y-auto px-3">
        <SidebarContent role={role} pathname={pathname} locale={locale} />
      </div>
    </aside>
  );
}

/**
 * Mobile sidebar content — rendered inside a Sheet.
 * Accepts role + onItemClick from parent (Header) which controls the Sheet state.
 */
export function MobileSidebarContent({
  role,
  onItemClick,
}: {
  role: UserRole;
  onItemClick: () => void;
}) {
  const pathname = usePathname();
  const locale = useLocale();
  return <SidebarContent role={role} pathname={pathname} locale={locale} onItemClick={onItemClick} />;
}
