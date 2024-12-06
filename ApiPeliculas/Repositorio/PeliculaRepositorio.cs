using ApiPeliculas.Data;
using ApiPeliculas.Modelos;
using ApiPeliculas.Repositorio.IRepositorio;
using Microsoft.EntityFrameworkCore;

namespace ApiPeliculas.Repositorio
{
    public class PeliculaRepositorio : IPeliculaRepositorio
    {
        private readonly AplicationDbContext _dbContext;

        public PeliculaRepositorio(AplicationDbContext db)
        {
            _dbContext = db;

        }

        public bool ActualizarPelicula(Pelicula pelicula)
        {
            pelicula.FechaCreacion = DateTime.Now;
            _dbContext.Peliculas.Update(pelicula);
            return Guardar();
        }

        public bool BorrarPelicula(Pelicula pelicula)
        {
            _dbContext.Peliculas.Remove(pelicula);
            return Guardar();
        }

        public bool CrearPelicula(Pelicula pelicula)
        {
            pelicula.FechaCreacion = DateTime.Now;
            _dbContext.Peliculas.Add(pelicula);
            return Guardar();
        }

        public bool ExistePelicula(int id)
        {
            return _dbContext.Peliculas.Any(p => p.Id == id);
        }

        public bool ExistePelicula(string nombre)
        {
            bool valor= _dbContext.Peliculas.Any(p => p.Nombre.ToLower().Trim() == nombre.ToLower());
            return valor;
        }

        /*public ICollection<Pelicula> GetPeliculas()
        {
            return _dbContext.Peliculas.OrderBy(p => p.Nombre).ToList();
        }*/

        public ICollection<Pelicula> GetPeliculas(int pageNumber, int pageSize)
        {

            //añadir paginacion a la lista de peliculas
            return _dbContext.Peliculas.OrderBy(p => p.Nombre)

                .Skip((pageNumber-1)*pageSize)
                .Take(pageSize)
                .ToList();
        }



        public Pelicula GetPelicula(int id)
        {
            return _dbContext.Peliculas.FirstOrDefault(p => p.Id == id);
        }

        public ICollection<Pelicula> GetPeliculasEnCategoria(int catId)
        {
            return _dbContext.Peliculas.Include(ca => ca.Categoria).Where(pe => pe.categoriaId == catId).ToList();
        }

        public IEnumerable<Pelicula> BuscarPelicula(string nombre)
        {
            IQueryable<Pelicula> query = _dbContext.Peliculas;
            if (!string.IsNullOrEmpty(nombre))
            {
                query = query.Where(e => e.Nombre.Contains(nombre) || e.Descripcion.Contains(nombre));
            }

            return query.ToList();
        }

        public bool Guardar()
        {
            return _dbContext.SaveChanges() >= 0 ;
        }

        public int GetTotalPeliculas()
        {
            return _dbContext.Peliculas.Count();
        }
    }
}
