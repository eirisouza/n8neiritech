import type { ReactNode } from 'react'
import { X } from 'lucide-react'

interface ModalProps {
  open: boolean
  title: string
  onClose: () => void
  children: ReactNode
  footer?: ReactNode
  widthClassName?: string
}

export const Modal = ({ open, title, onClose, children, footer, widthClassName = 'max-w-3xl' }: ModalProps) => {
  if (!open) return null

  return (
    <div className="fixed inset-0 z-50 flex items-center justify-center bg-slate-950/45 p-4" onClick={onClose} role="presentation">
      <div
        className={`max-h-[90vh] w-full overflow-hidden rounded-3xl bg-white shadow-2xl ${widthClassName}`}
        onClick={(event) => event.stopPropagation()}
        role="dialog"
        aria-modal="true"
      >
        <div className="flex items-center justify-between border-b border-slate-200 px-6 py-4">
          <h3 className="text-lg font-semibold text-slate-900">{title}</h3>
          <button className="rounded-full p-2 text-slate-500 transition hover:bg-slate-100" onClick={onClose} type="button">
            <X className="h-4 w-4" />
          </button>
        </div>
        <div className="max-h-[calc(90vh-140px)] overflow-y-auto px-6 py-5">{children}</div>
        {footer ? <div className="border-t border-slate-200 px-6 py-4">{footer}</div> : null}
      </div>
    </div>
  )
}
