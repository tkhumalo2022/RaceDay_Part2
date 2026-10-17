using Microsoft.AspNetCore.Mvc;

namespace RaCeDay.Controllers;

public class HomeController : Controller
{
    public IActionResult Index() => View();
}
