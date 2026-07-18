import { zodResolver } from '@hookform/resolvers/zod'
import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query'
import { Plus, Save, Trash2 } from 'lucide-react'
import { useEffect, useMemo, useState } from 'react'
import { useForm } from 'react-hook-form'
import toast from 'react-hot-toast'
import { z } from 'zod'
import { Button } from '../components/ui/Button'
import { Card } from '../components/ui/Card'
import { EmptyState } from '../components/ui/EmptyState'
import { Input } from '../components/ui/Input'
import { Select } from '../components/ui/Select'
import { getSettings, updateAiConfiguration, updateBusinessHours, updateCompany, updateFaqs, updateWhatsappProvider } from '../services/settings'
import { ProviderType, type AiConfiguration, type BusinessHour, type CompanySettings, type Faq, type WhatsAppProviderSettings } from '../types'

const tabs = [
  { key: 'company', label: 'Empresa' },
  { key: 'hours', label: 'Horários' },
  { key: 'faqs', label: 'FAQs' },
  { key: 'provider', label: 'WhatsApp' },
  { key: 'ai', label: 'IA' },
] as const

const companySchema = z.object({
  storeName: z.string().min(2, 'Informe o nome da loja'),
  description: z.string().optional(),
  logoUrl: z.string().url('Informe uma URL válida').or(z.literal('')),
  email: z.string().email('Informe um e-mail válido').or(z.literal('')),
  phone: z.string().optional(),
  whatsapp: z.string().optional(),
})

const providerSchema = z.object({
  provider: z.nativeEnum(ProviderType),
  baseUrl: z.string().url('Informe uma URL válida'),
  token: z.string().min(6, 'Informe o token do provedor'),
  instanceName: z.string().min(2, 'Informe o nome da instância'),
  webhookSecret: z.string().min(6, 'Informe o segredo do webhook'),
})

const aiSchema = z.object({
  provider: z.string().min(2, 'Informe o provedor'),
  apiKey: z.string().min(6, 'Informe a chave da API'),
  model: z.string().min(2, 'Informe o modelo'),
})

type CompanyFormValues = z.infer<typeof companySchema>
type ProviderFormValues = z.infer<typeof providerSchema>
type AiFormValues = z.infer<typeof aiSchema>

const dayLabels = ['Domingo', 'Segunda', 'Terça', 'Quarta', 'Quinta', 'Sexta', 'Sábado']

