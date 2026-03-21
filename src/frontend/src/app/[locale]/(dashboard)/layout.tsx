import type { Metadata } from "next";
import { Header } from "@/components/features/shell/Header";
import { MessagingSseProvider } from "@/components/features/messaging/MessagingSseProvider";
import { PostHogProvider } from "@/components/PostHogProvider";
import { ConsentBanner } from "@/components/features/analytics/ConsentBanner";

export const metadata: Metadata = {
  robots: { index: false, follow: false },
};

export default function DashboardLayout({
  children,
}: Readonly<{
  children: React.ReactNode;
}>) {
  return (
    <PostHogProvider>
      <MessagingSseProvider>
        <div className="flex h-screen flex-col bg-background text-foreground overflow-hidden">
          <Header />
          <main
            className="flex-1 flex flex-col w-full h-full relative animate-in fade-in-0 duration-300 ease-out overflow-y-auto"
            data-testid="dashboard-main"
          >
            {children}
          </main>
        </div>
        <ConsentBanner />
      </MessagingSseProvider>
    </PostHogProvider>
  );
}
