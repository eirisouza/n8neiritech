import { useQuery } from '@tanstack/react-query'
import { Activity, AlertTriangle, ArrowRightLeft, ShoppingBag, Users } from 'lucide-react'
import { Card } from '../components/ui/Card'
import { Badge } from '../components/ui/Badge'
import { getDashboardMetrics, getTodayConversations, getTopProducts } from '../services/dashboard'
import { formatCurrency, formatDateTime, getStatusLabel } from '../utils/formatters'

const skeletonMetricCards = [
  { key: 'todayConversations', label: 'Conversas hoje', icon: Activity, value: '' },
  { key: 'customers', label: 'Clientes ativos', icon: Users, value: '' },
  { key: 'orders', label: 'Pedidos do dia', icon: ShoppingBag, value: '' },
  { key: 'revenue', label: 'Receita', icon: ArrowRightLeft, value: '' },
]

const DashboardPage = () => {
  const metricsQuery = useQuery({
    queryKey: ['dashboard-metrics'],
    queryFn: getDashboardMetrics,
    refetchInterval: 30_000,
  })

  const todayConversationsQuery = useQuery({
    queryKey: ['dashboard-today-conversations'],
    queryFn: getTodayConversations,
    refetchInterval: 30_000,
  })

  const topProductsQuery = useQuery({
    queryKey: ['dashboard-top-products'],
    queryFn: getTopProducts,
    refetchInterval: 30_000,
  })

  if (metricsQuery.isError) {
    return (
      <Card title="Dashboard indisponível">
        <p className="text-sm text-rose-600">Não foi possível carregar os indicadores do painel.</p>
      </Card>
    )
  }

  const metrics = metricsQuery.data
  const conversations = todayConversationsQuery.data ?? []
  const topProducts = topProductsQuery.data ?? metrics?.topProducts ?? []
  const metricCards = metrics
    ? [
        { key: 'todayConversations', label: 'Conversas hoje', icon: Activity, value: metrics.todayConversations },
        { key: 'customers', label: 'Clientes ativos', icon: Users, value: metrics.customers },
        { key: 'orders', label: 'Pedidos do dia', icon: ShoppingBag, value: metrics.orders },
        { key: 'revenue', label: 'Receita', icon: ArrowRightLeft, value: formatCurrency(metrics.revenue) },
      ]
    : []

  return (
    <div className="space-y-6">
      <div>
        <h1 className="text-3xl font-semibold text-slate-900">Dashboard</h1>
        <p className="mt-1 text-sm text-slate-500">Acompanhe conversões, atendimento e saúde da operação em tempo real.</p>
      </div>

      <div className="grid gap-4 sm:grid-cols-2 xl:grid-cols-4">
        {(metricsQuery.isLoading ? skeletonMetricCards : metricCards).map((card) => {
          const Icon = card.icon
          return (
            <Card key={card.key} className="relative overflow-hidden">
              {metricsQuery.isLoading || !metrics ? (
                <div className="animate-pulse space-y-3">
                  <div className="h-4 w-28 rounded bg-slate-200" />
                  <div className="h-8 w-32 rounded bg-slate-200" />
                </div>
              ) : (
                <>
                  <div className="mb-5 flex items-center justify-between">
                    <p className="text-sm text-slate-500">{card.label}</p>
                    <div className="rounded-2xl bg-indigo-50 p-3 text-indigo-600">
                      <Icon className="h-5 w-5" />
                    </div>
                  </div>
                  <p className="text-3xl font-semibold text-slate-900">{card.value}</p>
                </>
              )}
            </Card>
          )
        })}
      </div>

      <div className="grid gap-6 xl:grid-cols-[1.35fr_1fr]">
        <Card title="Eficiência operacional" description="Indicadores críticos para calibrar a automação e o atendimento humano.">
          {metricsQuery.isLoading || !metrics ? (
            <div className="grid gap-4 sm:grid-cols-2">
              {Array.from({ length: 4 }).map((_, index) => (
                <div key={index} className="animate-pulse rounded-2xl border border-slate-100 p-4">
                  <div className="h-4 w-24 rounded bg-slate-200" />
                  <div className="mt-3 h-8 w-20 rounded bg-slate-200" />
                </div>
              ))}
            </div>
          ) : (
            <div className="grid gap-4 sm:grid-cols-2">
              {[
                ['Taxa de conversão', `${metrics.conversionRate.toFixed(1)}%`],
                ['Transferências humanas', metrics.humanHandoffs],
                ['Tempo médio de resposta', `${metrics.averageResponseTimeSeconds}s`],
                ['Erros recentes', metrics.errors],
              ].map(([label, value]) => (
                <div key={label} className="rounded-2xl border border-slate-100 bg-slate-50 p-4">
                  <p className="text-sm text-slate-500">{label}</p>
                  <p className="mt-2 text-2xl font-semibold text-slate-900">{value}</p>
                </div>
              ))}
            </div>
          )}
        </Card>

        <Card title="Instâncias WhatsApp" description="Status em tempo real dos conectores ativos.">
          <div className="space-y-3">
            {(metrics?.instanceStatuses ?? []).map((instance) => (
              <div key={instance.id} className="flex items-center justify-between rounded-2xl border border-slate-100 p-4">
                <div>
                  <p className="font-medium text-slate-900">{instance.name}</p>
                  <p className="text-sm text-slate-500">{instance.phoneNumber ?? 'Número não conectado'}</p>
                </div>
                <Badge status={instance.status}>{getStatusLabel(instance.status)}</Badge>
              </div>
            ))}
            {!metricsQuery.isLoading && (metrics?.instanceStatuses.length ?? 0) === 0 ? (
              <div className="rounded-2xl border border-dashed border-slate-200 p-5 text-sm text-slate-500">Nenhuma instância encontrada.</div>
            ) : null}
          </div>
        </Card>
      </div>

      <div className="grid gap-6 xl:grid-cols-[1.15fr_1fr]">
        <Card title="Conversas do dia" description="Últimas conversas com atividade hoje.">
          <div className="space-y-3">
            {todayConversationsQuery.isLoading ? (
              Array.from({ length: 4 }).map((_, index) => <div key={index} className="h-20 animate-pulse rounded-2xl bg-slate-100" />)
            ) : conversations.length > 0 ? (
              conversations.slice(0, 5).map((conversation) => (
                <div key={conversation.id} className="rounded-2xl border border-slate-100 p-4">
                  <div className="flex items-center justify-between gap-3">
                    <div>
                      <p className="font-medium text-slate-900">{conversation.customer?.name ?? 'Contato sem nome'}</p>
                      <p className="text-sm text-slate-500">{conversation.lastMessagePreview ?? 'Sem última mensagem'}</p>
                    </div>
                    <Badge status={conversation.status}>{getStatusLabel(conversation.status)}</Badge>
                  </div>
                  <div className="mt-3 flex items-center gap-4 text-xs text-slate-500">
                    <span>{conversation.lastMessageAt ? formatDateTime(conversation.lastMessageAt) : 'Sem horário'}</span>
                    <span>{conversation.unreadCount} não lidas</span>
                  </div>
                </div>
              ))
            ) : (
              <div className="rounded-2xl border border-dashed border-slate-200 p-6 text-sm text-slate-500">Sem conversas registradas hoje.</div>
            )}
          </div>
        </Card>

        <Card title="Produtos com melhor saída" description="Itens com mais vendas no dia.">
          <div className="space-y-3">
            {topProductsQuery.isLoading ? (
              Array.from({ length: 4 }).map((_, index) => <div key={index} className="h-16 animate-pulse rounded-2xl bg-slate-100" />)
            ) : topProducts.length > 0 ? (
              topProducts.map((product) => (
                <div key={product.productId} className="flex items-center justify-between rounded-2xl border border-slate-100 p-4">
                  <div>
                    <p className="font-medium text-slate-900">{product.name}</p>
                    <p className="text-sm text-slate-500">{product.quantity} unidades</p>
                  </div>
                  <div className="text-right">
                    <p className="font-semibold text-slate-900">{formatCurrency(product.revenue)}</p>
                    <p className="text-xs text-slate-500">Receita gerada</p>
                  </div>
                </div>
              ))
            ) : (
              <div className="flex items-center gap-3 rounded-2xl border border-dashed border-slate-200 p-6 text-sm text-slate-500">
                <AlertTriangle className="h-5 w-5 text-amber-500" />
                Ainda não há dados de produtos com saída hoje.
              </div>
            )}
          </div>
        </Card>
      </div>
    </div>
  )
}

export default DashboardPage
