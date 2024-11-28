using ApiPeliculas.Modelos;
using ApiPeliculas.Modelos.Dtos;

namespace ApiPeliculas.Repositorio.IRepositorio
{
    public interface IUsuarioRepositorio
    {
        ICollection<Usuario> GetUsuarios();
        Usuario Getusuario(int usuarioId);
        bool IsUniqueUser(string usuario);

        Task<UsuarioLoginRespuestaDto> Login(UsuarioLoginDto usuarioLoginDto);
        Task<Usuario> Register(UsuarioRegistroDto usuarioRegistroDto);

    }
}
