import { zodResolver } from '@hookform/resolvers/zod'
import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query'
import { FileUp, Pencil, Plus, Trash2 } from 'lucide-react'
import { useEffect, useRef, useState } from 'react'
import { useForm } from 'react-hook-form'
import toast from 'react-hot-toast'
import { z } from 'zod'
import { Button } from '../components/ui/Button'
import { Card } from '../components/ui/Card'
import { Badge } from '../components/ui/Badge'
import { EmptyState } from '../components/ui/EmptyState'
import { Input } from '../components/ui/Input'
import { Modal } from '../components/ui/Modal'
import { Pagination } from '../components/ui/Pagination'
import { SearchInput } from '../components/ui/SearchInput'
import { Select } from '../components/ui/Select'
import { Table } from '../components/ui/Table'
import { createProduct, deleteProduct, getCategories, getProducts, importProducts, updateProduct } from '../services/products'
import { ProductStatus, type Product, type ProductCategory, type ProductFilters } from '../types'
import { formatCurrency, getStatusLabel } from '../utils/formatters'

const productSchema = z.object({
  name: z.string().min(2, 'Informe o nome do produto'),
  sku: z.string().min(2, 'Informe o SKU'),
  description: z.string().optional(),
  categoryId: z.string().optional(),
  brand: z.string().optional(),
  price: z.string().min(1, 'Informe o preço'),
  promotionalPrice: z.string().optional(),
  stock: z.string().min(1, 'Informe o estoque'),
  unit: z.string().min(1, 'Informe a unidade'),
  status: z.nativeEnum(ProductStatus),
  active: z.boolean(),
  imageUrls: z.string().optional(),
})

type ProductFormValues = z.infer<typeof productSchema>

const defaultValues: ProductFormValues = {
  name: '',
  sku: '',
  description: '',
  categoryId: '',
  brand: '',
  price: '',
  promotionalPrice: '',
  stock: '',
  unit: 'un',
  status: ProductStatus.Active,
  active: true,
  imageUrls: '',
}

const toPayload = (values: ProductFormValues) => ({
  name: values.name,
  sku: values.sku,
  description: values.description,
  categoryId: values.categoryId || undefined,
  brand: values.brand,
  price: Number(values.price),
  promotionalPrice: values.promotionalPrice ? Number(values.promotionalPrice) : undefined,
  stock: Number(values.stock),
  unit: values.unit,
  status: values.status,
  active: values.active,
  images: (values.imageUrls ?? '')
    .split('\n')
    .map((url) => url.trim())
    .filter(Boolean)
    .map((url, index) => ({ id: `${index}`, url, sortOrder: index })),
})

