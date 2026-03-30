"use client"

import { useState } from "react"
import { useFormShake } from "@/hooks/use-form-shake"
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
import { toast } from "sonner"
import { registerClinic, type RegisterError } from "@/lib/api/auth"
import { useTranslations, useLocale } from "next-intl"
import { LanguageSwitcher } from "./LanguageSwitcher"

function buildSignupSchema(t: (key: string) => string) {
  return z
    .object({
      clinicName: z.string().min(1, t("errors.clinic_name_required")),
      email: z.string().email(t("errors.email_required")),
      phone: z.string().min(1, t("errors.phone_required")),
      password: z
        .string()
        .min(8, t("errors.password_min"))
        .regex(/[A-Z]/, t("errors.password_uppercase"))
        .regex(/[0-9]/, t("errors.password_number")),
      confirmPassword: z.string(),
    })
    .refine((data) => data.password === data.confirmPassword, {
      message: t("errors.passwords_no_match"),
      path: ["confirmPassword"],
    })
}

type SignupFormValues = {
  clinicName: string
  email: string
  phone: string
  password: string
  confirmPassword: string
}

export function SignupForm() {
  const t = useTranslations("auth.signup")
  const locale = useLocale()
  const router = useRouter()
  const [serverError, setServerError] = useState<string | null>(null)
  const [showPassword, setShowPassword] = useState(false)
  const [showConfirmPassword, setShowConfirmPassword] = useState(false)
  const { shakeForm, triggerShake } = useFormShake()

  const signupSchema = buildSignupSchema(t)

  const {
    register,
    handleSubmit,
    watch,
    formState: { errors, isSubmitting },
  } = useForm<SignupFormValues>({
    resolver: zodResolver(signupSchema),
  })

  // eslint-disable-next-line react-hooks/incompatible-library
  const passwordValue = watch("password", "")

  const passwordStrength = {
    hasMin: passwordValue.length >= 8,
    hasUpper: /[A-Z]/.test(passwordValue),
    hasNumber: /[0-9]/.test(passwordValue),
  }

  const strengthScore = [
    passwordStrength.hasMin,
    passwordStrength.hasUpper,
    passwordStrength.hasNumber,
  ].filter(Boolean).length

  const onSubmit = async (data: SignupFormValues) => {
    setServerError(null)
    try {
      await registerClinic({
        clinicName: data.clinicName,
        email: data.email,
        password: data.password,
        phone: data.phone,
      })
      toast.success(t("welcome_toast"))
      router.push(`/${locale}/dashboard`)
    } catch (err) {
      const regErr = err as RegisterError
      if (regErr.code === "EMAIL_TAKEN") {
        setServerError(t("errors.email_taken"))
      } else {
        setServerError(t("errors.connection_error"))
      }
      // Trigger shake animation on error
      triggerShake()
    }
  }

  const handleInvalidSubmit = () => {
    triggerShake()
  }

  return (
    <div className="space-y-4 animate-auth-card-in">
      <div className="flex justify-end">
        <LanguageSwitcher />
      </div>
      <Card data-testid="signup-card" className="shadow-lg">
        <CardHeader className="text-center">
          <h1 className="sr-only">{t("heading")}</h1>
          <CardTitle className="text-2xl font-bold tracking-tight">Vetara</CardTitle>
          <CardDescription>{t("subtitle")}</CardDescription>
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
            onSubmit={handleSubmit(onSubmit, handleInvalidSubmit)}
            className={`space-y-4 ${shakeForm ? "animate-auth-shake" : ""}`}
            noValidate
          >
            {/* Clinic Name */}
            <div className="space-y-2">
              <Label htmlFor="clinicName" className="transition-colors duration-200">{t("clinic_name")}</Label>
              <Input
                id="clinicName"
                type="text"
                placeholder={t("clinic_name_placeholder")}
                data-testid="clinic-name-input"
                disabled={isSubmitting}
                aria-invalid={!!errors.clinicName}
                aria-describedby={errors.clinicName ? "clinicName-error" : undefined}
                className={`transition-all duration-200 ease-in-out focus:scale-[1.01] ${errors.clinicName ? "border-red-500 focus-visible:ring-red-500" : ""}`}
                {...register("clinicName")}
              />
              {errors.clinicName && (
                <p
                  id="clinicName-error"
                  className="text-sm text-red-600 animate-auth-error-slide"
                  data-testid="clinic-name-error"
                  role="alert"
                >
                  {errors.clinicName.message}
                </p>
              )}
            </div>

            {/* Email */}
            <div className="space-y-2">
              <Label htmlFor="email" className="transition-colors duration-200">{t("email")}</Label>
              <Input
                id="email"
                type="email"
                placeholder={t("email_placeholder")}
                data-testid="email-input"
                disabled={isSubmitting}
                aria-invalid={!!errors.email}
                aria-describedby={errors.email ? "email-error" : undefined}
                className={`transition-all duration-200 ease-in-out focus:scale-[1.01] ${errors.email ? "border-red-500 focus-visible:ring-red-500" : ""}`}
                {...register("email")}
              />
              {errors.email && (
                <p
                  id="email-error"
                  className="text-sm text-red-600 animate-auth-error-slide"
                  data-testid="email-error"
                  role="alert"
                >
                  {errors.email.message}
                </p>
              )}
            </div>

            {/* Phone */}
            <div className="space-y-2">
              <Label htmlFor="phone" className="transition-colors duration-200">{t("phone")}</Label>
              <Input
                id="phone"
                type="tel"
                placeholder={t("phone_placeholder")}
                data-testid="phone-input"
                disabled={isSubmitting}
                aria-invalid={!!errors.phone}
                aria-describedby={errors.phone ? "phone-error" : undefined}
                className={`transition-all duration-200 ease-in-out focus:scale-[1.01] ${errors.phone ? "border-red-500 focus-visible:ring-red-500" : ""}`}
                {...register("phone")}
              />
              {errors.phone && (
                <p
                  id="phone-error"
                  className="text-sm text-red-600 animate-auth-error-slide"
                  data-testid="phone-error"
                  role="alert"
                >
                  {errors.phone.message}
                </p>
              )}
            </div>

            {/* Password */}
            <div className="space-y-2">
              <Label htmlFor="password" className="transition-colors duration-200">{t("password")}</Label>
              <div className="relative">
                <Input
                  id="password"
                  type={showPassword ? "text" : "password"}
                  placeholder={t("password_placeholder")}
                  data-testid="password-input"
                  disabled={isSubmitting}
                  aria-invalid={!!errors.password}
                  aria-describedby={errors.password ? "password-error" : undefined}
                  className={`pe-10 transition-all duration-200 ease-in-out focus:scale-[1.01] ${errors.password ? "border-red-500 focus-visible:ring-red-500" : ""}`}
                  {...register("password")}
                />
                <button
                  type="button"
                  onClick={() => setShowPassword(!showPassword)}
                  className="absolute right-3 top-1/2 -translate-y-1/2 text-muted-foreground transition-all duration-200 hover:text-foreground hover:scale-110 active:scale-95 focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-primary"
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
              {/* Password strength indicator */}
              {passwordValue.length > 0 && (
                <div data-testid="password-strength" className="animate-auth-error-slide">
                  {/* Strength bar */}
                  <div className="mb-2 flex gap-1">
                    {[1, 2, 3].map((level) => (
                      <div
                        key={level}
                        className={`h-1.5 flex-1 rounded-full transition-all duration-300 ${
                          strengthScore >= level
                            ? strengthScore === 1
                              ? "bg-red-400"
                              : strengthScore === 2
                                ? "bg-yellow-400"
                                : "bg-primary"
                            : "bg-stone-200"
                        }`}
                        data-testid={`strength-bar-${level}`}
                      />
                    ))}
                  </div>
                  <ul className="space-y-1 text-xs">
                    <li
                      className={`transition-colors duration-200 ${passwordStrength.hasMin ? "text-primary/85" : "text-stone-500"}`}
                      data-testid="strength-min"
                    >
                      {passwordStrength.hasMin ? "\u2713" : "\u25CB"} {t("strength_min")}
                    </li>
                    <li
                      className={`transition-colors duration-200 ${passwordStrength.hasUpper ? "text-primary/85" : "text-stone-500"}`}
                      data-testid="strength-upper"
                    >
                      {passwordStrength.hasUpper ? "\u2713" : "\u25CB"} {t("strength_upper")}
                    </li>
                    <li
                      className={`transition-colors duration-200 ${passwordStrength.hasNumber ? "text-primary/85" : "text-stone-500"}`}
                      data-testid="strength-number"
                    >
                      {passwordStrength.hasNumber ? "\u2713" : "\u25CB"} {t("strength_number")}
                    </li>
                  </ul>
                </div>
              )}
              {errors.password && (
                <p
                  id="password-error"
                  className="text-sm text-red-600 animate-auth-error-slide"
                  data-testid="password-error"
                  role="alert"
                >
                  {errors.password.message}
                </p>
              )}
            </div>

            {/* Confirm Password */}
            <div className="space-y-2">
              <Label htmlFor="confirmPassword" className="transition-colors duration-200">{t("confirm_password")}</Label>
              <div className="relative">
                <Input
                  id="confirmPassword"
                  type={showConfirmPassword ? "text" : "password"}
                  placeholder={t("confirm_password_placeholder")}
                  data-testid="confirm-password-input"
                  disabled={isSubmitting}
                  aria-invalid={!!errors.confirmPassword}
                  aria-describedby={errors.confirmPassword ? "confirmPassword-error" : undefined}
                  className={`pe-10 transition-all duration-200 ease-in-out focus:scale-[1.01] ${errors.confirmPassword ? "border-red-500 focus-visible:ring-red-500" : ""}`}
                  {...register("confirmPassword")}
                />
                <button
                  type="button"
                  onClick={() => setShowConfirmPassword(!showConfirmPassword)}
                  className="absolute right-3 top-1/2 -translate-y-1/2 text-muted-foreground transition-all duration-200 hover:text-foreground hover:scale-110 active:scale-95 focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-primary"
                  data-testid="confirm-password-toggle"
                  aria-label={showConfirmPassword ? t("hide_password") : t("show_password")}
                  aria-pressed={showConfirmPassword}
                >
                  <span className="inline-block transition-transform duration-200">
                    {showConfirmPassword ? (
                      <EyeOff className="h-4 w-4" />
                    ) : (
                      <Eye className="h-4 w-4" />
                    )}
                  </span>
                </button>
              </div>
              {errors.confirmPassword && (
                <p
                  id="confirmPassword-error"
                  className="text-sm text-red-600 animate-auth-error-slide"
                  data-testid="confirm-password-error"
                  role="alert"
                >
                  {errors.confirmPassword.message}
                </p>
              )}
            </div>

            {/* Submit */}
            <Button
              type="submit"
              className="w-full bg-primary text-white hover:bg-primary/90 transition-all duration-200 hover:scale-[1.02] active:scale-[0.98] disabled:hover:scale-100"
              data-testid="signup-submit-button"
              disabled={isSubmitting}
            >
              {isSubmitting ? (
                <>
                  <Loader2 className="h-4 w-4 animate-spin" />
                  <span className="ms-2">{t("submitting")}</span>
                </>
              ) : (
                t("submit")
              )}
            </Button>
          </form>

          {/* Link to login */}
          <p className="mt-4 text-center text-sm text-stone-500">
            {t("already_have_account")}{" "}
            <Link
              href={`/${locale}/login`}
              className="auth-link-underline font-medium text-primary transition-colors duration-200 hover:text-primary/90"
              data-testid="signin-link"
            >
              {t("sign_in")}
            </Link>
          </p>
        </CardContent>
      </Card>
    </div>
  )
}
