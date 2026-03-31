"use client";

import React, { useEffect, useRef, useState } from "react";
import {
  Bell,
  Heart,
  Clock,
  Package,
  MessageSquare,
  Info,
  Check,
} from "lucide-react";
import Link from "next/link";
import { useLocale, useTranslations } from "next-intl";
import { Button } from "@/components/ui/button";
import { cn } from "@/lib/utils";
import { useNotifications } from "@/hooks/use-notifications";
import type { NotificationType } from "@/lib/api/notifications";

function notificationIcon(type: NotificationType) {
  switch (type) {
    case "HealthAlert":
      return <Heart className="h-4 w-4 text-destructive shrink-0" />;
    case "Reminder":
      return <Clock className="h-4 w-4 text-amber-500 shrink-0" />;
    case "StockLow":
      return <Package className="h-4 w-4 text-orange-500 shrink-0" />;
    case "Message":
      return <MessageSquare className="h-4 w-4 text-blue-500 shrink-0" />;
    case "System":
      return <Info className="h-4 w-4 text-muted-foreground shrink-0" />;
  }
}

function formatTimeAgo(dateString: string, t: ReturnType<typeof useTranslations>): string {
  const now = Date.now();
  const date = new Date(dateString).getTime();
  const diffMs = now - date;
  const diffMin = Math.floor(diffMs / 60000);
  const diffHours = Math.floor(diffMs / 3600000);
  const diffDays = Math.floor(diffMs / 86400000);

  if (diffMin < 1) return t("just_now");
  if (diffMin < 60) return t("minutes_ago", { count: diffMin });
  if (diffHours < 24) return t("hours_ago", { count: diffHours });
  return t("days_ago", { count: diffDays });
}

export function NotificationCenter() {
  const [open, setOpen] = useState(false);
  const panelRef = useRef<HTMLDivElement>(null);
  const locale = useLocale();
  const t = useTranslations("notification_center");
  const { notifications, unreadCount, loading, markRead, markAllRead } =
    useNotifications();

  // Close on outside click
  useEffect(() => {
    function handleClickOutside(e: MouseEvent) {
      if (panelRef.current && !panelRef.current.contains(e.target as Node)) {
        setOpen(false);
      }
    }
    if (open) {
      document.addEventListener("mousedown", handleClickOutside);
    }
    return () => document.removeEventListener("mousedown", handleClickOutside);
  }, [open]);

  // Close on Escape
  useEffect(() => {
    function handleEscape(e: KeyboardEvent) {
      if (e.key === "Escape") setOpen(false);
    }
    if (open) {
      document.addEventListener("keydown", handleEscape);
    }
    return () => document.removeEventListener("keydown", handleEscape);
  }, [open]);

  return (
    <div className="relative" ref={panelRef}>
      <Button
        variant="ghost"
        size="icon"
        data-testid="notification-bell"
        className="relative text-muted-foreground hover:text-foreground"
        aria-label={t("aria_label")}
        aria-expanded={open}
        aria-haspopup="true"
        onClick={() => setOpen((prev) => !prev)}
      >
        <Bell className="h-5 w-5" />
        {unreadCount > 0 && (
          <span
            data-testid="notification-badge"
            className="absolute -top-0.5 -end-0.5 flex h-4 min-w-4 items-center justify-center rounded-full bg-destructive text-[10px] font-bold text-destructive-foreground px-1"
          >
            {unreadCount > 99 ? "99+" : unreadCount}
          </span>
        )}
      </Button>

      {open && (
        <div
          data-testid="notification-dropdown"
          className={cn(
            "absolute top-full mt-2 w-80 sm:w-96 rounded-lg border border-border bg-card shadow-lg z-50",
            "end-0"
          )}
          role="dialog"
          aria-label={t("panel_title")}
        >
          {/* Header */}
          <div className="flex items-center justify-between border-b border-border px-4 py-3">
            <h3
              data-testid="notification-panel-title"
              className="text-sm font-semibold text-foreground"
            >
              {t("panel_title")}
            </h3>
            {unreadCount > 0 && (
              <button
                data-testid="notification-mark-all-read"
                onClick={() => markAllRead()}
                className="text-xs font-medium text-primary hover:text-primary/80 transition-colors"
              >
                {t("mark_all_read")}
              </button>
            )}
          </div>

          {/* Notification list */}
          <div
            data-testid="notification-list"
            className="max-h-80 overflow-y-auto"
          >
            {loading && notifications.length === 0 ? (
              <div className="flex items-center justify-center py-8">
                <span className="text-sm text-muted-foreground">
                  {t("loading")}
                </span>
              </div>
            ) : notifications.length === 0 ? (
              <div
                data-testid="notification-empty"
                className="flex flex-col items-center justify-center py-8 gap-2"
              >
                <Bell className="h-8 w-8 text-muted-foreground/40" />
                <span className="text-sm text-muted-foreground">
                  {t("empty")}
                </span>
              </div>
            ) : (
              notifications.map((notif) => {
                const content = (
                  <div
                    className={cn(
                      "flex items-start gap-3 px-4 py-3 transition-colors hover:bg-accent/50 cursor-pointer",
                      !notif.isRead && "bg-primary/5"
                    )}
                  >
                    <div className="mt-0.5">
                      {notificationIcon(notif.type)}
                    </div>
                    <div className="flex-1 min-w-0">
                      <div className="flex items-start justify-between gap-2">
                        <p
                          className={cn(
                            "text-sm leading-tight",
                            notif.isRead
                              ? "text-muted-foreground"
                              : "text-foreground font-medium"
                          )}
                        >
                          {notif.title}
                        </p>
                        {!notif.isRead && (
                          <button
                            data-testid={`notification-mark-read-${notif.id}`}
                            onClick={(e) => {
                              e.preventDefault();
                              e.stopPropagation();
                              markRead(notif.id);
                            }}
                            className="shrink-0 rounded-full p-0.5 text-muted-foreground hover:text-primary hover:bg-primary/10 transition-colors"
                            aria-label={t("mark_read")}
                          >
                            <Check className="h-3 w-3" />
                          </button>
                        )}
                      </div>
                      <p className="text-xs text-muted-foreground mt-0.5 line-clamp-2">
                        {notif.message}
                      </p>
                      <span className="text-[10px] text-muted-foreground/70 mt-1 block">
                        {formatTimeAgo(notif.createdAt, t)}
                      </span>
                    </div>
                  </div>
                );

                return notif.actionUrl ? (
                  <Link
                    key={notif.id}
                    href={`/${locale}${notif.actionUrl}`}
                    data-testid={`notification-item-${notif.id}`}
                    onClick={() => {
                      if (!notif.isRead) markRead(notif.id);
                      setOpen(false);
                    }}
                    className="block"
                  >
                    {content}
                  </Link>
                ) : (
                  <div
                    key={notif.id}
                    data-testid={`notification-item-${notif.id}`}
                    onClick={() => {
                      if (!notif.isRead) markRead(notif.id);
                    }}
                  >
                    {content}
                  </div>
                );
              })
            )}
          </div>
        </div>
      )}
    </div>
  );
}
