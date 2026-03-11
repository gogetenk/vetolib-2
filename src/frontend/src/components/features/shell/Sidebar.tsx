"use client";

import React from "react";
import Link from "next/link";
import { usePathname } from "next/navigation";
import {
  CalendarDays,
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

const mainNavItems: NavItem[] = [
  {
    href: "/appointments",
    label: "Agenda",
    icon: <CalendarDays className="h-5 w-5" />,
    testId: "nav-appointments",
  },
  {
    href: "/patients",
    label: "Patients",
    icon: <PawPrint className="h-5 w-5" />,
    testId: "nav-patients",
  },
  {
    href: "/medical-records",
    label: "Medical Records",
    icon: <ClipboardList className="h-5 w-5" />,
    testId: "nav-medical-records",
    // RECEPTIONIST cannot see medical records
    roles: ["VET", "ASSISTANT"],
    badge: "Read only",
    // badge only shown for ASSISTANT role
    badgeForRoles: ["ASSISTANT"],
  },
  {
    href: "/billing",
    label: "Billing",
    icon: <CreditCard className="h-5 w-5" />,
    testId: "nav-billing",
  },
  {
    href: "/messages",
    label: "Messages",
    icon: <MessageSquare className="h-5 w-5" />,
    testId: "nav-messages",
  },
  {
    href: "/stock",
    label: "Stock",
    icon: <Package className="h-5 w-5" />,
    testId: "nav-stock",
    roles: ["VET", "ADMIN"],
  },
  {
    href: "/settings/team",
    label: "Team",
    icon: <Users className="h-5 w-5" />,
    testId: "nav-team",
    roles: ["ADMIN"],
  },
  {
    href: "/settings/messaging/templates",
    label: "Messaging Settings",
    icon: <Settings className="h-5 w-5" />,
    testId: "nav-messaging-settings",
    roles: ["ADMIN"],
  },
];

const bottomNavItems: NavItem[] = [
  {
    href: "/settings",
    label: "Settings",
    icon: <Settings className="h-5 w-5" />,
    testId: "nav-settings",
  },
  {
    href: "/profile",
    label: "Profile",
    icon: <User className="h-5 w-5" />,
    testId: "nav-profile",
  },
];

interface SidebarNavItemProps {
  item: NavItem;
  isActive: boolean;
  role: UserRole;
  onClick?: () => void;
  messagingUnreadCount?: number;
}

function SidebarNavItem({ item, isActive, role, onClick, messagingUnreadCount }: SidebarNavItemProps) {
  const showBadge = item.badge && item.badgeForRoles?.includes(role);
  const isMessagesItem = item.href === "/messages";
  const showUnreadBadge = isMessagesItem && messagingUnreadCount != null && messagingUnreadCount > 0;

  return (
    <Link
      href={item.href}
      data-testid={item.testId}
      onClick={onClick}
      className={cn(
        "flex items-center gap-3 rounded-md px-3 py-2 text-sm font-medium transition-colors",
        isActive
          ? "bg-primary/10 text-primary"
          : "text-sidebar-foreground hover:bg-sidebar-accent hover:text-sidebar-accent-foreground"
      )}
      aria-current={isActive ? "page" : undefined}
    >
      {item.icon}
      <span>{item.label}</span>
      {showUnreadBadge && (
        <Badge
          data-testid="nav-messages-unread-badge"
          className="ml-auto text-xs bg-destructive text-destructive-foreground animate-pulse"
        >
          {messagingUnreadCount}
        </Badge>
      )}
      {!showUnreadBadge && showBadge && (
        <Badge
          data-testid={`${item.testId}-badge`}
          variant="secondary"
          className="ml-auto text-xs"
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
  onItemClick?: () => void;
}

function SidebarContent({ role, pathname, onItemClick }: SidebarContentProps) {
  const { unreadCount } = useMessagingSseContext();

  const visibleMain = mainNavItems.filter(
    (item) => !item.roles || item.roles.includes(role)
  );
  const visibleBottom = bottomNavItems.filter(
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
            isActive={pathname.startsWith(item.href)}
            role={role}
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
              isActive={pathname.startsWith(item.href)}
              role={role}
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

  return (
    <aside
      data-testid="dashboard-sidebar"
      className="hidden w-64 border-r bg-sidebar text-sidebar-foreground md:flex md:flex-col"
    >
      <div className="flex h-16 items-center border-b px-4">
        <Link
          href="/appointments"
          data-testid="sidebar-logo"
          className="flex items-center gap-2 font-bold text-lg text-primary"
        >
          <PawPrint className="h-6 w-6" />
          <span>Vetolib</span>
        </Link>
      </div>
      <SidebarContent role={role} pathname={pathname} />
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
  return <SidebarContent role={role} pathname={pathname} onItemClick={onItemClick} />;
}
