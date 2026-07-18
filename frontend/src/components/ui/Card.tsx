import type { ReactNode } from 'react'

interface CardProps {
  title?: string
  description?: string
  action?: ReactNode
  children: ReactNode
  className?: string
}

export const Card = ({ title, description, action, children, className = '' }: CardProps) => (
  <section className={`rounded-3xl border border-slate-200 bg-white p-5 shadow-sm ${className}`.trim()}>
    {title || action ? (
      <div className="mb-4 flex items-start justify-between gap-4">
        <div>
          {title ? <h2 className="text-lg font-semibold text-slate-900">{title}</h2> : null}
          {description ? <p className="mt-1 text-sm text-slate-500">{description}</p> : null}
        </div>
        {action}
      </div>
    ) : null}
    {children}
  </section>
)
