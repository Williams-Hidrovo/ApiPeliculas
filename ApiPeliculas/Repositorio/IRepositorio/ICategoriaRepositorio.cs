﻿using ApiPeliculas.Modelos;

namespace ApiPeliculas.Repositorio.IRepositorio
{
    public interface ICategoriaRepositorio
    {
        ICollection<Categoria> GetCategorias();
        Categoria GetCategoria(int categoriaId);
        bool ExisteCategoria(int id);

        bool ExisteCategoria(string nombre);

        bool CrearCategoria(Categoria categoria);

        Task<bool> ActualizarCategoria(Categoria categoria);
        bool BorrarCategoria(Categoria categoria);
        bool Guardar();





    }
}
