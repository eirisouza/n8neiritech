import { Suspense, lazy, useEffect } from 'react'
import { Navigate, Outlet, Route, Routes } from 'react-router-dom'
import { useQuery } from '@tanstack/react-query'
import { Layout } from './components/layout/Layout'
import { Spinner } from './components/ui/Spinner'
import { getCurrentUser } from './services/auth'
import { useAuthStore } from './store/authStore'

const LoginPage = lazy(() => import('./pages/LoginPage'))
const DashboardPage = lazy(() => import('./pages/DashboardPage'))
const InboxPage = lazy(() => import('./pages/InboxPage'))
const ProductsPage = lazy(() => import('./pages/ProductsPage'))
const OrdersPage = lazy(() => import('./pages/OrdersPage'))
const CustomersPage = lazy(() => import('./pages/CustomersPage'))
const SettingsPage = lazy(() => import('./pages/SettingsPage'))
const WhatsAppPage = lazy(() => import('./pages/WhatsAppPage'))

const PageFallback = () => (
  <div className="flex min-h-[60vh] items-center justify-center">
    <Spinner className="h-10 w-10" />
  </div>
)

const ProtectedRoute = () => {
  const token = useAuthStore((state) => state.token)
  if (!token) {
    return <Navigate replace to="/login" />
  }
  return (
    <Layout>
      <Outlet />
    </Layout>
  )
}

const PublicRoute = () => {
  const token = useAuthStore((state) => state.token)
  return token ? <Navigate replace to="/" /> : <Outlet />
}

const AuthBootstrap = () => {
  const { token, refreshToken, user, setAuth, clearAuth } = useAuthStore()

  const currentUserQuery = useQuery({
    queryKey: ['auth-me', token],
    queryFn: getCurrentUser,
    enabled: Boolean(token) && !user,
    retry: 0,
  })

  useEffect(() => {
    if (currentUserQuery.data && token && refreshToken) {
      setAuth({ user: currentUserQuery.data, token, refreshToken })
    }
  }, [currentUserQuery.data, refreshToken, setAuth, token])

  useEffect(() => {
    if (currentUserQuery.isError) {
      clearAuth()
    }
  }, [clearAuth, currentUserQuery.isError])

  return null
}

function App() {
  return (
    <>
      <AuthBootstrap />
      <Suspense fallback={<PageFallback />}>
        <Routes>
          <Route element={<PublicRoute />}>
            <Route path="/login" element={<LoginPage />} />
          </Route>
          <Route element={<ProtectedRoute />}>
            <Route path="/" element={<DashboardPage />} />
            <Route path="/inbox" element={<InboxPage />} />
            <Route path="/products" element={<ProductsPage />} />
            <Route path="/orders" element={<OrdersPage />} />
            <Route path="/customers" element={<CustomersPage />} />
            <Route path="/settings" element={<SettingsPage />} />
            <Route path="/whatsapp" element={<WhatsAppPage />} />
          </Route>
          <Route path="*" element={<Navigate replace to="/" />} />
        </Routes>
      </Suspense>
    </>
  )
}

export default App
