using ApiPeliculas.Data;
using ApiPeliculas.Modelos;
using ApiPeliculas.Repositorio.IRepositorio;

namespace ApiPeliculas.Repositorio
{
    public class CategoriaRepositorio: ICategoriaRepositorio
    {
        private readonly AplicationDbContext _dbContext;

        public CategoriaRepositorio(AplicationDbContext db)
        {
            _dbContext = db;

        }

        public bool ActualizarCategoria(Categoria categoria)
        {
            categoria.FechaCreacion = DateTime.Now;
            _dbContext.Categorias.Update(categoria);
            return Guardar();
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

        public ICollection<Categoria> GetCategoria()
        {
            return _dbContext.Categorias.OrderBy(c => c.NombreCategoria).ToList();
        }

        public Categoria GetCategoria(int categoriaId)
        {
            return _dbContext.Categorias.FirstOrDefault(c => c.Id == categoriaId);
        }

        public bool Guardar()
        {
            return _dbContext.SaveChanges() >= 0 ? true : false;
        }
    }
}
