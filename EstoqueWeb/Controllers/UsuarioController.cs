using EstoqueWeb.Models;
using EstoqueWeb.Services;
using EstoqueWeb.ViewModel;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Encodings.Web;
using System.Threading.Tasks;

namespace EstoqueWeb.Controllers
{
    public class UsuarioController : Controller
    {
        private readonly EstoqueWebContext _context;

        private readonly UserManager<UsuarioModel> _userManager;

        private readonly SignInManager<UsuarioModel> _signInManager;

        private readonly IEmailSender _emailSender;

        private readonly RoleManager<IdentityRole<int>> _roleManager;

        public UsuarioController(
            EstoqueWebContext context,
            UserManager<UsuarioModel> userManager,
            SignInManager<UsuarioModel> signInManager,
            IEmailSender emailSender,
            RoleManager<IdentityRole<int>> roleManager)
        {
            this._context = context;
            this._emailSender = emailSender;
            this._userManager = userManager;
            this._signInManager = signInManager;
            this._roleManager = roleManager;
        }



        //LISTAR USUARIOS
        [Authorize(Roles = "administrador")]
        //Listar
        public async Task<IActionResult> Index()
        {
            var usuarios = await _userManager.Users.AsNoTracking().ToListAsync();
            var geren = (await _userManager.GetUsersInRoleAsync("gerente"))
                .Select(u => u.UserName);
            ViewBag.Gerentes = geren;
            var admins = (await _userManager.GetUsersInRoleAsync("administrador"))
                .Select(u => u.UserName);
            ViewBag.Administradores = admins;
            return View(usuarios);
        }


        //CADASTRAR
        [HttpGet]
        public async Task<IActionResult> Cadastrar(string id)
        {
            if (!string.IsNullOrEmpty(id))
            {
                var usuarioBD = await _userManager.FindByIdAsync(id);
                if (usuarioBD == null)
                {
                    TempData["mensagem"] = MensagemModel.Serializar("Usuário não encontrado.", TipoMensagem.Erro);
                    return RedirectToAction("Index", "Home");
                }
                var usuarioVM = new CadastrarUsuarioViewModel
                {
                    Id = usuarioBD.Id,
                    NomeCompleto = usuarioBD.NomeCompleto,
                    DataNascimento = usuarioBD.DataNascimento,
                    CPF = usuarioBD.CPF,
                    Email = usuarioBD.Email,
                    Telefone = usuarioBD.PhoneNumber
                };

                return View(usuarioVM);
            }
            return View(new CadastrarUsuarioViewModel());
        }

        private bool EntidadeExiste(int id)
        {
            return (_userManager.Users.AsNoTracking().Any(u => u.Id == id));
        }


        //MAPEANDO O CADASTRO
        private static void MapearCadastrarUsuarioViewModel
            (CadastrarUsuarioViewModel entidadeOrigem, UsuarioModel
            entidadeDestino)
        {
            entidadeDestino.NomeCompleto = entidadeOrigem.NomeCompleto;
            entidadeDestino.DataNascimento = entidadeOrigem.DataNascimento;
            entidadeDestino.CPF = entidadeOrigem.CPF;
            entidadeDestino.UserName = entidadeOrigem.Email;
            entidadeDestino.NormalizedUserName = entidadeOrigem.Email.ToUpper().Trim();
            entidadeDestino.Email = entidadeOrigem.Email;
            entidadeDestino.NormalizedEmail = entidadeOrigem.Email.ToUpper().Trim();
            entidadeDestino.PhoneNumber = entidadeOrigem.Telefone;
        }


