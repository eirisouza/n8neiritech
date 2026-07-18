import { Search } from 'lucide-react'
import { useEffect, useState } from 'react'

interface SearchInputProps {
  placeholder?: string
  defaultValue?: string
  onSearch: (value: string) => void
  delay?: number
}

export const SearchInput = ({ placeholder = 'Buscar...', defaultValue = '', onSearch, delay = 400 }: SearchInputProps) => {
  const [value, setValue] = useState(defaultValue)

  useEffect(() => {
    setValue(defaultValue)
  }, [defaultValue])

  useEffect(() => {
    const timer = window.setTimeout(() => onSearch(value), delay)
    return () => window.clearTimeout(timer)
  }, [value, delay, onSearch])

  return (
    <label className="flex h-11 items-center gap-2 rounded-xl border border-slate-200 bg-white px-3 shadow-sm focus-within:border-indigo-500 focus-within:ring-4 focus-within:ring-indigo-100">
      <Search className="h-4 w-4 text-slate-400" />
      <input
        className="w-full border-none bg-transparent text-sm text-slate-800 outline-none placeholder:text-slate-400"
        onChange={(event) => setValue(event.target.value)}
        placeholder={placeholder}
        value={value}
      />
    </label>
  )
}
