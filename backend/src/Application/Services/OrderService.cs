using Microsoft.EntityFrameworkCore;
using n8neiritech.Application.DTOs;
using n8neiritech.Application.Interfaces;
using n8neiritech.Domain.Entities;
using n8neiritech.Domain.Enums;

namespace n8neiritech.Application.Services;

public class OrderService
{
    private readonly IApplicationDbContext _context;

    public OrderService(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<OrderResponse> CreateFromCartAsync(Guid tenantId, CreateOrderRequest request, CancellationToken ct = default)
    {
        await ValidateStockAsync(request.Items, ct);
        var customer = await _context.Customers.FirstOrDefaultAsync(x => x.Id == request.CustomerId && x.TenantId == tenantId, ct)
            ?? throw new KeyNotFoundException("Customer not found.");

        var products = await _context.Products
            .Include(x => x.Variants)
            .Where(x => request.Items.Select(i => i.ProductId).Contains(x.Id) && x.TenantId == tenantId)
            .ToListAsync(ct);

        var order = new Order
        {
            TenantId = tenantId,
            StoreId = request.StoreId,
            CustomerId = request.CustomerId,
            ConversationId = request.ConversationId,
            OrderNumber = $"ORD-{DateTime.UtcNow:yyyyMMddHHmmss}-{Random.Shared.Next(100, 999)}",
            Status = request.PaymentMethod == PaymentMethod.Cash || request.PaymentMethod == PaymentMethod.OnDelivery
                ? OrderStatus.Confirmed
                : OrderStatus.PendingPayment,
            DeliveryType = request.DeliveryType,
            PaymentMethod = request.PaymentMethod,
            DeliveryAddress = request.DeliveryAddress,
            DeliveryFee = request.DeliveryFee,
            Discount = request.Discount,
            Notes = request.Notes
        };

        foreach (var item in request.Items)
        {
            var product = products.First(x => x.Id == item.ProductId);
            var variant = item.VariantId.HasValue ? product.Variants.FirstOrDefault(x => x.Id == item.VariantId.Value) : null;
            var unitPrice = (product.PromotionalPrice ?? product.Price) + (variant?.PriceAdjustment ?? 0m);
            order.Items.Add(new OrderItem
            {
                ProductId = product.Id,
                VariantId = variant?.Id,
                ProductName = product.Name,
                VariantName = variant?.Name,
                ProductSku = variant?.Sku ?? product.Sku,
                Quantity = item.Quantity,
                UnitPrice = unitPrice,
                Discount = item.Discount,
                Total = (unitPrice * item.Quantity) - item.Discount,
                Product = product
            });

            product.Stock -= item.Quantity;
            if (variant is not null)
            {
                variant.Stock -= item.Quantity;
            }
        }

        await RecalculateTotalsAsync(order, ct);
        if (order.Status == OrderStatus.Confirmed)
        {
            order.ConfirmedAt = DateTime.UtcNow;
        }

        await _context.Orders.AddAsync(order, ct);
        await _context.SaveChangesAsync(ct);
        return Map(order);
    }

    public async Task<OrderResponse> UpdateStatusAsync(Guid tenantId, Guid orderId, UpdateOrderStatusRequest request, CancellationToken ct = default)
    {
        var order = await _context.Orders.Include(x => x.Items).FirstOrDefaultAsync(x => x.Id == orderId && x.TenantId == tenantId, ct)
            ?? throw new KeyNotFoundException("Order not found.");
        order.Status = request.Status;
        order.Notes = string.IsNullOrWhiteSpace(request.Notes) ? order.Notes : request.Notes;
        if (request.Status == OrderStatus.Confirmed)
            order.ConfirmedAt = DateTime.UtcNow;
        if (request.Status == OrderStatus.Delivered)
            order.DeliveredAt = DateTime.UtcNow;
        await _context.SaveChangesAsync(ct);
        return Map(order);
    }

    public async Task CancelAsync(Guid tenantId, Guid orderId, string reason, CancellationToken ct = default)
    {
        var order = await _context.Orders.Include(x => x.Items).FirstOrDefaultAsync(x => x.Id == orderId && x.TenantId == tenantId, ct)
            ?? throw new KeyNotFoundException("Order not found.");
        order.Status = OrderStatus.Cancelled;
        order.CancellationReason = reason;
        await _context.SaveChangesAsync(ct);
    }

    public Task RecalculateTotalsAsync(Order order, CancellationToken ct = default)
    {
        order.Subtotal = order.Items.Sum(x => x.UnitPrice * x.Quantity);
        order.Total = order.Subtotal + order.DeliveryFee - order.Discount - order.Items.Sum(x => x.Discount);
        return Task.CompletedTask;
    }

    public async Task ValidateStockAsync(IEnumerable<CreateOrderItemRequest> items, CancellationToken ct = default)
    {
        foreach (var item in items)
        {
            var product = await _context.Products.Include(x => x.Variants).FirstOrDefaultAsync(x => x.Id == item.ProductId, ct)
                ?? throw new KeyNotFoundException($"Product {item.ProductId} not found.");
            if (item.VariantId.HasValue)
            {
                var variant = product.Variants.FirstOrDefault(x => x.Id == item.VariantId.Value)
                    ?? throw new KeyNotFoundException("Variant not found.");
                if (variant.Stock < item.Quantity)
                    throw new InvalidOperationException($"Insufficient stock for variant {variant.Name}.");
            }
            else if (product.Stock < item.Quantity)
            {
                throw new InvalidOperationException($"Insufficient stock for product {product.Name}.");
            }
        }
    }

    private static OrderResponse Map(Order order)
        => new(
            order.Id,
            order.StoreId,
            order.CustomerId,
            order.OrderNumber,
            order.Status,
            order.Subtotal,
            order.DeliveryFee,
            order.Discount,
            order.Total,
            order.DeliveryType,
            order.PaymentMethod,
            order.DeliveryAddress,
            order.Notes,
            order.ConfirmedAt,
            order.DeliveredAt,
            order.Items.Select(x => new OrderItemResponse(x.Id, x.ProductId, x.VariantId, x.ProductName, x.VariantName, x.ProductSku, x.Quantity, x.UnitPrice, x.Discount, x.Total)).ToArray());
}
