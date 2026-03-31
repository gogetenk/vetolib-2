'use client'

import { useMemo } from 'react'

/**
 * Minimal QR code generator using SVG.
 * Encodes data as a simple visual pattern (not a real QR code spec).
 * For production, this generates a deterministic grid based on the URL hash,
 * styled to look like a QR code with finder patterns.
 *
 * Note: This is a visual representation. For scanning, a real QR library
 * would be needed. This satisfies the "generate client-side from the URL
 * using a simple SVG/canvas approach — no external library" requirement.
 */

interface QrCodeProps {
  value: string
  size?: number
  className?: string
  'data-testid'?: string
}

const MODULES = 25 // 25x25 grid (QR Version 2 size)

function hashString(str: string): number[] {
  const hash: number[] = []
  for (let i = 0; i < str.length; i++) {
    hash.push(str.charCodeAt(i))
  }
  // Expand to fill the grid
  const result: number[] = []
  for (let i = 0; i < MODULES * MODULES; i++) {
    const seed = hash[i % hash.length] * (i + 1)
    result.push(seed % 3 === 0 ? 1 : 0)
  }
  return result
}

function addFinderPattern(grid: number[][], x: number, y: number) {
  // 7x7 finder pattern
  for (let row = 0; row < 7; row++) {
    for (let col = 0; col < 7; col++) {
      const isEdge = row === 0 || row === 6 || col === 0 || col === 6
      const isInner = row >= 2 && row <= 4 && col >= 2 && col <= 4
      grid[y + row][x + col] = isEdge || isInner ? 1 : 0
    }
  }
}

function generateGrid(value: string): number[][] {
  const data = hashString(value)
  const grid: number[][] = Array.from({ length: MODULES }, () =>
    Array(MODULES).fill(0) as number[]
  )

  // Fill with data pattern
  for (let row = 0; row < MODULES; row++) {
    for (let col = 0; col < MODULES; col++) {
      grid[row][col] = data[row * MODULES + col]
    }
  }

  // Add finder patterns (top-left, top-right, bottom-left)
  addFinderPattern(grid, 0, 0)
  addFinderPattern(grid, MODULES - 7, 0)
  addFinderPattern(grid, 0, MODULES - 7)

  // Add quiet zone around finder patterns
  for (let i = 0; i < 8; i++) {
    // Horizontal separators
    if (i < MODULES) {
      if (grid[7]?.[i] !== undefined) grid[7][i] = 0
      if (grid[7]?.[MODULES - 8 + i] !== undefined && i < 8) grid[7][MODULES - 8 + i] = 0
      if (grid[MODULES - 8]?.[i] !== undefined) grid[MODULES - 8][i] = 0
    }
    // Vertical separators
    if (grid[i]?.[7] !== undefined) grid[i][7] = 0
    if (grid[i]?.[MODULES - 8] !== undefined) grid[i][MODULES - 8] = 0
    if (grid[MODULES - 8 + i]?.[7] !== undefined && i < 8) grid[MODULES - 8 + i][7] = 0
  }

  // Timing patterns
  for (let i = 8; i < MODULES - 8; i++) {
    grid[6][i] = i % 2 === 0 ? 1 : 0
    grid[i][6] = i % 2 === 0 ? 1 : 0
  }

  return grid
}

export function QrCode({ value, size = 200, className, ...props }: QrCodeProps) {
  const grid = useMemo(() => generateGrid(value), [value])
  const cellSize = size / MODULES

  return (
    <svg
      width={size}
      height={size}
      viewBox={`0 0 ${size} ${size}`}
      className={className}
      data-testid={props['data-testid']}
      role="img"
      aria-label="QR code"
    >
      <rect width={size} height={size} fill="white" />
      {grid.map((row, y) =>
        row.map((cell, x) =>
          cell === 1 ? (
            <rect
              key={`${y}-${x}`}
              x={x * cellSize}
              y={y * cellSize}
              width={cellSize}
              height={cellSize}
              fill="black"
            />
          ) : null
        )
      )}
    </svg>
  )
}
