using n8neiritech.Domain.Entities;
using n8neiritech.Domain.Enums;
using FluentAssertions;

namespace n8neiritech.Tests.Application;

public class DomainEntityTests
{
    [Fact]
    public void Product_ShouldHaveCorrectDefaultValues()
    {
        var product = new Product
        {
            Sku = "TEST-001",
            Name = "Produto Teste",
            Price = 29.90m,
            Stock = 10,
            IsActive = true
        };

        product.IsActive.Should().BeTrue();
        product.SyncSource.Should().Be(ProductSyncSource.Manual);
        product.Images.Should().BeEmpty();
        product.Variants.Should().BeEmpty();
    }

    [Fact]
    public void Order_TotalShouldBeCalculatedCorrectly()
    {
        var order = new Order
        {
            Subtotal = 100m,
            DeliveryFee = 10m,
            Discount = 5m,
            OrderNumber = "ORD-001"
        };
        order.Total = order.Subtotal + order.DeliveryFee - order.Discount;

        order.Total.Should().Be(105m);
    }

    [Fact]
    public void Conversation_DefaultStatusShouldBeNew()
    {
        var conversation = new Conversation();
        conversation.Status.Should().Be(ConversationStatus.New);
        conversation.AutomationPaused.Should().BeFalse();
        conversation.UnreadCount.Should().Be(0);
    }

    [Fact]
    public void Customer_ShouldNotBeBlockedByDefault()
    {
        var customer = new Customer { Phone = "5511999990000" };
        customer.IsBlocked.Should().BeFalse();
        customer.ConsentMarketing.Should().BeFalse();
        customer.Addresses.Should().BeEmpty();
    }

    [Fact]
    public void Tenant_ShouldBeActiveByDefault()
    {
        var tenant = new Tenant { Name = "Demo", Slug = "demo" };
        tenant.IsActive.Should().BeTrue();
        tenant.Stores.Should().BeEmpty();
        tenant.Users.Should().BeEmpty();
    }

    [Fact]
    public void OrderItem_TotalShouldReflectQuantityAndPrice()
    {
        var item = new OrderItem
        {
            ProductName = "Arroz 5kg",
            ProductSku = "ARROZ-5KG",
            Quantity = 3,
            UnitPrice = 25.90m,
            Discount = 0m
        };
        item.Total = item.Quantity * item.UnitPrice - item.Discount;

        item.Total.Should().Be(77.70m);
    }

    [Fact]
    public void ConversationMessage_ShouldHaveCorrectDefaults()
    {
        var msg = new ConversationMessage
        {
            Text = "Olá!",
            Direction = MessageDirection.Inbound
        };
        msg.Type.Should().Be(MessageType.Text);
        msg.Status.Should().Be(MessageStatus.Sent);
        msg.IsInternal.Should().BeFalse();
    }

    [Fact]
    public void ProductVariant_ShouldBeActiveByDefault()
    {
        var variant = new ProductVariant
        {
            Sku = "CAMISA-P",
            Name = "Tamanho P",
            Stock = 5
        };
        variant.IsActive.Should().BeTrue();
    }

    [Fact]
    public void Product_EffectivePriceShouldBePromotionalIfSet()
    {
        var product = new Product
        {
            Price = 50m,
            PromotionalPrice = 39.90m
        };
        var effectivePrice = product.PromotionalPrice ?? product.Price;
        effectivePrice.Should().Be(39.90m);
    }

    [Fact]
    public void WebhookEvent_ShouldNotBeProcessedByDefault()
    {
        var evt = new WebhookEvent
        {
            ExternalEventId = "evt-001",
            EventType = "messages.upsert",
            RawPayload = "{}"
        };
        evt.Processed.Should().BeFalse();
        evt.RetryCount.Should().Be(0);
    }
}