        //CADASTRANDO
        [HttpPost]
        public async Task<IActionResult> Cadastrar([FromForm] CadastrarUsuarioViewModel usuarioVM)
        {

            if (ModelState.IsValid)
            {
                var usuarioBD = await _userManager.FindByEmailAsync(usuarioVM.Email);
                if (usuarioBD != null)
                {
                    TempData["mensagem"] = MensagemModel.Serializar("Esse e-mail já está sendo usado.", TipoMensagem.Erro);
                    return View("Cadastrar");
                }

                usuarioBD = new UsuarioModel();

                MapearCadastrarUsuarioViewModel(usuarioVM, usuarioBD);

                var resultado = await _userManager.CreateAsync(
                    usuarioBD, usuarioVM.Senha);
                if (resultado.Succeeded)
                {
                    var token = await _userManager.GenerateEmailConfirmationTokenAsync(usuarioBD);
                    var urlConfirmacaoEmail = Url.Action
                        ("ConfirmarEmail", "Usuario", new { email = usuarioBD.Email, token }, Request.Scheme);
                    var msg = new StringBuilder();
                    msg.Append("<h1>EstoqueWEB : Confirmação de E-mail </h1>");
                    msg.Append($"<p> Olá, {usuarioBD.NomeCompleto}.</p>");
                    msg.Append($"<p>Por favor, confirme seu e-mail " +
                        $"<a href='{urlConfirmacaoEmail} '> Clicando aqui</a>.</p>");
                    msg.Append("<p>Atenciosamente, <br> Equipe de Suporte EstoqueWEB</p>");
                    await _emailSender.SendEmailAsync(usuarioBD.Email, "Confirmação de E-mail", "", msg.ToString());
                    TempData["mensagem"] = MensagemModel.Serializar("Cadastrado com sucesso. Uma mensagem de confirmação foi enviada para seu e-mail. Confirme para entrar no sitema");
                    return View("Login");

                }
                else
                {

                    TempData["mensagem"] = MensagemModel.Serializar("Erro ao cadastrar usuário.", TipoMensagem.Erro);
                    foreach (var error in resultado.Errors)
                    {
                        ModelState.AddModelError(string.Empty, error.Description);
                    }
                    return View(usuarioVM);
                }

            }
            else
            {
                return View(usuarioVM);
            }
        }


        //CONFIRMANDO SE O USUARIO EXISTE PARA EXCLUSÃO
        [Authorize(Roles = "administrador")]
        [HttpGet]
        public async Task<IActionResult> Excluir(int? id)
        {
            if (!id.HasValue)
            {
                TempData["mensagem"] = MensagemModel.Serializar("Usuário não informado", TipoMensagem.Erro);
                return RedirectToAction(nameof(Index));
            }

            if (!EntidadeExiste(id.Value))
            {
                TempData["mensagem"] = MensagemModel.Serializar("Usuário não encontrado.", TipoMensagem.Erro);
                return RedirectToAction(nameof(Index));
            }

            var usuario = await _userManager.FindByIdAsync(id.ToString());

            return View(usuario);
        }

        //EXCLUINDO USUARIO
        [Authorize(Roles = "administrador")]
        [HttpPost]
        public async Task<IActionResult> ExcluirPost(int id)
        {
            var usuario = await _userManager.FindByIdAsync(id.ToString());
            if (usuario != null)
            {
                var resultado = await _userManager.DeleteAsync(usuario);
                if (resultado.Succeeded)
                {
                    TempData["mensagem"] = MensagemModel.Serializar("Usuário excluído com sucesso.");
                }
                else
                {
                    TempData["mensagem"] = MensagemModel.Serializar("Não foi possível excluir o usuário.", TipoMensagem.Erro);
                }

                return RedirectToAction(nameof(Index));
            }
            TempData["mensagem"] = MensagemModel.Serializar("Usuário não encontrado.", TipoMensagem.Erro);
            return RedirectToAction(nameof(Index));
        }

        [HttpGet]

        public IActionResult Login()
        {
            return View();
        }


