using BusTicketReservationSystem.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusTicketReservationSystem.Application.Contracts.Interfaces
{
    public interface IPassengerRepository
    {
        Task<Passenger?> FindByMobileAsync(string mobile);
        Task AddAsync(Passenger passenger);
    }
}
