using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace EstoqueWeb.Models
{
    public class EstoqueWebContext : IdentityDbContext<IdentityUser>
    {
        public DbSet<CategoriaModel> Categorias { get; set; }
        public DbSet<ProdutosModel> Produtos { get; set; }
        public DbSet<ClienteModel> Clientes { get; set; }
        public DbSet<PedidoModel> Pedidos { get; set; }
        public DbSet<ItemPedidoModel> ItemPedidos { get; set; }



        public EstoqueWebContext(DbContextOptions<EstoqueWebContext> options) : base(options)
        {

        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<CategoriaModel>().ToTable("Categoria");
            modelBuilder.Entity<ProdutosModel>().ToTable("Produto");
            modelBuilder.Entity<ClienteModel>().OwnsMany(c => c.Enderecos, e =>
            {
                e.WithOwner().HasForeignKey("IdUsuario");
                e.HasKey("IdUsuario", "IdEndereco");
            });

            modelBuilder.Entity<UsuarioModel>().Property(u => u.DataCadastro)
               .HasDefaultValueSql("datetime('now', 'localtime', 'start of day')")
               .Metadata.SetAfterSaveBehavior(PropertySaveBehavior.Ignore);
            modelBuilder.Entity<ProdutosModel>().Property(p => p.Estoque)
                .HasDefaultValue(0);
            modelBuilder.Entity<PedidoModel>()
                .OwnsOne(p => p.EnderecoEntrega, e =>
                {
                    e.Ignore(e => e.IdEndereco);
                    e.Ignore(e => e.Selecionado);
                    e.ToTable("Pedido");
                });
            modelBuilder.Entity<ItemPedidoModel>()
                .HasKey(ip => new { ip.IdPedido, ip.IdProduto });



        }
    }
}
