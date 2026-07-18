import type { ReactNode } from 'react'

interface Column<T> {
  key: string
  header: string
  className?: string
  render: (row: T) => ReactNode
}

interface TableProps<T> {
  columns: Column<T>[]
  data: T[]
  rowKey: (row: T) => string
  emptyState?: ReactNode
}

export const Table = <T,>({ columns, data, rowKey, emptyState }: TableProps<T>) => (
  <div className="overflow-hidden rounded-2xl border border-slate-200 bg-white shadow-sm">
    <div className="overflow-x-auto">
      <table className="min-w-full divide-y divide-slate-200 text-left text-sm">
        <thead className="bg-slate-50 text-slate-500">
          <tr>
            {columns.map((column) => (
              <th key={column.key} className={`px-4 py-3 font-medium ${column.className ?? ''}`.trim()}>
                {column.header}
              </th>
            ))}
          </tr>
        </thead>
        <tbody className="divide-y divide-slate-100">
          {data.length > 0
            ? data.map((row) => (
                <tr key={rowKey(row)} className="transition hover:bg-slate-50/80">
                  {columns.map((column) => (
                    <td key={column.key} className={`px-4 py-3 align-top text-slate-700 ${column.className ?? ''}`.trim()}>
                      {column.render(row)}
                    </td>
                  ))}
                </tr>
              ))
            : (
              <tr>
                <td className="px-4 py-10 text-center text-slate-500" colSpan={columns.length}>
                  {emptyState ?? 'Nenhum registro encontrado.'}
                </td>
              </tr>
            )}
        </tbody>
      </table>
    </div>
  </div>
)
