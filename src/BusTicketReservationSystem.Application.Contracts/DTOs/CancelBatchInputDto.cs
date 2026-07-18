using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusTicketReservationSystem.Application.Contracts.DTOs
{
    public class CancelBatchInputDto
    {
        public List<Guid> TicketIds { get; set; } = new();
    }

}
