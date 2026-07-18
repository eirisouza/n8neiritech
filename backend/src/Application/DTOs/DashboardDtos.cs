namespace n8neiritech.Application.DTOs;

public record DashboardMetrics(
    int TodayConversations, int TodayCustomers, int TodayOrders,
    decimal TodayRevenue, double ConversionRate, int HumanHandoffs,
    double AvgResponseSeconds, int AutomationErrors,
    IEnumerable<InstanceStatusDto> InstanceStatuses);
public record InstanceStatusDto(Guid Id, string Name, bool IsConnected, string? ConnectedNumber);
