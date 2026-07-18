import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query'
import { Eye, ShieldBan, ShieldCheck } from 'lucide-react'
import { useState } from 'react'
import toast from 'react-hot-toast'
import { Badge } from '../components/ui/Badge'
import { Button } from '../components/ui/Button'
import { Card } from '../components/ui/Card'
import { EmptyState } from '../components/ui/EmptyState'
import { Modal } from '../components/ui/Modal'
import { Pagination } from '../components/ui/Pagination'
import { SearchInput } from '../components/ui/SearchInput'
import { Table } from '../components/ui/Table'
import { blockCustomer, getCustomer, getCustomers, unblockCustomer } from '../services/customers'
import type { Conversation, Customer, CustomerFilters, Order } from '../types'
import { formatCurrency, formatDateTime, formatPhone, getStatusLabel } from '../utils/formatters'

interface CustomerDetail extends Customer {
  recentOrders?: Order[]
  recentConversations?: Conversation[]
}

const CustomersPage = () => {
  const queryClient = useQueryClient()
  const [filters, setFilters] = useState<CustomerFilters>({ page: 1, pageSize: 10, search: '' })
  const [selectedCustomerId, setSelectedCustomerId] = useState<string | null>(null)

  const customersQuery = useQuery({
    queryKey: ['customers', filters],
    queryFn: () => getCustomers(filters),
  })

  const customerDetailQuery = useQuery({
    queryKey: ['customer', selectedCustomerId],
    queryFn: () => getCustomer(selectedCustomerId as string) as Promise<CustomerDetail>,
    enabled: Boolean(selectedCustomerId),
  })

  const refreshCustomers = async () => {
    await Promise.all([
      queryClient.invalidateQueries({ queryKey: ['customers'] }),
      queryClient.invalidateQueries({ queryKey: ['customer', selectedCustomerId] }),
    ])
  }

  const blockMutation = useMutation({
    mutationFn: async (id: string) => blockCustomer(id),
    onSuccess: async () => {
      toast.success('Cliente bloqueado')
      await refreshCustomers()
    },
    onError: () => toast.error('Não foi possível bloquear o cliente'),
  })

  const unblockMutation = useMutation({
    mutationFn: async (id: string) => unblockCustomer(id),
    onSuccess: async () => {
      toast.success('Cliente desbloqueado')
      await refreshCustomers()
    },
    onError: () => toast.error('Não foi possível desbloquear o cliente'),
  })

  const selectedCustomer = customerDetailQuery.data

  return (
    <div className="space-y-6">
      <div>
        <h1 className="text-3xl font-semibold text-slate-900">Clientes</h1>
        <p className="mt-1 text-sm text-slate-500">Visualize histórico de pedidos, recorrência e status operacional do cliente.</p>
      </div>

      <Card>
        <SearchInput defaultValue={filters.search} onSearch={(search) => setFilters((current) => ({ ...current, page: 1, search }))} placeholder="Buscar por nome ou telefone" />
      </Card>

      <Table
        columns={[
          {
            key: 'name',
            header: 'Cliente',
            render: (customer) => (
              <div>
                <p className="font-medium text-slate-900">{customer.name}</p>
                <p className="text-xs text-slate-500">{formatPhone(customer.phone)}</p>
              </div>
            ),
          },
          { key: 'phone', header: 'Telefone', render: (customer) => formatPhone(customer.phone) },
          { key: 'orders', header: 'Pedidos', render: (customer) => customer.totalOrders },
          { key: 'lastInteraction', header: 'Última interação', render: (customer) => customer.lastInteractionAt ? formatDateTime(customer.lastInteractionAt) : 'Sem registro' },
          { key: 'status', header: 'Status', render: (customer) => <Badge status={customer.status}>{getStatusLabel(customer.status)}</Badge> },
          {
            key: 'actions',
            header: 'Ações',
            render: (customer) => (
              <div className="flex justify-end gap-2">
                <Button onClick={() => setSelectedCustomerId(customer.id)} size="sm" variant="secondary">
                  <Eye className="h-4 w-4" />
                </Button>
                {customer.blocked ? (
                  <Button loading={unblockMutation.isPending} onClick={() => void unblockMutation.mutateAsync(customer.id)} size="sm" variant="secondary">
                    <ShieldCheck className="h-4 w-4" />
                  </Button>
                ) : (
                  <Button loading={blockMutation.isPending} onClick={() => void blockMutation.mutateAsync(customer.id)} size="sm" variant="danger">
                    <ShieldBan className="h-4 w-4" />
                  </Button>
                )}
              </div>
            ),
          },
        ]}
        data={customersQuery.data?.items ?? []}
        emptyState={<EmptyState description="Os clientes aparecerão aqui conforme as conversas e pedidos forem sendo registrados." icon={Eye} title="Nenhum cliente encontrado" />}
        rowKey={(customer) => customer.id}
      />

      <Pagination page={customersQuery.data?.page ?? 1} totalPages={customersQuery.data?.totalPages ?? 1} onPageChange={(page) => setFilters((current) => ({ ...current, page }))} />

      <Modal open={Boolean(selectedCustomerId)} onClose={() => setSelectedCustomerId(null)} title={selectedCustomer?.name ?? 'Detalhes do cliente'} widthClassName="max-w-5xl">
        {customerDetailQuery.isLoading || !selectedCustomer ? (
          <div className="space-y-4">
            {Array.from({ length: 4 }).map((_, index) => <div key={index} className="h-20 animate-pulse rounded-2xl bg-slate-100" />)}
          </div>
        ) : (
          <div className="space-y-6">
            <div className="grid gap-4 lg:grid-cols-[0.8fr_1.2fr]">
              <Card title="Informações básicas" className="shadow-none">
                <div className="space-y-2 text-sm text-slate-600">
                  <p><strong>Telefone:</strong> {formatPhone(selectedCustomer.phone)}</p>
                  <p><strong>E-mail:</strong> {selectedCustomer.email ?? 'Não informado'}</p>
                  <p><strong>Documento:</strong> {selectedCustomer.document ?? 'Não informado'}</p>
                  <p><strong>Status:</strong> {getStatusLabel(selectedCustomer.status)}</p>
                  <p><strong>Total gasto:</strong> {formatCurrency(selectedCustomer.totalSpent)}</p>
                  <p><strong>Observações:</strong> {selectedCustomer.notes ?? 'Nenhuma observação cadastrada.'}</p>
                </div>
              </Card>
              <Card title="Endereços" className="shadow-none">
                <div className="space-y-3">
                  {selectedCustomer.addresses.length > 0 ? selectedCustomer.addresses.map((address) => (
                    <div key={address.id} className="rounded-2xl border border-slate-100 p-4 text-sm text-slate-600">
                      <p className="font-medium text-slate-900">{address.label ?? 'Principal'}</p>
                      <p>{address.street}, {address.number}</p>
                      <p>{address.neighborhood} - {address.city}/{address.state}</p>
                      <p>CEP {address.zipCode}</p>
                    </div>
                  )) : <p className="text-sm text-slate-500">Nenhum endereço cadastrado.</p>}
                </div>
              </Card>
            </div>
            <div className="grid gap-4 lg:grid-cols-2">
              <Card title="Histórico de pedidos" className="shadow-none">
                <div className="space-y-3">
                  {(selectedCustomer.recentOrders ?? []).length > 0 ? selectedCustomer.recentOrders?.map((order) => (
                    <div key={order.id} className="rounded-2xl border border-slate-100 p-4">
                      <div className="flex items-center justify-between gap-3">
                        <p className="font-medium text-slate-900">Pedido #{order.number}</p>
                        <Badge status={order.status}>{getStatusLabel(order.status)}</Badge>
                      </div>
                      <p className="mt-2 text-sm text-slate-500">{formatCurrency(order.total)} • {formatDateTime(order.createdAt)}</p>
                    </div>
                  )) : <p className="text-sm text-slate-500">Nenhum pedido relacionado.</p>}
                </div>
              </Card>
              <Card title="Histórico de conversas" className="shadow-none">
                <div className="space-y-3">
                  {(selectedCustomer.recentConversations ?? []).length > 0 ? selectedCustomer.recentConversations?.map((conversation) => (
                    <div key={conversation.id} className="rounded-2xl border border-slate-100 p-4">
                      <div className="flex items-center justify-between gap-3">
                        <p className="font-medium text-slate-900">{conversation.subject ?? 'Atendimento WhatsApp'}</p>
                        <Badge status={conversation.status}>{getStatusLabel(conversation.status)}</Badge>
                      </div>
                      <p className="mt-2 text-sm text-slate-500">{conversation.lastMessagePreview ?? 'Sem prévia de mensagem'}</p>
                    </div>
                  )) : <p className="text-sm text-slate-500">Nenhuma conversa relacionada.</p>}
                </div>
              </Card>
            </div>
            <div className="flex justify-end">
              {selectedCustomer.blocked ? (
                <Button loading={unblockMutation.isPending} onClick={() => void unblockMutation.mutateAsync(selectedCustomer.id)} variant="secondary">
                  <ShieldCheck className="h-4 w-4" />
                  Desbloquear cliente
                </Button>
              ) : (
                <Button loading={blockMutation.isPending} onClick={() => void blockMutation.mutateAsync(selectedCustomer.id)} variant="danger">
                  <ShieldBan className="h-4 w-4" />
                  Bloquear cliente
                </Button>
              )}
            </div>
          </div>
        )}
      </Modal>
    </div>
  )
}

export default CustomersPage
