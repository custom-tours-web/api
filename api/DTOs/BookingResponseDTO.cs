namespace api.DTOs;

public record BookingResponseDTO(Guid Id, string Message, string Status, DateTimeOffset CreatedAt);
