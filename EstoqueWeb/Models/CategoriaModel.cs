using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace EstoqueWeb.Models
{
    public class CategoriaModel
    {
        [Key]
        public int IdCategoria { get; set; }

        [Required, MaxLength(128)]
        public string Nome { get; set; }

        public ICollection<ProdutosModel> Produtos { get; set; }
        
    }
}
