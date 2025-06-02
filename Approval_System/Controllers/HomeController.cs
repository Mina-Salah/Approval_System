using System.Diagnostics;
using System.Security.Claims;
using Approval_System.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Approval_System.Controllers
{
    [Authorize]
    public class HomeController : Controller
    {
        public IActionResult Index()
        {
            var role = User.FindFirst(ClaimTypes.Role)?.Value;
            ViewBag.Role = role;
            return View();
        }
    }
}
