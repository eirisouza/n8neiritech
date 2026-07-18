import { Bell, Menu, LogOut } from 'lucide-react'
import { Button } from '../ui/Button'
import { useAuth } from '../../hooks/useAuth'

interface HeaderProps {
  onOpenSidebar: () => void
}

export const Header = ({ onOpenSidebar }: HeaderProps) => {
  const { user, logout, isLoggingOut } = useAuth()

  return (
    <header className="sticky top-0 z-20 flex items-center justify-between gap-4 border-b border-slate-200 bg-slate-50/90 px-4 py-4 backdrop-blur lg:px-8">
      <div className="flex items-center gap-3">
        <button className="rounded-xl border border-slate-200 bg-white p-2 text-slate-600 shadow-sm lg:hidden" onClick={onOpenSidebar} type="button">
          <Menu className="h-5 w-5" />
        </button>
        <div>
          <p className="text-sm font-semibold text-slate-900">Painel Administrativo</p>
          <p className="text-xs text-slate-500">Visão operacional em tempo real</p>
        </div>
      </div>

      <div className="flex items-center gap-3">
        <button className="rounded-xl border border-slate-200 bg-white p-2 text-slate-500 shadow-sm transition hover:bg-slate-100" type="button">
          <Bell className="h-5 w-5" />
        </button>
        <div className="hidden rounded-2xl border border-slate-200 bg-white px-4 py-2 text-right shadow-sm sm:block">
          <p className="text-sm font-medium text-slate-900">{user?.name ?? 'Equipe'}</p>
          <p className="text-xs text-slate-500">{user?.email ?? 'admin@neiritech.com'}</p>
        </div>
        <Button loading={isLoggingOut} onClick={() => void logout()} size="sm" variant="secondary">
          <LogOut className="h-4 w-4" />
          Sair
        </Button>
      </div>
    </header>
  )
}
