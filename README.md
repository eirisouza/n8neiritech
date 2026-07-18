# n8neiritech — Sistema de Atendimento Automático para Comércios

> Plataforma completa de atendimento automático via WhatsApp para diferentes tipos de comércio, orquestrada pelo n8n.

## Índice

1. [Visão Geral](#1-visão-geral)
2. [Arquitetura](#2-arquitetura)
3. [Requisitos](#3-requisitos)
4. [Instalação Rápida](#4-instalação-rápida)
5. [Configuração](#5-configuração)
6. [Como Iniciar](#6-como-iniciar)
7. [Importar Workflows no n8n](#7-importar-workflows-no-n8n)
8. [Configurar Credenciais no n8n](#8-configurar-credenciais-no-n8n)
9. [Configurar Provedor WhatsApp](#9-configurar-provedor-whatsapp)
10. [Conectar via QR Code](#10-conectar-via-qr-code)
11. [Configurar Google Sheets](#11-configurar-google-sheets)
12. [Cadastrar Produtos](#12-cadastrar-produtos)
13. [Importar Planilhas](#13-importar-planilhas)
14. [Configurar IA](#14-configurar-ia)
15. [Executar sem IA](#15-executar-sem-ia)
16. [Executar Testes](#16-executar-testes)
17. [Acessar Swagger](#17-acessar-swagger)
18. [Acessar o Painel](#18-acessar-o-painel)
19. [Solucionar Erros](#19-solucionar-erros)
20. [Backup](#20-backup)
21. [Atualizar o Sistema](#21-atualizar-o-sistema)
22. [Riscos da Integração Não Oficial](#22-riscos-da-integração-não-oficial)
23. [Migrar para API Oficial do WhatsApp](#23-migrar-para-api-oficial-do-whatsapp)

---

## 1. Visão Geral

O **n8neiritech** é uma plataforma multiempresa (multi-tenant) de atendimento automático via WhatsApp para comércios de qualquer tipo:

- Mercados, restaurantes, farmácias
- Lojas de roupas, eletrônicos, cosméticos
- Pet shops, materiais de construção, oficinas
- Distribuidores e pequenas empresas em geral

**Funcionalidades principais:**
- Recebe e responde mensagens WhatsApp automaticamente
- Identifica intenção e extrai entidades (produto, quantidade, endereço)
- Consulta produtos, preços e estoque em tempo real
- Monta e confirma pedidos via chat
- Encaminha para atendente humano quando necessário
- Painel web administrativo completo
- Suporte opcional a IA (OpenAI etc.)
- Funciona 100% sem IA com lógica de intenção por palavras-chave

---

## 2. Arquitetura

```
┌──────────────────────────────────────────────────────┐
│                   Cliente WhatsApp                    │
└─────────────────────────┬────────────────────────────┘
                          │
┌─────────────────────────▼────────────────────────────┐
│         Provedor WhatsApp (Evolution API / Fake)      │
└─────────────────────────┬────────────────────────────┘
                          │ Webhook
┌─────────────────────────▼────────────────────────────┐
│                      n8n (Orquestrador)               │
│  15 workflows: inbound → processamento → envio ...   │
└─────────────────────────┬────────────────────────────┘
                          │ HTTP (API Key)
┌─────────────────────────▼────────────────────────────┐
│              Backend ASP.NET Core (.NET 10)           │
│  Auth / Produtos / Conversas / Pedidos / WhatsApp    │
└───────────┬─────────────────────────┬────────────────┘
            │                         │
┌───────────▼────────┐    ┌───────────▼────────┐
│   PostgreSQL 16    │    │     Redis 7         │
│  (dados principais)│    │  (cache/sessões)    │
└────────────────────┘    └────────────────────┘
                    ┌──────────────┐
                    │  Frontend    │
                    │  React + TS  │
                    │  (Vite)      │
                    └──────────────┘
```

**Detalhes:**
- `/backend` — API ASP.NET Core 10, Clean Architecture em camadas
- `/frontend` — React 18 + TypeScript + Vite + Tailwind CSS
- `/n8n/workflows` — 15 workflows JSON importáveis
- `/database` — Migrations EF Core + seed SQL
- `/docs` — Diagramas e documentação adicional
- `/tests` — Payloads de teste + coleção Postman

---

## 3. Requisitos

| Ferramenta | Versão mínima |
|---|---|
| Docker | 24+ |
| Docker Compose | 2.20+ |
| (Opcional) .NET SDK | 10 LTS |
| (Opcional) Node.js | 20 LTS |

---

## 4. Instalação Rápida

```bash
# 1. Clone o repositório
git clone https://github.com/eirisouza/n8neiritech.git
cd n8neiritech

# 2. Configure o ambiente
cp .env.example .env
# Edite .env com suas configurações (mínimo: senhas seguras)

# 3. Suba todos os serviços
docker compose up -d --build

# 4. Aguarde os health checks (aprox. 60s)
docker compose ps

# 5. Acesse o painel
open http://localhost:3000
# Login: admin@demo.com / Admin@123456
```

> ⚠️ **Altere a senha no primeiro acesso!** O sistema exige a troca ao fazer login pela primeira vez.

---

## 5. Configuração

### Variáveis de ambiente essenciais (`.env`)

| Variável | Descrição | Exemplo |
|---|---|---|
| `POSTGRES_PASSWORD` | Senha PostgreSQL | `senha_super_segura` |
| `REDIS_PASSWORD` | Senha Redis | `redis_senha` |
| `JWT_SECRET` | Chave JWT (min 32 chars) | `openssl rand -base64 48` |
| `N8N_ENCRYPTION_KEY` | Chave de criptografia n8n | `openssl rand -hex 32` |
| `N8N_INTERNAL_API_KEY` | Chave API interna (n8n→backend) | `openssl rand -hex 32` |
| `WHATSAPP_WEBHOOK_SECRET` | Segredo do webhook | `openssl rand -hex 24` |
| `WHATSAPP_PROVIDER` | Provedor WA | `Fake` (dev) / `EvolutionApi` (prod) |
| `AI_PROVIDER` | Provedor IA | `None` (sem IA) / `OpenAI` |
| `AI_API_KEY` | Chave OpenAI | `sk-...` |

### Gerar chaves seguras

```bash
openssl rand -base64 48   # para JWT_SECRET
openssl rand -hex 32      # para N8N_ENCRYPTION_KEY
openssl rand -hex 32      # para N8N_INTERNAL_API_KEY
openssl rand -hex 24      # para WHATSAPP_WEBHOOK_SECRET
```

---

## 6. Como Iniciar

```bash
# Subir todos os serviços
docker compose up -d --build

# Ver logs em tempo real
docker compose logs -f backend
docker compose logs -f n8n

# Verificar status
docker compose ps

# Parar tudo
docker compose down

# Parar e remover volumes (CUIDADO: apaga dados)
docker compose down -v
```

**URLs dos serviços:**
| Serviço | URL |
|---|---|
| Frontend (Painel) | http://localhost:3000 |
| Backend (API) | http://localhost:5000 |
| Swagger | http://localhost:5000/swagger |
| n8n | http://localhost:5678 |
| Health Check | http://localhost:5000/health |

---

## 7. Importar Workflows no n8n

1. Acesse http://localhost:5678 (usuário: `admin`, senha: definida em `.env`)
2. Clique em **Workflows** → **New** → **Import from file**
3. Importe cada arquivo de `n8n/workflows/` na ordem:
   - `01-whatsapp-inbound-message.json`
   - `02-process-customer-message.json`
   - `03-whatsapp-send-message.json`
   - ... até `15-data-maintenance.json`
4. Após importar cada um, clique em **Activate** (toggle no canto superior direito)

> **Dica:** Importe o workflow `10-error-handler.json` primeiro, pois é referenciado pelos demais como workflow de erros.

---

## 8. Configurar Credenciais no n8n

Os workflows usam variáveis de ambiente e uma credencial **HTTP Header Auth** para autenticar com o backend.

1. No n8n, acesse **Credentials** → **New**
2. Selecione **HTTP Header Auth**
3. Configure:
   - Name: `Backend API Key`
   - Name: `X-Api-Key`
   - Value: (valor de `N8N_INTERNAL_API_KEY` do `.env`)
4. Salve e selecione esta credencial nos nós HTTP Request dos workflows

### Variáveis de ambiente no n8n

As variáveis `BACKEND_URL`, `N8N_INTERNAL_API_KEY` e `WHATSAPP_WEBHOOK_SECRET` são injetadas automaticamente via `docker-compose.yml`.

---

## 9. Configurar Provedor WhatsApp

### Modo Fake (desenvolvimento)

Por padrão, o sistema usa o provedor `Fake`, que apenas registra as mensagens nos logs sem enviá-las de verdade.

```env
WHATSAPP_PROVIDER=Fake
```

### Evolution API (produção)

1. Descomente o serviço `evolution-api` em `docker-compose.yml`
2. Configure no `.env`:
   ```env
   WHATSAPP_PROVIDER=EvolutionApi
   WHATSAPP_BASE_URL=http://localhost:8080
   WHATSAPP_API_TOKEN=seu-token-evolution
   WHATSAPP_INSTANCE=minha-loja
   ```
3. Acesse o painel → **WhatsApp** → **Instâncias** → **Nova instância**
4. Configure a instância com os mesmos dados

---

## 10. Conectar via QR Code

1. Acesse o Painel Web → **WhatsApp**
2. Clique em **Conectar** na instância desejada
3. Um QR Code será exibido na tela
4. Escaneie com o WhatsApp no celular: **Dispositivos Vinculados** → **Adicionar dispositivo**
5. Aguarde a confirmação de conexão

> ⚠️ **AVISO:** APIs não oficiais podem resultar no banimento da conta. Veja a [seção 22](#22-riscos-da-integração-não-oficial).

---

## 11. Configurar Google Sheets

1. Crie um projeto no [Google Cloud Console](https://console.cloud.google.com)
2. Habilite a **Google Sheets API**
3. Crie credenciais OAuth2 (tipo: aplicativo da Web)
4. Configure as variáveis no `.env`:
   ```env
   GOOGLE_SHEETS_CLIENT_ID=seu-client-id
   GOOGLE_SHEETS_CLIENT_SECRET=seu-client-secret
   GOOGLE_SHEETS_REFRESH_TOKEN=seu-refresh-token
   ```
5. No painel: **Produtos** → **Sincronizar** → **Google Sheets** → insira o ID da planilha

**Formato esperado da planilha:**
| SKU | Nome | Categoria | Preço | Estoque | Ativo |
|---|---|---|---|---|---|
| PROD-001 | Arroz 5kg | Mercearia | 25.90 | 100 | TRUE |

---

## 12. Cadastrar Produtos

### Via Painel Web
1. Acesse **Produtos** → **Novo Produto**
2. Preencha: SKU, Nome, Categoria, Preço, Estoque
3. Adicione imagens e variações se necessário
4. Salve

### Via API REST
```bash
curl -X POST http://localhost:5000/api/products \
  -H "Authorization: ****** <token>" \
  -H "Content-Type: application/json" \
  -d '{"sku":"PROD-001","name":"Arroz Branco 5kg","price":25.90,"stock":100,"isActive":true}'
```

---

## 13. Importar Planilhas

### CSV
```bash
curl -X POST http://localhost:5000/api/products/import \
  -H "Authorization: ****** <token>" \
  -F "file=@produtos.csv" \
  -F "source=Csv"
```

**Formato CSV:**
```csv
sku,name,description,category,brand,price,promotionalPrice,stock,barcode,tags
ARROZ-5KG,Arroz Branco 5kg,Tipo 1 grão longo,Mercearia,Tio João,25.90,,100,7891234567890,"arroz,grãos"
```

### XLSX
Mesmo endpoint, substitua `source=Csv` por `source=Xlsx`.

---

## 14. Configurar IA

```env
AI_PROVIDER=OpenAI
AI_API_KEY=sk-proj-sua-chave-aqui
AI_MODEL=gpt-4o-mini
AI_MAX_TOKENS=1000
```

> **Importante:** O sistema usa IA apenas para interpretar linguagem natural. **Nunca** gera preços, estoque ou informações inventadas. Todos os dados vêm do banco de dados.

### Fluxo RAG implementado
1. Interpreta pergunta do cliente
2. Busca produtos/FAQs no banco
3. Monta contexto com dados reais
4. Gera resposta baseada nos dados encontrados
5. Valida que a resposta não inventa informações

---

## 15. Executar sem IA

O sistema funciona completamente sem IA:

```env
AI_PROVIDER=None
```

Com `AI_Provider=None`, o sistema usa detecção de intenção por palavras-chave:
- "tem / vocês têm / quero" → busca de produto
- "preço / quanto custa / valor" → consulta de preço
- "falar com / atendente / pessoa / humano" → encaminhamento
- "pedido / comprar / finalizar" → gestão de pedido
- "horário / endereço / localização" → informações da loja
- Números (1, 2, 3...) → seleção de opções

---

## 16. Executar Testes

### Backend (.NET)
```bash
cd backend
dotnet test
```

### Frontend (React)
```bash
cd frontend
npm test      # se configurado
npm run build # verifica compilação TypeScript
```

### Testes de Webhook
Use os payloads em `tests/payloads/`:
```bash
# Teste de mensagem de texto
curl -X POST http://localhost:5000/api/whatsapp/webhook \
  -H "Content-Type: application/json" \
  -H "x-webhook-secret: seu-webhook-secret" \
  -d @tests/payloads/webhook-text-message.json
```

### Coleção Postman
Importe `tests/payloads/postman-collection.json` no Postman ou Insomnia.

---

## 17. Acessar Swagger

```
http://localhost:5000/swagger
```

1. Clique em **Authorize**
2. Faça login: POST `/api/auth/login` com `admin@demo.com` / `Admin@123456`
3. Copie o `accessToken` e cole no campo `Bearer`
4. Explore todos os endpoints

---

## 18. Acessar o Painel

```
http://localhost:3000
```

**Credenciais iniciais de desenvolvimento:**
| Campo | Valor |
|---|---|
| Email | `admin@demo.com` |
| Senha | `Admin@123456` |

> ⚠️ **ALTERE A SENHA NO PRIMEIRO ACESSO!**

**Telas disponíveis:**
- **Dashboard** — métricas em tempo real
- **Inbox** — conversas e atendimento
- **Produtos** — catálogo completo
- **Pedidos** — gestão de pedidos
- **Clientes** — base de clientes
- **Configurações** — empresa, horários, FAQs, IA
- **WhatsApp** — instâncias e QR Code

---

## 19. Solucionar Erros

### Backend não inicia
```bash
docker compose logs backend
# Verifique CONNECTION_STRING e credenciais PostgreSQL
```

### n8n não conecta ao backend
```bash
# Verifique se N8N_INTERNAL_API_KEY é igual em .env e no n8n
docker compose exec n8n env | grep N8N_INTERNAL_API_KEY
```

### WhatsApp não recebe mensagens
```bash
# Verifique se o webhook está configurado no provedor
# URL esperada: http://seu-dominio/api/whatsapp/webhook
# Header: x-webhook-secret: <WHATSAPP_WEBHOOK_SECRET>
docker compose logs backend | grep webhook
```

### Migrations não executam
```bash
docker compose exec backend dotnet ef database update
```

### Resetar banco de dados (CUIDADO: apaga tudo)
```bash
docker compose down -v
docker compose up -d
```

### Health check
```bash
curl http://localhost:5000/health
```

---

## 20. Backup

### Backup do PostgreSQL
```bash
docker compose exec postgres pg_dump \
  -U n8neiritech n8neiritech > backup_$(date +%Y%m%d_%H%M%S).sql
```

### Restaurar
```bash
docker compose exec -T postgres psql \
  -U n8neiritech n8neiritech < backup_20260718.sql
```

### Backup n8n (workflows e credenciais)
```bash
docker compose exec n8n n8n export:workflow --all --output=/tmp/workflows.json
docker cp n8neiritech-n8n:/tmp/workflows.json ./backup-workflows.json
```

---

## 21. Atualizar o Sistema

```bash
git pull origin main
docker compose down
docker compose up -d --build
```

As migrations são aplicadas automaticamente no startup.

---

## 22. Riscos da Integração Não Oficial

> ⚠️ **ATENÇÃO — LEIA ANTES DE USAR EM PRODUÇÃO**

Este sistema usa uma integração **não oficial** com o WhatsApp (baseada em WhatsApp Web emulado). Isso implica:

1. **Violação dos Termos de Serviço** do WhatsApp/Meta
2. **Risco de banimento** da conta a qualquer momento
3. **Sem suporte oficial** da Meta
4. **Sem SLA** ou garantias de funcionamento
5. **Pode parar de funcionar** após atualizações do WhatsApp

**Recomendações:**
- Use apenas para testes e desenvolvimento
- Para produção, migre para a [API Oficial do WhatsApp Business](https://developers.facebook.com/docs/whatsapp/)
- Não use em contas pessoais
- Mantenha backups frequentes

---

## 23. Migrar para API Oficial do WhatsApp

O sistema foi projetado para facilitar esta migração:

### Passo 1: Implementar o adaptador oficial

Crie `backend/src/Infrastructure/WhatsApp/OfficialWhatsAppAdapter.cs` implementando `IWhatsAppProvider`.

A interface já está definida em `Application/Interfaces/IWhatsAppProvider.cs`:
```csharp
public interface IWhatsAppProvider
{
    Task<SendMessageResult> SendTextAsync(...);
    Task<SendMessageResult> SendImageAsync(...);
    // ...
}
```

### Passo 2: Configurar variáveis

```env
WHATSAPP_PROVIDER=Official
WHATSAPP_BASE_URL=https://graph.facebook.com/v20.0
WHATSAPP_API_TOKEN=EAAxxxxxxxx  # Token de acesso da API
WHATSAPP_INSTANCE=+5511999990000  # Número de telefone registrado
```

### Passo 3: Registrar o adaptador

Em `Infrastructure/ServiceCollectionExtensions.cs`:
```csharp
case WhatsAppProviderType.Official:
    services.AddScoped<IWhatsAppProvider, OfficialWhatsAppAdapter>();
    break;
```

### Passo 4: Atualizar o webhook

O endpoint já existe: `POST /api/whatsapp/webhook`. Ajuste apenas a validação de assinatura (HMAC SHA-256 da Meta).

---

## Estrutura do Projeto

```
n8neiritech/
├── backend/
│   ├── src/
│   │   ├── Domain/          # Entidades, enums, interfaces
│   │   ├── Application/     # Serviços, DTOs, validators
│   │   ├── Infrastructure/  # EF Core, Redis, WhatsApp, IA
│   │   └── Api/             # Controllers, middleware, Program.cs
│   ├── tests/
│   │   ├── Application.Tests/
│   │   └── Api.Tests/
│   └── Dockerfile
├── frontend/
│   ├── src/
│   │   ├── components/     # UI e layout
│   │   ├── pages/          # Dashboard, Inbox, Produtos, etc.
│   │   ├── services/       # Chamadas à API
│   │   ├── store/          # Estado global (Zustand)
│   │   ├── types/          # Tipos TypeScript
│   │   └── utils/          # Formatadores, constantes
│   ├── Dockerfile
│   └── nginx.conf
├── n8n/
│   └── workflows/          # 15 workflows JSON
├── database/
│   └── seeds/
├── docs/
│   └── architecture.md
├── tests/
│   └── payloads/           # Exemplos de webhook + coleção Postman
├── docker-compose.yml
├── .env.example
├── IMPLEMENTATION_PLAN.md
└── README.md
```

---

## Licença

MIT — uso livre para qualquer finalidade.
