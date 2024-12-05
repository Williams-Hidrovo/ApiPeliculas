using ApiPeliculas.Modelos;
using Microsoft.AspNetCore.Components.Server.ProtectedBrowserStorage;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage;

namespace ApiPeliculas.Data
{

    //el Contexto debe heredar de IdentityDbContext<AppUsuario>
    public class AplicationDbContext : IdentityDbContext<AppUsuario>
    {

        //---------------------todos los modelos deben pasarse al context
        public DbSet<Categoria> Categorias { get; set; }
        public DbSet<Pelicula> Peliculas { get; set; }

        public DbSet<Usuario> Usuarios { get; set; }
        public DbSet<AppUsuario> AppUsuarios { get; set; }

        public AplicationDbContext(DbContextOptions<AplicationDbContext> options) : base(options)
        {
            try
            {
                var dbCreator = Database.GetService<IDatabaseCreator>() as RelationalDatabaseCreator;
                if (dbCreator != null)
                {
                    if (!dbCreator.CanConnect())
                    {
                        dbCreator.Create();
                    }
                    if (!dbCreator.HasTables())
                    {
                        dbCreator.CreateTables();
                    }
                }
            }

            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);

            }
        }

        //metodo que necesita identity
        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);
        }






    }
}
