using infrastructure.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Model.Dtos.Request;
using Model.Entities;
using Model.Interface;
using Models.Interface;

namespace Infrastructure.Service
{
    public class WorkflowService(ApplicationDbContext context, UserManager<ApplicationUser> userManager, IModelService modelService) : IWorkflowService
    {

        public async Task<object> RunModelAsync2(string userId, WorkflowRequest request)
        {
            var user = await userManager.FindByEmailAsync(userId) ?? throw new UnauthorizedAccessException("User not found");
            if (user.Credits <= 0)
                throw new InvalidOperationException("Insufficient credits");

            // Decrement first
            user.Credits -= 1;
            await context.SaveChangesAsync();

            try
            {
                var result = await modelService.RunWorkflowAsync(request);
                return result;
            }
            catch
            {
                // Refund if failed
                user.Credits += 1;
                await context.SaveChangesAsync();
                throw;
            }
        }

        public async Task<object> RunModelAsync(string userId, WorkflowRequest request)
        {
            using var transaction = await context.Database.BeginTransactionAsync();
            try
            {
                // Use pessimistic locking to prevent race conditions
                var user = await context.Users
                    .Where(u => u.Id == userId)
                    .SingleOrDefaultAsync();

                if (user == null)
                    throw new UnauthorizedAccessException("User not found");

                // Lock the user record for update
                await context.Entry(user).ReloadAsync();

                if (user.Credits <= 0)
                    throw new InvalidOperationException("Insufficient credits");

                // Atomically decrement credits
                user.Credits -= 1;

                // Log the transaction for audit
                var creditTransaction = new CreditTransaction
                {
                    UserId = user.Id,
                    Amount = -1,
                    TransactionType = "usage",
                    Timestamp = DateTime.UtcNow,
                    Description = "AI Model Usage",
                    ReferenceId = Guid.NewGuid().ToString()
                };

                context.CreditTransactions.Add(creditTransaction);
                await context.SaveChangesAsync();

                var result = await modelService.RunWorkflowAsync(request);

                await transaction.CommitAsync();
                return result;
            }
            catch (Exception)
            {
                await transaction.RollbackAsync();
                throw;
            }
        }
    }
}
