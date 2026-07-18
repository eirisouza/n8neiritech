import type { LucideIcon } from 'lucide-react'

interface EmptyStateProps {
  icon: LucideIcon
  title: string
  description: string
}

export const EmptyState = ({ icon: Icon, title, description }: EmptyStateProps) => (
  <div className="flex flex-col items-center justify-center rounded-3xl border border-dashed border-slate-200 bg-slate-50 px-6 py-12 text-center">
    <div className="mb-4 rounded-2xl bg-indigo-50 p-4 text-indigo-600">
      <Icon className="h-8 w-8" />
    </div>
    <h3 className="text-lg font-semibold text-slate-900">{title}</h3>
    <p className="mt-2 max-w-md text-sm text-slate-500">{description}</p>
  </div>
)
