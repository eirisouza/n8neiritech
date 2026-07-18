import {
  Boxes,
  House,
  Inbox,
  MessageCircleMore,
  Settings,
  ShoppingCart,
  Users,
  X,
} from 'lucide-react'
import { Link, useLocation } from 'react-router-dom'

const navigation = [
  { label: 'Dashboard', path: '/', icon: House },
  { label: 'Inbox', path: '/inbox', icon: Inbox },
  { label: 'Produtos', path: '/products', icon: Boxes },
  { label: 'Pedidos', path: '/orders', icon: ShoppingCart },
  { label: 'Clientes', path: '/customers', icon: Users },
  { label: 'Configurações', path: '/settings', icon: Settings },
  { label: 'WhatsApp', path: '/whatsapp', icon: MessageCircleMore },
]

interface SidebarProps {
  open: boolean
  onClose: () => void
}

export const Sidebar = ({ open, onClose }: SidebarProps) => {
  const location = useLocation()

  return (
    <>
      <div className={`fixed inset-0 z-30 bg-slate-950/40 lg:hidden ${open ? 'block' : 'hidden'}`} onClick={onClose} role="presentation" />
      <aside className={`fixed inset-y-0 left-0 z-40 flex w-72 flex-col border-r border-slate-200 bg-slate-950 px-5 py-6 text-white shadow-2xl transition-transform lg:translate-x-0 ${open ? 'translate-x-0' : '-translate-x-full'}`}>
        <div className="mb-8 flex items-center justify-between">
          <div>
            <p className="text-xs uppercase tracking-[0.3em] text-indigo-300">NeiriTech</p>
            <h1 className="text-2xl font-semibold">Commerce Admin</h1>
          </div>
          <button className="rounded-full p-2 text-slate-300 hover:bg-slate-800 lg:hidden" onClick={onClose} type="button">
            <X className="h-5 w-5" />
          </button>
        </div>

        <nav className="space-y-1.5">
          {navigation.map((item) => {
            const active = location.pathname === item.path
            const Icon = item.icon
            return (
              <Link
                key={item.path}
                className={`flex items-center gap-3 rounded-2xl px-4 py-3 text-sm font-medium transition ${active ? 'bg-indigo-600 text-white shadow-lg shadow-indigo-600/20' : 'text-slate-300 hover:bg-slate-900 hover:text-white'}`}
                onClick={onClose}
                to={item.path}
              >
                <Icon className="h-5 w-5" />
                {item.label}
              </Link>
            )
          })}
        </nav>

        <div className="mt-auto rounded-3xl bg-slate-900 p-4 text-sm text-slate-300">
          <p className="font-medium text-white">Automação operacional</p>
          <p className="mt-1">Acompanhe atendimento, catálogo e desempenho do WhatsApp em um só lugar.</p>
        </div>
      </aside>
    </>
  )
}
