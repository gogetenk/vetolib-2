import { Header } from "@/components/features/shell/Header";
import { Sidebar } from "@/components/features/shell/Sidebar";
import { MessagingSseProvider } from "@/components/features/messaging/MessagingSseProvider";
import { PostHogProvider } from "@/components/PostHogProvider";
import { ConsentBanner } from "@/components/features/analytics/ConsentBanner";

export default function DashboardLayout({
  children,
}: Readonly<{
  children: React.ReactNode;
}>) {
  return (
    <PostHogProvider>
      <MessagingSseProvider>
        <div className="flex min-h-screen flex-col">
          <Header />
          <div className="flex flex-1">
            <Sidebar />
            <main className="flex-1 p-6 animate-in fade-in-0 duration-300 ease-out" data-testid="dashboard-main">
              {children}
            </main>
          </div>
        </div>
        <ConsentBanner />
      </MessagingSseProvider>
    </PostHogProvider>
  );
}
