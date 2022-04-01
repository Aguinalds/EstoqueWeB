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
        private readonly UserManager<IdentityUser> _userManager;

        private readonly SignInManager<IdentityUser> _signInManager;

        private readonly IEmailSender _emailSender;

        public UsuarioController(UserManager<IdentityUser> userManager, SignInManager<IdentityUser> signInManager, IEmailSender emailSender)
        {
            this._emailSender = emailSender;
            this._userManager = userManager;
            this._signInManager = signInManager;

        }


        //LISTAR USUARIOS

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
                    NomeUsuario = usuarioBD.UserName,
                    Email = usuarioBD.Email,
                    Telefone = usuarioBD.PhoneNumber
                };

                return View(usuarioVM);
            }
            return View(new CadastrarUsuarioViewModel());
        }

        private bool EntidadeExiste(string id)
        {
            return (_userManager.Users.AsNoTracking().Any(u => u.Id == id));
        }


        //MAPEANDO O CADASTRO
        private static void MapearCadastrarUsuarioViewModel
            (CadastrarUsuarioViewModel entidadeOrigem, IdentityUser
            entidadeDestino)
        {
            entidadeDestino.UserName = entidadeOrigem.NomeUsuario;
            entidadeDestino.NormalizedUserName = entidadeOrigem.NomeUsuario.ToUpper().Trim();
            entidadeDestino.Email = entidadeOrigem.Email;
            entidadeDestino.NormalizedEmail = entidadeOrigem.Email.ToUpper().Trim();
            entidadeDestino.PhoneNumber = entidadeOrigem.Telefone;
        }


        //CADASTRANDO E ALTERANDO

        [HttpPost]
        public async Task<IActionResult> Cadastrar([FromForm] CadastrarUsuarioViewModel usuarioVM)
        {
            //se for alteração, não tem senha e confirmação de senha
            if (!string.IsNullOrEmpty(usuarioVM.Id))
            {
                ModelState.Remove("Senha");
                ModelState.Remove("ConfSenha");
            }

            if (ModelState.IsValid)
            {
                if (EntidadeExiste(usuarioVM.Id))
                {
                    var usuarioBD = await _userManager.FindByIdAsync(usuarioVM.Id);
                    if ((usuarioVM.Email != usuarioBD.Email) &&
                        (_userManager.Users.Any(u => u.NormalizedEmail == usuarioVM.Email.ToUpper().Trim())))
                    {
                        ModelState.AddModelError("Email",
                            "Já existe um usuário cadastrado com este e-mail.");
                        return View(usuarioVM);
                    }
                    MapearCadastrarUsuarioViewModel(usuarioVM, usuarioBD);

                    var resultado = await _userManager.UpdateAsync(usuarioBD);
                    if (resultado.Succeeded)
                    {
                        TempData["mensagem"] = MensagemModel.Serializar("Usuário alterado com sucesso.", TipoMensagem.Erro);
                        return RedirectToAction("Index");
                    }
                    else
                    {
                        TempData["mensagem"] = MensagemModel.Serializar("Erro ao alterar o usuário.", TipoMensagem.Erro);
                        foreach (var error in resultado.Errors)
                        {
                            ModelState.AddModelError(string.Empty, error.Description);
                        }
                        return View(usuarioVM);
                    }
                }
                else
                {
                    var usuarioBD = await _userManager.FindByEmailAsync(usuarioVM.Email);
                    if (usuarioBD != null)
                    {
                        TempData["mensagem"] = MensagemModel.Serializar("Esse e-mail já está sendo usado.", TipoMensagem.Erro);
                        return View("Cadastrar");
                    }

                    usuarioBD = new IdentityUser();

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
                        msg.Append($"<p> Olá, {usuarioBD.UserName}.</p>");
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
            }
            else
            {
                return View(usuarioVM);
            }
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
                if(usuario == null)
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
        [HttpPost]
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

    }
}