const SettingsPage = () => {
  const queryClient = useQueryClient()
  const [activeTab, setActiveTab] = useState<(typeof tabs)[number]['key']>('company')
  const [businessHours, setBusinessHours] = useState<BusinessHour[]>([])
  const [faqs, setFaqs] = useState<Faq[]>([])

  const settingsQuery = useQuery({
    queryKey: ['settings'],
    queryFn: getSettings,
  })

  const companyForm = useForm<CompanyFormValues>({
    resolver: zodResolver(companySchema),
    defaultValues: { storeName: '', description: '', logoUrl: '', email: '', phone: '', whatsapp: '' },
  })

  const providerForm = useForm<ProviderFormValues>({
    resolver: zodResolver(providerSchema),
    defaultValues: { provider: ProviderType.Evolution, baseUrl: '', token: '', instanceName: '', webhookSecret: '' },
  })

  const aiForm = useForm<AiFormValues>({
    resolver: zodResolver(aiSchema),
    defaultValues: { provider: '', apiKey: '', model: '' },
  })

  useEffect(() => {
    if (!settingsQuery.data) return
    const settings = settingsQuery.data
    companyForm.reset({
      storeName: settings.company.storeName,
      description: settings.company.description ?? '',
      logoUrl: settings.company.logoUrl ?? '',
      email: settings.company.email ?? '',
      phone: settings.company.phone ?? '',
      whatsapp: settings.company.whatsapp ?? '',
    })
    providerForm.reset({
      provider: settings.whatsappProvider.provider,
      baseUrl: settings.whatsappProvider.baseUrl,
      token: settings.whatsappProvider.token,
      instanceName: settings.whatsappProvider.instanceName,
      webhookSecret: settings.whatsappProvider.webhookSecret,
    })
    aiForm.reset({
      provider: settings.aiConfiguration.provider,
      apiKey: settings.aiConfiguration.apiKey,
      model: settings.aiConfiguration.model,
    })
    setBusinessHours(settings.businessHours)
    setFaqs(settings.faqs)
  }, [aiForm, companyForm, providerForm, settingsQuery.data])

  const invalidateSettings = async () => {
    await queryClient.invalidateQueries({ queryKey: ['settings'] })
  }

  const companyMutation = useMutation({
    mutationFn: (payload: CompanySettings) => updateCompany(payload),
    onSuccess: async () => {
      toast.success('Dados da empresa atualizados')
      await invalidateSettings()
    },
    onError: () => toast.error('Não foi possível salvar os dados da empresa'),
  })

  const businessHoursMutation = useMutation({
    mutationFn: (payload: BusinessHour[]) => updateBusinessHours(payload),
    onSuccess: async () => {
      toast.success('Horários atualizados')
      await invalidateSettings()
    },
    onError: () => toast.error('Não foi possível salvar os horários'),
  })

  const faqsMutation = useMutation({
    mutationFn: (payload: Faq[]) => updateFaqs(payload),
    onSuccess: async () => {
      toast.success('FAQs atualizadas')
      await invalidateSettings()
    },
    onError: () => toast.error('Não foi possível salvar as FAQs'),
  })

  const providerMutation = useMutation({
    mutationFn: (payload: WhatsAppProviderSettings) => updateWhatsappProvider(payload),
    onSuccess: async () => {
      toast.success('Configuração do provedor atualizada')
      await invalidateSettings()
    },
    onError: () => toast.error('Não foi possível salvar o provedor'),
  })

  const aiMutation = useMutation({
    mutationFn: (payload: AiConfiguration) => updateAiConfiguration(payload),
    onSuccess: async () => {
      toast.success('Configuração de IA atualizada')
      await invalidateSettings()
    },
    onError: () => toast.error('Não foi possível salvar a configuração de IA'),
  })

  const normalizedFaqs = useMemo(() => faqs.map((faq, index) => ({ ...faq, id: faq.id || `faq-${index}` })), [faqs])

  if (settingsQuery.isError) {
    return (
      <Card title="Configurações indisponíveis">
        <p className="text-sm text-rose-600">Não foi possível carregar as configurações do ambiente.</p>
      </Card>
    )
  }

  return (
    <div className="space-y-6">
      <div>
        <h1 className="text-3xl font-semibold text-slate-900">Configurações</h1>
        <p className="mt-1 text-sm text-slate-500">Ajuste dados da empresa, horários, FAQs, provedor WhatsApp e IA.</p>
      </div>

      <div className="flex flex-wrap gap-2">
        {tabs.map((tab) => (
          <button
            key={tab.key}
            className={`rounded-xl px-4 py-2 text-sm font-medium transition ${activeTab === tab.key ? 'bg-indigo-600 text-white shadow-lg shadow-indigo-600/20' : 'bg-white text-slate-600 ring-1 ring-slate-200 hover:bg-slate-50'}`}
            onClick={() => setActiveTab(tab.key)}
            type="button"
          >
            {tab.label}
          </button>
        ))}
      </div>

      {settingsQuery.isLoading ? (
        <div className="space-y-4">
          {Array.from({ length: 4 }).map((_, index) => <div key={index} className="h-28 animate-pulse rounded-3xl bg-white" />)}
        </div>
      ) : null}

      {!settingsQuery.isLoading && activeTab === 'company' ? (
        <Card title="Dados da empresa">
          <form className="grid gap-4 md:grid-cols-2" onSubmit={(event) => void companyForm.handleSubmit(async (values) => companyMutation.mutateAsync(values))(event)}>
            <Input error={companyForm.formState.errors.storeName?.message} label="Nome da loja" {...companyForm.register('storeName')} />
            <Input error={companyForm.formState.errors.logoUrl?.message} label="URL do logo" {...companyForm.register('logoUrl')} />
            <label className="md:col-span-2">
              <span className="mb-1.5 block text-sm font-medium text-slate-700">Descrição</span>
              <textarea className="min-h-28 w-full rounded-xl border border-slate-200 px-3 py-2 text-sm outline-none focus:border-indigo-500 focus:ring-4 focus:ring-indigo-100" {...companyForm.register('description')} />
            </label>
            <Input error={companyForm.formState.errors.email?.message} label="E-mail" {...companyForm.register('email')} />
            <Input label="Telefone" {...companyForm.register('phone')} />
            <Input label="WhatsApp" {...companyForm.register('whatsapp')} />
            <div className="md:col-span-2 flex justify-end">
              <Button loading={companyMutation.isPending} type="submit"><Save className="h-4 w-4" />Salvar empresa</Button>
            </div>
          </form>
        </Card>
      ) : null}

      {!settingsQuery.isLoading && activeTab === 'hours' ? (
        <Card title="Horários de atendimento">
          <div className="space-y-4">
            {businessHours.map((entry, index) => (
              <div key={entry.dayOfWeek} className="grid gap-3 rounded-2xl border border-slate-100 p-4 md:grid-cols-[1fr_150px_150px_120px] md:items-center">
                <div>
                  <p className="font-medium text-slate-900">{dayLabels[entry.dayOfWeek]}</p>
                  <p className="text-sm text-slate-500">Controle o expediente por dia da semana.</p>
                </div>
                <Input type="time" value={entry.openTime ?? ''} onChange={(event) => setBusinessHours((current) => current.map((item, currentIndex) => currentIndex === index ? { ...item, openTime: event.target.value } : item))} />
                <Input type="time" value={entry.closeTime ?? ''} onChange={(event) => setBusinessHours((current) => current.map((item, currentIndex) => currentIndex === index ? { ...item, closeTime: event.target.value } : item))} />
                <label className="flex items-center gap-3 text-sm text-slate-700">
                  <input checked={entry.enabled} onChange={(event) => setBusinessHours((current) => current.map((item, currentIndex) => currentIndex === index ? { ...item, enabled: event.target.checked } : item))} type="checkbox" />
                  Aberto
                </label>
              </div>
            ))}
            <div className="flex justify-end">
              <Button loading={businessHoursMutation.isPending} onClick={() => void businessHoursMutation.mutateAsync(businessHours)}><Save className="h-4 w-4" />Salvar horários</Button>
            </div>
          </div>
        </Card>
      ) : null}

      {!settingsQuery.isLoading && activeTab === 'faqs' ? (
        <Card title="Perguntas frequentes" action={<Button onClick={() => setFaqs((current) => [...current, { id: '', question: '', answer: '' }])} variant="secondary"><Plus className="h-4 w-4" />Nova FAQ</Button>}>
          {normalizedFaqs.length > 0 ? (
            <div className="space-y-4">
              {normalizedFaqs.map((faq, index) => (
                <div key={faq.id} className="rounded-2xl border border-slate-100 p-4">
                  <div className="mb-3 flex justify-end">
                    <Button onClick={() => setFaqs((current) => current.filter((_, currentIndex) => currentIndex !== index))} size="sm" variant="ghost">
                      <Trash2 className="h-4 w-4" />
                    </Button>
                  </div>
                  <div className="grid gap-4">
                    <Input label="Pergunta" value={faq.question} onChange={(event) => setFaqs((current) => current.map((item, currentIndex) => currentIndex === index ? { ...item, question: event.target.value } : item))} />
                    <label>
                      <span className="mb-1.5 block text-sm font-medium text-slate-700">Resposta</span>
                      <textarea className="min-h-28 w-full rounded-xl border border-slate-200 px-3 py-2 text-sm outline-none focus:border-indigo-500 focus:ring-4 focus:ring-indigo-100" value={faq.answer} onChange={(event) => setFaqs((current) => current.map((item, currentIndex) => currentIndex === index ? { ...item, answer: event.target.value } : item))} />
                    </label>
                  </div>
                </div>
              ))}
              <div className="flex justify-end">
                <Button
                  loading={faqsMutation.isPending}
                  onClick={() => {
                    if (normalizedFaqs.some((faq) => !faq.question.trim() || !faq.answer.trim())) {
                      toast.error('Preencha pergunta e resposta em todas as FAQs')
                      return
                    }
                    void faqsMutation.mutateAsync(normalizedFaqs)
                  }}
                >
                  <Save className="h-4 w-4" />Salvar FAQs
                </Button>
              </div>
            </div>
          ) : (
            <EmptyState description="Crie respostas padrão para dúvidas frequentes do WhatsApp." icon={Plus} title="Nenhuma FAQ cadastrada" />
          )}
        </Card>
      ) : null}

      {!settingsQuery.isLoading && activeTab === 'provider' ? (
        <Card title="Provedor WhatsApp">
          <form className="grid gap-4 md:grid-cols-2" onSubmit={(event) => void providerForm.handleSubmit(async (values) => providerMutation.mutateAsync(values))(event)}>
            <Select
              error={providerForm.formState.errors.provider?.message}
              label="Provedor"
              options={Object.values(ProviderType).map((provider) => ({ label: provider.toUpperCase(), value: provider }))}
              {...providerForm.register('provider')}
            />
            <Input error={providerForm.formState.errors.instanceName?.message} label="Nome da instância" {...providerForm.register('instanceName')} />
            <Input error={providerForm.formState.errors.baseUrl?.message} label="Base URL" {...providerForm.register('baseUrl')} />
            <Input error={providerForm.formState.errors.token?.message} label="Token" {...providerForm.register('token')} />
            <Input error={providerForm.formState.errors.webhookSecret?.message} label="Webhook secret" {...providerForm.register('webhookSecret')} />
            <div className="md:col-span-2 flex justify-end">
              <Button loading={providerMutation.isPending} type="submit"><Save className="h-4 w-4" />Salvar provedor</Button>
            </div>
          </form>
        </Card>
      ) : null}

      {!settingsQuery.isLoading && activeTab === 'ai' ? (
        <Card title="Configuração de IA">
          <form className="grid gap-4 md:grid-cols-2" onSubmit={(event) => void aiForm.handleSubmit(async (values) => aiMutation.mutateAsync(values))(event)}>
            <Input error={aiForm.formState.errors.provider?.message} label="Provedor" {...aiForm.register('provider')} />
            <Input error={aiForm.formState.errors.model?.message} label="Modelo" {...aiForm.register('model')} />
            <Input error={aiForm.formState.errors.apiKey?.message} label="API key" className="md:col-span-2" {...aiForm.register('apiKey')} />
            <div className="md:col-span-2 flex justify-end">
              <Button loading={aiMutation.isPending} type="submit"><Save className="h-4 w-4" />Salvar IA</Button>
            </div>
          </form>
        </Card>
      ) : null}
    </div>
  )
}

export default SettingsPage
