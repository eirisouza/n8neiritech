import { forwardRef, type SelectHTMLAttributes } from 'react'

interface SelectOption {
  label: string
  value: string
}

interface SelectProps extends SelectHTMLAttributes<HTMLSelectElement> {
  label?: string
  error?: string
  options: SelectOption[]
}

export const Select = forwardRef<HTMLSelectElement, SelectProps>(function Select(
  { label, error, options, className = '', id, ...props },
  ref,
) {
  return (
    <label className="flex w-full flex-col gap-1.5" htmlFor={id}>
      {label ? <span className="text-sm font-medium text-slate-700">{label}</span> : null}
      <select
        ref={ref}
        id={id}
        className={`h-11 rounded-xl border border-slate-200 bg-white px-3 text-sm text-slate-900 shadow-sm outline-none transition focus:border-indigo-500 focus:ring-4 focus:ring-indigo-100 ${error ? 'border-rose-300 focus:border-rose-500 focus:ring-rose-100' : ''} ${className}`.trim()}
        {...props}
      >
        {options.map((option) => (
          <option key={option.value} value={option.value}>
            {option.label}
          </option>
        ))}
      </select>
      {error ? <span className="text-xs text-rose-600">{error}</span> : null}
    </label>
  )
})
