using System;
using Microsoft.AspNetCore.Mvc;

namespace SimpleChat.Controllers
{
    public class HomeController : Controller
    {
        public IActionResult Index() => View();
    }
}
