using n8neiritech.Domain.Enums;

namespace n8neiritech.Application.DTOs;

public record CreateOrderItemRequest(Guid ProductId, Guid? VariantId, int Quantity, decimal Discount = 0);
public record CreateOrderRequest(Guid StoreId, Guid CustomerId, Guid? ConversationId, DeliveryType DeliveryType, PaymentMethod PaymentMethod, decimal DeliveryFee, decimal Discount, string? DeliveryAddress, string? Notes, IReadOnlyCollection<CreateOrderItemRequest> Items);
public record OrderItemResponse(Guid Id, Guid ProductId, Guid? VariantId, string ProductName, string? VariantName, string ProductSku, int Quantity, decimal UnitPrice, decimal Discount, decimal Total);
public record OrderResponse(Guid Id, Guid StoreId, Guid CustomerId, string OrderNumber, OrderStatus Status, decimal Subtotal, decimal DeliveryFee, decimal Discount, decimal Total, DeliveryType DeliveryType, PaymentMethod PaymentMethod, string? DeliveryAddress, string? Notes, DateTime? ConfirmedAt, DateTime? DeliveredAt, IReadOnlyCollection<OrderItemResponse> Items);
public record UpdateOrderStatusRequest(OrderStatus Status, string? Notes);
public record CancelOrderRequest(string Reason);
