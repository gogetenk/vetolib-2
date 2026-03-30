"use client";

import { MessageCircle } from "lucide-react";
import { Button } from "@/components/ui/button";

interface WhatsAppBookingButtonProps {
  label: string;
  clinicName?: string;
  phone?: string;
  className?: string;
  size?: "default" | "sm" | "lg" | "icon";
}

const DEFAULT_PHONE = "971501234567";
const DEFAULT_CLINIC_NAME = "our clinic";

export function WhatsAppBookingButton({
  label,
  clinicName = DEFAULT_CLINIC_NAME,
  phone = DEFAULT_PHONE,
  className,
  size = "lg",
}: WhatsAppBookingButtonProps) {
  const message = encodeURIComponent(
    `Hi, I'd like to book an appointment for my pet at ${clinicName}.`
  );
  const href = `https://wa.me/${phone}?text=${message}`;

  return (
    <a
      href={href}
      target="_blank"
      rel="noopener noreferrer"
      data-testid="whatsapp-booking-button"
    >
      <Button
        size={size}
        className={`bg-[#25D366] text-white hover:bg-[#1DA851] transition-all duration-300 hover:shadow-lg hover:shadow-[#25D366]/25 hover:scale-[1.02] ${className ?? ""}`}
      >
        <MessageCircle className="h-5 w-5 ltr:mr-2 rtl:ml-2" aria-hidden="true" />
        {label}
      </Button>
    </a>
  );
}
