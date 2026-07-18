using BusTicketReservationSystem.Application.Contracts.DTOs;
using BusTicketReservationSystem.Application.Contracts.Interfaces;
using BusTicketReservationSystem.Domain.Entities;
using BusTicketReservationSystem.Domain.Enums;
using BusTicketReservationSystem.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;
using System.Text.RegularExpressions;

namespace BusTicketReservationSystem.Application.Services
{
    public class BookingService
    {
        private static readonly Regex MobileNumberPattern = new(
            @"^03\d{9}$",
            RegexOptions.Compiled);

        private readonly IBusScheduleRepository _schedules;
        private readonly ISeatRepository _seats;
        private readonly ITicketRepository _tickets;
        private readonly IPassengerRepository _passengers;
        private readonly IUnitOfWork _uow;

        public BookingService(
            IBusScheduleRepository schedules,
            ISeatRepository seats,
            ITicketRepository tickets,
            IPassengerRepository passengers,
            IUnitOfWork uow)
        {
            _schedules = schedules;
            _seats = seats;
            _tickets = tickets;
            _passengers = passengers;
            _uow = uow;
        }

        public async Task<SeatPlanDto?> GetSeatPlanAsync(Guid busScheduleId)
        {
            if (busScheduleId == Guid.Empty)
                return null;

            var schedule = await _schedules.GetByIdAsync(busScheduleId);
            if (schedule is null || schedule.Bus is null || schedule.Route is null)
                return null;

            var busSeats = await _seats.GetByBusIdAsync(schedule.BusId);

            var seatStatusMap = schedule.Tickets
                .Where(t => t.Status == TicketStatus.Booked || t.Status == TicketStatus.Sold)
                .ToDictionary(t => t.SeatId, t => t.Status);

            return new SeatPlanDto
            {
                BusScheduleId = schedule.Id,
                BusName = schedule.Bus.BusName,
                CompanyName = schedule.Bus.CompanyName,
                BoardingPoint = schedule.Route.BoardingPoint,
                DroppingPoint = schedule.Route.DroppingPoint,
                Seats = busSeats.Select(s => new SeatDto
                {
                    SeatId = s.Id,
                    SeatNumber = s.SeatNumber,
                    Row = s.Row,
                    Status = seatStatusMap.TryGetValue(s.Id, out var status)
                        ? status == TicketStatus.Sold ? "Sold" : "Booked"
                        : "Available"
                }).ToList()
            };
        }

        public async Task<BookSeatResultDto> BookSeatAsync(BookSeatInputDto input)
        {
            var validationFailure = ValidateBookingInput(input);
            if (validationFailure is not null)
                return validationFailure;

            var requestedSeatIds = input.SeatId.Distinct().ToList();
            if (requestedSeatIds.Count != input.SeatId.Count)
                return Failure(BookingResultCode.ValidationError, "Duplicate seat IDs are not allowed.");

            var schedule = await _schedules.GetByIdAsync(input.BusScheduleId);
            if (schedule is null)
                return Failure(BookingResultCode.NotFound, "Schedule not found.");

            if (schedule.BusId == Guid.Empty || schedule.Bus is null)
                return Failure(BookingResultCode.Conflict, "The selected schedule does not reference a valid bus.");

            if (schedule.JourneyDate.Date < DateTime.UtcNow.Date)
                return Failure(BookingResultCode.ValidationError, "Bookings cannot be made for a past journey date.");

            if (!MatchesRoutePoint(input.BoardingPoint, schedule.Route?.BoardingPoint) ||
                !MatchesRoutePoint(input.DroppingPoint, schedule.Route?.DroppingPoint))
            {
                return Failure(BookingResultCode.ValidationError, "Boarding or dropping point is not valid for the selected schedule.");
            }

            var seats = await _seats.GetByIdsAsync(requestedSeatIds);
            if (seats.Count != requestedSeatIds.Count)
                return Failure(BookingResultCode.ValidationError, "One or more selected seats do not exist.");

            if (seats.Any(seat => seat.BusId != schedule.BusId))
                return Failure(BookingResultCode.ValidationError, "One or more selected seats do not belong to the scheduled bus.");

            var transactionStarted = false;

            try
            {
                await _uow.BeginTransactionAsync();
                transactionStarted = true;

                var mobile = input.PassengerMobile.Trim();
                var passenger = await _passengers.FindByMobileAsync(mobile);
                if (passenger is null)
                {
                    passenger = new Passenger(
                        new PassengerContact(input.PassengerName.Trim(), mobile),
                        input.BoardingPoint.Trim(),
                        input.DroppingPoint.Trim())
                    {
                        Id = Guid.NewGuid()
                    };
                    await _passengers.AddAsync(passenger);
                }

                var bookedDtos = new List<SeatDto>();
                var isBuyAction = string.Equals(input.Action, "buy", StringComparison.OrdinalIgnoreCase);

                foreach (var seat in seats)
                {
                    var existingTicket = await _tickets.FindBySeatAndScheduleAsync(seat.Id, schedule.Id);

                    if (existingTicket is not null &&
                        (existingTicket.Status == TicketStatus.Booked || existingTicket.Status == TicketStatus.Sold))
                    {
                        await _uow.RollbackAsync();
                        transactionStarted = false;
                        return Failure(BookingResultCode.Conflict, $"Seat {seat.SeatNumber} is already booked or sold.");
                    }

                    Ticket ticket;
                    if (existingTicket is not null && existingTicket.Status == TicketStatus.Cancelled)
                    {
                        ticket = existingTicket;
                        ticket.Status = isBuyAction ? TicketStatus.Sold : TicketStatus.Booked;
                        ticket.PassengerId = passenger.Id;
                        ticket.BoardingPoint = input.BoardingPoint.Trim();
                        ticket.DroppingPoint = input.DroppingPoint.Trim();
                    }
                    else
                    {
                        ticket = new Ticket
                        {
                            Id = Guid.NewGuid(),
                            SeatId = seat.Id,
                            PassengerId = passenger.Id,
                            BusScheduleId = schedule.Id,
                            Status = isBuyAction ? TicketStatus.Sold : TicketStatus.Booked,
                            BoardingPoint = input.BoardingPoint.Trim(),
                            DroppingPoint = input.DroppingPoint.Trim()
                        };
                        await _tickets.AddAsync(ticket);
                    }

                    bookedDtos.Add(new SeatDto
                    {
                        SeatId = seat.Id,
                        TicketId = ticket.Id,
                        SeatNumber = seat.SeatNumber,
                        Row = seat.Row,
                        Status = isBuyAction ? "Sold" : "Booked"
                    });
                }

                await _uow.SaveChangesAsync();
                await _uow.CommitAsync();
                transactionStarted = false;

                return new BookSeatResultDto
                {
                    Success = true,
                    Code = BookingResultCode.Success,
                    Message = "Seats booked successfully.",
                    Seat = bookedDtos
                };
            }
            catch (DbUpdateException)
            {
                if (transactionStarted)
                    await _uow.RollbackAsync();

                return Failure(
                    BookingResultCode.Conflict,
                    "The selected seat was booked by another request. Refresh the seat plan and try again.");
            }
            catch
            {
                if (transactionStarted)
                    await _uow.RollbackAsync();

                throw;
            }
        }

