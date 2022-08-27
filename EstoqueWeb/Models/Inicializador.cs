using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace EstoqueWeb.Models
{
    public static class Inicializador
    {
        private static void InicializadorPerfis(RoleManager<IdentityRole<int>> roleManager)
        {
            if(!roleManager.RoleExistsAsync("administrador").Result)
            {
                var perfil = new IdentityRole<int>();
                perfil.Name = "administrador";
                roleManager.CreateAsync(perfil).Wait();
            }

            if (!roleManager.RoleExistsAsync("gerente").Result)
            {
                var perfil = new IdentityRole<int>();
                perfil.Name = "gerente";
                roleManager.CreateAsync(perfil).Wait();
            }
        }



        private static void InicializadorUsuarios(UserManager<UsuarioModel> userManager)
        {
            if (userManager.FindByNameAsync("estoqueweb2020@gmail.com").Result == null)
            {
                var usuario = new UsuarioModel();
                usuario.UserName = "estoqueweb2020@gmail.com";
                usuario.Email = "estoqueweb2020@gmail.com";
                usuario.NomeCompleto = "Administrador do Sistema";
                usuario.DataNascimento = new DateTime(1999, 1, 1);
                usuario.PhoneNumber = "(99) 99999-9999";
                usuario.CPF = "99999999999";
                usuario.EmailConfirmed = true;
                var resultado = userManager.CreateAsync(usuario, "1010Aa!").Result;

                if(resultado.Succeeded)
                {
              
                    userManager.AddToRoleAsync(usuario, "administrador").Wait();
                }
            }


            if (userManager.FindByNameAsync("gerente2020@gmail.com").Result == null)
            {
                var usuario = new UsuarioModel();
                usuario.UserName = "gerente2020@gmail.com";
                usuario.Email = "gerente2020@gmail.com";
                usuario.NomeCompleto = "Gerente do Sistema";
                usuario.DataNascimento = new DateTime(1999, 1, 1);
                usuario.PhoneNumber = "(99) 99999-9999";
                usuario.CPF = "00000000000";
                usuario.EmailConfirmed = true;
                var resultado = userManager.CreateAsync(usuario, "1010Aa!").Result;

                if (resultado.Succeeded)
                {

                    userManager.AddToRoleAsync(usuario, "gerente").Wait();
                }
            }

        }

        public static void InicializadorIdentity(
            UserManager<UsuarioModel> userManager,
            RoleManager<IdentityRole<int>> roleManager)
        {
            InicializadorPerfis(roleManager);
            InicializadorUsuarios(userManager);
        }

    }
}
