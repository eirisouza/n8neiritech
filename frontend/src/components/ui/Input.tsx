import { forwardRef, type InputHTMLAttributes } from 'react'

interface InputProps extends InputHTMLAttributes<HTMLInputElement> {
  label?: string
  error?: string
  helperText?: string
}

export const Input = forwardRef<HTMLInputElement, InputProps>(function Input(
  { label, error, helperText, className = '', id, ...props },
  ref,
) {
  return (
    <label className="flex w-full flex-col gap-1.5" htmlFor={id}>
      {label ? <span className="text-sm font-medium text-slate-700">{label}</span> : null}
      <input
        ref={ref}
        id={id}
        className={`h-11 rounded-xl border border-slate-200 bg-white px-3 text-sm text-slate-900 shadow-sm outline-none transition placeholder:text-slate-400 focus:border-indigo-500 focus:ring-4 focus:ring-indigo-100 ${error ? 'border-rose-300 focus:border-rose-500 focus:ring-rose-100' : ''} ${className}`.trim()}
        {...props}
      />
      {error ? <span className="text-xs text-rose-600">{error}</span> : helperText ? <span className="text-xs text-slate-500">{helperText}</span> : null}
    </label>
  )
})
