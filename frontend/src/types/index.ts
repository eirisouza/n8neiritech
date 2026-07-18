export enum UserRole {
  Admin = 'admin',
  Manager = 'manager',
  Agent = 'agent',
}

export enum ConversationStatus {
  Open = 'open',
  Pending = 'pending',
  Assigned = 'assigned',
  Paused = 'paused',
  Closed = 'closed',
  Escalated = 'escalated',
}

export enum MessageType {
  Text = 'text',
  Image = 'image',
  Audio = 'audio',
  Video = 'video',
  Document = 'document',
  Template = 'template',
  System = 'system',
}

export enum OrderStatus {
  Draft = 'draft',
  Pending = 'pending',
  Confirmed = 'confirmed',
  Processing = 'processing',
  Shipped = 'shipped',
  Delivered = 'delivered',
  Cancelled = 'cancelled',
}

export enum ProductStatus {
  Active = 'active',
  Inactive = 'inactive',
  OutOfStock = 'out_of_stock',
}

export enum CustomerStatus {
  Active = 'active',
  Blocked = 'blocked',
  Inactive = 'inactive',
  Vip = 'vip',
}

export enum WhatsAppInstanceStatus {
  Connected = 'connected',
  Disconnected = 'disconnected',
  Connecting = 'connecting',
  Error = 'error',
}

export enum ProviderType {
  Evolution = 'evolution',
  Meta = 'meta',
  Twilio = 'twilio',
  Other = 'other',
}

export enum PaymentMethod {
  Pix = 'pix',
  CreditCard = 'credit_card',
  DebitCard = 'debit_card',
  Cash = 'cash',
  BankSlip = 'bank_slip',
}

export interface PagedResult<T> {
  items: T[]
  page: number
  pageSize: number
  total: number
  totalPages: number
}

export interface Tenant {
  id: string
  name: string
  slug: string
  createdAt: string
}

export interface Store {
  id: string
  tenantId: string
  name: string
  description?: string
  logoUrl?: string
  email?: string
  phone?: string
  whatsapp?: string
  createdAt: string
  updatedAt: string
}

export interface User {
  id: string
  tenantId: string
  storeId?: string
  name: string
  email: string
  phone?: string
  role: UserRole
  avatarUrl?: string
  active: boolean
  createdAt: string
  updatedAt: string
}

export interface ProductCategory {
  id: string
  name: string
  description?: string
  active: boolean
  createdAt: string
  updatedAt: string
}

export interface ProductImage {
  id: string
  url: string
  alt?: string
  sortOrder: number
}

export interface ProductVariant {
  id: string
  sku: string
  name: string
  price: number
  promotionalPrice?: number
  stock: number
  active: boolean
}

export interface Product {
  id: string
  tenantId: string
  categoryId?: string
  category?: ProductCategory
  name: string
  sku: string
  description?: string
  brand?: string
  price: number
  promotionalPrice?: number
  stock: number
  unit: string
  status: ProductStatus
  active: boolean
  images: ProductImage[]
  variants: ProductVariant[]
  createdAt: string
  updatedAt: string
}

export interface CustomerAddress {
  id: string
  label?: string
  street: string
  number: string
  complement?: string
  neighborhood?: string
  city: string
  state: string
  zipCode: string
  reference?: string
}

export interface Customer {
  id: string
  tenantId: string
  name: string
  email?: string
  phone: string
  document?: string
  notes?: string
  status: CustomerStatus
  blocked: boolean
  totalOrders: number
  totalSpent: number
  lastInteractionAt?: string
  addresses: CustomerAddress[]
  createdAt: string
  updatedAt: string
}

export interface ConversationMessage {
  id: string
  conversationId: string
  senderId?: string
  senderName: string
  direction: 'inbound' | 'outbound'
  type: MessageType
  content: string
  createdAt: string
  deliveredAt?: string
  readAt?: string
  automated: boolean
}

export interface Conversation {
  id: string
  customerId: string
  customer?: Customer
  assigneeId?: string
  assignee?: User
  whatsappInstanceId?: string
  status: ConversationStatus
  subject?: string
  unreadCount: number
  lastMessagePreview?: string
  lastMessageAt?: string
  botPaused: boolean
  tags: string[]
  createdAt: string
  updatedAt: string
}

export interface OrderItem {
  id: string
  productId: string
  productName: string
  sku?: string
  quantity: number
  unitPrice: number
  totalPrice: number
}

export interface StatusHistoryEntry {
  id: string
  status: OrderStatus
  description?: string
  createdAt: string
  createdBy?: string
}

export interface Order {
  id: string
  tenantId: string
  customerId: string
  customer?: Customer
  number: string
  status: OrderStatus
  paymentMethod: PaymentMethod
  subtotal: number
  discount: number
  deliveryFee: number
  total: number
  notes?: string
  cancellationReason?: string
  deliveryAddress?: CustomerAddress
  items: OrderItem[]
  statusHistory: StatusHistoryEntry[]
  createdAt: string
  updatedAt: string
}

export interface WhatsAppInstance {
  id: string
  name: string
  phoneNumber?: string
  provider: ProviderType
  status: WhatsAppInstanceStatus
  connectedAt?: string
  qrCode?: string
  lastError?: string
  webhookUrl?: string
  recentErrors: string[]
}

export interface BusinessHour {
  dayOfWeek: number
  enabled: boolean
  openTime?: string
  closeTime?: string
}

export interface Faq {
  id: string
  question: string
  answer: string
  createdAt?: string
  updatedAt?: string
}

export interface TopProductMetric {
  productId: string
  name: string
  quantity: number
  revenue: number
}

export interface DashboardMetrics {
  todayConversations: number
  customers: number
  orders: number
  revenue: number
  conversionRate: number
  humanHandoffs: number
  averageResponseTimeSeconds: number
  errors: number
  instanceStatuses: WhatsAppInstance[]
  topProducts: TopProductMetric[]
}

export interface LoginRequest {
  email: string
  password: string
}

export interface AuthResponse {
  token: string
  refreshToken: string
  expiresAt?: string
  user: User
}

export interface ApiError {
  message: string
  errors?: Record<string, string[]>
}

export interface ProductFilters {
  page?: number
  pageSize?: number
  search?: string
  categoryId?: string
  status?: ProductStatus | 'all'
}

export interface ConversationFilters {
  page?: number
  pageSize?: number
  search?: string
  status?: ConversationStatus | 'all'
  assignment?: 'all' | 'me' | 'unassigned'
}

export interface OrderFilters {
  page?: number
  pageSize?: number
  status?: OrderStatus | 'all'
  startDate?: string
  endDate?: string
}

export interface CustomerFilters {
  page?: number
  pageSize?: number
  search?: string
  status?: CustomerStatus | 'all'
}

export interface CompanySettings {
  storeName: string
  description?: string
  logoUrl?: string
  email?: string
  phone?: string
  whatsapp?: string
}

export interface WhatsAppProviderSettings {
  provider: ProviderType
  baseUrl: string
  token: string
  instanceName: string
  webhookSecret: string
}

export interface AiConfiguration {
  provider: string
  apiKey: string
  model: string
}

export interface AppSettings {
  company: CompanySettings
  businessHours: BusinessHour[]
  faqs: Faq[]
  whatsappProvider: WhatsAppProviderSettings
  aiConfiguration: AiConfiguration
}
