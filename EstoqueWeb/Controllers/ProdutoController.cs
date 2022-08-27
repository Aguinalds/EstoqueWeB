using EstoqueWeb.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Drawing;


namespace EstoqueWeb.Controllers
{
    //BANCO DE DADOS
    public class ProdutoController : Controller
    {
        private readonly EstoqueWebContext _context;

        IWebHostEnvironment _webHostEnvironment;

        public ProdutoController(EstoqueWebContext context, IWebHostEnvironment webHost)
        {
            this._context = context;
            this._webHostEnvironment = webHost;
        }

        [Authorize(Roles = "administrador,gerente")]
        //Listar
        public async Task<IActionResult> Index()
        {
            return View(await _context.Produtos.OrderBy(x => x.Nome).Include(x => x.Categoria).AsNoTracking().ToListAsync());
        }

        [Authorize(Roles = "administrador,gerente")]
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
        private bool ProdutoExist(string nome)
        {
            return _context.Produtos.Any(x => x.Nome == nome);
        }

        [Authorize(Roles = "administrador,gerente")]
        //CADASTRANDO E ALTERANDO
        [HttpPost]
        public async Task<IActionResult> Cadastrar(int? id, string nome, [FromForm] ProdutosModel produto, IList<IFormFile> files)
        {
            if (ModelState.IsValid)
            {
             

                if (id.HasValue)
                {
                    if (ProdutoExiste(id.Value))
                    {
                        string uniqueFileName = UploadedFile(produto);
                        produto.ImageUrl = uniqueFileName;
                        _context.Attach(produto);
                        _context.Entry(produto).State = EntityState.Added;
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
                    if (ProdutoExist(nome.ToString()))
                    {
                        TempData["mensagem"] = MensagemModel.Serializar("Esse produto já foi cadastrado", TipoMensagem.Erro);
                        return RedirectToAction(nameof(Index));

                    }

                    string uniqueFileName = UploadedFile(produto);
                    produto.ImageUrl = uniqueFileName;
                    _context.Attach(produto);
                    _context.Entry(produto).State = EntityState.Added;              
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

        [Authorize(Roles = "administrador,gerente")]
        private string UploadedFile(ProdutosModel produto)
        {
            string uniqueFileName = null;

            if(produto.ImgProduto != null)
            {
                string uploadsFolder = Path.Combine(_webHostEnvironment.WebRootPath, "Upload", "Images");
                uniqueFileName = Guid.NewGuid().ToString() + "_" + produto.ImgProduto.FileName;
                string filePath = Path.Combine(uploadsFolder, uniqueFileName);
                using (var fileStream = new FileStream(filePath, FileMode.Create))
                {
                    produto.ImgProduto.CopyTo(fileStream);
                }
            }
            return uniqueFileName;
        }




        //VISUALIZANDO SE O PRODUTO EXISTE PARA SER EXCLUIDO
        [Authorize(Roles = "administrador,gerente")]
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

        //EXCLUIR PRODUTO
        [Authorize(Roles = "administrador,gerente")]
        [HttpPost]
        public async Task<IActionResult> Excluir(int id)
        {
            var produto = await _context.Produtos.FindAsync(id);
            if (produto != null)
            {
                string path = Path.Combine(_webHostEnvironment.WebRootPath, "Upload", "Images");
                var imageProduto = produto.ImageUrl;
                if (imageProduto == null)
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
                    var caminhoImage = Path.Combine(path, imageProduto);
                    System.IO.File.Delete(caminhoImage);
                    _context.Produtos.Remove(produto);
                    if (await _context.SaveChangesAsync() > 0)
                        TempData["mensagem"] = MensagemModel.Serializar("Produto excluído com sucesso.");
                    else
                        TempData["mensagem"] = MensagemModel.Serializar("Não foi possível excluir o produto.", TipoMensagem.Erro);
                    return RedirectToAction(nameof(Index));
                }
            }
            else
            {
                TempData["mensagem"] = MensagemModel.Serializar("Produto não encontrado.", TipoMensagem.Erro);
                return RedirectToAction(nameof(Index));
            }
        }


    }
}
