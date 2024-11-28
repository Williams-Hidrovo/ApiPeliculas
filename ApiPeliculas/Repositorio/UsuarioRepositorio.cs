using ApiPeliculas.Data;
using ApiPeliculas.Modelos;
using ApiPeliculas.Modelos.Dtos;
using ApiPeliculas.Repositorio.IRepositorio;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using XSystem.Security.Cryptography;

namespace ApiPeliculas.Repositorio
{
    public class UsuarioRepositorio : IUsuarioRepositorio
    {
        private readonly AplicationDbContext _dbContext;
        private string claveSecreta;
        public UsuarioRepositorio(AplicationDbContext db,IConfiguration config)
        {
            _dbContext = db;
            claveSecreta = config.GetValue<string>("ApiSettings:Secreta");
        }


        public Usuario Getusuario(int usuarioId)
        {
            return _dbContext.Usuarios.FirstOrDefault(u => u.Id == usuarioId);
        }

        public ICollection<Usuario> GetUsuarios()
        {
            //si se desean ordenar por nombre
            //return _dbContext.Usuarios.OrderBy(u => u.NombreUsuario).ToList();
            return _dbContext.Usuarios.ToList();
        }

        public bool IsUniqueUser(string nombreUsuario)
        {
            var existe= _dbContext.Usuarios.Any((u) => u.NombreUsuario == nombreUsuario);
            return !existe;
        }

        public async Task<UsuarioLoginRespuestaDto> Login(UsuarioLoginDto usuarioLoginDto)
        {
            var passwordEncriptado=obtenerMD5(usuarioLoginDto.Password);
            var usuario = _dbContext.Usuarios.FirstOrDefault(u => u.NombreUsuario.ToLower() ==
            usuarioLoginDto.NombreUsuario.ToLower()
            && u.Password== passwordEncriptado
            );
            //si el usuario no existe con la combinacion de usario y contraseña
            if (usuario == null)
            {
                return new UsuarioLoginRespuestaDto()
                {
                    Token = "",
                    Usuario = null
                };
            }
            //aqui existe el usuario entonces podemos procesar el login
            var manejadorToken = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(claveSecreta);

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new Claim[]
                {
                    new Claim(ClaimTypes.Name,usuario.NombreUsuario.ToString()),
                    new Claim(ClaimTypes.Role,usuario.Role.ToString()),
                }),
                Expires = DateTime.UtcNow.AddDays(7),
                SigningCredentials = new(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            };

            var token=manejadorToken.CreateToken(tokenDescriptor);

            UsuarioLoginRespuestaDto usuarioLoginRespuestaDto = new UsuarioLoginRespuestaDto()
            {
                Token=manejadorToken.WriteToken(token),
                Role=usuario.Role,
                Usuario=usuario
            };

            return usuarioLoginRespuestaDto;


            
        }

        public async Task<Usuario> Register(UsuarioRegistroDto usuarioRegistroDto)
        {
            var passwordEncriptado = obtenerMD5(usuarioRegistroDto.Password);
            Usuario usuario = new Usuario()
            {
                NombreUsuario= usuarioRegistroDto.NombreUsuario,
                Password=usuarioRegistroDto.Password,
                Nombre=usuarioRegistroDto.Nombre,
                Role=usuarioRegistroDto.Role
            };

            _dbContext.Usuarios.Add(usuario);
            await _dbContext.SaveChangesAsync();
            usuario.Password = passwordEncriptado;
            return usuario;

        }


        //metodo para encriptar MD5
        public static string obtenerMD5(string valor)
        {
            MD5CryptoServiceProvider x= new MD5CryptoServiceProvider();
            byte[] data=System.Text.Encoding.UTF8.GetBytes(valor);
            data=x.ComputeHash(data);
            string resp = "";
            for(int i =0;i< data.Length; i++)
            {
                resp += data[i].ToString("x2").ToLower();
            }
            return resp;
        }
    }
}