        //LOGIN
        [HttpPost]
        public async Task<IActionResult> Login([FromForm] LoginUsuarioViewModel login)
        {
            if (ModelState.IsValid)
            {
                var usuario = await _userManager.FindByNameAsync(login.Usuario);
                if (usuario == null)
                {
                    TempData["mensagem"] = MensagemModel.Serializar("Tentativa de login inválida, Esse Usuário não foi encontrado.", TipoMensagem.Erro);
                    return View(login);
                }
                if (!_userManager.IsEmailConfirmedAsync(usuario).Result)
                {
                    TempData["mensagem"] = MensagemModel.Serializar("Esta conta ainda não foi confirmada, Confirme o e-mail e tente novamente.", TipoMensagem.Erro);
                    return View(login);
                }
                
                var resultado = await _signInManager.PasswordSignInAsync(login.
                    Usuario, login.Senha, login.Lembrar, false);
                if (resultado.Succeeded)
                {
                    login.ReturnUrl = login.ReturnUrl ?? "~/";
                    return LocalRedirect(login.ReturnUrl);
                }
                else
                {
                    TempData["mensagem"] = MensagemModel.Serializar("Tentativa de login inválida, Confirme seu e-mail ou reveja seus dados de acesso e tente novamente.", TipoMensagem.Erro);
                    return View(login);
                }
            }
            else
            {
                return View(login);
            }
        }

        //LOGOUT
        public async Task<IActionResult> Logout(string returnUrl = null)
        {
            await _signInManager.SignOutAsync();
            if (returnUrl != null)
            {
                return LocalRedirect(returnUrl);
            }
            else
            {
                return RedirectToAction("Index", "Home");
            }
        }


        //ESQUECI SENHA
        public async Task<IActionResult> EsqueciSenha([FromForm] EsqueciSenhaModel dados)
        {
            if (ModelState.IsValid)
            {
                if (_userManager.Users.AsNoTracking().Any(u => u.NormalizedEmail == dados.Email.ToUpper().Trim()))
                {
                    var usuario = await _userManager.FindByEmailAsync(dados.Email);
                    var token = await _userManager.GeneratePasswordResetTokenAsync(usuario);
                    var urlConfirmacao = Url.Action(nameof(RedefinirSenha), "Usuario", new { token }, Request.Scheme);
                    var mensagem = new StringBuilder();
                    mensagem.Append($"<p>Olá, {usuario.UserName}.</p>");
                    mensagem.Append("<p>Houve uma solicitação de redefinição de senha para seu usuário em EstoqueWEB." +
                        " Se não foi você que fez a solicitação ignore essa mensagem, caso tenha clique no link abaixo");
                    mensagem.Append($"<p><a href='{urlConfirmacao}'> Redefinir Senha</a></p>");
                    mensagem.Append("<p>Atenciosamente, <br>Equipe de Suporte</p>");
                    await _emailSender.SendEmailAsync(usuario.Email, "Redefinição de Senha", "", mensagem.ToString());
                    return View(nameof(EmailRedefinicaoEnviado));
                }
                else
                {
                    TempData["mensagem"] = MensagemModel.Serializar("Usuário/E-mail não encontrado.", TipoMensagem.Erro);
                    return View();

                }
            }
            else
            {
                return View(dados);
            }
        }


        public IActionResult EmailRedefinicaoEnviado()
        {
            return View();
        }

        //REDEFENIR SENHA
        [HttpGet]
        public IActionResult RedefinirSenha(string token)
        {
            var modelo = new RedefinirSenhaModel();
            modelo.Token = token;
            return View(modelo);
        }

        //REDEFINIR SENHA
        [HttpPost]
        public async Task<IActionResult> RedefinirSenha([FromForm] RedefinirSenhaModel dados)
        {
            if (ModelState.IsValid)
            {

                var usuario = await _userManager.FindByEmailAsync(dados.Email);
                if (usuario == null)
                {
                    TempData["mensagem"] = MensagemModel.Serializar("Não foi possível redefinir a senha. Verifique se preencheu o email corretamente." +
                           " Se o problema persistir, entre em contato com o suporte.", TipoMensagem.Erro);

                    return View(dados);
                }
                var resultado = await _userManager.ResetPasswordAsync(usuario, dados.Token, dados.NovaSenha);
                if (resultado.Succeeded)
                {
                    TempData["mensagem"] = MensagemModel.Serializar("Senha redefinida com sucesso! Agora você já pode fazer login com a nova senha.");
                    return View(nameof(Login));
                }
                else
                {
                    TempData["mensagem"] = MensagemModel.Serializar("Não foi possível redefinir a senha. Verifique se preencheu a senha corretamente." +
                        " Se o problema persistir, entre em contato com o suporte.", TipoMensagem.Erro);
                    foreach (var error in resultado.Errors)
                    {
                        ModelState.AddModelError(string.Empty, error.Description);
                    }
                    return View(dados);
                }
            }
            else
            {
                return View(dados);
            }
        }


