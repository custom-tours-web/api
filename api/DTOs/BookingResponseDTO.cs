namespace api.DTOs;

public record BookingResponseDTO(int Id, string Message, string Status, DateTimeOffset CreatedAt);
