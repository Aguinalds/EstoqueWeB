using EstoqueWeb.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading.Tasks;
using iTextSharp.text;
using System.IO;
using iTextSharp.text.pdf;
using Microsoft.AspNetCore.Hosting;

namespace EstoqueWeb.Controllers
{
    //BANCO DE DADOS
    public class PedidoController : Controller
    {
        private readonly EstoqueWebContext _context;

        //PARA QUE A FONTE SEJA GLOBAL E USE NO MÉTODO CRIARCELULATEXTO.
        static BaseFont fonteBase = BaseFont.CreateFont(BaseFont.HELVETICA, BaseFont.CP1252, false);

        IWebHostEnvironment _webHostEnvironment;

        public PedidoController(EstoqueWebContext context, IWebHostEnvironment webHost)
        {
            this._context = context;
            this._webHostEnvironment = webHost;
        }

        [Authorize(Roles = "administrador,gerente")]
        //LISTAR PEDIDOS
        public async Task<IActionResult> Index(int? cid)
        {
            if (cid.HasValue)
            {
                var cliente = await _context.Clientes.FindAsync(cid);
                if (cliente != null)
                {
                    var pedidos = await _context.Pedidos
                        .Where(p => p.IdCliente == cid)
                        .OrderByDescending(x => x.IdPedido)
                        .AsNoTracking().ToListAsync();

                    ViewBag.Clientes = cliente;
                    return View(pedidos);

                }
                else
                {
                    TempData["mensagem"] = MensagemModel.Serializar("Cliente não encontrado.", TipoMensagem.Erro);
                    return RedirectToAction("Index", "Cliente");
                }


            }
            else
            {
                TempData["mensagem"] = MensagemModel.Serializar("Cliente não informado.", TipoMensagem.Erro);
                return RedirectToAction("Index", "Cliente");
            }

        }


        [Authorize(Roles = "administrador,gerente")]
        //CADASTRAR
        [HttpGet]
        public async Task<IActionResult> Cadastrar(int? cid)
        {
            if (cid.HasValue)
            {
                var cliente = await _context.Clientes.FindAsync(cid);
                if (cliente != null)
                {
                    _context.Entry(cliente).Collection(c => c.Pedidos).Load();
                    PedidoModel pedido = null;
                    if (_context.Pedidos.Any(p => p.IdCliente == cid && !p.DataPedido.HasValue))
                    {
                        pedido = await _context.Pedidos.FirstOrDefaultAsync(p => p.IdCliente == cid && !p.DataPedido.HasValue);
                    }
                    else
                    {
                        pedido = new PedidoModel { IdCliente = cid.Value, ValorTotal = 0 };
                        cliente.Pedidos.Add(pedido);
                        await _context.SaveChangesAsync();
                    }
                    return RedirectToAction("Index", "ItemPedido", new { ped = pedido.IdPedido });
                }
                TempData["mensagem"] = MensagemModel.Serializar("Cliente não encontrado.", TipoMensagem.Erro);
                return RedirectToAction("Index", "Cliente");
            }
            TempData["mensagem"] = MensagemModel.Serializar("Cliente não informado.", TipoMensagem.Erro);
            return RedirectToAction("Index", "Cliente");
        }

        //CONFIRMANDO SE CATEGORIA EXISTE
        private bool PedidoExiste(int id)
        {
            return _context.Pedidos.Any(x => x.IdPedido == id);
        }

