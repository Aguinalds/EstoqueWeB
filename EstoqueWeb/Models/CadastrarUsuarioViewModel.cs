using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace EstoqueWeb.ViewModel
{
    public class CadastrarUsuarioViewModel
    {
        public int Id { get; set; }

        [Display(Name  = "Nome de Usuário")]
        [DataType(DataType.Text)]
        [Required(ErrorMessage = "O campo {0} é de preenchimento obrigatório.")]
        public string NomeCompleto { get; set; }

        [Display(Name = "Data de Nascimento")]
        [Required(ErrorMessage = "O campo {0} é de preenchimento obrigatório.")]
        [DataType(DataType.Date)]
        public DateTime DataNascimento { get; set; }


        [Display(Name = "CPF")]
        [Required(ErrorMessage = "O campo {0} é de preenchimento obrigatório.")]
        public string CPF { get; set; }

        [DataType(DataType.PhoneNumber)] 
        [Required(ErrorMessage = "O campo {0} é de preenchimento obrigatório.")]
        public string Telefone { get; set; }

        [Display(Name = "E-mail")]
        [MaxLength(64, ErrorMessage = "O tamanho máximo do campo {0} é de {1}")]
        [MinLength(6, ErrorMessage = "O tamanho mínimo do campo {0} é de {1}")]
        [Required(ErrorMessage = "O campo {0} é de preenchimento obrigatório;")]
        public string Email { get; set; }

        public string Senha { get; set; }

        [Display(Name = "Confirmação de Senha")]
        [DataType(DataType.Password)]
        [Required(ErrorMessage = "O campo {0} é de preenchimento obrigatório.")]
        [MaxLength(16, ErrorMessage = "O tamanho máximo do campo {0} é de {1}")]
        [MinLength(8, ErrorMessage = "O tamanho mínimo do campo {0} é de {1}")]
        [Compare(nameof(Senha), ErrorMessage = "A confirmação de senha não confere com a senha")]
        public string ConfSenha { get; set; }
    }
}
