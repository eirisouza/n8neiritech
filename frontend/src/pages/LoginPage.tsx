import { zodResolver } from '@hookform/resolvers/zod'
import { MessageCircleMore } from 'lucide-react'
import { useForm } from 'react-hook-form'
import { Navigate } from 'react-router-dom'
import { z } from 'zod'
import { useAuth } from '../hooks/useAuth'
import { Button } from '../components/ui/Button'
import { Input } from '../components/ui/Input'

const loginSchema = z.object({
  email: z.string().email('Informe um e-mail válido'),
  password: z.string().min(6, 'A senha deve ter pelo menos 6 caracteres'),
})

type LoginFormValues = z.infer<typeof loginSchema>

const LoginPage = () => {
  const { login, isLoggingIn, isAuthenticated } = useAuth()
  const {
    register,
    handleSubmit,
    formState: { errors },
  } = useForm<LoginFormValues>({
    resolver: zodResolver(loginSchema),
    defaultValues: { email: '', password: '' },
  })

  if (isAuthenticated) {
    return <Navigate replace to="/" />
  }

  const onSubmit = async (values: LoginFormValues) => {
    await login(values)
  }

  return (
    <div className="flex min-h-screen items-center justify-center bg-slate-950 px-4 py-10">
      <div className="grid w-full max-w-5xl overflow-hidden rounded-[2rem] bg-white shadow-2xl lg:grid-cols-[1.1fr_0.9fr]">
        <div className="hidden bg-gradient-to-br from-indigo-600 via-indigo-700 to-slate-950 p-10 text-white lg:flex lg:flex-col lg:justify-between">
          <div>
            <div className="flex items-center gap-3">
              <div className="rounded-2xl bg-white/10 p-3 backdrop-blur">
                <MessageCircleMore className="h-8 w-8" />
              </div>
              <div>
                <p className="text-sm uppercase tracking-[0.25em] text-indigo-200">NeiriTech</p>
                <h1 className="text-3xl font-semibold">WhatsApp Commerce</h1>
              </div>
            </div>
            <p className="mt-8 max-w-md text-base text-indigo-100">
              Centralize catálogo, atendimento, pedidos e performance da automação comercial em um painel moderno e responsivo.
            </p>
          </div>
          <div className="grid gap-4 sm:grid-cols-3">
            {[
              ['Atendimento', 'Mensagens com SLA em tempo real'],
              ['Catálogo', 'Produtos e estoque sincronizados'],
              ['Pedidos', 'Status e conversão monitorados'],
            ].map(([title, description]) => (
              <div key={title} className="rounded-2xl border border-white/10 bg-white/5 p-4">
                <p className="font-semibold">{title}</p>
                <p className="mt-2 text-sm text-indigo-100">{description}</p>
              </div>
            ))}
          </div>
        </div>

        <div className="p-8 sm:p-10">
          <div className="mx-auto w-full max-w-md">
            <div className="mb-8 lg:hidden">
              <p className="text-sm uppercase tracking-[0.25em] text-indigo-600">NeiriTech</p>
              <h1 className="mt-2 text-3xl font-semibold text-slate-900">Commerce Admin</h1>
            </div>
            <div className="mb-8">
              <h2 className="text-2xl font-semibold text-slate-900">Entrar no painel</h2>
              <p className="mt-2 text-sm text-slate-500">Use suas credenciais para acessar operação, catálogo e métricas.</p>
            </div>

            <form className="space-y-5" onSubmit={(event) => void handleSubmit(onSubmit)(event)}>
              <Input
                autoComplete="email"
                error={errors.email?.message}
                label="E-mail"
                placeholder="voce@empresa.com"
                type="email"
                {...register('email')}
              />
              <Input
                autoComplete="current-password"
                error={errors.password?.message}
                label="Senha"
                placeholder="••••••••"
                type="password"
                {...register('password')}
              />
              <Button fullWidth loading={isLoggingIn} size="lg" type="submit">
                Acessar painel
              </Button>
            </form>
          </div>
        </div>
      </div>
    </div>
  )
}

export default LoginPage