        [Authorize(Roles = "administrador,gerente")]
        //CADASTRANDO E ALTERANDO
        [HttpPost]
        public async Task<IActionResult> Cadastrar(int? id, [FromForm] PedidoModel pedidos)
        {
            if (ModelState.IsValid)
            {
                if (id.HasValue)
                {
                    if (PedidoExiste(id.Value))
                    {
                        _context.Update(pedidos);
                        if (await _context.SaveChangesAsync() > 0)
                        {
                            TempData["mensagem"] = MensagemModel.Serializar("Pedido alterada com sucesso.");
                        }
                        else
                        {
                            TempData["mensagem"] = MensagemModel.Serializar("Erro ao alterar a pedido.", TipoMensagem.Erro);
                        }
                    }
                    else
                    {
                        TempData["mensagem"] = MensagemModel.Serializar("Pedido não encontrada.", TipoMensagem.Erro);
                    }
                }
                else
                {
                    _context.Add(pedidos);
                    if (await _context.SaveChangesAsync() > 0)
                    {
                        TempData["mensagem"] = MensagemModel.Serializar("Pedido cadastrada com sucesso.");
                    }
                    else
                    {
                        TempData["mensagem"] = MensagemModel.Serializar("Erro ao cadastrar a pedido.", TipoMensagem.Erro);
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            else
            {
                return View(pedidos);
            }


        }

        [Authorize(Roles = "administrador,gerente")]
        //EXCLUINDO
        [HttpGet]
        public async Task<IActionResult> Excluir(int? id)
        {
            if (!id.HasValue)
            {
                TempData["mensagem"] = MensagemModel.Serializar("Pedido não informado", TipoMensagem.Erro);
                return RedirectToAction("Index");
            }

            if (!PedidoExiste(id.Value))
            {
                TempData["mensagem"] = MensagemModel.Serializar("Pedido não encontrado", TipoMensagem.Erro);
                return RedirectToAction("Index", "Cliente");
            }

            var pedido = await _context.Pedidos
                .Include(p => p.Cliente)
                .Include(p => p.ItensPedido)
                .ThenInclude(i => i.Produto)
                .FirstOrDefaultAsync(p => p.IdPedido == id);

            return View(pedido);
        }

        [Authorize(Roles = "administrador,gerente")]
        [HttpPost]
        public async Task<IActionResult> Excluir(int id)
        {
            var pedidos = await _context.Pedidos.FindAsync(id);
            if (pedidos != null)
            {
           
                string path = Path.Combine(_webHostEnvironment.WebRootPath, "Upload", "PdfPedidos");
                var pdfPedidos = pedidos.EntregaPdf;
                if (pdfPedidos == null)
                {
                    _context.Pedidos.Remove(pedidos);
                    if (await _context.SaveChangesAsync() > 0)
                        TempData["mensagem"] = MensagemModel.Serializar("Pedido excluído com sucesso.");
                    else
                        TempData["mensagem"] = MensagemModel.Serializar("Não foi possível excluir o pedido.", TipoMensagem.Erro);
                    return RedirectToAction(nameof(Index), new { cid = pedidos.IdCliente });
                }
                else
                {
                    var caminhoPDF = Path.Combine(path, pdfPedidos);
                    System.IO.File.Delete(caminhoPDF);
                    _context.Pedidos.Remove(pedidos);
                    if (await _context.SaveChangesAsync() > 0)
                        TempData["mensagem"] = MensagemModel.Serializar("Pedido excluído com sucesso.");
                    else
                        TempData["mensagem"] = MensagemModel.Serializar("Não foi possível excluir o pedido.", TipoMensagem.Erro);
                    return RedirectToAction(nameof(Index), new { cid = pedidos.IdCliente });
                }
            }
            else
            {
                TempData["mensagem"] = MensagemModel.Serializar("Pedido não encontrado.", TipoMensagem.Erro);
                return RedirectToAction(nameof(Index), "Cliente");
            }
        }

        [Authorize(Roles = "administrador,gerente")]
        //FECHANDO PEDIDO
        [HttpGet]
        public async Task<IActionResult> Fechar(int? id)
        {
            if (!id.HasValue)
            {
                TempData["mensagem"] = MensagemModel.Serializar("Pedido não informado", TipoMensagem.Erro);
                return RedirectToAction("Index");
            }

            if (!PedidoExiste(id.Value))
            {
                TempData["mensagem"] = MensagemModel.Serializar("Pedido não encontrado", TipoMensagem.Erro);
                return RedirectToAction("Index", "Cliente");
            }

            var pedido = await _context.Pedidos
                .Include(p => p.Cliente)
                .Include(p => p.ItensPedido)
                .ThenInclude(i => i.Produto)
                .FirstOrDefaultAsync(p => p.IdPedido == id);

            return View(pedido);
        }

        [Authorize(Roles = "administrador,gerente")]
        //FECHAR PEDIDO
        [HttpPost]
        public async Task<IActionResult> Fechar(int id)
        {
            if (PedidoExiste(id))
            {
                var pedido = await _context.Pedidos
                               .Include(p => p.Cliente)
                               .Include(p => p.ItensPedido)
                               .ThenInclude(i => i.Produto)
                               .FirstOrDefaultAsync(p => p.IdPedido == id);
                if (pedido.ItensPedido.Count() > 0)
                {
                    pedido.DataPedido = DateTime.Now;
                    foreach (var item in pedido.ItensPedido)
                        item.Produto.Estoque -= item.Quantidade;
                    if (await _context.SaveChangesAsync() > 0)
                        TempData["mensagem"] = MensagemModel.Serializar("Pedido fechado com sucesso.");
                    else
                        TempData["mensagem"] = MensagemModel.Serializar("Não foi possível fechar o pedido.", TipoMensagem.Erro);
                    return RedirectToAction("Index", new { cid = pedido.IdCliente });

                }
                else
                {
                    TempData["mensagem"] = MensagemModel.Serializar("Não é possível fechar um pedido sem itens.", TipoMensagem.Erro);
                    return RedirectToAction("Index", new { cid = pedido.IdCliente });
                }
            }
            else
            {
                TempData["mensagem"] = MensagemModel.Serializar("Pedido não encontrado.");
                return RedirectToAction("Index", "Cliente");
            }

        }

        [Authorize(Roles = "administrador,gerente")]
        //ENTREGAR PEDIDO
        [HttpGet]
        public async Task<IActionResult> Entregar(int? id)
        {
            if (!id.HasValue)
            {
                TempData["mensagem"] = MensagemModel.Serializar("Pedido não informado", TipoMensagem.Erro);
                return RedirectToAction("Index");
            }

            if (!PedidoExiste(id.Value))
            {
                TempData["mensagem"] = MensagemModel.Serializar("Pedido não encontrado", TipoMensagem.Erro);
                return RedirectToAction("Index", "Cliente");
            }

            var pedido = await _context.Pedidos
                .Include(p => p.Cliente)
                .ThenInclude(c => c.Enderecos)
                .Include(p => p.ItensPedido)
                .ThenInclude(i => i.Produto)
                .FirstOrDefaultAsync(p => p.IdPedido == id);

            return View(pedido);
        }

        [Authorize(Roles = "administrador,gerente")]
        //ENTREGAR PEDIDO
        [HttpPost]
        public async Task<IActionResult> Entregar(int idPedido, int idEndereco)
        {
            if (PedidoExiste(idPedido))
            {
                var pedido = await _context.Pedidos
           .Include(p => p.Cliente)
           .ThenInclude(c => c.Enderecos)
           .Include(p => p.ItensPedido)
           .ThenInclude(i => i.Produto)
           .FirstOrDefaultAsync(p => p.IdPedido == idPedido);

                var endereco = pedido.Cliente.Enderecos
                               .FirstOrDefault(e => e.IdEndereco == idEndereco);

                if (endereco != null)
                {
                    pedido.EnderecoEntrega = endereco;
                    pedido.DataEntrega = DateTime.Now;
                    string pedidos = GerarRelatorioEmPDF(pedido);
                    pedido.EntregaPdf = pedidos;
                    if (await _context.SaveChangesAsync() > 0)
                        TempData["mensagem"] = MensagemModel.Serializar("Entrega registrada com sucesso.");
                    else
                        TempData["mensagem"] = MensagemModel.Serializar("Não foi possível registra a entrega do pedido.", TipoMensagem.Erro);
                    return RedirectToAction("Index", new { cid = pedido.IdCliente });

                }
                else
                {
                    TempData["mensagem"] = MensagemModel.Serializar("Endereço não encontrado.", TipoMensagem.Erro);
                    return RedirectToAction("Index", new { cid = pedido.IdCliente });
                }
            }
            else
            {
                TempData["mensagem"] = MensagemModel.Serializar("Pedido não encontrado.");
                return RedirectToAction("Index", "Cliente");
            }

        }


        [Authorize(Roles = "administrador,gerente")]
        //VISUALIZAR PEDIDO
        [HttpGet]
        public async Task<IActionResult> Visualizar(int? id)
        {
            if (!id.HasValue)
            {
                TempData["mensagem"] = MensagemModel.Serializar("Pedido não informado", TipoMensagem.Erro);
                return RedirectToAction("Index");
            }

            if (!PedidoExiste(id.Value))
            {
                TempData["mensagem"] = MensagemModel.Serializar("Pedido não encontrado", TipoMensagem.Erro);
                return RedirectToAction("Index", "Cliente");
            }

            var pedido = await _context.Pedidos
                .Include(p => p.Cliente)
                .ThenInclude(c => c.Enderecos)
                .Include(p => p.ItensPedido)
                .ThenInclude(i => i.Produto)
                .FirstOrDefaultAsync(p => p.IdPedido == id);

            return View(pedido);
        }

        [Authorize(Roles = "administrador,gerente")]
        //VISUALIZAR PEDIDO
        [HttpPost]
        public async Task<IActionResult> Visualizar(int id)
        {
            if (PedidoExiste(id))
            {
                var pedido = await _context.Pedidos
                               .Include(p => p.Cliente)
                               .Include(p => p.ItensPedido)
                               .ThenInclude(i => i.Produto)
                               .FirstOrDefaultAsync(p => p.IdPedido == id);
                if (pedido.ItensPedido.Count() > 0)
                {
                    pedido.DataPedido = DateTime.Now;
                    if (await _context.SaveChangesAsync() > 0)
                        TempData["mensagem"] = MensagemModel.Serializar("Pedido imprimido com sucesso.");
                    else
                        TempData["mensagem"] = MensagemModel.Serializar("Não foi imprimir o pedido.", TipoMensagem.Erro);
                    return RedirectToAction("Index", new { cid = pedido.IdCliente });
                }
                else
                {
                    TempData["mensagem"] = MensagemModel.Serializar("Não é possível imprimir um pedido sem itens.", TipoMensagem.Erro);
                    return RedirectToAction("Index", new { cid = pedido.IdCliente });
                }
            }
            else
            {
                TempData["mensagem"] = MensagemModel.Serializar("Pedido não encontrado.");
                return RedirectToAction("Index", "Cliente");
            }

        }

        [Authorize(Roles = "administrador,gerente")]
        private string GerarRelatorioEmPDF(PedidoModel pedido)
        {
            string pedidos = null;

            var pedidosSelecionado = pedido;

            if (pedidosSelecionado != null)
            {
                var pxPorMm = 72 / 25.2F;
                var pdf = new Document(PageSize.A4, 15 * pxPorMm, 15 * pxPorMm,
                    15 * pxPorMm, 20 * pxPorMm);
                string uploadsFolder = Path.Combine(_webHostEnvironment.WebRootPath, "Upload", "PdfPedidos");
                pedidos = Guid.NewGuid().ToString() + "_" + $"pedidos.{DateTime.Now.ToString("yyy.MM.dd.HH.mm.ss")}.pdf";
                var caminhoPDF = Path.Combine(uploadsFolder, pedidos);
                var arquivo = new FileStream(caminhoPDF, FileMode.Create);
                var writer = PdfWriter.GetInstance(pdf, arquivo);
                pdf.Open();



                //TITULO DO PDF
                var fonteParagrafo = new iTextSharp.text.Font(fonteBase, 15,
                    iTextSharp.text.Font.NORMAL, BaseColor.Black);
                var titulo = new Paragraph($"Entrega do Pedido {pedido.IdPedido.ToString("D4")}\n{pedido.Cliente.NomeUsuario}\n\n CPF:{pedido.Cliente.CPFUsuario}", fonteParagrafo);
                titulo.Alignment = Element.ALIGN_CENTER;
                pdf.Add(titulo);

                //TABELA DO PDF
                var tabela = new PdfPTable(4);
                tabela.DefaultCell.BorderWidth = 0;
                tabela.WidthPercentage = 100;


                //CÉLULAS DE TITULO DAS COLUNAS


                CriarCelulaTexto(tabela, "Data do pedido", PdfCell.ALIGN_CENTER, true);
                CriarCelulaTexto(tabela, "Data da Entrega", PdfCell.ALIGN_CENTER, true);
                CriarCelulaTexto(tabela, "Valor Unit", PdfCell.ALIGN_CENTER, true);
                CriarCelulaTexto(tabela, "Pedidos", PdfCell.ALIGN_CENTER, true);

                foreach (var item in pedido.ItensPedido)
                {
                    CriarCelulaTexto(tabela, pedidosSelecionado.DataPedido.ToString(), PdfPCell.ALIGN_CENTER);
                    CriarCelulaTexto(tabela, pedidosSelecionado.DataEntrega.ToString(), PdfPCell.ALIGN_CENTER);
                    CriarCelulaTexto(tabela, item.ValorUnitario.ToString("C"), PdfPCell.ALIGN_CENTER);
                    CriarCelulaTexto(tabela, item.Produto.Nome, PdfPCell.ALIGN_CENTER);

                }
                var titulo2 = new Paragraph($"Valor total {pedido.ValorTotal.Value.ToString("C")}\n\n", fonteParagrafo);
                titulo.Alignment = Element.ALIGN_RIGHT;
                pdf.Add(titulo2);    
                var titulo3 = new Paragraph($"Endereço: {pedido.EnderecoEntrega.EnderecoCompleto.ToString()}\n\n", fonteParagrafo);
                titulo.Alignment = Element.ALIGN_RIGHT;
                pdf.Add(titulo3);

                pdf.Add(tabela);





                pdf.Close();
                arquivo.Close();


            }

            return pedidos;
        }

        [Authorize(Roles = "administrador,gerente")]
        static void CriarCelulaTexto(PdfPTable tabela, string texto,
           int alinhamentoHorz = PdfPCell.ALIGN_LEFT,
           bool negrito = false, bool italico = false,
           int tamanhoFonte = 10, int alturaCelula = 25)
        {
            int estilo = iTextSharp.text.Font.NORMAL;
            if (negrito && italico)
            {
                estilo = iTextSharp.text.Font.BOLDITALIC;
            }
            else if (negrito)
            {
                estilo = iTextSharp.text.Font.BOLD;
            }
            else if (italico)
            {
                estilo = iTextSharp.text.Font.ITALIC;
            }
            var fonteCelula = new iTextSharp.text.Font(fonteBase, tamanhoFonte,
                estilo, BaseColor.Black);
            var celula = new PdfPCell(new Phrase(texto, fonteCelula));
            celula.HorizontalAlignment = alinhamentoHorz;
            celula.VerticalAlignment = PdfPCell.ALIGN_MIDDLE;
            celula.Border = 0;
            celula.BorderWidthBottom = 1;
            celula.FixedHeight = alturaCelula;
            tabela.AddCell(celula);
        }
    }
}
