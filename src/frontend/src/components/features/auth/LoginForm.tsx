"use client"

import { useState, useEffect } from "react"
import { useRouter } from "next/navigation"
import { useForm } from "react-hook-form"
import { z } from "zod"
import { zodResolver } from "@hookform/resolvers/zod"
import {
  Card,
  CardContent,
  CardDescription,
  CardHeader,
  CardTitle,
} from "@/components/ui/card"
import { Button } from "@/components/ui/button"
import { Input } from "@/components/ui/input"
import { Label } from "@/components/ui/label"
import { login, type LoginError } from "@/lib/api/auth"
import { trackEvent, AnalyticsEvents } from "@/lib/analytics"
import { useTranslations, useLocale } from "next-intl"

function buildLoginSchema(t: (key: string) => string) {
  return z.object({
    email: z.string().email(t("errors.email_invalid")),
    password: z.string().min(1, t("errors.password_required")),
  })
}

type LoginFormValues = {
  email: string
  password: string
}

export function LoginForm() {
  const router = useRouter()
  const t = useTranslations("auth.login")
  const locale = useLocale()
  const [serverError, setServerError] = useState<string | null>(null)

  const loginSchema = buildLoginSchema(t)

  const {
    register,
    handleSubmit,
    formState: { errors, isSubmitting },
  } = useForm<LoginFormValues>({
    resolver: zodResolver(loginSchema),
  })

  const onSubmit = async (data: LoginFormValues) => {
    setServerError(null)
    try {
      await login(data.email, data.password)
      router.push(`/${locale}/appointments`)
    } catch (err) {
      const loginErr = err as LoginError
      if (loginErr.code === "ACCOUNT_LOCKED") {
        setServerError(t("errors.account_locked"))
      } else if (loginErr.code === "INVALID_CREDENTIALS") {
        setServerError(t("errors.invalid_credentials"))
      } else {
        setServerError(t("errors.connection_error"))
      }
    }
  }

  const emailError = errors.email?.message
  const passwordError = errors.password?.message

  useEffect(() => {
    if (emailError) {
      trackEvent(AnalyticsEvents.FORM_VALIDATION_ERROR, {
        form_name: "login",
        field_name: "email",
        error_type: emailError,
      })
    }
  }, [emailError])

  useEffect(() => {
    if (passwordError) {
      trackEvent(AnalyticsEvents.FORM_VALIDATION_ERROR, {
        form_name: "login",
        field_name: "password",
        error_type: passwordError,
      })
    }
  }, [passwordError])

  const displayError = emailError || passwordError || serverError

  return (
    <Card data-testid="login-card">
      <CardHeader className="text-center">
        <CardTitle className="text-2xl">Vetolib</CardTitle>
        <CardDescription>{t("veterinary_management")}</CardDescription>
      </CardHeader>
      <CardContent>
        <form onSubmit={handleSubmit(onSubmit)} className="space-y-4" noValidate>
          <div className="space-y-2">
            <Label htmlFor="email">{t("email")}</Label>
            <Input
              id="email"
              type="email"
              placeholder={t("email_placeholder")}
              data-testid="email-input"
              disabled={isSubmitting}
              aria-invalid={!!errors.email}
              {...register("email")}
            />
          </div>
          <div className="space-y-2">
            <Label htmlFor="password">{t("password")}</Label>
            <Input
              id="password"
              type="password"
              placeholder={t("password_placeholder")}
              data-testid="password-input"
              disabled={isSubmitting}
              aria-invalid={!!errors.password}
              {...register("password")}
            />
          </div>
          <Button
            type="submit"
            className="w-full"
            data-testid="signin-button"
            disabled={isSubmitting}
          >
            {isSubmitting ? t("signing_in") : t("submit")}
          </Button>
          {displayError && (
            <p
              className="text-sm text-destructive text-center"
              data-testid="error-message"
              role="alert"
            >
              {displayError}
            </p>
          )}
        </form>
      </CardContent>
    </Card>
  )
}
