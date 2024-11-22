using ApiPeliculas.Modelos;

namespace ApiPeliculas.Repositorio.IRepositorio
{
    public interface ICategoriaRepositorio
    {
        ICollection<Categoria> GetCategoria();
        Categoria GetCategoria(int categoriaId);
        bool ExisteCategoria(int id);

        bool ExisteCategoria(string nombre);

        bool CrearCategoria(Categoria categoria);

        bool ActualizarCategoria(Categoria categoria);
        bool BorrarCategoria(Categoria categoria);
        bool Guardar();





    }
}
