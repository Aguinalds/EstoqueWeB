using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace EstoqueWeb.Models
{
    public class LoginUsuarioViewModel
    {

        [Display(Name = "Usuário")]
        [Required(ErrorMessage = "O campo {0} é de preenchimento obrigatório.")]
        public string Usuario { get; set; }

        [DataType(DataType.Password)]
        [Required(ErrorMessage = "O campo {0} é de preenchimento obrigatório.")]
        public string Senha { get; set; }

        [Required]
        [Display(Name = "Lebrar de mim.")]
        public bool Lembrar { get; set; }

        public string ReturnUrl { get; set; }
    }
}
