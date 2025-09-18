using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Model.Dtos.Request;
using Models.Interface;
using System.Security.Claims;

namespace API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class WorkflowController( IWorkflowService workflowService) : ControllerBase
    {
        private readonly IWorkflowService _workflowService = workflowService;


        [Authorize]
        [HttpPost]
        [Route("run-model")]
        public async Task<IActionResult> RunModel(WorkflowRequest request)
        {
            Console.WriteLine("starting runmodel");
            var userIdClaim = User.FindFirstValue(ClaimTypes.Email);
            if (string.IsNullOrEmpty(userIdClaim))
                return Unauthorized(new { error = "User ID not found in token" });
            Console.WriteLine("userid claim " + userIdClaim);
            var userId = userIdClaim;
            Console.WriteLine("user Id " + userId);
            try
            {
                var result = await _workflowService.RunModelAsync(userId, request);
                return Ok(result);
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(new { error = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { error = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = ex.Message });
            }
        }
    }
}
