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
    public class ItemPedidoController : Controller
    {
        private readonly EstoqueWebContext _context;

        public ItemPedidoController(EstoqueWebContext context)
        {
            this._context = context;
        }

        [Authorize]
        //LISTAR
        public async Task<IActionResult> Index(int? ped)
        {
            if (ped.HasValue)
            {
                if (_context.Pedidos.Any(p => p.IdPedido == ped))
                {
                    var pedido = await _context.Pedidos
                        .Include(p => p.Cliente)
                        .Include(p => p.ItensPedido.OrderBy(i => i.Produto.Nome))
                        .ThenInclude(i => i.Produto)
                        .FirstOrDefaultAsync(p => p.IdPedido == ped);

                    ViewBag.Pedido = pedido;
                    return View(pedido.ItensPedido);
                }
                TempData["mensagem"] = MensagemModel.Serializar("Pedido não encontrado", TipoMensagem.Erro);
                return RedirectToAction("Index", "Cliente");
            }
            TempData["mensagem"] = MensagemModel.Serializar("Pedido não informado", TipoMensagem.Erro);
            return RedirectToAction("Index", "Cliente");
        }

        [Authorize]
        //CADASTRAR
        [HttpGet]
        public async Task<IActionResult> Cadastrar(int? ped, int? prod)
        {
            if (ped.HasValue)
            {
                if (_context.Pedidos.Any(p => p.IdPedido == ped))
                {
                    var produtos = _context.Produtos
                        .OrderBy(x => x.Nome)
                        .Select(p => new { p.IdProduto, NomePreco = $"{p.Nome} ({p.Preco:C})" })
                        .AsNoTracking().ToList();
                    var produtosSelectList = new SelectList(produtos, "IdProduto", "NomePreco");
                    ViewBag.Produtos = produtosSelectList;

                    if (prod.HasValue && ItemPedidoExiste(ped.Value, prod.Value))
                    {
                        var itemPedido = await _context.ItemPedidos
                            .Include(i => i.Produto)
                            .FirstOrDefaultAsync(i => i.IdPedido == ped && i.IdProduto == prod);

                        return View(itemPedido);
                    }
                    else
                    {
                        return View(new ItemPedidoModel()
                        {
                            IdPedido = ped.Value,
                            ValorUnitario = 0,
                            Quantidade = 1

                        });
                    }

                }
                TempData["mensagem"] = MensagemModel.Serializar("Pedido não encontrado.", TipoMensagem.Erro);
                return RedirectToAction("Index", "Cliente");
            }
            TempData["mensagem"] = MensagemModel.Serializar("Pedido não informado.", TipoMensagem.Erro);
            return RedirectToAction("Index", "Cliente");
        }

        //CONFIRMANDO SE CATEGORIA EXISTE
        private bool ItemPedidoExiste(int ped, int prod)
        {
            return _context.ItemPedidos.Any(x => x.IdPedido == ped && x.IdProduto == prod);
        }


        [Authorize]
        //CADASTRANDO E ALTERANDO
        [HttpPost]
        public async Task<IActionResult> Cadastrar([FromForm] ItemPedidoModel itemPedidos)
        {
            if (ModelState.IsValid)
            {
                if (itemPedidos.IdPedido > 0)
                {
                    var produto = await _context.Produtos.FindAsync(itemPedidos.IdProduto);
                    itemPedidos.ValorUnitario = produto.Preco;
                    if (ItemPedidoExiste(itemPedidos.IdPedido, itemPedidos.IdProduto))
                    {
                        _context.ItemPedidos.Update(itemPedidos);
                        if (await _context.SaveChangesAsync() > 0)
                            TempData["mensagem"] = MensagemModel.Serializar("Item alterado com sucesso.");
                        else
                        {
                            TempData["mensagem"] = MensagemModel.Serializar("Erro ao alterar item", TipoMensagem.Erro);
                        }
                    }
                    else
                    {
                        _context.ItemPedidos.Add(itemPedidos);
                        if (await _context.SaveChangesAsync() > 0)
                            TempData["mensagem"] = MensagemModel.Serializar("Item cadastrado com sucesso.");
                        else
                        {
                            TempData["mensagem"] = MensagemModel.Serializar("Erro ao cadastrar item", TipoMensagem.Erro);
                        }
                    }

                    var pedido = await _context.Pedidos.FindAsync(itemPedidos.IdPedido);
                    pedido.ValorTotal = _context.ItemPedidos
                        .Where(i => i.IdPedido == itemPedidos.IdPedido)
                        .Sum(i => i.ValorUnitario * i.Quantidade);
                    await _context.SaveChangesAsync();
                    return RedirectToAction("Index", new { ped = itemPedidos.IdPedido });

                }
                else
                {
                    TempData["mensagem"] = MensagemModel.Serializar("Pedido não informado", TipoMensagem.Erro);
                    return RedirectToAction("Index", "Cliente");
                }

            }
            else
            {
                var produtos = _context.Produtos
                        .OrderBy(x => x.Nome)
                        .Select(p => new { p.IdProduto, NomePreco = $"{p.Nome} ({p.Preco:C})" })
                        .AsNoTracking().ToList();
                var produtosSelectList = new SelectList(produtos, "IdProduto", "NomePreco");
                ViewBag.Produtos = produtosSelectList;

                return View(itemPedidos);
            }


        }

        [Authorize]
        //EXCLUINDO
        [HttpGet]
        public async Task<IActionResult> Excluir(int? ped, int? prod)
        {
            if (!ped.HasValue || !prod.HasValue)
            {
                TempData["mensagem"] = MensagemModel.Serializar("Item de pedido não informado.", TipoMensagem.Erro);
                return RedirectToAction("Index", "Cliente");
            }

            if (!ItemPedidoExiste(ped.Value, prod.Value))
            {
                TempData["mensagem"] = MensagemModel.Serializar("Item de pedido não encontrado.", TipoMensagem.Erro);
                return RedirectToAction("Index", "Cliente");
            }

            var itemPedido = await _context.ItemPedidos.FindAsync(ped, prod);
            _context.Entry(itemPedido).Reference(i => i.Produto).Load();
            return View(itemPedido);
        }

        [Authorize]
        [HttpPost]
        public async Task<IActionResult> Excluir(int idPedido, int idProduto)
        {
            var itemPedidos = await _context.ItemPedidos.FindAsync(idPedido, idProduto);
            if (itemPedidos != null)
            {
                _context.ItemPedidos.Remove(itemPedidos);
                if (await _context.SaveChangesAsync() > 0)
                {
                    TempData["mensagem"] = MensagemModel.Serializar("Item do Pedido excluído com sucesso.");
                    var pedido = await _context.Pedidos.FindAsync(itemPedidos.IdPedido);
                    pedido.ValorTotal = _context.ItemPedidos
                        .Where(i => i.IdPedido == itemPedidos.IdPedido)
                        .Sum(i => i.ValorUnitario * i.Quantidade);
                    await _context.SaveChangesAsync();
                }
                else
                    TempData["mensagem"] = MensagemModel.Serializar("Não foi possível excluir o item do pedido.", TipoMensagem.Erro);
                return RedirectToAction("Index", new { ped = idPedido });

            }
            else
            {
                TempData["mensagem"] = MensagemModel.Serializar("Item de pedido não encontrado.", TipoMensagem.Erro);
                return RedirectToAction("Index", new { ped = idPedido });
            }
        }


    }
}
