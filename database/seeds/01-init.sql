-- ═══════════════════════════════════════════════════════════════════════════
-- n8neiritech — Seed inicial de dados de demonstração
-- Este script é executado automaticamente pelo Docker no primeiro start
-- ═══════════════════════════════════════════════════════════════════════════

-- Nota: As migrations do EF Core criam as tabelas. Este script
-- insere dados iniciais caso não existam.

-- Garante que o schema público está disponível
SET search_path TO public;

-- Este arquivo é reservado para dados que não podem ser
-- inseridos via migrations do EF Core. O seed principal é
-- executado pelo backend via DataSeeder.cs no startup.

DO $$
BEGIN
  RAISE NOTICE 'Banco de dados inicializado. O seed de dados será executado pelo backend.';
END $$;
