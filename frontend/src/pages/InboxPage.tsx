import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query'
import { BotOff, Bot, Send, UserPlus, XCircle } from 'lucide-react'
import { useEffect, useMemo, useState } from 'react'
import toast from 'react-hot-toast'
import { Button } from '../components/ui/Button'
import { Card } from '../components/ui/Card'
import { Badge } from '../components/ui/Badge'
import { EmptyState } from '../components/ui/EmptyState'
import { Pagination } from '../components/ui/Pagination'
import { SearchInput } from '../components/ui/SearchInput'
import { Select } from '../components/ui/Select'
import { conversationsApi } from '../services/api'
import { useAuth } from '../hooks/useAuth'
import type { ConversationFilters } from '../types'
import { ConversationStatus } from '../types'
import { formatDateTime, formatPhone, getStatusLabel } from '../utils/formatters'

const InboxPage = () => {
  const queryClient = useQueryClient()
  const { user } = useAuth()
  const [filters, setFilters] = useState<ConversationFilters>({
    page: 1,
    pageSize: 10,
    search: '',
    status: 'all',
    assignment: 'all',
  })
  const [selectedConversationId, setSelectedConversationId] = useState<string | null>(null)
  const [message, setMessage] = useState('')

  const conversationsQuery = useQuery({
    queryKey: ['conversations', filters],
    queryFn: () => conversationsApi.getConversations(filters),
  })

  useEffect(() => {
    if (!selectedConversationId && conversationsQuery.data?.items[0]) {
      setSelectedConversationId(conversationsQuery.data.items[0].id)
    }
  }, [conversationsQuery.data, selectedConversationId])

  const conversationQuery = useQuery({
    queryKey: ['conversation', selectedConversationId],
    queryFn: () => conversationsApi.getConversation(selectedConversationId as string),
    enabled: Boolean(selectedConversationId),
    refetchInterval: selectedConversationId ? 5_000 : false,
  })

  const messagesQuery = useQuery({
    queryKey: ['conversation-messages', selectedConversationId],
    queryFn: () => conversationsApi.getMessages(selectedConversationId as string),
    enabled: Boolean(selectedConversationId),
    refetchInterval: selectedConversationId ? 5_000 : false,
  })

  const selectedConversation = useMemo(() => {
    return conversationQuery.data ?? conversationsQuery.data?.items.find((item) => item.id === selectedConversationId)
  }, [conversationQuery.data, conversationsQuery.data?.items, selectedConversationId])

  const refreshConversation = async () => {
    await Promise.all([
      queryClient.invalidateQueries({ queryKey: ['conversations'] }),
      queryClient.invalidateQueries({ queryKey: ['conversation', selectedConversationId] }),
      queryClient.invalidateQueries({ queryKey: ['conversation-messages', selectedConversationId] }),
    ])
  }

  const sendMessageMutation = useMutation({
    mutationFn: async () => conversationsApi.sendMessage(selectedConversationId as string, message),
    onSuccess: async () => {
      setMessage('')
      toast.success('Mensagem enviada')
      await refreshConversation()
    },
    onError: () => toast.error('Não foi possível enviar a mensagem'),
  })

  const assignMutation = useMutation({
    mutationFn: async () => conversationsApi.assignConversation(selectedConversationId as string, user?.id ?? ''),
    onSuccess: async () => {
      toast.success('Conversa atribuída')
      await refreshConversation()
    },
    onError: () => toast.error('Não foi possível atribuir a conversa'),
  })

  const closeMutation = useMutation({
    mutationFn: async () => conversationsApi.closeConversation(selectedConversationId as string),
    onSuccess: async () => {
      toast.success('Conversa encerrada')
      await refreshConversation()
    },
    onError: () => toast.error('Não foi possível encerrar a conversa'),
  })

  const pauseBotMutation = useMutation({
    mutationFn: async () => conversationsApi.pauseBot(selectedConversationId as string),
    onSuccess: async () => {
      toast.success('Bot pausado')
      await refreshConversation()
    },
    onError: () => toast.error('Não foi possível pausar o bot'),
  })

  const resumeBotMutation = useMutation({
    mutationFn: async () => conversationsApi.resumeBot(selectedConversationId as string),
    onSuccess: async () => {
      toast.success('Bot reativado')
      await refreshConversation()
    },
    onError: () => toast.error('Não foi possível reativar o bot'),
  })

  return (
    <div className="grid gap-6 xl:grid-cols-[360px_minmax(0,1fr)]">
      <Card className="h-[calc(100vh-10rem)] overflow-hidden p-0" title="Inbox" description="Filtre, acompanhe e responda conversas em tempo real.">
        <div className="space-y-4 border-b border-slate-200 px-5 pb-5">
          <SearchInput defaultValue={filters.search} onSearch={(search) => setFilters((current) => ({ ...current, search, page: 1 }))} placeholder="Buscar por cliente ou telefone" />
          <div className="grid gap-3 sm:grid-cols-2">
            <Select
              options={[
                { label: 'Todos os status', value: 'all' },
                { label: 'Abertas', value: ConversationStatus.Open },
                { label: 'Pendentes', value: ConversationStatus.Pending },
                { label: 'Atribuídas', value: ConversationStatus.Assigned },
                { label: 'Bot pausado', value: ConversationStatus.Paused },
                { label: 'Encerradas', value: ConversationStatus.Closed },
              ]}
              value={filters.status ?? 'all'}
              onChange={(event) => setFilters((current) => ({ ...current, status: event.target.value as ConversationFilters['status'], page: 1 }))}
            />
            <Select
              options={[
                { label: 'Todas', value: 'all' },
                { label: 'Minhas', value: 'me' },
                { label: 'Sem atribuição', value: 'unassigned' },
              ]}
              value={filters.assignment ?? 'all'}
              onChange={(event) => setFilters((current) => ({ ...current, assignment: event.target.value as ConversationFilters['assignment'], page: 1 }))}
            />
          </div>
        </div>
        <div className="flex h-[calc(100%-11rem)] flex-col">
          <div className="flex-1 space-y-2 overflow-y-auto px-3 py-4">
            {conversationsQuery.isLoading ? (
              Array.from({ length: 6 }).map((_, index) => <div key={index} className="h-24 animate-pulse rounded-2xl bg-slate-100" />)
            ) : conversationsQuery.data?.items.length ? (
              conversationsQuery.data.items.map((conversation) => (
                <button
                  key={conversation.id}
                  className={`w-full rounded-2xl border p-4 text-left transition ${selectedConversationId === conversation.id ? 'border-indigo-500 bg-indigo-50' : 'border-slate-200 bg-white hover:border-indigo-200 hover:bg-slate-50'}`}
                  onClick={() => setSelectedConversationId(conversation.id)}
                  type="button"
                >
                  <div className="flex items-start justify-between gap-3">
                    <div>
                      <p className="font-medium text-slate-900">{conversation.customer?.name ?? 'Contato sem nome'}</p>
                      <p className="mt-1 line-clamp-2 text-sm text-slate-500">{conversation.lastMessagePreview ?? 'Sem mensagens recentes'}</p>
                    </div>
                    <div className="text-right">
                      <p className="text-xs text-slate-400">{conversation.lastMessageAt ? formatDateTime(conversation.lastMessageAt) : '--'}</p>
                      {conversation.unreadCount > 0 ? <Badge className="mt-2" status={conversation.status}>{conversation.unreadCount}</Badge> : null}
                    </div>
                  </div>
                </button>
              ))
            ) : (
              <EmptyState description="Ajuste os filtros para localizar novas conversas." icon={XCircle} title="Nenhuma conversa encontrada" />
            )}
          </div>
          <div className="border-t border-slate-200 px-5 pt-4">
            <Pagination
              onPageChange={(page) => setFilters((current) => ({ ...current, page }))}
              page={conversationsQuery.data?.page ?? 1}
              totalPages={conversationsQuery.data?.totalPages ?? 1}
            />
          </div>
        </div>
      </Card>

      <Card className="h-[calc(100vh-10rem)] overflow-hidden p-0">
        {selectedConversation ? (
          <div className="grid h-full xl:grid-cols-[minmax(0,1fr)_320px]">
            <div className="flex min-h-0 flex-col border-b border-slate-200 xl:border-b-0 xl:border-r">
              <div className="flex flex-wrap items-center justify-between gap-3 border-b border-slate-200 px-5 py-4">
                <div>
                  <div className="flex items-center gap-3">
                    <h2 className="text-xl font-semibold text-slate-900">{selectedConversation.customer?.name ?? 'Contato sem nome'}</h2>
                    <Badge status={selectedConversation.status}>{getStatusLabel(selectedConversation.status)}</Badge>
                  </div>
                  <p className="mt-1 text-sm text-slate-500">{selectedConversation.customer?.phone ? formatPhone(selectedConversation.customer.phone) : 'Telefone indisponível'}</p>
                </div>
                <div className="flex flex-wrap gap-2">
                  <Button disabled={!user?.id} loading={assignMutation.isPending} onClick={() => void assignMutation.mutateAsync()} size="sm" variant="secondary">
                    <UserPlus className="h-4 w-4" />
                    Atribuir
                  </Button>
                  <Button loading={closeMutation.isPending} onClick={() => void closeMutation.mutateAsync()} size="sm" variant="secondary">
                    <XCircle className="h-4 w-4" />
                    Encerrar
                  </Button>
                  {selectedConversation.botPaused ? (
                    <Button loading={resumeBotMutation.isPending} onClick={() => void resumeBotMutation.mutateAsync()} size="sm" variant="secondary">
                      <Bot className="h-4 w-4" />
                      Retomar bot
                    </Button>
                  ) : (
                    <Button loading={pauseBotMutation.isPending} onClick={() => void pauseBotMutation.mutateAsync()} size="sm" variant="secondary">
                      <BotOff className="h-4 w-4" />
                      Pausar bot
                    </Button>
                  )}
                </div>
              </div>

              <div className="flex-1 space-y-4 overflow-y-auto bg-slate-50 px-5 py-5">
                {messagesQuery.isLoading ? (
                  Array.from({ length: 5 }).map((_, index) => <div key={index} className="h-20 animate-pulse rounded-2xl bg-white" />)
                ) : (
                  messagesQuery.data?.map((item) => (
                    <div key={item.id} className={`flex ${item.direction === 'outbound' ? 'justify-end' : 'justify-start'}`}>
                      <div className={`max-w-[80%] rounded-3xl px-4 py-3 shadow-sm ${item.direction === 'outbound' ? 'bg-indigo-600 text-white' : 'bg-white text-slate-700'}`}>
                        <p className="text-sm font-medium">{item.senderName}</p>
                        <p className="mt-1 whitespace-pre-wrap text-sm">{item.content}</p>
                        <p className={`mt-2 text-xs ${item.direction === 'outbound' ? 'text-indigo-100' : 'text-slate-400'}`}>
                          {formatDateTime(item.createdAt)}
                        </p>
                      </div>
                    </div>
                  ))
                )}
              </div>

              <div className="border-t border-slate-200 bg-white px-5 py-4">
                <div className="flex gap-3">
                  <textarea
                    className="min-h-24 flex-1 rounded-2xl border border-slate-200 px-4 py-3 text-sm outline-none transition placeholder:text-slate-400 focus:border-indigo-500 focus:ring-4 focus:ring-indigo-100"
                    onChange={(event) => setMessage(event.target.value)}
                    placeholder="Digite a resposta para o cliente..."
                    value={message}
                  />
                  <Button disabled={!message.trim()} loading={sendMessageMutation.isPending} onClick={() => void sendMessageMutation.mutateAsync()}>
                    <Send className="h-4 w-4" />
                    Enviar
                  </Button>
                </div>
              </div>
            </div>

            <aside className="space-y-5 overflow-y-auto bg-white px-5 py-5">
              <div>
                <p className="text-xs uppercase tracking-[0.2em] text-slate-400">Cliente</p>
                <h3 className="mt-2 text-lg font-semibold text-slate-900">{selectedConversation.customer?.name ?? 'Contato sem nome'}</h3>
                <p className="mt-1 text-sm text-slate-500">{selectedConversation.customer?.phone ? formatPhone(selectedConversation.customer.phone) : 'Telefone indisponível'}</p>
                <p className="mt-1 text-sm text-slate-500">{selectedConversation.customer?.email ?? 'E-mail não informado'}</p>
              </div>
              <div className="grid gap-3 sm:grid-cols-2 xl:grid-cols-1">
                <div className="rounded-2xl border border-slate-100 bg-slate-50 p-4">
                  <p className="text-sm text-slate-500">Pedidos</p>
                  <p className="mt-2 text-2xl font-semibold text-slate-900">{selectedConversation.customer?.totalOrders ?? 0}</p>
                </div>
                <div className="rounded-2xl border border-slate-100 bg-slate-50 p-4">
                  <p className="text-sm text-slate-500">Última atualização</p>
                  <p className="mt-2 text-sm font-medium text-slate-900">{formatDateTime(selectedConversation.updatedAt)}</p>
                </div>
              </div>
              <div>
                <p className="text-sm font-medium text-slate-700">Tags</p>
                <div className="mt-3 flex flex-wrap gap-2">
                  {selectedConversation.tags.length > 0 ? selectedConversation.tags.map((tag) => <Badge key={tag}>{tag}</Badge>) : <span className="text-sm text-slate-400">Sem tags</span>}
                </div>
              </div>
            </aside>
          </div>
        ) : (
          <div className="flex h-full items-center justify-center p-8">
            <EmptyState description="Selecione uma conversa na lista para ver o histórico e responder o cliente." icon={XCircle} title="Nenhuma conversa selecionada" />
          </div>
        )}
      </Card>
    </div>
  )
}

export default InboxPage
