using ApiPeliculas.Modelos.Dtos;
using ApiPeliculas.Repositorio.IRepositorio;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;

namespace ApiPeliculas.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UsuariosController :ControllerBase
    {
        private readonly IUsuarioRepositorio _IRepo;
        private readonly IMapper _mapper;

        public UsuariosController(IUsuarioRepositorio Irepo,IMapper mapper)
        {
            _IRepo = Irepo;
            _mapper = mapper;

        }

        [HttpGet]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public IActionResult GetCategorias()
        {
            var listaUsuarios = _IRepo.GetUsuarios();
            var listaUsuariosDto = new List<UsuarioDto>();
            foreach (var usuario in listaUsuarios)
            {
                listaUsuariosDto.Add(_mapper.Map<UsuarioDto>(usuario));
            }

            return Ok(listaUsuariosDto);
        }

        [HttpGet]
        [Route("{id:int}")]
        public IActionResult GetUsuario(int id) {
            var usuario = _IRepo.Getusuario(id);
            if (usuario == null)
            {
                return NotFound();
            }
            var usuarioDto = _mapper.Map<UsuarioDto>(usuario);
            return Ok(usuarioDto);
        }


    }
}
