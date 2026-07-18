using Microsoft.EntityFrameworkCore;
using n8neiritech.Domain.Entities;

namespace n8neiritech.Application.Interfaces;

public interface IApplicationDbContext
{
    DbSet<Tenant> Tenants { get; }
    DbSet<Store> Stores { get; }
    DbSet<User> Users { get; }
    DbSet<RefreshToken> RefreshTokens { get; }
    DbSet<WhatsAppInstance> WhatsAppInstances { get; }
    DbSet<Customer> Customers { get; }
    DbSet<CustomerAddress> CustomerAddresses { get; }
    DbSet<ProductCategory> ProductCategories { get; }
    DbSet<Product> Products { get; }
    DbSet<ProductImage> ProductImages { get; }
    DbSet<ProductVariant> ProductVariants { get; }
    DbSet<Conversation> Conversations { get; }
    DbSet<ConversationMessage> ConversationMessages { get; }
    DbSet<Order> Orders { get; }
    DbSet<OrderItem> OrderItems { get; }
    DbSet<BusinessHour> BusinessHours { get; }
    DbSet<Faq> Faqs { get; }
    DbSet<AutomationRule> AutomationRules { get; }
    DbSet<WebhookEvent> WebhookEvents { get; }
    DbSet<AuditLog> AuditLogs { get; }
    DbSet<SystemSetting> SystemSettings { get; }
    DbSet<Promotion> Promotions { get; }
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
