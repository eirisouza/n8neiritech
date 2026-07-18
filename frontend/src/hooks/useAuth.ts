import { useMutation, useQueryClient } from '@tanstack/react-query'
import { useNavigate } from 'react-router-dom'
import toast from 'react-hot-toast'
import * as authService from '../services/auth'
import { useAuthStore } from '../store/authStore'
import type { LoginRequest } from '../types'

export const useAuth = () => {
  const navigate = useNavigate()
  const queryClient = useQueryClient()
  const { user, token, refreshToken, setAuth, clearAuth } = useAuthStore()

  const loginMutation = useMutation({
    mutationFn: (payload: LoginRequest) => authService.login(payload),
    onSuccess: (data) => {
      setAuth({ user: data.user, token: data.token, refreshToken: data.refreshToken })
      toast.success('Login realizado com sucesso')
      navigate('/')
    },
    onError: () => {
      toast.error('Não foi possível realizar o login')
    },
  })

  const logoutMutation = useMutation({
    mutationFn: authService.logout,
    onSettled: () => {
      clearAuth()
      queryClient.clear()
      navigate('/login')
    },
  })

  return {
    user,
    token,
    refreshToken,
    isAuthenticated: Boolean(token),
    login: (payload: LoginRequest) => loginMutation.mutateAsync(payload),
    logout: () => logoutMutation.mutateAsync(),
    clearAuth,
    setAuth,
    isLoggingIn: loginMutation.isPending,
    isLoggingOut: logoutMutation.isPending,
  }
}
