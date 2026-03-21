"use client";

import { Globe, MessageCircle, Server, Shield } from "lucide-react";
import { ScrollReveal } from "./ScrollReveal";

interface TrustSignalsMessages {
  title: string;
  badges: {
    arabic_english: string;
    whatsapp: string;
    uae_hosting: string;
    moccae: string;
  };
}

interface Props {
  messages: TrustSignalsMessages;
}

const BADGES = [
  { key: "arabic_english" as const, Icon: Globe },
  { key: "whatsapp" as const, Icon: MessageCircle },
  { key: "uae_hosting" as const, Icon: Server },
  { key: "moccae" as const, Icon: Shield },
];

export function TrustSignalsSection({ messages: m }: Props) {
  return (
    <section
      data-testid="section-trust-signals"
      className="border-y border-stone-100 bg-gradient-to-br from-emerald-50 via-white to-teal-50 py-16 sm:py-20"
    >
      <div className="mx-auto max-w-7xl px-4 sm:px-6 lg:px-8">
        <ScrollReveal direction="fade-up">
          <h2 className="text-center text-xl font-bold tracking-tight text-stone-900 sm:text-2xl lg:text-3xl">
            {m.title}
          </h2>
        </ScrollReveal>

        <div className="mt-12 grid grid-cols-2 gap-6 sm:gap-8 lg:grid-cols-4">
          {BADGES.map(({ key, Icon }, index) => (
            <ScrollReveal key={key} delay={index * 100} direction="fade-up">
              <div
                data-testid={`trust-badge-${key}`}
                className="flex flex-col items-center gap-3 rounded-2xl border border-stone-100 bg-white p-6 shadow-sm transition-all duration-300 hover:shadow-md hover:-translate-y-1"
              >
                <div className="flex h-12 w-12 items-center justify-center rounded-full bg-emerald-100 text-emerald-700">
                  <Icon className="h-6 w-6" aria-hidden="true" />
                </div>
                <span className="text-center text-sm font-semibold text-stone-800">
                  {m.badges[key]}
                </span>
              </div>
            </ScrollReveal>
          ))}
        </div>
      </div>
    </section>
  );
}
