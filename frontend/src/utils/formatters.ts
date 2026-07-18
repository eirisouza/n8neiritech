import { format } from 'date-fns'
import { ptBR } from 'date-fns/locale'
import { STATUS_COLORS, STATUS_LABELS } from './constants'

export const formatCurrency = (value: number) =>
  new Intl.NumberFormat('pt-BR', {
    style: 'currency',
    currency: 'BRL',
  }).format(value ?? 0)

export const formatPhone = (phone: string) => {
  const digits = phone.replace(/\D/g, '')
  if (digits.length === 11) {
    return digits.replace(/(\d{2})(\d{5})(\d{4})/, '($1) $2-$3')
  }
  if (digits.length === 10) {
    return digits.replace(/(\d{2})(\d{4})(\d{4})/, '($1) $2-$3')
  }
  return phone
}

export const formatDate = (date: string) => format(new Date(date), 'dd/MM/yyyy', { locale: ptBR })

export const formatDateTime = (date: string) =>
  format(new Date(date), 'dd/MM/yyyy HH:mm', { locale: ptBR })

export const getStatusColor = (status: string) => STATUS_COLORS[status] ?? 'bg-slate-100 text-slate-700'

export const getStatusLabel = (status: string) => STATUS_LABELS[status] ?? status
