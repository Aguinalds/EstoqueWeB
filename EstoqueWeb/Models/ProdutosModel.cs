using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace EstoqueWeb.Models
{

    public class ProdutosModel
    {
        [Key]
        public int IdProduto { get; set; }

        public string ImageUrl { get; set; }

        [NotMapped]
        public IFormFile ImgProduto { get; set; }

        [Required, MaxLength(128)]
        public string Nome { get; set; }

        public int Estoque { get; set; }
        public double Preco { get; set; }

        [Required]
        public int IdCategoria { get; set; }
        [ForeignKey("IdCategoria")]

        public CategoriaModel Categoria { get; set; }

        

    }
}
