using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Model.Entities
{
    public class ProcessedEvent
    {
        public int Id { get; set; }
        public string StripeEventId { get; set; } // Stripe's unique event ID
        public string EventType { get; set; } // e.g., "checkout.session.completed"
        public string? UserId { get; set; }
        public DateTime ProcessedAt { get; set; } = DateTime.UtcNow;
        public string? EventData { get; set; } // JSO
    }
}
