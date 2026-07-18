namespace n8neiritech.Application.DTOs;

public record CustomerAddressDto(Guid Id, string Label, string Street, string? Number, string? Complement, string Neighborhood, string City, string State, string PostalCode, bool IsDefault);
public record CustomerResponse(Guid Id, Guid StoreId, string Phone, string? Name, string? Email, DateTime? BirthDate, bool IsBlocked, string? BlockReason, bool ConsentMarketing, string? Notes, DateTime? LastInteractionAt, IReadOnlyCollection<CustomerAddressDto> Addresses);
public record CreateCustomerRequest(Guid StoreId, string Phone, string? Name, string? Email, DateTime? BirthDate, bool ConsentMarketing, string? Notes);
public record UpdateCustomerRequest(string Phone, string? Name, string? Email, DateTime? BirthDate, bool ConsentMarketing, string? Notes);
public record BlockCustomerRequest(string? Reason);
