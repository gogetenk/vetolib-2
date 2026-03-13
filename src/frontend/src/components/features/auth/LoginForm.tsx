"use client"

import { useState, useEffect, useRef } from "react"
import { useRouter } from "next/navigation"
import { useForm } from "react-hook-form"
import { z } from "zod"
import { zodResolver } from "@hookform/resolvers/zod"
import Link from "next/link"
import { Eye, EyeOff, Loader2 } from "lucide-react"
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
import { LanguageSwitcher } from "./LanguageSwitcher"

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
  const [showPassword, setShowPassword] = useState(false)
  const [shakeForm, setShakeForm] = useState(false)
  const formRef = useRef<HTMLFormElement>(null)

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
      // Trigger shake animation on error
      setShakeForm(true)
      setTimeout(() => setShakeForm(false), 500)
    }
  }

  const handleInvalidSubmit = () => {
    setShakeForm(true)
    setTimeout(() => setShakeForm(false), 500)
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

  return (
    <div className="space-y-4 animate-auth-card-in">
      <div className="flex justify-end">
        <LanguageSwitcher />
      </div>
      <Card
        data-testid="login-card"
        className="shadow-lg"
      >
        <CardHeader className="text-center">
          <CardTitle className="text-2xl font-bold tracking-tight">Vetolib</CardTitle>
          <CardDescription className="text-muted-foreground">{t("veterinary_management")}</CardDescription>
        </CardHeader>
        <CardContent>
          {/* Server error displayed above the form */}
          {serverError && (
            <div
              className="mb-4 rounded-md border border-red-200 bg-red-50 px-4 py-3 text-sm text-red-700 animate-auth-error-slide"
              data-testid="server-error"
              role="alert"
            >
              {serverError}
            </div>
          )}
          <form
            ref={formRef}
            onSubmit={handleSubmit(onSubmit, handleInvalidSubmit)}
            className={`space-y-4 ${shakeForm ? "animate-auth-shake" : ""}`}
            noValidate
          >
            <div className="space-y-2">
              <Label htmlFor="email" className="transition-colors duration-200">{t("email")}</Label>
              <Input
                id="email"
                type="email"
                placeholder={t("email_placeholder")}
                data-testid="email-input"
                disabled={isSubmitting}
                aria-invalid={!!errors.email}
                className={`transition-all duration-200 ease-in-out focus:scale-[1.01] ${errors.email ? "border-red-500 focus-visible:ring-red-500" : ""}`}
                {...register("email")}
              />
              {errors.email && (
                <p
                  className="text-sm text-red-600 animate-auth-error-slide"
                  data-testid="email-error"
                  role="alert"
                >
                  {errors.email.message}
                </p>
              )}
            </div>
            <div className="space-y-2">
              <div className="flex items-center justify-between">
                <Label htmlFor="password" className="transition-colors duration-200">{t("password")}</Label>
                <Link
                  href={`/${locale}/forgot-password`}
                  className="auth-link-underline text-xs text-emerald-700 transition-colors duration-200 hover:text-emerald-800"
                  data-testid="forgot-password-link"
                >
                  {t("forgot_password")}
                </Link>
              </div>
              <div className="relative">
                <Input
                  id="password"
                  type={showPassword ? "text" : "password"}
                  placeholder={t("password_placeholder")}
                  data-testid="password-input"
                  disabled={isSubmitting}
                  aria-invalid={!!errors.password}
                  className={`pr-10 transition-all duration-200 ease-in-out focus:scale-[1.01] ${errors.password ? "border-red-500 focus-visible:ring-red-500" : ""}`}
                  {...register("password")}
                />
                <button
                  type="button"
                  onClick={() => setShowPassword(!showPassword)}
                  className="absolute right-3 top-1/2 -translate-y-1/2 text-gray-400 transition-all duration-200 hover:text-gray-600 hover:scale-110 active:scale-95 focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-emerald-500"
                  data-testid="password-toggle"
                  aria-label={showPassword ? t("hide_password") : t("show_password")}
                  aria-pressed={showPassword}
                >
                  <span className="inline-block transition-transform duration-200">
                    {showPassword ? (
                      <EyeOff className="h-4 w-4" />
                    ) : (
                      <Eye className="h-4 w-4" />
                    )}
                  </span>
                </button>
              </div>
              {errors.password && (
                <p
                  className="text-sm text-red-600 animate-auth-error-slide"
                  data-testid="password-error"
                  role="alert"
                >
                  {errors.password.message}
                </p>
              )}
            </div>
            <Button
              type="submit"
              className="w-full bg-emerald-700 text-white hover:bg-emerald-800 transition-all duration-200 hover:scale-[1.02] active:scale-[0.98] disabled:hover:scale-100"
              data-testid="signin-button"
              disabled={isSubmitting}
            >
              {isSubmitting ? (
                <>
                  <Loader2 className="h-4 w-4 animate-spin" />
                  <span className="ml-2">{t("signing_in")}</span>
                </>
              ) : (
                t("submit")
              )}
            </Button>
            {/* Removed old single displayError — now field-level + server error above */}
          </form>
          <p className="mt-4 text-center text-sm text-gray-500">
            {t("no_account")}{" "}
            <Link
              href={`/${locale}/signup`}
              className="auth-link-underline font-medium text-emerald-700 transition-colors duration-200 hover:text-emerald-800"
              data-testid="signup-link"
            >
              {t("sign_up_link")}
            </Link>
          </p>
        </CardContent>
      </Card>
    </div>
  )
}
