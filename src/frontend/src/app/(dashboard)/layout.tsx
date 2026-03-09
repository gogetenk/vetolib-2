import { Header } from "@/components/features/shell/Header";
import { Sidebar } from "@/components/features/shell/Sidebar";

export default function DashboardLayout({
  children,
}: Readonly<{
  children: React.ReactNode;
}>) {
  return (
    <div className="flex min-h-screen flex-col">
      <Header />
      <div className="flex flex-1">
        <Sidebar />
        <main className="flex-1 p-6" data-testid="dashboard-main">
          {children}
        </main>
      </div>
    </div>
  );
}
