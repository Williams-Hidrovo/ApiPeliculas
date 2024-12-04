using ApiPeliculas.Modelos;
using ApiPeliculas.Modelos.Dtos;
using ApiPeliculas.Repositorio.IRepositorio;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ApiPeliculas.Controllers
{
    [Authorize(Roles = "admin")]
    [Route("api/[controller]")]
    [ApiController]
    public class PeliculasController : ControllerBase
    {
        private readonly IPeliculaRepositorio _Irepo;
        private readonly IMapper _mapper;

        public PeliculasController(IPeliculaRepositorio irepo,IMapper imapper)
        {
            _Irepo = irepo;
            _mapper = imapper;

        }

        [HttpGet]
        [AllowAnonymous]    //permite que no necesite autenticacion
        public IActionResult GetPeliculas()
        {
            var peliculas = _Irepo.GetPeliculas();
            var peliculasDto = new List<PeliculaDto>();
            foreach(var peli in peliculas)
            {
                peliculasDto.Add(_mapper.Map<PeliculaDto>(peli));
            }

            return Ok(peliculasDto);

        }

        [HttpGet("{id:int}",Name = "GetPelicula")]
        [AllowAnonymous]    //permite que no necesite autenticacion
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public IActionResult GetPelicula(int id)
        {
            var itemPelicula = _Irepo.GetPelicula(id);
            if (itemPelicula == null)
            {
                return NotFound();
            }

            var itemPeliculaDto = _mapper.Map<PeliculaDto>(itemPelicula);
            return Ok(itemPeliculaDto);

        }


        [HttpPost]
        [ProducesResponseType(201,Type=typeof(PeliculaDto))]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public IActionResult CrearPelicula([FromBody] CrearPeliculaDto crearPeliculaDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (crearPeliculaDto == null)
            {
                return BadRequest(ModelState);
            }

            if (_Irepo.ExistePelicula(crearPeliculaDto.Nombre))
            {
                ModelState.AddModelError("", $"La pelicula ya existe");
                return StatusCode(404, ModelState);
            }

            var pelicula = _mapper.Map<Pelicula>(crearPeliculaDto);

            if (!_Irepo.CrearPelicula(pelicula))
            {
                ModelState.AddModelError("", $"Algo salio mal al guardar {pelicula.Nombre}");
                return StatusCode(404, ModelState);
            }

            return CreatedAtRoute("GetPelicula", new { id = pelicula.Id }, pelicula);

        }

        [HttpPut]
        [Route("{peliculaId:int}")]
        public IActionResult ActualizarPelicula([FromRoute]int peliculaId, [FromBody] PeliculaDto peliculaDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (peliculaDto == null)
            {
                return BadRequest("El cuerpo de la solicitud no puede ser nulo.");
            }

            var existingPelicula = _Irepo.ExistePelicula(peliculaId);
            if (!existingPelicula)
            {
                return NotFound("Pelicula no encontrada.");
            }
            var pelicula = _mapper.Map<Pelicula>(peliculaDto);

            if (!_Irepo.ActualizarPelicula(pelicula))
            {
                return StatusCode(500,ModelState);
            }

            return NoContent();
        }

        [HttpDelete]
        [Route("{id:int}")]
        public IActionResult BorrarPelicula([FromRoute]int id)
        {
            var existePelicula = _Irepo.GetPelicula(id);
            if (existePelicula == null)
            {
                return NotFound();
            }

            if (!_Irepo.BorrarPelicula(existePelicula))
            {
                ModelState.AddModelError("", $"No se pudo borrar la pelicula: {existePelicula.Nombre}");
                return StatusCode(500, ModelState);
            }
            return Ok();

        }

        [HttpGet]
        [AllowAnonymous]    //permite que no necesite autenticacion
        [Route("GetPeliculasEnCategorias/{id:int}")]
        public IActionResult GetPeliculasEnCategoria([FromRoute]int id)
        {
            var peliculas = _Irepo.GetPeliculasEnCategoria(id);
            var peliculasDto =new  List<PeliculaDto>();
            foreach(var peli in peliculas)
            {
                peliculasDto.Add(_mapper.Map<PeliculaDto>(peli));
            }
            return Ok(peliculasDto);
        }

        [HttpGet]
        [AllowAnonymous]    //permite que no necesite autenticacion
        [Route("Buscar/{search}")]
        public IActionResult Buscar([FromRoute]string search)
        {
            if (string.IsNullOrEmpty(search))
            {
                return BadRequest();
            }
            var peliculas = _Irepo.BuscarPelicula(search);

            var peliculasDto = new List<PeliculaDto>();

            foreach(var p in peliculas)
            {
                peliculasDto.Add(_mapper.Map<PeliculaDto>(p));
            }
            return Ok(peliculasDto);
        }



    }
}
