import { ChevronLeft, ChevronRight } from 'lucide-react'
import { Button } from './Button'

interface PaginationProps {
  page: number
  totalPages: number
  onPageChange: (page: number) => void
}

export const Pagination = ({ page, totalPages, onPageChange }: PaginationProps) => {
  if (totalPages <= 1) return null

  const pages = Array.from({ length: totalPages }, (_, index) => index + 1).slice(
    Math.max(0, page - 3),
    Math.min(totalPages, page + 2),
  )

  return (
    <div className="flex flex-wrap items-center justify-between gap-3">
      <p className="text-sm text-slate-500">Página {page} de {totalPages}</p>
      <div className="flex items-center gap-2">
        <Button variant="secondary" size="sm" disabled={page === 1} onClick={() => onPageChange(page - 1)}>
          <ChevronLeft className="h-4 w-4" />
          Anterior
        </Button>
        <div className="flex items-center gap-1">
          {pages.map((value) => (
            <button
              key={value}
              className={`h-9 min-w-9 rounded-lg px-3 text-sm font-medium transition ${value === page ? 'bg-indigo-600 text-white' : 'text-slate-600 hover:bg-slate-100'}`}
              onClick={() => onPageChange(value)}
              type="button"
            >
              {value}
            </button>
          ))}
        </div>
        <Button variant="secondary" size="sm" disabled={page === totalPages} onClick={() => onPageChange(page + 1)}>
          Próxima
          <ChevronRight className="h-4 w-4" />
        </Button>
      </div>
    </div>
  )
}
