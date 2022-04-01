using EstoqueWeb.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace EstoqueWeb.Controllers
{
    //BANCO DE DADOS
    public class ProdutoController : Controller
    {
        private readonly EstoqueWebContext _context;

        public ProdutoController(EstoqueWebContext context)
        {
            this._context = context;
        }

        [Authorize]
        //Listar
        public async Task<IActionResult> Index()
        {
            return View(await _context.Produtos.OrderBy(x => x.Nome).Include(x => x.Categoria).AsNoTracking().ToListAsync());
        }

        [Authorize]
        //CADASTRAR
        [HttpGet]
        public async Task<IActionResult> Cadastrar(int? id)
        {
            var produtos = _context.Categorias.OrderBy(x => x.Nome).AsNoTracking().ToList();
            var produtosSelectList = new SelectList(produtos,
                nameof(CategoriaModel.IdCategoria), nameof(CategoriaModel.Nome));
            ViewBag.Categorias = produtosSelectList;

            if (id.HasValue)
            {
                var produto = await _context.Produtos.FindAsync(id);
                if (produto == null)
                {
                    return NotFound();
                }

                return View(produto);

            }

            return View(new ProdutosModel());
        }

        //CONFIRMANDO SE PRODUTO EXISTE
        private bool ProdutoExiste(int id)
        {
            return _context.Produtos.Any(x => x.IdProduto == id);
        }

        [Authorize]
        //CADASTRANDO E ALTERANDO
        [HttpPost]
        public async Task<IActionResult> Cadastrar(int? id, [FromForm] ProdutosModel produto)
        {
            if (ModelState.IsValid)
            {
                if (id.HasValue)
                {
                    if (ProdutoExiste(id.Value))
                    {
                        _context.Produtos.Update(produto);
                        if (await _context.SaveChangesAsync() > 0)
                        {
                            TempData["mensagem"] = MensagemModel.Serializar("Produto alterado com sucesso.");
                        }
                        else
                        {
                            TempData["mensagem"] = MensagemModel.Serializar("Erro ao alterar a produto.", TipoMensagem.Erro);
                        }
                    }
                    else
                    {
                        TempData["mensagem"] = MensagemModel.Serializar("Produto não encontrado.", TipoMensagem.Erro);
                    }
                }
                else
                {
                    _context.Produtos.Add(produto);
                    if (await _context.SaveChangesAsync() > 0)
                    {
                        TempData["mensagem"] = MensagemModel.Serializar("Produto cadastrado com sucesso.");
                    }
                    else
                    {
                        TempData["mensagem"] = MensagemModel.Serializar("Erro ao cadastrar o produto.", TipoMensagem.Erro);
                    }
                }

                return RedirectToAction(nameof(Index));
            }
            else
            {
                return View(produto);
            }


        }

        [Authorize]
        //EXCLUINDO
        [HttpGet]
        public async Task<IActionResult> Excluir(int? id)
        {
            if (!id.HasValue)
            {
                TempData["mensagem"] = MensagemModel.Serializar("Produto não informado", TipoMensagem.Erro);
                return RedirectToAction(nameof(Index));
            }

            var produto = await _context.Produtos.FindAsync(id);
            if (produto == null)
            {
                TempData["mensagem"] = MensagemModel.Serializar("Produto não encontrado.", TipoMensagem.Erro);
                return RedirectToAction(nameof(Index));
            }

            return View(produto);
        }

        [Authorize]
        [HttpPost]
        public async Task<IActionResult> Excluir(int id)
        {
            var produto = await _context.Produtos.FindAsync(id);
            if (produto != null)
            {
                _context.Produtos.Remove(produto);
                if (await _context.SaveChangesAsync() > 0)
                    TempData["mensagem"] = MensagemModel.Serializar("Produto excluído com sucesso.");
                else
                    TempData["mensagem"] = MensagemModel.Serializar("Não foi possível excluir o produto.", TipoMensagem.Erro);
                return RedirectToAction(nameof(Index));

            }
            else
            {
                TempData["mensagem"] = MensagemModel.Serializar("Produto não encontrado.", TipoMensagem.Erro);
                return RedirectToAction(nameof(Index));
            }
        }
    }
}
