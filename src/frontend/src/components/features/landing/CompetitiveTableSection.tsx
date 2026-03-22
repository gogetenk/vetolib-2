"use client";

import { Check, X, Minus } from "lucide-react";
import { ScrollReveal } from "./ScrollReveal";

type CellStatus = "yes" | "no" | "partial";

interface CompetitiveRow {
  feature: string;
  vetolib: CellStatus;
  ezyvet: CellStatus;
  digitail: CellStatus;
}

interface CompetitiveTableMessages {
  title: string;
  subtitle: string;
  columns: {
    feature: string;
    vetolib: string;
    ezyvet: string;
    digitail: string;
  };
  rows: CompetitiveRow[];
}

interface Props {
  messages: CompetitiveTableMessages;
}

function StatusIcon({ status }: { status: CellStatus }) {
  switch (status) {
    case "yes":
      return (
        <span className="inline-flex h-7 w-7 items-center justify-center rounded-full bg-secondary">
          <Check className="h-4 w-4 text-primary" aria-label="Yes" />
        </span>
      );
    case "no":
      return (
        <span className="inline-flex h-7 w-7 items-center justify-center rounded-full bg-red-100">
          <X className="h-4 w-4 text-red-600" aria-label="No" />
        </span>
      );
    case "partial":
      return (
        <span className="inline-flex h-7 w-7 items-center justify-center rounded-full bg-amber-100">
          <Minus className="h-4 w-4 text-amber-600" aria-label="Partial" />
        </span>
      );
  }
}

export function CompetitiveTableSection({ messages: m }: Props) {
  return (
    <section
      data-testid="section-competitive"
      className="bg-stone-50 py-20 sm:py-28"
    >
      <div className="mx-auto max-w-4xl px-4 sm:px-6 lg:px-8">
        <ScrollReveal direction="fade-up">
          <div className="mx-auto max-w-2xl text-center">
            <h2 className="text-2xl font-bold tracking-tight text-stone-900 sm:text-3xl lg:text-4xl">
              {m.title}
            </h2>
            <p className="mt-4 text-lg text-stone-600">{m.subtitle}</p>
          </div>
        </ScrollReveal>

        <ScrollReveal direction="fade-up" delay={200}>
          <div className="mt-12 overflow-x-auto rounded-2xl border border-stone-200 bg-white shadow-sm">
            <table
              className="w-full text-sm"
              data-testid="competitive-table"
            >
              <thead>
                <tr className="border-b border-stone-100 bg-stone-50">
                  <th className="px-6 py-4 text-start font-semibold text-stone-700">
                    {m.columns.feature}
                  </th>
                  <th className="px-4 py-4 text-center font-bold text-primary">
                    {m.columns.vetolib}
                  </th>
                  <th className="px-4 py-4 text-center font-semibold text-stone-500">
                    {m.columns.ezyvet}
                  </th>
                  <th className="px-4 py-4 text-center font-semibold text-stone-500">
                    {m.columns.digitail}
                  </th>
                </tr>
              </thead>
              <tbody className="divide-y divide-stone-100">
                {m.rows.map((row, i) => (
                  <tr
                    key={i}
                    data-testid={`competitive-row-${i}`}
                    className="transition-colors hover:bg-stone-50/50"
                  >
                    <td className="px-6 py-4 font-medium text-stone-800">
                      {row.feature}
                    </td>
                    <td className="px-4 py-4 text-center">
                      <StatusIcon status={row.vetolib} />
                    </td>
                    <td className="px-4 py-4 text-center">
                      <StatusIcon status={row.ezyvet} />
                    </td>
                    <td className="px-4 py-4 text-center">
                      <StatusIcon status={row.digitail} />
                    </td>
                  </tr>
                ))}
              </tbody>
            </table>
          </div>
        </ScrollReveal>
      </div>
    </section>
  );
}
