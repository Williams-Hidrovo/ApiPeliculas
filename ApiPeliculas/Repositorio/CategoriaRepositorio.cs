using ApiPeliculas.Data;
using ApiPeliculas.Modelos;
using ApiPeliculas.Repositorio.IRepositorio;
using Microsoft.EntityFrameworkCore;

namespace ApiPeliculas.Repositorio
{
    public class CategoriaRepositorio: ICategoriaRepositorio
    {
        private readonly AplicationDbContext _dbContext;

        public CategoriaRepositorio(AplicationDbContext db)
        {
            _dbContext = db;

        }

        public async Task<bool> ActualizarCategoria(Categoria categoria)
        {
            var existingCategoria = await _dbContext.Categorias.FindAsync(categoria.Id);
            if (existingCategoria == null)
            {
                return false; // Si no se encuentra la categoría, devolvemos false.
            }

            // Si la categoría existe, actualizamos los campos.
            existingCategoria.NombreCategoria = categoria.NombreCategoria ?? existingCategoria.NombreCategoria;
            existingCategoria.FechaCreacion = categoria.FechaCreacion != default ? categoria.FechaCreacion : existingCategoria.FechaCreacion;

            // Guardamos los cambios en la base de datos.
            _dbContext.Categorias.Update(existingCategoria);
            await _dbContext.SaveChangesAsync();

            return true; // Si la actualización fue exitosa, devolvemos true.
        }

        public bool BorrarCategoria(Categoria categoria)
        {
            _dbContext.Categorias.Remove(categoria);
            return Guardar();
        }

        public bool CrearCategoria(Categoria categoria)
        {
            categoria.FechaCreacion = DateTime.Now;
            _dbContext.Categorias.Add(categoria);
            return Guardar();
        }

        public bool ExisteCategoria(int id)
        {
            return _dbContext.Categorias.Any(c => c.Id == id);
        }

        public bool ExisteCategoria(string nombre)
        {
            bool valor= _dbContext.Categorias.Any(c => c.NombreCategoria.ToLower().Trim() == nombre.ToLower());
            return valor;
        }

        public ICollection<Categoria> GetCategorias()
        {
            return _dbContext.Categorias.OrderBy(c => c.NombreCategoria).ToList();
        }

        public Categoria GetCategoria(int categoriaId)
        {
            return _dbContext.Categorias.FirstOrDefault(c => c.Id == categoriaId);
        }

        public bool Guardar()
        {
            return _dbContext.SaveChanges() >= 0 ;
        }

    }
}
