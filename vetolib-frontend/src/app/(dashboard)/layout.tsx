import Link from "next/link";
import { Separator } from "@/components/ui/separator";

const navItems = [
  { href: "/appointments", label: "Appointments", testId: "nav-appointments" },
  { href: "/patients", label: "Patients", testId: "nav-patients" },
  {
    href: "/medical-records",
    label: "Medical Records",
    testId: "nav-medical-records",
  },
  { href: "/billing", label: "Billing", testId: "nav-billing" },
];

export default function DashboardLayout({
  children,
}: Readonly<{
  children: React.ReactNode;
}>) {
  return (
    <div className="flex min-h-screen">
      <aside
        className="hidden w-64 border-r bg-sidebar text-sidebar-foreground md:block"
        data-testid="dashboard-sidebar"
      >
        <div className="flex h-16 items-center px-6 font-semibold text-lg">
          <Link href="/appointments" data-testid="sidebar-logo">
            Vetolib
          </Link>
        </div>
        <Separator />
        <nav className="flex flex-col gap-1 p-4" data-testid="sidebar-nav">
          {navItems.map((item) => (
            <Link
              key={item.href}
              href={item.href}
              className="rounded-md px-3 py-2 text-sm font-medium hover:bg-sidebar-accent hover:text-sidebar-accent-foreground transition-colors"
              data-testid={item.testId}
            >
              {item.label}
            </Link>
          ))}
        </nav>
      </aside>
      <div className="flex flex-1 flex-col">
        <header
          className="flex h-16 items-center border-b px-6"
          data-testid="dashboard-header"
        >
          <h2 className="text-lg font-semibold">Dashboard</h2>
        </header>
        <main className="flex-1 p-6" data-testid="dashboard-main">
          {children}
        </main>
      </div>
    </div>
  );
}
