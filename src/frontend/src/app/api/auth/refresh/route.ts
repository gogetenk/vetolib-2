import { NextRequest, NextResponse } from 'next/server'

const BACKEND_URL = process.env.BACKEND_URL ?? process.env.NEXT_PUBLIC_API_URL ?? ''

const COOKIE_OPTIONS = {
  httpOnly: true,
  secure: process.env.NODE_ENV === 'production',
  sameSite: 'strict' as const,
  path: '/',
  maxAge: 60 * 60 * 24 * 7,
}

export async function POST(request: NextRequest) {
  try {
    const refreshToken = request.cookies.get('refresh_token')?.value

    if (!refreshToken) {
      return NextResponse.json(
        { title: 'No refresh token' },
        { status: 401 }
      )
    }

    const backendRes = await fetch(`${BACKEND_URL}/api/auth/refresh`, {
      method: 'POST',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify({ refreshToken }),
    })

    const data = await backendRes.json()

    if (!backendRes.ok) {
      return NextResponse.json(data, { status: backendRes.status })
    }

    const response = NextResponse.json({ expiresIn: data.expiresIn }, { status: 200 })

    response.cookies.set('access_token', data.accessToken, COOKIE_OPTIONS)
    if (data.refreshToken) {
      response.cookies.set('refresh_token', data.refreshToken, COOKIE_OPTIONS)
    }

    return response
  } catch {
    return NextResponse.json(
      { title: 'Connection error. Please try again.' },
      { status: 503 }
    )
  }
}
