using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Model.Entities
{
    public class CreditTransaction
    {
        public int Id { get; set; }
        public string UserId { get; set; } // Foreign key to ApplicationUser
        public int Amount { get; set; } // Positive for credit, negative for debit
        public string TransactionType { get; set; } // "purchase", "usage", "refund", "bonus"
        public string? Description { get; set; }
        public string? ReferenceId { get; set; } // Payment ID, Model call ID, etc.
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;

        // Navigation property
        public virtual ApplicationUser User { get; set; }
    }
}
