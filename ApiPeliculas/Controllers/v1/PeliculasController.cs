using ApiPeliculas.Modelos;
using ApiPeliculas.Modelos.Dtos;
using ApiPeliculas.Repositorio.IRepositorio;
using Asp.Versioning;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ApiPeliculas.Controllers.v1
{
    [Authorize(Roles = "Admin")]
    [ApiVersion("1.0")]
    [Route("api/v{version:ApiVersion}/peliculas")]
    [ApiController]
    public class PeliculasController : ControllerBase
    {
        private readonly IPeliculaRepositorio _Irepo;
        private readonly IMapper _mapper;

        public PeliculasController(IPeliculaRepositorio irepo, IMapper imapper)
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
            foreach (var peli in peliculas)
            {
                peliculasDto.Add(_mapper.Map<PeliculaDto>(peli));
            }

            return Ok(peliculasDto);

        }

        [HttpGet("{id:int}", Name = "GetPelicula")]
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
        [ProducesResponseType(201, Type = typeof(PeliculaDto))]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public IActionResult CrearPelicula([FromForm] CrearPeliculaDto crearPeliculaDto)
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

            //subida de archivos
            if (crearPeliculaDto.Image != null)
            {
                string nombreArchivo = pelicula.Id + System.Guid.NewGuid().ToString() + Path.GetExtension(crearPeliculaDto.Image.FileName);
                string rutaArchivo = @"wwwroot\ImagenesPeliculas\" + nombreArchivo;
                var ubicacionDirectorio = Path.Combine(Directory.GetCurrentDirectory(), rutaArchivo);

                FileInfo file = new(ubicacionDirectorio);

                if (file.Exists)
                {
                    file.Delete();
                }

                using(var fileStream=new FileStream(ubicacionDirectorio,FileMode.Create))
                {
                    crearPeliculaDto.Image.CopyTo(fileStream);
                }

                var baseUrl = $"{HttpContext.Request.Scheme}://{HttpContext.Request.Host.Value}{HttpContext.Request.PathBase.Value}";
                pelicula.RutaImagen = baseUrl + "/ImagenesPeliculas/" + nombreArchivo;
                pelicula.RutaLocalImagen = rutaArchivo;

            }
            else
            {
                pelicula.RutaImagen = "https://placehold.co/600x400";
            }
            _Irepo.CrearPelicula(pelicula);
            return CreatedAtRoute("GetPelicula", new { id = pelicula.Id }, pelicula);

        }

        [HttpPut]
        [Route("{peliculaId:int}")]
        public IActionResult ActualizarPelicula([FromRoute] int peliculaId, [FromForm] ActualizarPeliculaDto actualizarPeliculaDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (actualizarPeliculaDto == null)
            {
                return BadRequest("El cuerpo de la solicitud no puede ser nulo.");
            }

            var existingPelicula = _Irepo.ExistePelicula(peliculaId);
            if (!existingPelicula)
            {
                return NotFound("Pelicula no encontrada.");
            }
            var pelicula = _mapper.Map<Pelicula>(actualizarPeliculaDto);


            //subida de archivos
            if (actualizarPeliculaDto.Image != null)
            {
                string nombreArchivo = pelicula.Id + System.Guid.NewGuid().ToString() + Path.GetExtension(actualizarPeliculaDto.Image.FileName);
                string rutaArchivo = @"wwwroot\ImagenesPeliculas\" + nombreArchivo;
                var ubicacionDirectorio = Path.Combine(Directory.GetCurrentDirectory(), rutaArchivo);

                FileInfo file = new(ubicacionDirectorio);

                if (file.Exists)
                {
                    file.Delete();
                }

                using (var fileStream = new FileStream(ubicacionDirectorio, FileMode.Create))
                {
                    actualizarPeliculaDto.Image.CopyTo(fileStream);
                }

                var baseUrl = $"{HttpContext.Request.Scheme}://{HttpContext.Request.Host.Value}{HttpContext.Request.PathBase.Value}";
                pelicula.RutaImagen = baseUrl + "/ImagenesPeliculas/" + nombreArchivo;
                pelicula.RutaLocalImagen = rutaArchivo;

            }
            else
            {
                pelicula.RutaImagen = "https://placehold.co/600x400";
            }


            _Irepo.ActualizarPelicula(pelicula);
            return NoContent();
        }

        [HttpDelete]
        [Route("{id:int}")]
        public IActionResult BorrarPelicula([FromRoute] int id)
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
        public IActionResult GetPeliculasEnCategoria([FromRoute] int id)
        {
            var peliculas = _Irepo.GetPeliculasEnCategoria(id);
            var peliculasDto = new List<PeliculaDto>();
            foreach (var peli in peliculas)
            {
                peliculasDto.Add(_mapper.Map<PeliculaDto>(peli));
            }
            return Ok(peliculasDto);
        }

        [HttpGet]
        [AllowAnonymous]    //permite que no necesite autenticacion
        [Route("Buscar/{search}")]
        public IActionResult Buscar([FromRoute] string search)
        {
            if (string.IsNullOrEmpty(search))
            {
                return BadRequest();
            }
            var peliculas = _Irepo.BuscarPelicula(search);

            var peliculasDto = new List<PeliculaDto>();

            foreach (var p in peliculas)
            {
                peliculasDto.Add(_mapper.Map<PeliculaDto>(p));
            }
            return Ok(peliculasDto);
        }



    }
}
