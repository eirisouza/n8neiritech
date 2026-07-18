namespace n8neiritech.Application.DTOs;

public record FaqRequest(Guid StoreId, string Question, string Answer, string? Category, int SortOrder, bool IsActive);
public record FaqResponse(Guid Id, Guid StoreId, string Question, string Answer, string? Category, int SortOrder, bool IsActive);
