using BusTicketReservationSystem.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusTicketReservationSystem.Application.Contracts.Interfaces
{
    public interface ISeatRepository
    {
        Task<IReadOnlyList<Seat>> GetByBusIdAsync(Guid busId);
        Task<IReadOnlyList<Seat>> GetByIdsAsync(IEnumerable<Guid> ids);
    }

}
