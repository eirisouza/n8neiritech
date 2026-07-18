import axios, { type AxiosError, type AxiosRequestConfig, type InternalAxiosRequestConfig } from 'axios'
import { useAuthStore } from '../store/authStore'
import type {
  AiConfiguration,
  AppSettings,
  AuthResponse,
  BusinessHour,
  CompanySettings,
  Conversation,
  ConversationFilters,
  ConversationMessage,
  Customer,
  CustomerFilters,
  DashboardMetrics,
  Faq,
  LoginRequest,
  Order,
  OrderFilters,
  OrderStatus,
  PagedResult,
  Product,
  ProductCategory,
  ProductFilters,
  TopProductMetric,
  User,
  WhatsAppInstance,
  WhatsAppProviderSettings,
} from '../types'

const API_URL = import.meta.env.VITE_API_URL ?? 'http://localhost:5000'

const api = axios.create({
  baseURL: API_URL,
})

type RetryableRequest = InternalAxiosRequestConfig & { _retry?: boolean }

let isRefreshing = false
let pendingRequests: Array<(token: string | null) => void> = []

const flushPendingRequests = (token: string | null) => {
  pendingRequests.forEach((callback) => callback(token))
  pendingRequests = []
}

api.interceptors.request.use((config) => {
  const { token } = useAuthStore.getState()
  if (token) {
    config.headers.set('Authorization', 'Bearer ' + token)
  }
  return config
})

api.interceptors.response.use(
  (response) => response,
  async (error: AxiosError) => {
    const originalRequest = error.config as RetryableRequest | undefined
    const status = error.response?.status

    if (!originalRequest || status !== 401 || originalRequest._retry || originalRequest.url?.includes('/api/auth/refresh')) {
      return Promise.reject(error)
    }

    originalRequest._retry = true
    const authState = useAuthStore.getState()

    if (!authState.refreshToken) {
      authState.clearAuth()
      window.location.href = '/login'
      return Promise.reject(error)
    }

    if (isRefreshing) {
      return new Promise((resolve, reject) => {
        pendingRequests.push((token) => {
          if (!token) {
            reject(error)
            return
          }
          originalRequest.headers.set('Authorization', 'Bearer ' + token)
          resolve(api(originalRequest))
        })
      })
    }

    isRefreshing = true

    try {
      const response = await axios.post<AuthResponse>(`${API_URL}/api/auth/refresh`, {
        refreshToken: authState.refreshToken,
      })
      authState.setAuth({
        user: response.data.user,
        token: response.data.token,
        refreshToken: response.data.refreshToken,
      })
      flushPendingRequests(response.data.token)
      originalRequest.headers.set('Authorization', 'Bearer ' + response.data.token)
      return api(originalRequest)
    } catch (refreshError) {
      flushPendingRequests(null)
      authState.clearAuth()
      window.location.href = '/login'
      return Promise.reject(refreshError)
    } finally {
      isRefreshing = false
    }
  },
)

const get = async <T>(url: string, params?: object) => {
  const response = await api.get<T>(url, { params })
  return response.data
}

const post = async <T>(url: string, data?: unknown, config?: AxiosRequestConfig) => {
  const response = await api.post<T>(url, data, config)
  return response.data
}

const put = async <T>(url: string, data?: unknown) => {
  const response = await api.put<T>(url, data)
  return response.data
}

const patch = async <T>(url: string, data?: unknown) => {
  const response = await api.patch<T>(url, data)
  return response.data
}

const del = async <T>(url: string, data?: unknown) => {
  const response = await api.delete<T>(url, { data })
  return response.data
}

export const authApi = {
  login: (payload: LoginRequest) => post<AuthResponse>('/api/auth/login', payload),
  logout: () => post<void>('/api/auth/logout'),
  refreshToken: (refreshToken: string) => post<AuthResponse>('/api/auth/refresh', { refreshToken }),
  getCurrentUser: () => get<User>('/api/auth/me'),
}

