using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using n8neiritech.Application.Services;
using n8neiritech.Domain.Entities;
using n8neiritech.Domain.Enums;

namespace n8neiritech.Infrastructure.Data;

public static class DataSeeder
{
    public static readonly Guid DemoTenantId = Guid.Parse("11111111-1111-1111-1111-111111111111");
    public static readonly Guid DemoStoreId = Guid.Parse("22222222-2222-2222-2222-222222222222");
    public static readonly Guid DemoAdminId = Guid.Parse("33333333-3333-3333-3333-333333333333");

    public static async Task SeedAsync(IServiceProvider serviceProvider, IConfiguration configuration, CancellationToken ct = default)
    {
        if (!configuration.GetValue<bool>("SeedData"))
        {
            return;
        }

        using var scope = serviceProvider.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        if (await context.Tenants.AnyAsync(ct))
        {
            return;
        }

        var tenant = new Tenant
        {
            Id = DemoTenantId,
            Name = "Loja Demo",
            Slug = "loja-demo",
            Plan = "Professional",
            IsActive = true,
            PrimaryColor = "#16a34a",
            PlanExpiresAt = DateTime.UtcNow.AddYears(1)
        };

        var store = new Store
        {
            Id = DemoStoreId,
            TenantId = DemoTenantId,
            Name = "Mercado do Bairro",
            Description = "Mercado de conveniência com entrega rápida por WhatsApp.",
            Phone = "+5511999999999",
            Email = "contato@demo.com",
            AddressLine = "Rua das Flores, 123",
            City = "São Paulo",
            State = "SP",
            PostalCode = "01000-000",
            Country = "BR",
            BusinessType = "Supermercado",
            Tenant = tenant
        };

        var user = new User
        {
            Id = DemoAdminId,
            TenantId = DemoTenantId,
            StoreId = DemoStoreId,
            Name = "Administrador Demo",
            Email = "admin@demo.com",
            PasswordHash = AuthService.HashPassword("Admin@123456"),
            Role = UserRole.TenantAdmin,
            MustChangePassword = true,
            Tenant = tenant,
            Store = store
        };

        var instance = new WhatsAppInstance
        {
            TenantId = DemoTenantId,
            StoreId = DemoStoreId,
            Name = "Instância Demo",
            InstanceName = "default",
            ProviderType = WhatsAppProviderType.Fake,
            IsActive = true,
            IsConnected = true,
            ConnectedNumber = "+5511999999999",
            Tenant = tenant,
            Store = store
        };

        var categoryNames = new[] { "Mercearia", "Bebidas", "Hortifruti", "Laticínios", "Limpeza", "Higiene" };
        var categories = categoryNames.Select((name, index) => new ProductCategory
        {
            Id = Guid.NewGuid(),
            TenantId = DemoTenantId,
            StoreId = DemoStoreId,
            Name = name,
            SortOrder = index + 1,
            Tenant = tenant,
            Store = store
        }).ToList();

        var products = new[]
        {
            new Product { TenantId = DemoTenantId, StoreId = DemoStoreId, CategoryId = categories[0].Id, Sku = "MER-001", Name = "Arroz Tipo 1 5kg", Brand = "Tio João", Price = 32.90m, Stock = 40, MinStock = 10, Barcode = "789000000001", Keywords = "arroz grão branco", Tags = "mercearia", Category = categories[0], Store = store, Tenant = tenant },
            new Product { TenantId = DemoTenantId, StoreId = DemoStoreId, CategoryId = categories[0].Id, Sku = "MER-002", Name = "Feijão Carioca 1kg", Brand = "Camil", Price = 8.90m, Stock = 60, MinStock = 15, Barcode = "789000000002", Keywords = "feijão carioca", Tags = "mercearia", Category = categories[0], Store = store, Tenant = tenant },
            new Product { TenantId = DemoTenantId, StoreId = DemoStoreId, CategoryId = categories[1].Id, Sku = "BEB-001", Name = "Refrigerante Cola 2L", Brand = "Coca-Cola", Price = 10.99m, Stock = 50, MinStock = 12, Barcode = "789000000003", Keywords = "refrigerante cola", Tags = "bebidas", Category = categories[1], Store = store, Tenant = tenant },
            new Product { TenantId = DemoTenantId, StoreId = DemoStoreId, CategoryId = categories[1].Id, Sku = "BEB-002", Name = "Suco de Laranja 1L", Brand = "Del Valle", Price = 7.50m, Stock = 35, MinStock = 8, Barcode = "789000000004", Keywords = "suco laranja", Tags = "bebidas", Category = categories[1], Store = store, Tenant = tenant },
            new Product { TenantId = DemoTenantId, StoreId = DemoStoreId, CategoryId = categories[2].Id, Sku = "HOR-001", Name = "Banana Prata 1kg", Brand = "Fazenda Boa", Price = 6.80m, Stock = 25, MinStock = 5, Barcode = "789000000005", Keywords = "banana fruta", Tags = "hortifruti", Category = categories[2], Store = store, Tenant = tenant },
            new Product { TenantId = DemoTenantId, StoreId = DemoStoreId, CategoryId = categories[2].Id, Sku = "HOR-002", Name = "Tomate Italiano 1kg", Brand = "Fazenda Boa", Price = 9.20m, Stock = 20, MinStock = 5, Barcode = "789000000006", Keywords = "tomate italiano", Tags = "hortifruti", Category = categories[2], Store = store, Tenant = tenant },
            new Product { TenantId = DemoTenantId, StoreId = DemoStoreId, CategoryId = categories[3].Id, Sku = "LAT-001", Name = "Leite Integral 1L", Brand = "Italac", Price = 5.49m, Stock = 80, MinStock = 20, Barcode = "789000000007", Keywords = "leite integral", Tags = "laticínios", Category = categories[3], Store = store, Tenant = tenant },
            new Product { TenantId = DemoTenantId, StoreId = DemoStoreId, CategoryId = categories[3].Id, Sku = "LAT-002", Name = "Queijo Mussarela 500g", Brand = "Tirolez", Price = 18.90m, Stock = 18, MinStock = 4, Barcode = "789000000008", Keywords = "queijo mussarela", Tags = "laticínios", Category = categories[3], Store = store, Tenant = tenant },
            new Product { TenantId = DemoTenantId, StoreId = DemoStoreId, CategoryId = categories[4].Id, Sku = "LIM-001", Name = "Detergente Neutro 500ml", Brand = "Ypê", Price = 2.99m, Stock = 90, MinStock = 20, Barcode = "789000000009", Keywords = "detergente limpeza", Tags = "limpeza", Category = categories[4], Store = store, Tenant = tenant },
            new Product { TenantId = DemoTenantId, StoreId = DemoStoreId, CategoryId = categories[5].Id, Sku = "HIG-001", Name = "Sabonete 90g", Brand = "Dove", Price = 4.49m, Stock = 75, MinStock = 15, Barcode = "789000000010", Keywords = "sabonete higiene", Tags = "higiene", Category = categories[5], Store = store, Tenant = tenant },
            new Product { TenantId = DemoTenantId, StoreId = DemoStoreId, CategoryId = categories[5].Id, Sku = "HIG-002", Name = "Shampoo 400ml", Brand = "Pantene", Price = 16.90m, Stock = 22, MinStock = 6, Barcode = "789000000011", Keywords = "shampoo cabelo", Tags = "higiene", Category = categories[5], Store = store, Tenant = tenant }
        };

        var businessHours = Enumerable.Range(0, 7).Select(day => new BusinessHour
        {
            TenantId = DemoTenantId,
            StoreId = DemoStoreId,
            DayOfWeek = (DayOfWeek)day,
            IsOpen = day != 0 || true,
            OpenTime = day == 0 ? new TimeOnly(9, 0) : new TimeOnly(8, 0),
            CloseTime = day == 0 ? new TimeOnly(13, 0) : new TimeOnly(20, 0),
            Store = store,
            Tenant = tenant
        }).ToList();

        businessHours[0].DayOfWeek = DayOfWeek.Sunday;
        businessHours[1].DayOfWeek = DayOfWeek.Monday;
        businessHours[2].DayOfWeek = DayOfWeek.Tuesday;
        businessHours[3].DayOfWeek = DayOfWeek.Wednesday;
        businessHours[4].DayOfWeek = DayOfWeek.Thursday;
        businessHours[5].DayOfWeek = DayOfWeek.Friday;
        businessHours[6].DayOfWeek = DayOfWeek.Saturday;

        var faqs = new[]
        {
            new Faq { TenantId = DemoTenantId, StoreId = DemoStoreId, Question = "Qual o horário de atendimento?", Answer = "Funcionamos de segunda a sábado, das 8h às 20h, e domingo das 9h às 13h.", Category = "Horário", SortOrder = 1, Store = store, Tenant = tenant },
            new Faq { TenantId = DemoTenantId, StoreId = DemoStoreId, Question = "Vocês fazem entrega?", Answer = "Sim, entregamos na região em até 60 minutos conforme disponibilidade.", Category = "Entrega", SortOrder = 2, Store = store, Tenant = tenant },
            new Faq { TenantId = DemoTenantId, StoreId = DemoStoreId, Question = "Quais formas de pagamento aceitam?", Answer = "Aceitamos Pix, cartão, dinheiro e pagamento na entrega.", Category = "Pagamento", SortOrder = 3, Store = store, Tenant = tenant },
            new Faq { TenantId = DemoTenantId, StoreId = DemoStoreId, Question = "Posso pedir o cardápio ou catálogo?", Answer = "Claro! É só pedir que enviamos os principais produtos e promoções.", Category = "Catálogo", SortOrder = 4, Store = store, Tenant = tenant },
            new Faq { TenantId = DemoTenantId, StoreId = DemoStoreId, Question = "Onde vocês estão localizados?", Answer = "Estamos na Rua das Flores, 123 - São Paulo/SP.", Category = "Localização", SortOrder = 5, Store = store, Tenant = tenant }
        };

        var rules = new[]
        {
            new AutomationRule { TenantId = DemoTenantId, StoreId = DemoStoreId, Name = "Boas-vindas", Trigger = "message_received", Actions = "send:greeting", Priority = 1, Description = "Recepciona novos clientes.", Store = store, Tenant = tenant },
            new AutomationRule { TenantId = DemoTenantId, StoreId = DemoStoreId, Name = "Transferir para humano", Trigger = "keyword:atendente", Actions = "handoff:agent", Priority = 2, Description = "Encaminha para um operador quando solicitado.", Store = store, Tenant = tenant },
            new AutomationRule { TenantId = DemoTenantId, StoreId = DemoStoreId, Name = "Fora do horário", Trigger = "out_of_hours", Actions = "send:out_of_hours_message", Priority = 3, Description = "Informa indisponibilidade fora do horário comercial.", Store = store, Tenant = tenant }
        };

        context.Tenants.Add(tenant);
        context.Stores.Add(store);
        context.Users.Add(user);
        context.WhatsAppInstances.Add(instance);
        context.ProductCategories.AddRange(categories);
        context.Products.AddRange(products);
        context.BusinessHours.AddRange(businessHours);
        context.Faqs.AddRange(faqs);
        context.AutomationRules.AddRange(rules);
        await context.SaveChangesAsync(ct);
    }
}
