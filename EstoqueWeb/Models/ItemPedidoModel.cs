using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace EstoqueWeb.Models
{
    [Table("ItemPedido")]
    public class ItemPedidoModel
    {
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int IdPedido { get; set; }

        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int IdProduto { get; set; }

        public int Quantidade { get; set; }

        public double ValorUnitario { get; set; }


        [ForeignKey("IdPedido")]
        public PedidoModel Pedido { get; set; }

        [ForeignKey("IdProduto")]
        public ProdutosModel Produto { get; set; }

        [NotMapped]
        public double ValorItem { get => this.Quantidade * this.ValorUnitario; }
    }
}
