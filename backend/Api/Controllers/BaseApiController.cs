using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace FixMessageAnalyzer.Api.Controllers
{
    [ApiController]
    public abstract class BaseApiController : ControllerBase
    {
        protected int GetCurrentUserId()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);

            if (userIdClaim != null && int.TryParse(userIdClaim.Value, out int userId))
            {
                return userId;
            }

            return 0; // Return 0 or -1 to indicate no authenticated user
        }

        // Optional helper to verify if we have a valid user
        protected bool IsAuthenticated()
        {
            return GetCurrentUserId() > 0;
        }
    }
}