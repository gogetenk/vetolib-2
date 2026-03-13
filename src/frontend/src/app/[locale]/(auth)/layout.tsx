import { PostHogProvider } from "@/components/PostHogProvider";

export default function AuthLayout({
  children,
}: Readonly<{
  children: React.ReactNode;
}>) {
  return (
    <PostHogProvider>
      <div className="flex min-h-screen items-center justify-center bg-gradient-to-br from-background via-background to-emerald-50/30">
        <div className="w-full max-w-md px-4">{children}</div>
      </div>
    </PostHogProvider>
  );
}