        public async Task<BookSeatResultDto> CancelAsync(CancelInputDto input)
        {
            if (input.TicketId == Guid.Empty)
                return Failure(BookingResultCode.ValidationError, "A valid ticket ID is required.");

            var mobile = input.PassengerMobile?.Trim() ?? string.Empty;
            if (!MobileNumberPattern.IsMatch(mobile))
                return Failure(BookingResultCode.ValidationError, "Enter the mobile number used for the booking.");

            var ticket = await _tickets.GetByIdAsync(input.TicketId);
            if (ticket is null)
                return Failure(BookingResultCode.NotFound, "Ticket not found.");

            if (ticket.Passenger is null ||
                !string.Equals(ticket.Passenger.MobileNumber, mobile, StringComparison.Ordinal))
            {
                return Failure(BookingResultCode.Forbidden, "The supplied passenger details do not own this ticket.");
            }

            if (ticket.Status != TicketStatus.Booked && ticket.Status != TicketStatus.Sold)
                return Failure(BookingResultCode.Conflict, "Ticket is already cancelled or cannot be cancelled.");

            ticket.Status = TicketStatus.Cancelled;
            await _uow.SaveChangesAsync();

            return new BookSeatResultDto
            {
                Success = true,
                Code = BookingResultCode.Success,
                Message = "Booking cancelled."
            };
        }

        private static BookSeatResultDto? ValidateBookingInput(BookSeatInputDto input)
        {
            if (input.BusScheduleId == Guid.Empty)
                return Failure(BookingResultCode.ValidationError, "A valid schedule ID is required.");

            if (input.SeatId is null || input.SeatId.Count == 0)
                return Failure(BookingResultCode.ValidationError, "Select at least one seat.");

            if (input.SeatId.Any(id => id == Guid.Empty))
                return Failure(BookingResultCode.ValidationError, "One or more seat IDs are invalid.");

            if (string.IsNullOrWhiteSpace(input.PassengerName) || input.PassengerName.Trim().Length < 2)
                return Failure(BookingResultCode.ValidationError, "Passenger name is required.");

            if (!MobileNumberPattern.IsMatch(input.PassengerMobile?.Trim() ?? string.Empty))
                return Failure(BookingResultCode.ValidationError, "Enter a valid Pakistan mobile number in 03XXXXXXXXX format.");

            if (string.IsNullOrWhiteSpace(input.BoardingPoint) || string.IsNullOrWhiteSpace(input.DroppingPoint))
                return Failure(BookingResultCode.ValidationError, "Boarding and dropping points are required.");

            if (!string.Equals(input.Action, "book", StringComparison.OrdinalIgnoreCase) &&
                !string.Equals(input.Action, "buy", StringComparison.OrdinalIgnoreCase))
            {
                return Failure(BookingResultCode.ValidationError, "Booking action must be either book or buy.");
            }

            return null;
        }

        private static bool MatchesRoutePoint(string submittedValue, string? configuredValue)
        {
            return string.IsNullOrWhiteSpace(configuredValue) ||
                   string.Equals(submittedValue.Trim(), configuredValue.Trim(), StringComparison.OrdinalIgnoreCase);
        }

        private static BookSeatResultDto Failure(BookingResultCode code, string message)
        {
            return new BookSeatResultDto
            {
                Success = false,
                Code = code,
                Message = message
            };
        }
    }
}
