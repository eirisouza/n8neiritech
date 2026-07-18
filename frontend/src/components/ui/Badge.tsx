import type { ReactNode } from 'react'
import { getStatusColor } from '../../utils/formatters'

interface BadgeProps {
  children: ReactNode
  status?: string
  className?: string
}

export const Badge = ({ children, status, className = '' }: BadgeProps) => (
  <span className={`inline-flex items-center rounded-full px-2.5 py-1 text-xs font-semibold ${status ? getStatusColor(status) : 'bg-slate-100 text-slate-700'} ${className}`.trim()}>
    {children}
  </span>
)
