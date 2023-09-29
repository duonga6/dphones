using App.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace App.Areas.AdminCP.Controllers
{
    [Area("AdminCP")]
    [Route("/AdminCP/[action]")]
    [Authorize(Roles = RoleName.Administrator)]
    public class AdminCPController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}