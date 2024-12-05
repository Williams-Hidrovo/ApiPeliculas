using ApiPeliculas.Data;
using ApiPeliculas.Modelos;
using ApiPeliculas.Modelos.Dtos;
using ApiPeliculas.Repositorio.IRepositorio;
using AutoMapper;
using Microsoft.AspNetCore.Identity;
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
        //clase de identity para manejar los usuarios
        private readonly UserManager<AppUsuario> _userManager;
        //clase para manejar los roles
        private readonly RoleManager<IdentityRole> _roleManager;

        private readonly IMapper _mapper;

        public UsuarioRepositorio(AplicationDbContext db,IConfiguration config, UserManager<AppUsuario> userManager, RoleManager<IdentityRole> roleManager, IMapper mapper)
        {
            _dbContext = db;
            claveSecreta = config.GetValue<string>("ApiSettings:Secreta");
            _userManager = userManager;
            _roleManager = roleManager;
            _mapper = mapper;

        }


        public AppUsuario Getusuario(string usuarioId)
        {
            return _dbContext.AppUsuarios.FirstOrDefault(u => u.Id == usuarioId);
        }

        public ICollection<AppUsuario> GetUsuarios()
        {
            //si se desean ordenar por nombre
            //return _dbContext.Usuarios.OrderBy(u => u.NombreUsuario).ToList();
            return _dbContext.AppUsuarios.ToList();
        }

        public bool IsUniqueUser(string nombreUsuario)
        {
            var existe= _dbContext.AppUsuarios.Any((u) => u.UserName == nombreUsuario);
            return !existe;
        }



        public async Task<UsuarioLoginRespuestaDto> Login(UsuarioLoginDto usuarioLoginDto)
        {
            //var passwordEncriptado = obtenermd5(usuarioLoginDto.Password);

            var usuario = _dbContext.AppUsuarios.FirstOrDefault(
                u => u.UserName.ToLower() == usuarioLoginDto.NombreUsuario.ToLower());

            bool isValid = await _userManager.CheckPasswordAsync(usuario, usuarioLoginDto.Password);

            //Validamos si el usuario no existe con la combinación de usuario y contraseña correcta
            if (usuario == null || isValid == false)
            {
                return new UsuarioLoginRespuestaDto()
                {
                    Token = "",
                    Usuario = null
                };
            }

            //Aquí existe el usuario entonces podemos procesar el login
            var roles = await _userManager.GetRolesAsync(usuario);
            var manejadoToken = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(claveSecreta);

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new Claim[]
                {
                    new Claim(ClaimTypes.Name, usuario.UserName.ToString()),
                    new Claim(ClaimTypes.Role, roles.FirstOrDefault())
                }),
                Expires = DateTime.UtcNow.AddDays(7),
                SigningCredentials = new(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            };

            var token = manejadoToken.CreateToken(tokenDescriptor);

            UsuarioLoginRespuestaDto usuarioLoginRespuestaDto = new UsuarioLoginRespuestaDto()
            {
                Token = manejadoToken.WriteToken(token),
                Usuario = _mapper.Map<UsuarioDatosDto>(usuario),
            };

            return usuarioLoginRespuestaDto;
        }




        public async Task<UsuarioDatosDto> Register(UsuarioRegistroDto usuarioRegistroDto)
        {
            //var passwordEncriptado = obtenermd5(usuarioRegistroDto.Password);

            AppUsuario usuario = new AppUsuario()
            {
                UserName = usuarioRegistroDto.NombreUsuario,
                Email = usuarioRegistroDto.NombreUsuario,
                NormalizedEmail = usuarioRegistroDto.NombreUsuario.ToUpper(),
                Nombre = usuarioRegistroDto.Nombre
            };

            var result = await _userManager.CreateAsync(usuario, usuarioRegistroDto.Password);
            if (result.Succeeded)
            {
                if (!_roleManager.RoleExistsAsync("Admin").GetAwaiter().GetResult())
                {
                    await _roleManager.CreateAsync(new IdentityRole("Admin"));
                    await _roleManager.CreateAsync(new IdentityRole("Registrado"));
                }

                await _userManager.AddToRoleAsync(usuario, "Admin");
                var usuarioRetornado = _dbContext.AppUsuarios.FirstOrDefault(u => u.UserName == usuarioRegistroDto.NombreUsuario);

                return _mapper.Map<UsuarioDatosDto>(usuarioRetornado);
            }

            //_bd.Usuario.Add(usuario);
            //await _bd.SaveChangesAsync();
            //usuario.Password = passwordEncriptado;
            return new UsuarioDatosDto();
        }

    }
}
