import { NextRequest, NextResponse } from 'next/server'

const BACKEND_URL = process.env.BACKEND_URL ?? process.env.NEXT_PUBLIC_API_URL ?? ''

const COOKIE_OPTIONS = {
  httpOnly: true,
  secure: process.env.NODE_ENV === 'production',
  sameSite: 'strict' as const,
  path: '/',
  maxAge: 60 * 60 * 24 * 7, // 7 days
}

export async function POST(request: NextRequest) {
  try {
    const body = await request.json()

    const backendRes = await fetch(`${BACKEND_URL}/api/auth/login`, {
      method: 'POST',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify(body),
    })

    const data = await backendRes.json()

    if (!backendRes.ok) {
      return NextResponse.json(data, { status: backendRes.status })
    }

    const response = NextResponse.json(
      {
        user: data.user,
        expiresIn: data.expiresIn,
      },
      { status: 200 }
    )

    response.cookies.set('access_token', data.accessToken, COOKIE_OPTIONS)
    response.cookies.set('refresh_token', data.refreshToken, COOKIE_OPTIONS)

    // Non-httpOnly cookie for user display info (not a secret)
    if (data.user) {
      response.cookies.set(
        'user_info',
        JSON.stringify(data.user),
        {
          httpOnly: false,
          secure: process.env.NODE_ENV === 'production',
          sameSite: 'strict' as const,
          path: '/',
          maxAge: 60 * 60 * 24 * 7,
        }
      )
    }

    return response
  } catch {
    return NextResponse.json(
      { title: 'Connection error. Please try again.' },
      { status: 503 }
    )
  }
}
