using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using n8neiritech.Application.Common;
using n8neiritech.Application.Interfaces;
using n8neiritech.Application.Services;
using n8neiritech.Domain.Enums;
using n8neiritech.Infrastructure.AI;
using n8neiritech.Infrastructure.Data;
using n8neiritech.Infrastructure.Health;
using n8neiritech.Infrastructure.Options;
using n8neiritech.Infrastructure.Services;
using n8neiritech.Infrastructure.WhatsApp;
using StackExchange.Redis;

namespace n8neiritech.Infrastructure;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<WhatsAppOptions>(configuration.GetSection("WhatsApp"));
        services.Configure<AiOptions>(configuration.GetSection("Ai"));
        services.AddSingleton(new AuthOptions
        {
            Secret = configuration["Jwt:Secret"] ?? throw new InvalidOperationException("Jwt:Secret missing."),
            Issuer = configuration["Jwt:Issuer"] ?? string.Empty,
            Audience = configuration["Jwt:Audience"] ?? string.Empty,
            ExpirationMinutes = configuration.GetValue<int>("Jwt:ExpirationMinutes", 15),
            RefreshTokenExpirationDays = configuration.GetValue<int>("Jwt:RefreshTokenExpirationDays", 30)
        });

        services.AddDbContext<AppDbContext>(options =>
            options.UseNpgsql(configuration.GetConnectionString("DefaultConnection")));

        services.AddScoped<IApplicationDbContext>(sp => sp.GetRequiredService<AppDbContext>());
        services.AddHttpContextAccessor();
        services.AddScoped<ICurrentUser, CurrentUserService>();
        services.AddScoped<ICacheService, CacheService>();

        var redisConnectionString = configuration["Redis:ConnectionString"] ?? "localhost:6379,abortConnect=false";
        services.AddSingleton<IConnectionMultiplexer>(_ => ConnectionMultiplexer.Connect(redisConnectionString));

        services.AddHttpClient<EvolutionApiAdapter>();
        services.AddScoped<FakeWhatsAppAdapter>();
        services.AddScoped<WhatsAppProviderFactory>();
        services.AddScoped<IWhatsAppProvider>(sp =>
        {
            var options = sp.GetRequiredService<IOptions<WhatsAppOptions>>().Value;
            var type = Enum.TryParse<WhatsAppProviderType>(options.Provider, true, out var parsed)
                ? parsed
                : WhatsAppProviderType.Fake;
            return sp.GetRequiredService<WhatsAppProviderFactory>().Create(type);
        });

        services.AddHttpClient<OpenAiAdapter>();
        services.AddScoped<NullAiAdapter>();
        services.AddScoped<IAiProvider>(sp =>
        {
            var options = sp.GetRequiredService<IOptions<AiOptions>>().Value;
            return string.Equals(options.Provider, "OpenAI", StringComparison.OrdinalIgnoreCase)
                ? sp.GetRequiredService<OpenAiAdapter>()
                : sp.GetRequiredService<NullAiAdapter>();
        });

        services.AddScoped<AuthService>();
        services.AddScoped<ProductSearchService>();
        services.AddScoped<ConversationService>();
        services.AddScoped<OrderService>();
        services.AddScoped<WebhookService>();

        services.AddHealthChecks()
            .AddCheck<DatabaseHealthCheck>("postgresql")
            .AddCheck<RedisHealthCheck>("redis");

        return services;
    }
}
