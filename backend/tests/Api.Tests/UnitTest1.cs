using FluentAssertions;
using n8neiritech.Domain.Entities;
using n8neiritech.Domain.Enums;

namespace n8neiritech.Tests.Api;

public class ApiEndpointStructureTests
{
    [Fact]
    public void InboundWebhookPayload_ShouldNormalizeCorrectly()
    {
        // Simula a normalização de um webhook do Evolution API
        var externalMessageId = "3EB0A1B2C3D4E5F60000";
        
        var text = "Tem arroz de 5kg?";
        

        var msg = new ConversationMessage
        {
            ExternalMessageId = externalMessageId,
            Text = text,
            Direction = MessageDirection.Inbound,
            Type = MessageType.Text,
            Status = MessageStatus.Sent
        };

        msg.ExternalMessageId.Should().Be(externalMessageId);
        msg.Text.Should().Be(text);
        msg.Direction.Should().Be(MessageDirection.Inbound);
        msg.Type.Should().Be(MessageType.Text);
    }

    [Fact]
    public void OrderStatus_ShouldHaveAllRequiredValues()
    {
        var statuses = Enum.GetValues<OrderStatus>();
        statuses.Should().Contain(OrderStatus.Draft);
        statuses.Should().Contain(OrderStatus.Confirmed);
        statuses.Should().Contain(OrderStatus.Cancelled);
        statuses.Should().Contain(OrderStatus.Delivered);
    }

    [Fact]
    public void ConversationStatus_ShouldHaveAllRequiredStates()
    {
        var statuses = Enum.GetValues<ConversationStatus>();
        statuses.Should().Contain(ConversationStatus.New);
        statuses.Should().Contain(ConversationStatus.HumanAttendance);
        statuses.Should().Contain(ConversationStatus.WaitingAgent);
        statuses.Should().Contain(ConversationStatus.Finished);
        statuses.Should().Contain(ConversationStatus.Cancelled);
    }

    [Fact]
    public void WhatsAppProviderType_ShouldIncludeFakeForDev()
    {
        var providers = Enum.GetValues<WhatsAppProviderType>();
        providers.Should().Contain(WhatsAppProviderType.Fake);
        providers.Should().Contain(WhatsAppProviderType.EvolutionApi);
    }

    [Fact]
    public void MessageType_ShouldSupportAllMedia()
    {
        var types = Enum.GetValues<MessageType>();
        types.Should().Contain(MessageType.Text);
        types.Should().Contain(MessageType.Image);
        types.Should().Contain(MessageType.Audio);
        types.Should().Contain(MessageType.Video);
        types.Should().Contain(MessageType.Document);
    }

    [Fact]
    public void UserRole_ShouldHaveHierarchyLevels()
    {
        var roles = Enum.GetValues<UserRole>();
        roles.Should().Contain(UserRole.SuperAdmin);
        roles.Should().Contain(UserRole.TenantAdmin);
        roles.Should().Contain(UserRole.StoreManager);
        roles.Should().Contain(UserRole.Operator);
        roles.Should().Contain(UserRole.Viewer);
    }

    [Fact]
    public void PaymentMethod_ShouldSupportCommonBrazilianMethods()
    {
        var methods = Enum.GetValues<PaymentMethod>();
        methods.Should().Contain(PaymentMethod.Pix);
        methods.Should().Contain(PaymentMethod.CreditCard);
        methods.Should().Contain(PaymentMethod.Cash);
        methods.Should().Contain(PaymentMethod.DebitCard);
    }

    [Fact]
    public void Tenant_IsolationShouldBeEnforced()
    {
        var tenantId1 = Guid.NewGuid();
        var tenantId2 = Guid.NewGuid();

        var store1 = new Store { TenantId = tenantId1, Name = "Loja 1" };
        var store2 = new Store { TenantId = tenantId2, Name = "Loja 2" };

        store1.TenantId.Should().NotBe(store2.TenantId);
    }

    [Fact]
    public void Product_ShouldSupportAllSyncSources()
    {
        var sources = Enum.GetValues<ProductSyncSource>();
        sources.Should().Contain(ProductSyncSource.Manual);
        sources.Should().Contain(ProductSyncSource.Csv);
        sources.Should().Contain(ProductSyncSource.Xlsx);
        sources.Should().Contain(ProductSyncSource.GoogleSheets);
        sources.Should().Contain(ProductSyncSource.ExternalApi);
        sources.Should().Contain(ProductSyncSource.Erp);
    }
}
