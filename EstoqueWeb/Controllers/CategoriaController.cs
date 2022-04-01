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
    public class CategoriaController : Controller
    {
        private readonly EstoqueWebContext _context;

        public CategoriaController(EstoqueWebContext context)
        {
            this._context = context;
        }

        [Authorize]
        //Listar
        public async Task<IActionResult> Index()
        {
            var categorias = await _context.Categorias.OrderBy(x => x.Nome).AsNoTracking().ToListAsync();
            return View(categorias);
        }

        [Authorize]
        //CADASTRAR
        [HttpGet]
        public async Task<IActionResult> Cadastrar(int? id)
        {
            if (id.HasValue)
            {
                var categorias = await _context.Categorias.FindAsync(id);
                if (categorias == null)
                {
                    return NotFound();
                }

                return View(categorias);

            }

            return View(new CategoriaModel());
        }

        //CONFIRMANDO SE CATEGORIA EXISTE
        private bool CategoriaExiste(int id)
        {
            return _context.Categorias.Any(x => x.IdCategoria == id);
        }

        [Authorize]
        //CADASTRANDO E ALTERANDO
        [HttpPost]
        public async Task<IActionResult> Cadastrar(int? id, [FromForm] CategoriaModel categorias)
        {
            if (ModelState.IsValid)
            {
                if (id.HasValue)
                {
                    if (CategoriaExiste(id.Value))
                    {
                        _context.Update(categorias);
                        if (await _context.SaveChangesAsync() > 0)
                        {
                            TempData["mensagem"] = MensagemModel.Serializar("Categoria alterada com sucesso.");
                        }
                        else
                        {
                            TempData["mensagem"] = MensagemModel.Serializar("Erro ao alterar a categoria.", TipoMensagem.Erro);
                        }
                    }
                    else
                    {
                        TempData["mensagem"] = MensagemModel.Serializar("Categoria não encontrada.", TipoMensagem.Erro);
                    }
                }
                else
                {
                    _context.Add(categorias);
                    if (await _context.SaveChangesAsync() > 0)
                    {
                        TempData["mensagem"] = MensagemModel.Serializar("Categoria cadastrada com sucesso.");
                    }
                    else
                    {
                        TempData["mensagem"] = MensagemModel.Serializar("Erro ao cadastrar a categoria.", TipoMensagem.Erro);
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            else
            {
                return View(categorias);
            }


        }

        [Authorize]
        //EXCLUINDO
        [HttpGet]
        public async Task<IActionResult> Excluir(int? id)
        {
            if(!id.HasValue)
            {
                TempData["mensagem"] = MensagemModel.Serializar("Categoria não informada", TipoMensagem.Erro);
                return RedirectToAction(nameof(Index));
            }

            var categoria = await _context.Categorias.FindAsync(id);
            if(categoria == null)
            {
                TempData["mensagem"] = MensagemModel.Serializar("Categoria não encontrada.", TipoMensagem.Erro);
                return RedirectToAction(nameof(Index));
            }

            return View(categoria);
        }

        [Authorize]
        [HttpPost]
        public async Task<IActionResult> Excluir(int id)
        {
            var categorias = await _context.Categorias.FindAsync(id);
            if (categorias != null)
            {
                _context.Categorias.Remove(categorias);
                if (await _context.SaveChangesAsync() > 0)
                    TempData["mensagem"] = MensagemModel.Serializar("Categoria excluída com sucesso.");
                else
                    TempData["mensagem"] = MensagemModel.Serializar("Não foi possível excluir a categoria.", TipoMensagem.Erro);
                return RedirectToAction(nameof(Index));

            }else
            {
                TempData["mensagem"] = MensagemModel.Serializar("Categoria não encontrada.", TipoMensagem.Erro);
                return RedirectToAction(nameof(Index));
            }
        }
        

    }
}
