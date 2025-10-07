using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using StudentStudyAI.Services;

namespace StudentStudyAI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class UserController : ControllerBase
    {
        // TODO: Implement user management endpoints
        // - GET /api/user/profile
        // - PUT /api/user/profile
        // - GET /api/user/study-sessions
        // - POST /api/user/study-sessions
    }
}
