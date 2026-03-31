"use client";

import { useState, useCallback } from "react";
import { CheckCircle2, Loader2 } from "lucide-react";
import { Input } from "@/components/ui/input";
import { Button } from "@/components/ui/button";
import { Label } from "@/components/ui/label";
import { Textarea } from "@/components/ui/textarea";
import { askVetToJoin } from "@/lib/api/pet-owners";

interface Props {
  messages: {
    vet_email_label: string;
    vet_email_placeholder: string;
    your_name_label: string;
    your_name_placeholder: string;
    pet_name_label: string;
    pet_name_placeholder: string;
    message_label: string;
    message_placeholder: string;
    submit: string;
    submitting: string;
    success_title: string;
    success_description: string;
  };
}

export function AskVetForm({ messages }: Props) {
  const [vetEmail, setVetEmail] = useState("");
  const [ownerName, setOwnerName] = useState("");
  const [petName, setPetName] = useState("");
  const [message, setMessage] = useState("");
  const [submitting, setSubmitting] = useState(false);
  const [success, setSuccess] = useState(false);

  const handleSubmit = useCallback(
    async (e: React.FormEvent) => {
      e.preventDefault();
      if (!vetEmail || !ownerName || !petName) return;

      setSubmitting(true);
      try {
        await askVetToJoin({
          vetEmail,
          ownerName,
          petName,
          message: message || undefined,
        });
        setSuccess(true);
      } catch {
        // Silently handle — in production this would show a toast
      } finally {
        setSubmitting(false);
      }
    },
    [vetEmail, ownerName, petName, message]
  );

  if (success) {
    return (
      <div
        data-testid="ask-vet-success"
        className="flex flex-col items-center gap-3 rounded-2xl border border-green-100 bg-green-50 p-8 text-center"
      >
        <CheckCircle2
          className="h-12 w-12 text-green-600"
          aria-hidden="true"
        />
        <h3 className="text-lg font-semibold text-stone-900">
          {messages.success_title}
        </h3>
        <p className="text-sm text-stone-600">
          {messages.success_description}
        </p>
      </div>
    );
  }

  return (
    <form
      data-testid="ask-vet-form"
      onSubmit={handleSubmit}
      className="space-y-4"
    >
      <div className="grid gap-4 sm:grid-cols-2">
        <div>
          <Label htmlFor="vet-email" data-testid="ask-vet-email-label">
            {messages.vet_email_label}
          </Label>
          <Input
            id="vet-email"
            data-testid="ask-vet-email-input"
            type="email"
            required
            placeholder={messages.vet_email_placeholder}
            value={vetEmail}
            onChange={(e) => setVetEmail(e.target.value)}
            className="mt-1"
          />
        </div>
        <div>
          <Label htmlFor="owner-name" data-testid="ask-vet-name-label">
            {messages.your_name_label}
          </Label>
          <Input
            id="owner-name"
            data-testid="ask-vet-name-input"
            type="text"
            required
            placeholder={messages.your_name_placeholder}
            value={ownerName}
            onChange={(e) => setOwnerName(e.target.value)}
            className="mt-1"
          />
        </div>
      </div>

      <div>
        <Label htmlFor="pet-name" data-testid="ask-vet-pet-name-label">
          {messages.pet_name_label}
        </Label>
        <Input
          id="pet-name"
          data-testid="ask-vet-pet-name-input"
          type="text"
          required
          placeholder={messages.pet_name_placeholder}
          value={petName}
          onChange={(e) => setPetName(e.target.value)}
          className="mt-1"
        />
      </div>

      <div>
        <Label htmlFor="ask-vet-message" data-testid="ask-vet-message-label">
          {messages.message_label}
        </Label>
        <Textarea
          id="ask-vet-message"
          data-testid="ask-vet-message-input"
          placeholder={messages.message_placeholder}
          value={message}
          onChange={(e) => setMessage(e.target.value)}
          className="mt-1"
          rows={3}
        />
      </div>

      <Button
        type="submit"
        data-testid="ask-vet-submit-button"
        disabled={submitting || !vetEmail || !ownerName || !petName}
        className="w-full bg-primary text-white hover:bg-primary/90 sm:w-auto"
      >
        {submitting ? (
          <>
            <Loader2 className="mr-2 h-4 w-4 animate-spin" aria-hidden="true" />
            {messages.submitting}
          </>
        ) : (
          messages.submit
        )}
      </Button>
    </form>
  );
}
