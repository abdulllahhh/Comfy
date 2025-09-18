namespace API.Controllers
{
    using infrastructure.Data;
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Identity;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.EntityFrameworkCore;
    using Model.Dtos;
    using Model.Entities;
    using Stripe;
    using Stripe.Checkout;
    using System.Security.Claims;

    /// <summary>
    /// Defines the <see cref="PaymentController" />
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    public class PaymentController : ControllerBase
    {
        /// <summary>
        /// Defines the _userManager
        /// </summary>
        private readonly UserManager<ApplicationUser> _userManager;

        /// <summary>
        /// Defines the _config
        /// </summary>
        private readonly IConfiguration _config;

        /// <summary>
        /// Defines the _context
        /// </summary>
        private readonly ApplicationDbContext _context;

        /// <summary>
        /// Initializes a new instance of the <see cref="PaymentController"/> class.
        /// </summary>
        /// <param name="userManager">The userManager<see cref="UserManager{ApplicationUser}"/></param>
        /// <param name="context">The context<see cref="ApplicationDbContext"/></param>
        /// <param name="configuration">The configuration<see cref="IConfiguration"/></param>
        public PaymentController(UserManager<ApplicationUser> userManager, ApplicationDbContext context, IConfiguration configuration)
        {
            _userManager = userManager;
            _config = configuration;
            _context = context;
        }

        /// <summary>
        /// The CreateCheckoutSession
        /// </summary>
        /// <param name="req">The req<see cref="PaymentRequest"/></param>
        /// <returns>The <see cref="Task{IActionResult}"/></returns>
        [Authorize]
        [HttpPost("create-checkout-session")]
        public async Task<IActionResult> CreateCheckoutSession([FromBody] PaymentRequest req)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier); // get the logged-in user's ID
            if (string.IsNullOrEmpty(userId))
                return Unauthorized(new { error = "User not found" });

            var creditPackages = new Dictionary<int, (int amount, string name)>
            {
                { 50, (500, "50 AI Credits - $5.00") },
                { 100, (1000, "100 AI Credits - $10.00") },
                { 500, (4500, "500 AI Credits - $45.00") }
            };
            if (!creditPackages.ContainsKey(req.Credits))
                return BadRequest("Invalid credit package");
            var package = creditPackages[req.Credits];
            // Stripe session create options
            var options = new SessionCreateOptions
            {
                PaymentMethodTypes = new List<string> { "card" },
                LineItems = new List<SessionLineItemOptions>
                {
                    new SessionLineItemOptions
                    {
                        PriceData = new SessionLineItemPriceDataOptions
                        {
                            UnitAmount = package.amount,
                            Currency = "usd",
                            ProductData = new SessionLineItemPriceDataProductDataOptions
                            {
                                Name = package.name
                            }
                        },
                        Quantity = 1
                    }
                },
                Mode = "payment",
                SuccessUrl = $"{_config["Frontend:Url"]}/payment-success?session_id={{CHECKOUT_SESSION_ID}}",
                CancelUrl = $"{_config["Frontend:Url"]}/payment-cancel",

                // 👇 Pass userId so you can find them in webhook
                Metadata = new Dictionary<string, string>
                {
                    { "UserId", userId },
                    { "Credits", req.Credits.ToString() }
                }
            };

            var service = new SessionService();
            var session = await service.CreateAsync(options);

            return Ok(new { sessionUrl = session.Url });
        }

        // Webhook to confirm payment and add credits

        /// <summary>
        /// The StripeWebhook
        /// </summary>
        /// <returns>The <see cref="Task{IActionResult}"/></returns>
        [HttpPost("stripe-webhook")]
        public async Task<IActionResult> StripeWebhook()
        {
            var json = await new StreamReader(HttpContext.Request.Body).ReadToEndAsync();
            try
            {
                var stripeEvent = EventUtility.ConstructEvent(
                    json,
                    Request.Headers["Stripe-Signature"],
                    _config["Stripe:WebhookSecret"]
                );
                // Check event timestamp (prevent replay attacks)
                if (DateTime.UtcNow.Subtract(stripeEvent.Created).TotalMinutes > 5)
                {
                    return BadRequest("Event too old");
                }
                // Idempotency check
                var existingEvent = await _context.ProcessedEvents
                    .AnyAsync(e => e.StripeEventId == stripeEvent.Id);

                if (existingEvent)
                {
                    return Ok("Event already processed");
                }

                if (stripeEvent.Type != EventTypes.CheckoutSessionCompleted)
                {
                    return BadRequest("Checkout Session is not Completed");
                }
                var session = stripeEvent.Data.Object as Session;

                var userId = session.Metadata["UserId"];
                var creditsToAdd = int.Parse(session.Metadata["Credits"]); // ✅ Get from metadata


                using var transaction = await _context.Database.BeginTransactionAsync();
                try
                {
                    var user = await _userManager.FindByIdAsync(userId);
                    if (user != null)
                    {
                        user.Credits += creditsToAdd; // add credits
                                                      // Log payment
                        var payment = new Payment
                        {
                            Id = Guid.NewGuid().ToString(),
                            UserId = userId,
                            Amount = (decimal)session.AmountTotal / 100, // $5.00 in cents
                            CreditsAdded = creditsToAdd,
                            StripeSessionId = session.Id,
                            CompletedAt = DateTime.UtcNow,
                            Status = "Completed"
                        };

                        _context.Payments.Add(payment);

                        var creditTransaction = new CreditTransaction
                        {
                            UserId = userId,
                            Amount = creditsToAdd,
                            TransactionType = "purchase",
                            Description = $"Purchased {creditsToAdd} credits",
                            ReferenceId = session.Id
                        };
                        _context.CreditTransactions.Add(creditTransaction);

                        // Mark event as processed
                        _context.ProcessedEvents.Add(new ProcessedEvent
                        {
                            StripeEventId = stripeEvent.Id,
                            ProcessedAt = DateTime.UtcNow,
                            EventType = stripeEvent.Type,
                            EventData = json,
                            UserId = userId, // ✅ Add UserId for tracking

                        });
                        await _userManager.UpdateAsync(user);
                        await _context.SaveChangesAsync();
                        await transaction.CommitAsync();
                    }
                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync();
                    throw;
                }
                return Ok();


            }
            catch (StripeException e)
            {
                return BadRequest(new { error = e.Message });
            }
        }
    }
}
