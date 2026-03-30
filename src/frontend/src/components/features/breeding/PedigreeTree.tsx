'use client'

import type { PedigreeNodeDto } from '@/lib/api/breeding'

interface PedigreeTreeProps {
  node: PedigreeNodeDto
}

function PedigreeNode({ node, depth = 0 }: { node: PedigreeNodeDto; depth?: number }) {
  const sexIcon = node.sex === 'Male' ? '\u2642' : node.sex === 'Female' ? '\u2640' : '\u26A5'
  const bgColor = depth === 0 ? 'bg-primary/10 border-primary/30' : depth === 1 ? 'bg-blue-50 border-blue-200' : 'bg-muted border-border'

  return (
    <div className="flex flex-col items-center" data-testid={`pedigree-node-${node.id}`}>
      <button
        type="button"
        className={`${bgColor} border rounded-xl px-4 py-2.5 text-center min-w-[140px] hover:shadow-md transition-shadow cursor-pointer`}
        data-testid={`pedigree-node-btn-${node.id}`}
        aria-label={`${node.name} - ${node.species} ${node.breed || ''}`}
      >
        <p className="text-[13px] font-bold text-foreground">
          {sexIcon} {node.name}
        </p>
        <p className="text-[11px] text-muted-foreground">
          {node.breed || node.species}
        </p>
      </button>

      {(node.mother || node.father) && (
        <div className="flex gap-6 mt-4 relative">
          {/* Connector line */}
          <div className="absolute top-0 left-1/2 -translate-x-1/2 w-px h-4 bg-border" />

          <div className="flex gap-6 pt-4 relative">
            {/* Horizontal connector */}
            {node.mother && node.father && (
              <div className="absolute top-4 left-1/4 right-1/4 h-px bg-border" />
            )}

            {node.mother && (
              <div className="relative">
                <div className="absolute top-0 left-1/2 -translate-x-1/2 w-px h-4 bg-border" />
                <div className="pt-4">
                  <PedigreeNode node={node.mother} depth={depth + 1} />
                </div>
              </div>
            )}
            {node.father && (
              <div className="relative">
                <div className="absolute top-0 left-1/2 -translate-x-1/2 w-px h-4 bg-border" />
                <div className="pt-4">
                  <PedigreeNode node={node.father} depth={depth + 1} />
                </div>
              </div>
            )}
          </div>
        </div>
      )}
    </div>
  )
}

export function PedigreeTree({ node }: PedigreeTreeProps) {
  return (
    <div
      className="bg-card border border-border/80 rounded-xl shadow-sm p-6 overflow-x-auto"
      data-testid="pedigree-tree"
    >
      <div className="flex justify-center min-w-[320px] sm:min-w-[500px]">
        <PedigreeNode node={node} />
      </div>
    </div>
  )
}