        [HttpGet, Authorize]
        public IActionResult AlterarSenha()
        {
            return View();
        }

        //ALTERAR SENHA
        [HttpPost, Authorize]
        public async Task<IActionResult> AlterarSenha([FromForm] AlterarSenhaModel dados)
        {
            if (ModelState.IsValid)
            {
                var usuario = await _userManager.FindByNameAsync(HttpContext.User.Identity.Name);
                var resultado = await _userManager.ChangePasswordAsync(usuario, dados.SenhaAtual, dados.NovaSenha);
                if (resultado.Succeeded)
                {
                    TempData["mensagem"] = MensagemModel.Serializar("Senha foi alterada com sucesso, Identifique-se usando a nova senha");
                    await _signInManager.SignOutAsync();
                    return RedirectToAction(nameof(Login), "Usuario");
                }
                else
                {
                    TempData["mensagem"] = MensagemModel.Serializar("Erro na alteração de senha, Confirme seus dados e tente novamente.", TipoMensagem.Erro);
                    foreach (var error in resultado.Errors)
                    {
                        ModelState.AddModelError(string.Empty, error.Description);
                    }
                    return View(dados);
                }
            }
            else
            {
                return View(dados);
            }

        }

        //CONFIRMAÇÃO DE EMAIL
        [HttpGet]
        public async Task<IActionResult> ConfirmarEmail(string email, string token)
        {
            var usuario = await _userManager.FindByEmailAsync(email);
            if (usuario == null)
            {
                TempData["mensagem"] = MensagemModel.Serializar("Usuário não encontrado.", TipoMensagem.Erro);
            }
            var resultado = await _userManager.ConfirmEmailAsync(usuario, token);
            if (resultado.Succeeded)
            {
                TempData["mensagem"] = MensagemModel.Serializar("E-mail confirmado com sucesso! Agora você já está liberado para fazer login.");
            }
            else
            {
                TempData["mensagem"] = MensagemModel.Serializar("Erro na confirmação de e-mail. Tente novamente em alguns minutos.", TipoMensagem.Erro);
            }

            return View(nameof(Login));

        }

        //PÁGINA ACESSO RESTRITO
        public IActionResult AcessoRestrito([FromQuery] string returnUrl)
        {
            return View(model: returnUrl);
        }

        //ADICIONADO PERFIL ADMINISTRADOR
        [Authorize(Roles = "administrador")]
        public async Task<IActionResult> AddAdministrador(int id)
        {
            var usuario = await _userManager.FindByIdAsync(id.ToString());
            if (usuario != null)
            {
                var resultado = await _userManager.AddToRoleAsync(usuario, "administrador");
                if (resultado.Succeeded)
                {
                    TempData["mensagem"] = MensagemModel.Serializar($"Perfil administrador adicionado com sucesso para {usuario.NomeCompleto}.");
                }
                else
                {
                    TempData["mensagem"] = MensagemModel.Serializar($"Não foi possível adicionar esse perfil como administrador {usuario.NomeCompleto}.");
                }
                return RedirectToAction(nameof(Index));
            }
            else
            {
                TempData["mensagem"] = MensagemModel.Serializar("Usuário não encontrado", TipoMensagem.Erro);
                return RedirectToAction(nameof(Index));
            }
        }

