using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace EstoqueWeb.Controllers
{
    public class EmailController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
