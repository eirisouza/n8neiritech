# Plano de Implementação — n8neiritech

## Visão Geral

Sistema completo de atendimento automático via WhatsApp para comércios,
orquestrado pelo n8n, com API ASP.NET Core, painel React e PostgreSQL + Redis.

## Fases

### Fase 1 — Infraestrutura Base
- [x] Estrutura de diretórios
- [x] `docker-compose.yml`
- [x] `.env.example`
- [x] `README.md` completo

### Fase 2 — Backend (ASP.NET Core)
- [x] Solução e projetos (.sln, Domain, Application, Infrastructure, Api)
- [x] Entidades de domínio
- [x] DbContext + configurações EF Core
- [x] Migrations iniciais
- [x] Seed de dados
- [x] Autenticação JWT + Refresh Token
- [x] Endpoints de Auth, Tenants, Stores, Products, Customers, Conversations, Orders, WhatsApp, Dashboard
- [x] FluentValidation
- [x] Serilog
- [x] Health Checks
- [x] Swagger/OpenAPI
- [x] Rate Limiting
- [x] CORS
- [x] RBAC
- [x] Idempotência (Redis)
- [x] WhatsApp abstraction layer
- [x] AI abstraction layer
- [x] Background services

### Fase 3 — Frontend (React + TypeScript + Vite)
- [x] Scaffold Vite + React + TypeScript
- [x] Autenticação (Login, Refresh Token)
- [x] Dashboard
- [x] Inbox (conversas)
- [x] Produtos (CRUD + importação)
- [x] Pedidos
- [x] Clientes
- [x] Configurações
- [x] Instância WhatsApp

### Fase 4 — Workflows n8n (15 JSON)
- [x] 01-whatsapp-inbound-message.json
- [x] 02-process-customer-message.json
- [x] 03-whatsapp-send-message.json
- [x] 04-product-search.json
- [x] 05-order-builder.json
- [x] 06-human-handoff.json
- [x] 07-product-sync.json
- [x] 08-abandoned-cart-recovery.json
- [x] 09-customer-satisfaction.json
- [x] 10-error-handler.json
- [x] 11-whatsapp-instance-monitor.json
- [x] 12-daily-operator-summary.json
- [x] 13-order-status-notification.json
- [x] 14-media-processing.json
- [x] 15-data-maintenance.json

### Fase 5 — Banco de Dados
- [x] Migrations EF Core
- [x] Seed SQL (empresa demo, produtos, usuário admin)
- [x] Índices e constraints

### Fase 6 — Testes
- [x] Testes unitários (Domain + Application)
- [x] Testes de integração (API endpoints)
- [x] Payloads de exemplo para webhook
- [x] Testes de isolamento de tenant
- [x] Testes de idempotência

### Fase 7 — Documentação
- [x] README.md com todos os tópicos obrigatórios
- [x] Diagrama de arquitetura (Mermaid)
- [x] Diagrama de fluxo de mensagens
- [x] Diagrama de fluxo de pedidos
- [x] Modelo entidade-relacionamento
- [x] Exemplos de requests/responses
- [x] Coleção Postman/HTTP

## Decisões Técnicas

| Decisão | Escolha |
|---|---|
| Backend | ASP.NET Core 10 (LTS) |
| ORM | Entity Framework Core 10 |
| Banco principal | PostgreSQL 16 |
| Cache/Sessão | Redis 7 |
| Frontend | React 18 + TypeScript + Vite |
| UI Components | Shadcn/ui + Tailwind CSS |
| HTTP Client | Axios + React Query |
| Estado global | Zustand |
| Orquestração | n8n |
| WhatsApp | Abstração + adaptador Evolution API (compatível WA Web) |
| IA | Abstração + adaptador OpenAI (com fallback sem IA) |
| Autenticação | JWT (15min) + Refresh Token (30 dias) |
| Logs | Serilog + Seq (opcional) |
| Containers | Docker Compose v2 |
