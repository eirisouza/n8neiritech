import { useMutation, useQueries, useQuery, useQueryClient } from '@tanstack/react-query'
import { MessageSquareShare, Power, RefreshCcw, RotateCcw } from 'lucide-react'
import toast from 'react-hot-toast'
import { Badge } from '../components/ui/Badge'
import { Button } from '../components/ui/Button'
import { Card } from '../components/ui/Card'
import { EmptyState } from '../components/ui/EmptyState'
import { whatsappApi } from '../services/api'
import { WhatsAppInstanceStatus } from '../types'
import { formatDateTime, getStatusLabel } from '../utils/formatters'

const WhatsAppPage = () => {
  const queryClient = useQueryClient()
  const instancesQuery = useQuery({
    queryKey: ['whatsapp-instances'],
    queryFn: whatsappApi.getInstances,
    refetchInterval: 15_000,
  })

  const statusQueries = useQueries({
    queries: (instancesQuery.data ?? []).map((instance) => ({
      queryKey: ['whatsapp-instance-status', instance.id],
      queryFn: () => whatsappApi.getInstanceStatus(instance.id),
      refetchInterval: 15_000,
    })),
  })

  const qrQueries = useQueries({
    queries: (instancesQuery.data ?? []).map((instance) => ({
      queryKey: ['whatsapp-instance-qr', instance.id],
      queryFn: () => whatsappApi.getQrCode(instance.id),
      enabled: instance.status !== WhatsAppInstanceStatus.Connected,
      refetchInterval: instance.status !== WhatsAppInstanceStatus.Connected ? 5_000 : false,
    })),
  })

  const refreshAll = async () => {
    await Promise.all([
      queryClient.invalidateQueries({ queryKey: ['whatsapp-instances'] }),
      queryClient.invalidateQueries({ queryKey: ['whatsapp-instance-status'] }),
      queryClient.invalidateQueries({ queryKey: ['whatsapp-instance-qr'] }),
    ])
  }

  const restartMutation = useMutation({
    mutationFn: whatsappApi.restartInstance,
    onSuccess: async () => {
      toast.success('Instância reiniciada')
      await refreshAll()
    },
    onError: () => toast.error('Não foi possível reiniciar a instância'),
  })

  const connectMutation = useMutation({
    mutationFn: (id: string) => whatsappApi.restartInstance(id),
    onSuccess: async () => {
      toast.success('Reconexão iniciada')
      await refreshAll()
    },
    onError: () => toast.error('Não foi possível iniciar a conexão'),
  })

  const disconnectMutation = useMutation({
    mutationFn: (id: string) => whatsappApi.restartInstance(id),
    onSuccess: async () => {
      toast.success('Solicitação enviada para desconectar/reinicializar')
      await refreshAll()
    },
    onError: () => toast.error('Não foi possível processar a ação'),
  })

  const testMutation = useMutation({
    mutationFn: whatsappApi.testInstance,
    onSuccess: (data) => {
      toast.success(data.message || 'Teste realizado com sucesso')
    },
    onError: () => toast.error('Falha no teste da instância'),
  })

  const instances = instancesQuery.data ?? []

  return (
    <div className="space-y-6">
      <div className="flex items-center justify-between gap-4">
        <div>
          <h1 className="text-3xl font-semibold text-slate-900">WhatsApp</h1>
          <p className="mt-1 text-sm text-slate-500">Monitore conexões, QR codes e eventos recentes das instâncias de mensageria.</p>
        </div>
        <Button onClick={() => void refreshAll()} variant="secondary">
          <RefreshCcw className="h-4 w-4" />
          Atualizar agora
        </Button>
      </div>

      {instances.length === 0 && !instancesQuery.isLoading ? (
        <EmptyState description="Cadastre uma instância no backend para começar a monitorar conexões e QR Code." icon={MessageSquareShare} title="Nenhuma instância encontrada" />
      ) : null}

      <div className="grid gap-6 xl:grid-cols-2">
        {instances.map((instance, index) => {
          const status = statusQueries[index]?.data ?? instance
          const qrCode = qrQueries[index]?.data?.qrCode

          return (
            <Card key={instance.id} title={instance.name} description={status.phoneNumber ?? 'Número ainda não conectado'} action={<Badge status={status.status}>{getStatusLabel(status.status)}</Badge>}>
              <div className="grid gap-5 lg:grid-cols-[1fr_220px]">
                <div className="space-y-4">
                  <div className="grid gap-3 sm:grid-cols-2">
                    <div className="rounded-2xl border border-slate-100 bg-slate-50 p-4">
                      <p className="text-sm text-slate-500">Status</p>
                      <p className="mt-2 text-lg font-semibold text-slate-900">{getStatusLabel(status.status)}</p>
                    </div>
                    <div className="rounded-2xl border border-slate-100 bg-slate-50 p-4">
                      <p className="text-sm text-slate-500">Última conexão</p>
                      <p className="mt-2 text-sm font-semibold text-slate-900">{status.connectedAt ? formatDateTime(status.connectedAt) : 'Sem registro'}</p>
                    </div>
                  </div>
                  <div className="flex flex-wrap gap-3">
                    <Button loading={connectMutation.isPending} onClick={() => void connectMutation.mutateAsync(instance.id)} variant="secondary">
                      <Power className="h-4 w-4" />
                      {status.status === WhatsAppInstanceStatus.Connected ? 'Reconectar' : 'Conectar'}
                    </Button>
                    <Button loading={disconnectMutation.isPending} onClick={() => void disconnectMutation.mutateAsync(instance.id)} variant="secondary">
                      <RotateCcw className="h-4 w-4" />
                      Desconectar
                    </Button>
                    <Button loading={restartMutation.isPending} onClick={() => void restartMutation.mutateAsync(instance.id)} variant="secondary">
                      <RefreshCcw className="h-4 w-4" />
                      Reiniciar
                    </Button>
                    <Button loading={testMutation.isPending} onClick={() => void testMutation.mutateAsync(instance.id)}>
                      Testar conexão
                    </Button>
                  </div>
                  <div>
                    <p className="text-sm font-medium text-slate-700">Erros recentes</p>
                    <div className="mt-3 space-y-2">
                      {status.recentErrors.length > 0 ? status.recentErrors.map((error) => (
                        <div key={error} className="rounded-2xl border border-rose-100 bg-rose-50 px-4 py-3 text-sm text-rose-700">{error}</div>
                      )) : <div className="rounded-2xl border border-slate-100 bg-slate-50 px-4 py-3 text-sm text-slate-500">Nenhum erro recente.</div>}
                    </div>
                  </div>
                </div>
                <div className="rounded-3xl border border-slate-100 bg-slate-50 p-4">
                  <p className="text-sm font-medium text-slate-700">QR Code</p>
                  <div className="mt-4 flex min-h-52 items-center justify-center rounded-2xl border border-dashed border-slate-200 bg-white p-4">
                    {status.status !== WhatsAppInstanceStatus.Connected && qrCode ? (
                      <img alt={`QR code da instância ${instance.name}`} className="h-48 w-48 object-contain" src={qrCode} />
                    ) : (
                      <p className="text-center text-sm text-slate-500">{status.status === WhatsAppInstanceStatus.Connected ? 'Instância já conectada.' : 'Aguardando QR Code...'}</p>
                    )}
                  </div>
                </div>
              </div>
            </Card>
          )
        })}
      </div>
    </div>
  )
}

export default WhatsAppPage
