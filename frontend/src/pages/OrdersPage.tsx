import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query'
import { Ban, Eye } from 'lucide-react'
import { useState } from 'react'
import toast from 'react-hot-toast'
import { Badge } from '../components/ui/Badge'
import { Button } from '../components/ui/Button'
import { Card } from '../components/ui/Card'
import { EmptyState } from '../components/ui/EmptyState'
import { Input } from '../components/ui/Input'
import { Modal } from '../components/ui/Modal'
import { Pagination } from '../components/ui/Pagination'
import { Select } from '../components/ui/Select'
import { Table } from '../components/ui/Table'
import { getOrder, getOrders, updateOrderStatus, cancelOrder } from '../services/orders'
import { OrderStatus, PaymentMethod, type Order, type OrderFilters } from '../types'
import { formatCurrency, formatDateTime, getStatusLabel } from '../utils/formatters'

const paymentMethodLabels: Record<PaymentMethod, string> = {
  [PaymentMethod.Pix]: 'Pix',
  [PaymentMethod.CreditCard]: 'Cartão de crédito',
  [PaymentMethod.DebitCard]: 'Cartão de débito',
  [PaymentMethod.Cash]: 'Dinheiro',
  [PaymentMethod.BankSlip]: 'Boleto',
}

const OrdersPage = () => {
  const queryClient = useQueryClient()
  const [filters, setFilters] = useState<OrderFilters>({ page: 1, pageSize: 10, status: 'all', startDate: '', endDate: '' })
  const [selectedOrderId, setSelectedOrderId] = useState<string | null>(null)
  const [statusValue, setStatusValue] = useState<OrderStatus>(OrderStatus.Pending)
  const [cancelReason, setCancelReason] = useState('')

  const ordersQuery = useQuery({
    queryKey: ['orders', filters],
    queryFn: () => getOrders(filters),
  })

  const orderDetailQuery = useQuery({
    queryKey: ['order', selectedOrderId],
    queryFn: () => getOrder(selectedOrderId as string),
    enabled: Boolean(selectedOrderId),
  })

  const refreshOrders = async () => {
    await Promise.all([
      queryClient.invalidateQueries({ queryKey: ['orders'] }),
      queryClient.invalidateQueries({ queryKey: ['order', selectedOrderId] }),
    ])
  }

  const updateStatusMutation = useMutation({
    mutationFn: async () => updateOrderStatus(selectedOrderId as string, statusValue),
    onSuccess: async () => {
      toast.success('Status atualizado')
      await refreshOrders()
    },
    onError: () => toast.error('Não foi possível atualizar o status'),
  })

  const cancelMutation = useMutation({
    mutationFn: async () => cancelOrder(selectedOrderId as string, cancelReason),
    onSuccess: async () => {
      toast.success('Pedido cancelado')
      setCancelReason('')
      await refreshOrders()
    },
    onError: () => toast.error('Não foi possível cancelar o pedido'),
  })

  const openOrder = (order: Order) => {
    setSelectedOrderId(order.id)
    setStatusValue(order.status)
  }

  const order = orderDetailQuery.data

  return (
    <div className="space-y-6">
      <div>
        <h1 className="text-3xl font-semibold text-slate-900">Pedidos</h1>
        <p className="mt-1 text-sm text-slate-500">Acompanhe o ciclo do pedido, endereço de entrega e histórico de status.</p>
      </div>

      <Card>
        <div className="grid gap-3 md:grid-cols-4">
          <Select
            options={[
              { label: 'Todos os status', value: 'all' },
              ...Object.values(OrderStatus).map((status) => ({ label: getStatusLabel(status), value: status })),
            ]}
            value={filters.status ?? 'all'}
            onChange={(event) => setFilters((current) => ({ ...current, page: 1, status: event.target.value as OrderFilters['status'] }))}
          />
          <Input label="Data inicial" type="date" value={filters.startDate ?? ''} onChange={(event) => setFilters((current) => ({ ...current, page: 1, startDate: event.target.value }))} />
          <Input label="Data final" type="date" value={filters.endDate ?? ''} onChange={(event) => setFilters((current) => ({ ...current, page: 1, endDate: event.target.value }))} />
          <div className="flex items-end">
            <Button fullWidth onClick={() => setFilters({ page: 1, pageSize: 10, status: 'all', startDate: '', endDate: '' })} variant="secondary">
              Limpar filtros
            </Button>
          </div>
        </div>
      </Card>

      <Table
        columns={[
          { key: 'number', header: 'Número', render: (item) => <span className="font-medium text-slate-900">#{item.number}</span> },
          {
            key: 'customer',
            header: 'Cliente',
            render: (item) => (
              <div>
                <p className="font-medium text-slate-900">{item.customer?.name ?? 'Cliente não identificado'}</p>
                <p className="text-xs text-slate-500">{item.items.length} itens</p>
              </div>
            ),
          },
          { key: 'items', header: 'Itens', render: (item) => item.items.length },
          { key: 'total', header: 'Total', render: (item) => formatCurrency(item.total) },
          { key: 'status', header: 'Status', render: (item) => <Badge status={item.status}>{getStatusLabel(item.status)}</Badge> },
          { key: 'date', header: 'Data', render: (item) => formatDateTime(item.createdAt) },
          {
            key: 'actions',
            header: 'Ações',
            render: (item) => (
              <Button onClick={() => openOrder(item)} size="sm" variant="secondary">
                <Eye className="h-4 w-4" />
                Detalhes
              </Button>
            ),
          },
        ]}
        data={ordersQuery.data?.items ?? []}
        emptyState={<EmptyState description="Os pedidos aparecerão aqui assim que houver transações capturadas pelo WhatsApp." icon={Ban} title="Nenhum pedido encontrado" />}
        rowKey={(item) => item.id}
      />

      <Pagination page={ordersQuery.data?.page ?? 1} totalPages={ordersQuery.data?.totalPages ?? 1} onPageChange={(page) => setFilters((current) => ({ ...current, page }))} />

      <Modal open={Boolean(selectedOrderId)} onClose={() => setSelectedOrderId(null)} title={order ? `Pedido #${order.number}` : 'Detalhes do pedido'} widthClassName="max-w-5xl">
        {orderDetailQuery.isLoading || !order ? (
          <div className="space-y-4">
            {Array.from({ length: 4 }).map((_, index) => <div key={index} className="h-20 animate-pulse rounded-2xl bg-slate-100" />)}
          </div>
        ) : (
          <div className="space-y-6">
            <div className="grid gap-4 xl:grid-cols-[1.2fr_0.8fr]">
              <Card title="Itens do pedido" className="shadow-none">
                <div className="space-y-3">
                  {order.items.map((item) => (
                    <div key={item.id} className="flex items-center justify-between rounded-2xl border border-slate-100 p-4">
                      <div>
                        <p className="font-medium text-slate-900">{item.productName}</p>
                        <p className="text-sm text-slate-500">SKU {item.sku ?? 'N/A'} • {item.quantity}x</p>
                      </div>
                      <p className="font-semibold text-slate-900">{formatCurrency(item.totalPrice)}</p>
                    </div>
                  ))}
                </div>
              </Card>

              <div className="space-y-4">
                <Card title="Resumo" className="shadow-none">
                  <div className="space-y-3 text-sm text-slate-600">
                    <div className="flex justify-between"><span>Subtotal</span><span>{formatCurrency(order.subtotal)}</span></div>
                    <div className="flex justify-between"><span>Desconto</span><span>{formatCurrency(order.discount)}</span></div>
                    <div className="flex justify-between"><span>Entrega</span><span>{formatCurrency(order.deliveryFee)}</span></div>
                    <div className="flex justify-between text-base font-semibold text-slate-900"><span>Total</span><span>{formatCurrency(order.total)}</span></div>
                    <div className="flex justify-between"><span>Pagamento</span><span>{paymentMethodLabels[order.paymentMethod] ?? order.paymentMethod}</span></div>
                  </div>
                </Card>
                <Card title="Cliente" className="shadow-none">
                  <p className="font-medium text-slate-900">{order.customer?.name ?? 'Cliente não identificado'}</p>
                  <p className="mt-1 text-sm text-slate-500">{order.customer?.phone ?? 'Telefone não informado'}</p>
                  <p className="mt-1 text-sm text-slate-500">{order.customer?.email ?? 'E-mail não informado'}</p>
                </Card>
                <Card title="Endereço de entrega" className="shadow-none">
                  {order.deliveryAddress ? (
                    <p className="text-sm text-slate-600">
                      {order.deliveryAddress.street}, {order.deliveryAddress.number}
                      <br />
                      {order.deliveryAddress.neighborhood} - {order.deliveryAddress.city}/{order.deliveryAddress.state}
                      <br />
                      CEP {order.deliveryAddress.zipCode}
                    </p>
                  ) : <p className="text-sm text-slate-500">Endereço não informado.</p>}
                </Card>
              </div>
            </div>

            <div className="grid gap-6 xl:grid-cols-[1fr_360px]">
              <Card title="Histórico de status" className="shadow-none">
                <div className="space-y-3">
                  {order.statusHistory.map((entry) => (
                    <div key={entry.id} className="rounded-2xl border border-slate-100 p-4">
                      <div className="flex items-center justify-between gap-3">
                        <Badge status={entry.status}>{getStatusLabel(entry.status)}</Badge>
                        <span className="text-xs text-slate-400">{formatDateTime(entry.createdAt)}</span>
                      </div>
                      <p className="mt-2 text-sm text-slate-600">{entry.description ?? 'Atualização sem observações.'}</p>
                    </div>
                  ))}
                </div>
              </Card>
              <Card title="Ações rápidas" className="shadow-none">
                <div className="space-y-4">
                  <Select
                    label="Atualizar status"
                    options={Object.values(OrderStatus).map((status) => ({ label: getStatusLabel(status), value: status }))}
                    value={statusValue}
                    onChange={(event) => setStatusValue(event.target.value as OrderStatus)}
                  />
                  <Button fullWidth loading={updateStatusMutation.isPending} onClick={() => void updateStatusMutation.mutateAsync()}>
                    Salvar status
                  </Button>
                  <label>
                    <span className="mb-1.5 block text-sm font-medium text-slate-700">Motivo do cancelamento</span>
                    <textarea
                      className="min-h-28 w-full rounded-xl border border-slate-200 px-3 py-2 text-sm outline-none focus:border-indigo-500 focus:ring-4 focus:ring-indigo-100"
                      onChange={(event) => setCancelReason(event.target.value)}
                      placeholder="Informe o motivo para registrar o cancelamento..."
                      value={cancelReason}
                    />
                  </label>
                  <Button
                    fullWidth
                    loading={cancelMutation.isPending}
                    onClick={() => cancelReason.trim() ? void cancelMutation.mutateAsync() : toast.error('Informe o motivo do cancelamento')}
                    variant="danger"
                  >
                    Cancelar pedido
                  </Button>
                </div>
              </Card>
            </div>
          </div>
        )}
      </Modal>
    </div>
  )
}

export default OrdersPage
