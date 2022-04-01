using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace EstoqueWeb.Models
{
    public class EmailModel
    {
        public string NomeRemetente { get; set; }
        public string EmailRemetente { get; set; }
        public string Senha { get; set; }
        public string EnderecoServidorEmail { get; set; }
        public string PortaServidorEmail { get; set; }
        public bool UsarSsl { get; set; }


    }
}
