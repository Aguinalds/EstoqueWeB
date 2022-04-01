using EstoqueWeb.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.WebUtilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EstoqueWeb.Controllers
{
    public class ConfirmacaoEmail : Controller
    {
        private readonly UserManager<IdentityUser> _userManager;

        public ConfirmacaoEmail(UserManager<IdentityUser> userManager)
        {
            _userManager = userManager;
        }

        public string StatusMessage { get; set; }

        public bool EmailConfirmado { get; set; }

        public async Task<IActionResult> OnGetAsync(string userId, string token)
        {
            if (userId == null)
            {
                return RedirectToAction("Index");

            }

            if (token == null)
            {
                return RedirectToAction("Index");
            }


            var usuario = await _userManager.FindByIdAsync(userId);
            if (usuario == null)
            {
                TempData["mensagem"] = MensagemModel.Serializar("Não foi possível encontrado o usuário.", TipoMensagem.Erro);
                return RedirectToAction("Index");
            }

            token = Encoding.UTF8.GetString(WebEncoders.Base64UrlDecode(token));
            var result = await _userManager.ConfirmEmailAsync(usuario, token);
            EmailConfirmado = result.Succeeded;
            StatusMessage = EmailConfirmado ? "Email confirmado com sucesso!" : "Ocorreu um erro ao confirmar seu e-mail";

            return RedirectToAction("Index");
        }
    }
}
