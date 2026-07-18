import { Loader2 } from 'lucide-react'

export const Spinner = ({ className = 'h-8 w-8' }: { className?: string }) => (
  <Loader2 className={`${className} animate-spin text-indigo-600`.trim()} />
)
