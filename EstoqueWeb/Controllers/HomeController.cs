using EstoqueWeb.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace EstoqueWeb.Controllers
{
    public class HomeController : Controller
    {
        private readonly EstoqueWebContext _context;

        public HomeController(EstoqueWebContext context)
        {
            this._context = context;
        }

        [Authorize(Roles = "administrador, gerente")]
        public async Task<IActionResult> Index()
        {
            var pedidos = await _context.Pedidos
                .Where(p => !p.DataPedido.HasValue)
                .Include(p => p.Cliente)
                .OrderByDescending(p => p.IdPedido)
                .AsNoTracking().ToListAsync();
            return View(pedidos);
        }
    }
}
