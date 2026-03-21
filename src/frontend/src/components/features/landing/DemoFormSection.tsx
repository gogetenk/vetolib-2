"use client";

import { useState, type FormEvent } from "react";
import { toast } from "sonner";
import { Button } from "@/components/ui/button";
import { Input } from "@/components/ui/input";
import { Label } from "@/components/ui/label";
import { ScrollReveal } from "./ScrollReveal";

interface DemoFormMessages {
  title: string;
  subtitle: string;
  clinic_name: string;
  clinic_name_placeholder: string;
  email: string;
  email_placeholder: string;
  phone: string;
  phone_placeholder: string;
  preferred_time: string;
  preferred_time_placeholder: string;
  submit: string;
  success_title: string;
  success_description: string;
}

interface Props {
  messages: DemoFormMessages;
}

export function DemoFormSection({ messages: m }: Props) {
  const [loading, setLoading] = useState(false);

  function handleSubmit(e: FormEvent<HTMLFormElement>) {
    e.preventDefault();
    setLoading(true);

    // Simulate submission — no backend yet
    setTimeout(() => {
      setLoading(false);
      toast.success(m.success_title, {
        description: m.success_description,
      });
      (e.target as HTMLFormElement).reset();
    }, 800);
  }

  return (
    <section
      id="demo"
      data-testid="section-demo"
      className="bg-white py-20 sm:py-28"
    >
      <div className="mx-auto max-w-2xl px-4 sm:px-6 lg:px-8">
        <ScrollReveal direction="fade-up">
          <div className="text-center">
            <h2 className="text-2xl font-bold tracking-tight text-stone-900 sm:text-3xl lg:text-4xl">
              {m.title}
            </h2>
            <p className="mt-4 text-lg text-stone-600">{m.subtitle}</p>
          </div>
        </ScrollReveal>

        <ScrollReveal direction="fade-up" delay={200}>
          <form
            onSubmit={handleSubmit}
            data-testid="demo-form"
            className="mt-10 space-y-5 rounded-2xl border border-stone-100 bg-stone-50 p-6 shadow-sm sm:p-8"
          >
            <div className="space-y-2">
              <Label htmlFor="demo-clinic-name">{m.clinic_name}</Label>
              <Input
                id="demo-clinic-name"
                name="clinicName"
                required
                placeholder={m.clinic_name_placeholder}
                data-testid="demo-input-clinic-name"
              />
            </div>
            <div className="space-y-2">
              <Label htmlFor="demo-email">{m.email}</Label>
              <Input
                id="demo-email"
                name="email"
                type="email"
                required
                placeholder={m.email_placeholder}
                data-testid="demo-input-email"
              />
            </div>
            <div className="space-y-2">
              <Label htmlFor="demo-phone">{m.phone}</Label>
              <Input
                id="demo-phone"
                name="phone"
                type="tel"
                placeholder={m.phone_placeholder}
                data-testid="demo-input-phone"
              />
            </div>
            <div className="space-y-2">
              <Label htmlFor="demo-preferred-time">{m.preferred_time}</Label>
              <Input
                id="demo-preferred-time"
                name="preferredTime"
                placeholder={m.preferred_time_placeholder}
                data-testid="demo-input-preferred-time"
              />
            </div>
            <Button
              type="submit"
              disabled={loading}
              className="w-full bg-emerald-700 font-semibold text-white transition-all duration-200 hover:bg-emerald-800 hover:shadow-md"
              data-testid="demo-submit-btn"
            >
              {loading ? "..." : m.submit}
            </Button>
          </form>
        </ScrollReveal>
      </div>
    </section>
  );
}