export const productsApi = {
  getProducts: (filters: ProductFilters) => get<PagedResult<Product>>('/api/products', filters),
  getProduct: (id: string) => get<Product>(`/api/products/${id}`),
  createProduct: (payload: Partial<Product>) => post<Product>('/api/products', payload),
  updateProduct: (id: string, payload: Partial<Product>) => put<Product>(`/api/products/${id}`, payload),
  deleteProduct: (id: string) => del<void>(`/api/products/${id}`),
  getCategories: () => get<ProductCategory[]>('/api/products/categories'),
  createCategory: (payload: Partial<ProductCategory>) => post<ProductCategory>('/api/products/categories', payload),
  importProducts: (file: File) => {
    const formData = new FormData()
    formData.append('file', file)
    return post<{ imported: number; errors: string[] }>('/api/products/import', formData, {
      headers: { 'Content-Type': 'multipart/form-data' },
    })
  },
  searchProducts: (query: string) => get<Product[]>('/api/products/search', { query }),
}

export const conversationsApi = {
  getConversations: (filters: ConversationFilters) => get<PagedResult<Conversation>>('/api/conversations', filters),
  getConversation: (id: string) => get<Conversation>(`/api/conversations/${id}`),
  getMessages: (conversationId: string) => get<ConversationMessage[]>(`/api/conversations/${conversationId}/messages`),
  sendMessage: (conversationId: string, content: string) => post<ConversationMessage>(`/api/conversations/${conversationId}/messages`, { content }),
  assignConversation: (conversationId: string, assigneeId: string) => patch<Conversation>(`/api/conversations/${conversationId}/assign`, { assigneeId }),
  closeConversation: (conversationId: string) => patch<Conversation>(`/api/conversations/${conversationId}/close`),
  pauseBot: (conversationId: string) => patch<Conversation>(`/api/conversations/${conversationId}/pause-bot`),
  resumeBot: (conversationId: string) => patch<Conversation>(`/api/conversations/${conversationId}/resume-bot`),
}

export const ordersApi = {
  getOrders: (filters: OrderFilters) => get<PagedResult<Order>>('/api/orders', filters),
  getOrder: (id: string) => get<Order>(`/api/orders/${id}`),
  updateOrderStatus: (id: string, status: OrderStatus) => patch<Order>(`/api/orders/${id}/status`, { status }),
  cancelOrder: (id: string, reason: string) => patch<Order>(`/api/orders/${id}/cancel`, { reason }),
}

export const customersApi = {
  getCustomers: (filters: CustomerFilters) => get<PagedResult<Customer>>('/api/customers', filters),
  getCustomer: (id: string) => get<Customer>(`/api/customers/${id}`),
  updateCustomer: (id: string, payload: Partial<Customer>) => put<Customer>(`/api/customers/${id}`, payload),
  blockCustomer: (id: string) => patch<Customer>(`/api/customers/${id}/block`),
  unblockCustomer: (id: string) => patch<Customer>(`/api/customers/${id}/unblock`),
}

export const dashboardApi = {
  getDashboardMetrics: () => get<DashboardMetrics>('/api/dashboard/metrics'),
  getTodayConversations: () => get<Conversation[]>('/api/dashboard/today-conversations'),
  getTopProducts: () => get<TopProductMetric[]>('/api/dashboard/top-products'),
}

export const whatsappApi = {
  getInstances: () => get<WhatsAppInstance[]>('/api/whatsapp/instances'),
  getInstanceStatus: (id: string) => get<WhatsAppInstance>(`/api/whatsapp/instances/${id}/status`),
  getQrCode: (id: string) => get<{ qrCode: string }>(`/api/whatsapp/instances/${id}/qr-code`),
  connectInstance: (id: string) => post<WhatsAppInstance>(`/api/whatsapp/instances/${id}/connect`),
  disconnectInstance: (id: string) => post<WhatsAppInstance>(`/api/whatsapp/instances/${id}/disconnect`),
  restartInstance: (id: string) => post<WhatsAppInstance>(`/api/whatsapp/instances/${id}/restart`),
  testInstance: (id: string) => post<{ success: boolean; message: string }>(`/api/whatsapp/instances/${id}/test`),
}

export const settingsApi = {
  getSettings: () => get<AppSettings>('/api/settings'),
  updateCompany: (payload: CompanySettings) => put<CompanySettings>('/api/settings/company', payload),
  updateBusinessHours: (payload: BusinessHour[]) => put<BusinessHour[]>('/api/settings/business-hours', payload),
  updateFaqs: (payload: Faq[]) => put<Faq[]>('/api/settings/faqs', payload),
  updateWhatsappProvider: (payload: WhatsAppProviderSettings) => put<WhatsAppProviderSettings>('/api/settings/whatsapp-provider', payload),
  updateAiConfiguration: (payload: AiConfiguration) => put<AiConfiguration>('/api/settings/ai-configuration', payload),
}

export default api
