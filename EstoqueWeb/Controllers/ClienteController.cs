using EstoqueWeb.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace EstoqueWeb.Controllers
{
    //BANCO DE DADOS
    public class ClienteController : Controller
    {
        private readonly EstoqueWebContext _context;

        public ClienteController(EstoqueWebContext context)
        {
            this._context = context;
        }


        [Authorize(Roles = "administrador,gerente")]
        //Listar
        public async Task<IActionResult> Index()
        {
            var clientes = await _context.Clientes.OrderBy(x => x.NomeUsuario).AsNoTracking().ToListAsync();
            return View(clientes);
        }

        [Authorize(Roles = "administrador,gerente")]
        //CADASTRAR
        [HttpGet]
        public async Task<IActionResult> Cadastrar(int? id)
        {
            if (id.HasValue)
            {
                var cliente = await _context.Clientes.FindAsync(id);
                if (cliente == null)
                {
                    TempData["mensagem"] = MensagemModel.Serializar("Cliente não encontrado.", TipoMensagem.Erro);
                    return RedirectToAction("Index");
                }

                return View(cliente);

            }

            return View(new ClienteModel());
        }

        //CONFIRMANDO SE CLIENTE EXISTE
        private bool ClienteExiste(int id)
        {
            return _context.Clientes.Any(x => x.IdUsuario == id);
        }

        [Authorize(Roles = "administrador,gerente")]
        //CADASTRANDO E ALTERANDO
        [HttpPost]
        public async Task<IActionResult> Cadastrar(int? id, [FromForm] ClienteModel cliente)
        {
            if (ModelState.IsValid)
            {
                if (id > 0)
                {
                    if (ClienteExiste(id.Value))
                    {
                        _context.Update(cliente);
                        if (await _context.SaveChangesAsync() > 0)
                        {
                            TempData["mensagem"] = MensagemModel.Serializar("Cliente alterado com sucesso.");
                        }
                        else
                        {
                            TempData["mensagem"] = MensagemModel.Serializar("Erro ao alterar a Cliente.", TipoMensagem.Erro);
                        }
                    }
                    else
                    {
                        TempData["mensagem"] = MensagemModel.Serializar("Cliente não encontrado.", TipoMensagem.Erro);
                    }
                }
                else
                {
                    _context.Add(cliente);
                    if (await _context.SaveChangesAsync() > 0)
                    {
                        TempData["mensagem"] = MensagemModel.Serializar("Cliente cadastrado com sucesso.");
                    }
                    else
                    {
                        TempData["mensagem"] = MensagemModel.Serializar("Erro ao cadastrar a Cliente.", TipoMensagem.Erro);
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            else
            {
                return View(cliente);
            }


        }

        [Authorize(Roles = "administrador,gerente")]
        //EXCLUINDO
        [HttpGet]
        public async Task<IActionResult> Excluir(int? id)
        {
            if (!id.HasValue)
            {
                TempData["mensagem"] = MensagemModel.Serializar("Cliente não informado", TipoMensagem.Erro);
                return RedirectToAction(nameof(Index));
            }

            var cliente = await _context.Clientes.FindAsync(id);
            if (cliente == null)
            {
                TempData["mensagem"] = MensagemModel.Serializar("Cliente não encontrado", TipoMensagem.Erro);
                return RedirectToAction(nameof(Index));
            }

            return View(cliente);
        }

        [Authorize(Roles = "administrador,gerente")]
        [HttpPost]
        public async Task<IActionResult> Excluir(int id)
        {
            var cliente = await _context.Clientes.FindAsync(id);
            if (cliente != null)
            {
                _context.Clientes.Remove(cliente);
                if (await _context.SaveChangesAsync() > 0)
                    TempData["mensagem"] = MensagemModel.Serializar("Cliente excluído com sucesso.");
                else
                    TempData["mensagem"] = MensagemModel.Serializar("Não foi possível excluir o Cliente.", TipoMensagem.Erro);
                return RedirectToAction(nameof(Index));

            }
            else
            {
                TempData["mensagem"] = MensagemModel.Serializar("Cliente não encontrado.", TipoMensagem.Erro);
                return RedirectToAction(nameof(Index));
            }
        }


    }
}
