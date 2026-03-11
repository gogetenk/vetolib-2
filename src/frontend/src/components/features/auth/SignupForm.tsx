"use client"

import { useState } from "react"
import { useRouter } from "next/navigation"
import { useForm } from "react-hook-form"
import { z } from "zod"
import { zodResolver } from "@hookform/resolvers/zod"
import Link from "next/link"
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
    }
  }

  return (
    <Card data-testid="signup-card">
      <CardHeader className="text-center">
        <CardTitle className="text-2xl">Vetolib</CardTitle>
        <CardDescription>{t("subtitle")}</CardDescription>
      </CardHeader>
      <CardContent>
        <form onSubmit={handleSubmit(onSubmit)} className="space-y-4" noValidate>
          {/* Clinic Name */}
          <div className="space-y-2">
            <Label htmlFor="clinicName">{t("clinic_name")}</Label>
            <Input
              id="clinicName"
              type="text"
              placeholder={t("clinic_name_placeholder")}
              data-testid="clinic-name-input"
              disabled={isSubmitting}
              aria-invalid={!!errors.clinicName}
              {...register("clinicName")}
            />
            {errors.clinicName && (
              <p
                className="text-sm text-destructive"
                data-testid="clinic-name-error"
                role="alert"
              >
                {errors.clinicName.message}
              </p>
            )}
          </div>

          {/* Email */}
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
            {errors.email && (
              <p
                className="text-sm text-destructive"
                data-testid="email-error"
                role="alert"
              >
                {errors.email.message}
              </p>
            )}
          </div>

          {/* Phone */}
          <div className="space-y-2">
            <Label htmlFor="phone">{t("phone")}</Label>
            <Input
              id="phone"
              type="tel"
              placeholder={t("phone_placeholder")}
              data-testid="phone-input"
              disabled={isSubmitting}
              aria-invalid={!!errors.phone}
              {...register("phone")}
            />
            {errors.phone && (
              <p
                className="text-sm text-destructive"
                data-testid="phone-error"
                role="alert"
              >
                {errors.phone.message}
              </p>
            )}
          </div>

          {/* Password */}
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
            {/* Password strength hints */}
            {passwordValue.length > 0 && (
              <ul
                className="space-y-1 text-xs"
                data-testid="password-strength"
              >
                <li
                  className={passwordStrength.hasMin ? "text-emerald-600" : "text-gray-500"}
                  data-testid="strength-min"
                >
                  {passwordStrength.hasMin ? "✓" : "○"} {t("strength_min")}
                </li>
                <li
                  className={passwordStrength.hasUpper ? "text-emerald-600" : "text-gray-500"}
                  data-testid="strength-upper"
                >
                  {passwordStrength.hasUpper ? "✓" : "○"} {t("strength_upper")}
                </li>
                <li
                  className={passwordStrength.hasNumber ? "text-emerald-600" : "text-gray-500"}
                  data-testid="strength-number"
                >
                  {passwordStrength.hasNumber ? "✓" : "○"} {t("strength_number")}
                </li>
              </ul>
            )}
            {errors.password && (
              <p
                className="text-sm text-destructive"
                data-testid="password-error"
                role="alert"
              >
                {errors.password.message}
              </p>
            )}
          </div>

          {/* Confirm Password */}
          <div className="space-y-2">
            <Label htmlFor="confirmPassword">{t("confirm_password")}</Label>
            <Input
              id="confirmPassword"
              type="password"
              placeholder={t("confirm_password_placeholder")}
              data-testid="confirm-password-input"
              disabled={isSubmitting}
              aria-invalid={!!errors.confirmPassword}
              {...register("confirmPassword")}
            />
            {errors.confirmPassword && (
              <p
                className="text-sm text-destructive"
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
            className="w-full bg-emerald-700 text-white hover:bg-emerald-800"
            data-testid="signup-submit-button"
            disabled={isSubmitting}
          >
            {isSubmitting ? t("submitting") : t("submit")}
          </Button>

          {/* Server error */}
          {serverError && (
            <p
              className="text-sm text-destructive text-center"
              data-testid="server-error"
              role="alert"
            >
              {serverError}
            </p>
          )}

          {/* Link to login */}
          <p className="text-center text-sm text-gray-500">
            {t("already_have_account")}{" "}
            <Link
              href={`/${locale}/login`}
              className="font-medium text-emerald-700 hover:underline"
              data-testid="signin-link"
            >
              {t("sign_in")}
            </Link>
          </p>
        </form>
      </CardContent>
    </Card>
  )
}
