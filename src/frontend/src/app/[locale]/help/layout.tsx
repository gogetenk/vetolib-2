import type { Metadata } from "next";

export const metadata: Metadata = {
  title: "Help Center",
  description:
    "Find answers, guides, and troubleshooting tips to get the most out of Vetara. Browse our help articles on clinic setup, appointments, billing, messaging, and more.",
};

export default function HelpLayout({
  children,
}: {
  children: React.ReactNode;
}) {
  return <>{children}</>;
}
