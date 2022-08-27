using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace EstoqueWeb.Models
{
    [Table("Usuario")]
    public class UsuarioModel : IdentityUser<int>
    {
        public UsuarioModel()
        {
            DataCadastro = DateTime.Now;
            Status = true;
        }
    
        [Display(Name = "Nome completo")]
        [Required(ErrorMessage = "O campo {0} é de preenchimento obrigatório.")]
        public string NomeCompleto { get; set; }


        [Display(Name = "Data de Nascimento")]
        [Required(ErrorMessage = "O campo {0} é de preenchimento obrigatório.")]
        [DataType(DataType.Date)]
        public DateTime DataNascimento { get; set; }

        [Display(Name = "CPF")]
        [Required(ErrorMessage = "O campo {0} é de preenchimento obrigatório.")]
        [StringLength(11, ErrorMessage = "O campo {0} deve ter {1} dígitos.")]
        public string CPF { get; set; }

        [NotMapped]
        public int Idade
        {
            get => (int)Math.Floor((DateTime.Now - DataNascimento).TotalDays / 365.2425);
        }


        public DateTime DataCadastro { get; set; }

        public bool Status { get; set; }

     
  
        [MaxLength(16, ErrorMessage = "O tamanho máximo do campo {0} é de {1}")]
        [MinLength(8, ErrorMessage = "O tamanho mínimo do campo {0} é de {1}")]
        public string Senha { get; set; }
    }
}
