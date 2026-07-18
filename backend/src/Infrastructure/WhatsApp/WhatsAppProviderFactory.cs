using Microsoft.Extensions.DependencyInjection;
using n8neiritech.Application.Interfaces;
using n8neiritech.Domain.Enums;

namespace n8neiritech.Infrastructure.WhatsApp;

public class WhatsAppProviderFactory
{
    private readonly IServiceProvider _serviceProvider;

    public WhatsAppProviderFactory(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    public IWhatsAppProvider Create(WhatsAppProviderType type)
        => type switch
        {
            WhatsAppProviderType.EvolutionApi => _serviceProvider.GetRequiredService<EvolutionApiAdapter>(),
            _ => _serviceProvider.GetRequiredService<FakeWhatsAppAdapter>()
        };
}
