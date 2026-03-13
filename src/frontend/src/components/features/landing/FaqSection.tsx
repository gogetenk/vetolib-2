"use client";

import { useState, useEffect, useRef } from "react";
import { ChevronDown } from "lucide-react";
import { trackEvent, AnalyticsEvents } from "@/lib/analytics";
import { ScrollReveal } from "./ScrollReveal";

interface FaqItem {
  question: string;
  answer: string;
}

interface Props {
  title: string;
  subtitle: string;
  items: FaqItem[];
}

export function FaqSection({ title, subtitle, items }: Props) {
  const [openIndex, setOpenIndex] = useState<number | null>(null);
  const sectionRef = useRef<HTMLElement>(null);
  const reportedDepths = useRef<Set<number>>(new Set());

  useEffect(() => {
    const thresholds = [25, 50, 75, 100];

    function getScrollDepth(): number {
      const scrollTop = window.scrollY;
      const docHeight =
        document.documentElement.scrollHeight - window.innerHeight;
      if (docHeight <= 0) return 100;
      return Math.round((scrollTop / docHeight) * 100);
    }

    function handleScroll() {
      const depth = getScrollDepth();
      for (const threshold of thresholds) {
        if (depth >= threshold && !reportedDepths.current.has(threshold)) {
          reportedDepths.current.add(threshold);
          trackEvent(AnalyticsEvents.SCROLL_DEPTH, {
            depth: `${threshold}%`,
          });
        }
      }
    }

    window.addEventListener("scroll", handleScroll, { passive: true });
    return () => window.removeEventListener("scroll", handleScroll);
  }, []);

  function toggle(index: number) {
    const isOpening = openIndex !== index;
    setOpenIndex(isOpening ? index : null);
    if (isOpening) {
      trackEvent(AnalyticsEvents.FAQ_EXPAND, { question_index: String(index) });
    }
  }

  return (
    <section
      ref={sectionRef}
      id="faq"
      data-testid="section-faq"
      className="bg-white py-20 sm:py-28"
    >
      <div className="mx-auto max-w-3xl px-4 sm:px-6 lg:px-8">
        <ScrollReveal direction="fade-up">
          <div className="text-center">
            <h2 className="text-2xl font-bold tracking-tight text-gray-900 sm:text-3xl lg:text-4xl">
              {title}
            </h2>
            <p className="mt-4 text-lg text-gray-600">{subtitle}</p>
          </div>
        </ScrollReveal>

        <ScrollReveal direction="fade-up" delay={200}>
          <div className="mt-12 divide-y divide-gray-100 rounded-2xl border border-gray-100 shadow-sm">
            {items.map((item, index) => {
              const isOpen = openIndex === index;
              return (
                <div
                  key={index}
                  data-testid={`faq-${index}`}
                >
                  <button
                    type="button"
                    data-testid={`faq-${index}-trigger`}
                    aria-expanded={isOpen}
                    onClick={() => toggle(index)}
                    className="flex w-full items-center justify-between px-6 py-5 text-start transition-colors duration-200 hover:bg-gray-50/50"
                  >
                    <span className="text-sm font-semibold text-gray-900 sm:text-base">
                      {item.question}
                    </span>
                    <ChevronDown
                      className={`ms-4 h-5 w-5 shrink-0 text-gray-400 transition-transform duration-300 ${
                        isOpen ? "rotate-180" : ""
                      }`}
                      aria-hidden="true"
                    />
                  </button>
                  <div
                    data-testid={`faq-${index}-content`}
                    className={`grid transition-all duration-300 ease-in-out ${
                      isOpen ? "grid-rows-[1fr] opacity-100" : "grid-rows-[0fr] opacity-0"
                    }`}
                  >
                    <div className="overflow-hidden">
                      <div className="px-6 pb-5">
                        <p className="text-sm leading-relaxed text-gray-600">
                          {item.answer}
                        </p>
                      </div>
                    </div>
                  </div>
                </div>
              );
            })}
          </div>
        </ScrollReveal>
      </div>
    </section>
  );
}
