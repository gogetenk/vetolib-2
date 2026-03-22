"use client";

import React from "react";
import Link from "next/link";
import { usePathname } from "next/navigation";
import { PawPrint } from "lucide-react";
import { cn } from "@/lib/utils";
import { Badge } from "@/components/ui/badge";
import { useRole } from "@/hooks/use-role";
import { useMessagingSseContext } from "@/components/features/messaging/MessagingSseProvider";
import { useLocale, useTranslations } from "next-intl";
import { getMainNavItems, type NavItem, type UserRole } from "./nav-items";

interface SidebarNavItemProps {
  item: NavItem;
  isActive: boolean;
  role: UserRole;
  locale: string;
  onClick?: () => void;
  messagingUnreadCount?: number;
  t: (key: string) => string;
}

function SidebarNavItem({ item, isActive, role, locale, onClick, messagingUnreadCount, t }: SidebarNavItemProps) {
  const showBadge = item.badgeLabelKey && item.badgeForRoles?.includes(role);
  const isMessagesItem = item.href === "/messages";
  const localizedHref = `/${locale}${item.href}`;
  const showUnreadBadge = isMessagesItem && messagingUnreadCount != null && messagingUnreadCount > 0;
  const Icon = item.icon;

  return (
    <Link
      href={localizedHref}
      data-testid={item.testId}
      onClick={onClick}
      className={cn(
        "group/nav-item relative flex items-center gap-3 rounded-xl px-3 py-2.5 text-[13px] font-semibold",
        "transition-all duration-200 ease-in-out",
        "focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-primary/30 focus-visible:ring-offset-1",
        isActive
          ? "bg-primary/10 text-primary"
          : "text-muted-foreground hover:bg-muted hover:text-foreground"
      )}
      aria-current={isActive ? "page" : undefined}
    >
      {/* Active indicator bar */}
      <span
        className={cn(
          "absolute ltr:left-0 rtl:right-0 top-1/2 -translate-y-1/2 h-6 w-[3px] rounded-full bg-primary",
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
        isActive && "text-primary"
      )}>
        <Icon className="h-5 w-5" />
      </span>
      <span>{t(item.labelKey)}</span>
      {showUnreadBadge && (
        <Badge
          data-testid="nav-messages-unread-badge"
          className="ltr:ml-auto rtl:mr-auto text-[10px] font-bold bg-primary text-primary-foreground rounded-full h-5 min-w-5 flex items-center justify-center px-1 animate-pulse"
        >
          {messagingUnreadCount}
        </Badge>
      )}
      {!showUnreadBadge && showBadge && item.badgeLabelKey && (
        <Badge
          data-testid={`${item.testId}-badge`}
          variant="secondary"
          className="ltr:ml-auto rtl:mr-auto text-xs"
        >
          {t(item.badgeLabelKey)}
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

  const visibleMain = getMainNavItems().filter(
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
            t={t}
          />
        ))}
      </nav>
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
          className="group/logo flex items-center gap-2 font-bold text-xl text-primary transition-all duration-200 ease-in-out hover:opacity-80"
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
