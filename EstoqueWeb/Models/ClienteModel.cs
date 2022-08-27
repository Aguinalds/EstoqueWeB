using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace EstoqueWeb.Models
{
    [Table("Cliente")]
    public class ClienteModel
    {
     
        [Key]
        public int IdUsuario { get; set; }

        [Display(Name = "Nome")]
        [Required(ErrorMessage = "O campo {0} é de preenchimento obrigatório.")]
        public string NomeUsuario { get; set; }

        [Display(Name = "CPF")]
        [Required(ErrorMessage = "O campo {0} é de preenchimento obrigatório.")]
        [MinLength(11, ErrorMessage = "O tamanho mínimo do campo {0} é de {1} caracteres.")]
        [StringLength(11, ErrorMessage = "O campo {0} deve ter {1} dígitos.")]
        public string CPFUsuario { get; set; } 

        public string EmailUsuario { get; set; }


        [Display(Name = "Data de Nascimento")]
        [Required(ErrorMessage = "O campo {0} é de preenchimento obrigatório.")]
        [DataType(DataType.Date)]
        public DateTime DataNascimento { get; set; }

        [NotMapped]
        public int Idade
        {
            get => (int)Math.Floor((DateTime.Now - DataNascimento).TotalDays / 365.2425);
        }


        public ICollection<EnderecoModel> Enderecos { get; set; }

        public ICollection<PedidoModel> Pedidos { get; set; }
    }
}
