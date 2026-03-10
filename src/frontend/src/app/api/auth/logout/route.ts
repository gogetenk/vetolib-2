import { NextResponse } from 'next/server'

const EXPIRED_COOKIE = {
  httpOnly: true,
  secure: process.env.NODE_ENV === 'production',
  sameSite: 'strict' as const,
  path: '/',
  maxAge: 0,
}

export async function POST() {
  const response = NextResponse.json({ ok: true }, { status: 200 })

  response.cookies.set('access_token', '', EXPIRED_COOKIE)
  response.cookies.set('refresh_token', '', EXPIRED_COOKIE)
  response.cookies.set('user_info', '', { ...EXPIRED_COOKIE, httpOnly: false })

  return response
}
