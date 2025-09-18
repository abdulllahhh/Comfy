using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Model.Entities;

public class Payment
{
    public string Id { get; set; }
    public string UserId { get; set; } // Foreign key to ApplicationUser
    public decimal Amount { get; set; } // Amount in cents (e.g., 500 = $5.00)
    public string Currency { get; set; } = "USD";
    public int CreditsAdded { get; set; }
    public string Status { get; set; } // "pending", "completed", "failed", "refunded"
    public string? StripeSessionId { get; set; }
    public string? StripePaymentIntentId { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? CompletedAt { get; set; }

    // Navigation property
    public virtual ApplicationUser User { get; set; }
}
