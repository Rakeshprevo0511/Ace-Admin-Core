using Ace_Admin.Models;
using dotnet_core_MVC.Models;
using Microsoft.AspNetCore.Mvc;

namespace Ace_Admin.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class Auth : ControllerBase
    {
        private readonly PracticeDbContext _context;
        private readonly TokenService _tokenService;
        public Auth(PracticeDbContext context, TokenService tokenService)
        {
            _context = context;
            _tokenService = tokenService;
        }

    }
}