const ProductsPage = () => {
  const queryClient = useQueryClient()
  const importInputRef = useRef<HTMLInputElement | null>(null)
  const [filters, setFilters] = useState<ProductFilters>({ page: 1, pageSize: 10, search: '', categoryId: '', status: 'all' })
  const [isModalOpen, setIsModalOpen] = useState(false)
  const [deleteTarget, setDeleteTarget] = useState<Product | null>(null)
  const [editingProduct, setEditingProduct] = useState<Product | null>(null)

  const productsQuery = useQuery({
    queryKey: ['products', filters],
    queryFn: () => getProducts(filters),
  })

  const categoriesQuery = useQuery({
    queryKey: ['product-categories'],
    queryFn: getCategories,
  })

  const {
    register,
    handleSubmit,
    reset,
    formState: { errors },
    setValue,
    watch,
  } = useForm<ProductFormValues>({
    resolver: zodResolver(productSchema),
    defaultValues,
  })

  useEffect(() => {
    if (!editingProduct) {
      reset(defaultValues)
      return
    }

    reset({
      name: editingProduct.name,
      sku: editingProduct.sku,
      description: editingProduct.description ?? '',
      categoryId: editingProduct.categoryId ?? '',
      brand: editingProduct.brand ?? '',
      price: String(editingProduct.price),
      promotionalPrice: editingProduct.promotionalPrice ? String(editingProduct.promotionalPrice) : '',
      stock: String(editingProduct.stock),
      unit: editingProduct.unit,
      status: editingProduct.status,
      active: editingProduct.active,
      imageUrls: editingProduct.images.map((image) => image.url).join('\n'),
    })
  }, [editingProduct, reset])

  const invalidateProducts = async () => {
    await Promise.all([
      queryClient.invalidateQueries({ queryKey: ['products'] }),
      queryClient.invalidateQueries({ queryKey: ['product-categories'] }),
    ])
  }

  const saveMutation = useMutation({
    mutationFn: async (values: ProductFormValues) => {
      const payload = toPayload(values)
      return editingProduct ? updateProduct(editingProduct.id, payload) : createProduct(payload)
    },
    onSuccess: async () => {
      toast.success(editingProduct ? 'Produto atualizado' : 'Produto criado')
      setIsModalOpen(false)
      setEditingProduct(null)
      reset(defaultValues)
      await invalidateProducts()
    },
    onError: () => toast.error('Não foi possível salvar o produto'),
  })

  const deleteMutation = useMutation({
    mutationFn: async (productId: string) => deleteProduct(productId),
    onSuccess: async () => {
      toast.success('Produto removido')
      setDeleteTarget(null)
      await invalidateProducts()
    },
    onError: () => toast.error('Não foi possível remover o produto'),
  })

  const importMutation = useMutation({
    mutationFn: importProducts,
    onSuccess: async (data) => {
      toast.success(`${data.imported} produtos importados`)
      await invalidateProducts()
    },
    onError: () => toast.error('Não foi possível importar o arquivo'),
  })

  const onSubmit = async (values: ProductFormValues) => {
    await saveMutation.mutateAsync(values)
  }

  const openCreateModal = () => {
    setEditingProduct(null)
    reset(defaultValues)
    setIsModalOpen(true)
  }

  const openEditModal = (product: Product) => {
    setEditingProduct(product)
    setIsModalOpen(true)
  }

  const categories = categoriesQuery.data ?? []
  const watchedImageUrls = watch('imageUrls')
  const imagePreview = (watchedImageUrls ?? '').split('\n').map((value) => value.trim()).filter(Boolean)

  return (
    <div className="space-y-6">
      <div className="flex flex-col gap-4 xl:flex-row xl:items-center xl:justify-between">
        <div>
          <h1 className="text-3xl font-semibold text-slate-900">Produtos</h1>
          <p className="mt-1 text-sm text-slate-500">Gerencie catálogo, preços, estoque e importações de forma centralizada.</p>
        </div>
        <div className="flex flex-wrap gap-3">
          <input
            className="hidden"
            onChange={(event) => {
              const file = event.target.files?.[0]
              if (file) {
                void importMutation.mutateAsync(file)
              }
            }}
            ref={importInputRef}
            type="file"
            accept=".csv,.xlsx,.xls"
          />
          <Button loading={importMutation.isPending} onClick={() => importInputRef.current?.click()} variant="secondary">
            <FileUp className="h-4 w-4" />
            Importar CSV/XLSX
          </Button>
          <Button onClick={openCreateModal}>
            <Plus className="h-4 w-4" />
            Novo produto
          </Button>
        </div>
      </div>

      <Card>
        <div className="grid gap-3 lg:grid-cols-[minmax(0,1fr)_220px_220px]">
          <SearchInput defaultValue={filters.search} onSearch={(search) => setFilters((current) => ({ ...current, page: 1, search }))} placeholder="Buscar por nome ou SKU" />
          <Select
            options={[{ label: 'Todas as categorias', value: '' }, ...categories.map((category: ProductCategory) => ({ label: category.name, value: category.id }))]}
            value={filters.categoryId ?? ''}
            onChange={(event) => setFilters((current) => ({ ...current, page: 1, categoryId: event.target.value }))}
          />
          <Select
            options={[
              { label: 'Todos os status', value: 'all' },
              { label: 'Ativo', value: ProductStatus.Active },
              { label: 'Inativo', value: ProductStatus.Inactive },
              { label: 'Sem estoque', value: ProductStatus.OutOfStock },
            ]}
            value={filters.status ?? 'all'}
            onChange={(event) => setFilters((current) => ({ ...current, page: 1, status: event.target.value as ProductFilters['status'] }))}
          />
        </div>
      </Card>

      <Table
        columns={[
          { key: 'sku', header: 'SKU', render: (product) => <span className="font-medium">{product.sku}</span> },
          {
            key: 'name',
            header: 'Produto',
            render: (product) => (
              <div>
                <p className="font-medium text-slate-900">{product.name}</p>
                <p className="text-xs text-slate-500">{product.brand ?? 'Sem marca'} • {product.unit}</p>
              </div>
            ),
          },
          { key: 'category', header: 'Categoria', render: (product) => product.category?.name ?? 'Sem categoria' },
          { key: 'price', header: 'Preço', render: (product) => formatCurrency(product.promotionalPrice ?? product.price) },
          { key: 'stock', header: 'Estoque', render: (product) => product.stock },
          { key: 'status', header: 'Status', render: (product) => <Badge status={product.status}>{getStatusLabel(product.status)}</Badge> },
          {
            key: 'actions',
            header: 'Ações',
            render: (product) => (
              <div className="flex justify-end gap-2">
                <Button onClick={() => openEditModal(product)} size="sm" variant="secondary">
                  <Pencil className="h-4 w-4" />
                </Button>
                <Button onClick={() => setDeleteTarget(product)} size="sm" variant="danger">
                  <Trash2 className="h-4 w-4" />
                </Button>
              </div>
            ),
          },
        ]}
        data={productsQuery.data?.items ?? []}
        emptyState={<EmptyState description="Cadastre itens ou importe um arquivo para começar." icon={Plus} title="Nenhum produto encontrado" />}
        rowKey={(product) => product.id}
      />

      <Pagination
        onPageChange={(page) => setFilters((current) => ({ ...current, page }))}
        page={productsQuery.data?.page ?? 1}
        totalPages={productsQuery.data?.totalPages ?? 1}
      />

      <Modal
        footer={
          <div className="flex justify-end gap-3">
            <Button onClick={() => setIsModalOpen(false)} variant="secondary">Cancelar</Button>
            <Button loading={saveMutation.isPending} onClick={() => void handleSubmit(onSubmit)()}>{editingProduct ? 'Salvar alterações' : 'Criar produto'}</Button>
          </div>
        }
        onClose={() => setIsModalOpen(false)}
        open={isModalOpen}
        title={editingProduct ? 'Editar produto' : 'Novo produto'}
      >
        <div className="grid gap-4 md:grid-cols-2">
          <Input error={errors.name?.message} label="Nome" {...register('name')} />
          <Input error={errors.sku?.message} label="SKU" {...register('sku')} />
          <label className="md:col-span-2">
            <span className="mb-1.5 block text-sm font-medium text-slate-700">Descrição</span>
            <textarea className="min-h-28 w-full rounded-xl border border-slate-200 px-3 py-2 text-sm outline-none focus:border-indigo-500 focus:ring-4 focus:ring-indigo-100" {...register('description')} />
          </label>
          <Select label="Categoria" options={[{ label: 'Selecione', value: '' }, ...categories.map((category) => ({ label: category.name, value: category.id }))]} {...register('categoryId')} />
          <Input label="Marca" {...register('brand')} />
          <Input error={errors.price?.message} label="Preço" type="number" step="0.01" {...register('price')} />
          <Input label="Preço promocional" type="number" step="0.01" {...register('promotionalPrice')} />
          <Input error={errors.stock?.message} label="Estoque" type="number" {...register('stock')} />
          <Input error={errors.unit?.message} label="Unidade" {...register('unit')} />
          <Select
            error={errors.status?.message}
            label="Status"
            options={[
              { label: 'Ativo', value: ProductStatus.Active },
              { label: 'Inativo', value: ProductStatus.Inactive },
              { label: 'Sem estoque', value: ProductStatus.OutOfStock },
            ]}
            {...register('status')}
          />
          <label className="flex items-center gap-3 rounded-xl border border-slate-200 px-4 py-3 md:mt-7">
            <input checked={watch('active')} onChange={(event) => setValue('active', event.target.checked)} type="checkbox" />
            <span className="text-sm text-slate-700">Produto ativo no catálogo</span>
          </label>
          <label className="md:col-span-2">
            <span className="mb-1.5 block text-sm font-medium text-slate-700">Imagens (uma URL por linha)</span>
            <textarea className="min-h-28 w-full rounded-xl border border-slate-200 px-3 py-2 text-sm outline-none focus:border-indigo-500 focus:ring-4 focus:ring-indigo-100" {...register('imageUrls')} />
          </label>
          <div className="md:col-span-2">
            <p className="mb-2 text-sm font-medium text-slate-700">Pré-visualização das imagens</p>
            <div className="grid gap-3 sm:grid-cols-3">
              {imagePreview.length > 0 ? imagePreview.map((url) => <img key={url} alt="Produto" className="h-28 w-full rounded-2xl border border-slate-200 object-cover" src={url} />) : <div className="rounded-2xl border border-dashed border-slate-200 px-4 py-8 text-sm text-slate-400">Adicione URLs para visualizar as imagens.</div>}
            </div>
          </div>
        </div>
      </Modal>

      <Modal
        footer={
          <div className="flex justify-end gap-3">
            <Button onClick={() => setDeleteTarget(null)} variant="secondary">Cancelar</Button>
            <Button loading={deleteMutation.isPending} onClick={() => deleteTarget && void deleteMutation.mutateAsync(deleteTarget.id)} variant="danger">Excluir produto</Button>
          </div>
        }
        onClose={() => setDeleteTarget(null)}
        open={Boolean(deleteTarget)}
        title="Confirmar exclusão"
        widthClassName="max-w-lg"
      >
        <p className="text-sm text-slate-600">Tem certeza que deseja remover <strong>{deleteTarget?.name}</strong>? Esta ação não poderá ser desfeita.</p>
      </Modal>
    </div>
  )
}

export default ProductsPage