        //REMOVENDO PERFIL ADMINISTRADO
        [Authorize(Roles = "administrador")]
        public async Task<IActionResult> RemAdministrador(int id)
        {
            var usuario = await _userManager.FindByIdAsync(id.ToString());
            if (usuario != null)
            {
                var resultado = await _userManager.RemoveFromRoleAsync(usuario, "administrador");
                if (resultado.Succeeded)
                {
                    TempData["mensagem"] = MensagemModel.Serializar($"Perfil administrador removido com sucesso do {usuario.NomeCompleto}.");
                }
                else
                {
                    TempData["mensagem"] = MensagemModel.Serializar($"Não foi possível remover esse perfil como administrador {usuario.NomeCompleto}.");
                }
                return RedirectToAction(nameof(Index));
            }
            else
            {
                TempData["mensagem"] = MensagemModel.Serializar("Usuário não encontrado", TipoMensagem.Erro);
                return RedirectToAction(nameof(Index));
            }
        }

        //ADICIONANDO PERFIL GERENTE
        [Authorize(Roles = "administrador")]
        public async Task<IActionResult> AddGerente(int id)
        {
            var usuario = await _userManager.FindByIdAsync(id.ToString());
            if (usuario != null)
            {
                var resultado = await _userManager.AddToRoleAsync(usuario, "gerente");
                if (resultado.Succeeded)
                {
                    TempData["mensagem"] = MensagemModel.Serializar($"Perfil Gerente adicionado com sucesso do {usuario.NomeCompleto}.");
                }
                else
                {
                    TempData["mensagem"] = MensagemModel.Serializar($"Não foi possível remover esse perfil como Gerente {usuario.NomeCompleto}.", TipoMensagem.Erro);
                }
                return RedirectToAction(nameof(Index));
            }
            else
            {
                TempData["mensagem"] = MensagemModel.Serializar("Usuário não encontrado", TipoMensagem.Erro);
                return RedirectToAction(nameof(Index));
            }
        }

        //REMOVENDO PERFIL GERENTE
        [Authorize(Roles = "administrador")]
        public async Task<IActionResult> RemGerente(int id)
        {
            var usuario = await _userManager.FindByIdAsync(id.ToString());
            if (usuario != null)
            {
                var resultado = await _userManager.RemoveFromRoleAsync(usuario, "gerente");
                if (resultado.Succeeded)
                {
                    TempData["mensagem"] = MensagemModel.Serializar($"Perfil Gerente removido com sucesso do {usuario.NomeCompleto}.");
                }
                else
                {
                    TempData["mensagem"] = MensagemModel.Serializar($"Não foi possível remover esse perfil como Gerente {usuario.NomeCompleto}.", TipoMensagem.Erro);
                }
                return RedirectToAction(nameof(Index));
            }
            else
            {
                TempData["mensagem"] = MensagemModel.Serializar("Usuário não encontrado", TipoMensagem.Erro);
                return RedirectToAction(nameof(Index));
            }
        }

        //ENVIANDO LINK DE CONFIRMAÇÃO DE CONTA
        private async Task EnviarLinkConfirmacaoEmailAsync(UsuarioModel usuario)
        {
            var token = await _userManager.GenerateEmailConfirmationTokenAsync(usuario);
            var urlConfirmacao = Url.Action
                ("ConfirmarEmail", "Usuario", new { email = usuario.Email, token }, Request.Scheme);
            var msg = new StringBuilder();
            msg.Append("<h1>EstoqueWEB : Confirmação de E-mail </h1>");
            msg.Append($"<p> Olá, {usuario.NomeCompleto}.</p>");
            msg.Append($"<p>Recebemos seu cadastro em nosso sistema, Para concluir o processo de cadastro, clique no link a seguir " +
                $"<a href='{urlConfirmacao} '> Clicando aqui</a>.</p>");
            msg.Append("<p>Atenciosamente, <br> Equipe de Suporte EstoqueWEB</p>");
            await _emailSender.SendEmailAsync(usuario.Email, "Confirmação de E-mail", "", msg.ToString());

        }

    }
}


