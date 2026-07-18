namespace BusTicketReservationSystem.Application.Contracts.DTOs
{
    public enum BookingResultCode
    {
        Success,
        ValidationError,
        NotFound,
        Conflict,
        Forbidden,
        UnexpectedError
    }
}
