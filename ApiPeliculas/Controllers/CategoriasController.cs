using ApiPeliculas.Modelos;
using ApiPeliculas.Modelos.Dtos;
using ApiPeliculas.Repositorio.IRepositorio;
using Asp.Versioning;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;

namespace ApiPeliculas.Controllers
{
    //authorize protejera todas las rutas
    [Authorize(Roles="admin")]
    [ApiVersion("1.0")]
    [ApiVersion("2.0")]
    [Route("api/v{version:ApiVersion}/[controller]")]
    [ApiController]
    public class CategoriasController : ControllerBase
    {
        private readonly ICategoriaRepositorio _IRepo;
        private readonly IMapper _mapper;
        public CategoriasController(ICategoriaRepositorio ctRepo,IMapper imapper)
        {
            _IRepo = ctRepo;
            _mapper = imapper;

        }

        [HttpGet]
        [MapToApiVersion("1.0")]
        [AllowAnonymous]    //permite que no necesite autenticacion
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [EnableCors("PoliticaCors")]
        public IActionResult GetCategorias()
        {
            var listaCategorias = _IRepo.GetCategorias();
            var listaCategoriasDto = new List<CategoriaDto>();
            foreach(var categoria in listaCategorias)
            {
                listaCategoriasDto.Add(_mapper.Map<CategoriaDto>(categoria));
            }

            return Ok(listaCategoriasDto);
        }


        [HttpGet]
        [AllowAnonymous]
        [MapToApiVersion("2.0")]
        public IEnumerable<string> Get()
        {
            return new string[] { "valor1", "valor2", "valor3", "valor4" };
        }




        [HttpGet("{categoriaId:int}",Name= "GetCategoria")]
        [MapToApiVersion("1.0")]
        [ResponseCache(CacheProfileName= "PorDefecto20Sec")]    //perfil de cache global
        //[ResponseCache(Location =ResponseCacheLocation.None,NoStore =true)]    nunca guardara en cache
        [AllowAnonymous]    //permite que no necesite autenticacion
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public IActionResult GetCategoria(int categoriaId)
        {
            var itemCategoria = _IRepo.GetCategoria(categoriaId);
            if (itemCategoria == null)
            {
                return NotFound();
            }
            var itemCategoriaDto = _mapper.Map<CategoriaDto>(itemCategoria);
            return Ok(itemCategoriaDto);
        }

        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public IActionResult CrearCategoria([FromBody]CrearCategoriaDto crearCategoriaDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (crearCategoriaDto == null) {
                return BadRequest(ModelState);
            }

            if (_IRepo.ExisteCategoria(crearCategoriaDto.NombreCategoria))
            {
                ModelState.AddModelError("",$"La categoria ya existe");
                return StatusCode(404,ModelState);
            }

            var categoria = _mapper.Map<Categoria>(crearCategoriaDto);

            if (!_IRepo.CrearCategoria(categoria))
            {
                ModelState.AddModelError("", $"Algo salio mal al guardar {categoria.NombreCategoria}");
                return StatusCode(404, ModelState);
            }

            return CreatedAtRoute("GetCategoria", new { categoriaId = categoria.Id }, categoria);

        }


        [HttpPatch("{categoriaId:int}", Name = "ActualizarPatchCategoria")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> ActualizarPatchCategoria(int categoriaId, [FromBody]  CategoriaDto categoriaDto)
        {
            if (categoriaDto == null)
            {
                return BadRequest("El cuerpo de la solicitud no puede ser nulo.");
            }

            var existingCategoria = _IRepo.GetCategoria(categoriaId);
            if (existingCategoria == null)
            {
                return NotFound("Categoría no encontrada.");
            }

            // Aquí puedes realizar una actualización de los valores completos de la categoría
            existingCategoria.NombreCategoria = categoriaDto.NombreCategoria ?? existingCategoria.NombreCategoria;
            existingCategoria.FechaCreacion = categoriaDto.FechaCreacion != default ? categoriaDto.FechaCreacion : existingCategoria.FechaCreacion;

            // Llamamos al repositorio para actualizar
            var updatedSuccessfully =await _IRepo.ActualizarCategoria(existingCategoria);

            if (!updatedSuccessfully)
            {
                return StatusCode(500, "Hubo un error al actualizar la categoría.");
            }

            return Ok("Categoría actualizada exitosamente.");

        }


        [HttpDelete("{categoriaId:int}", Name = "BorrarCategoria")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public IActionResult BorrarCategoria(int categoriaId)
        {

            var categoriaExiste = _IRepo.GetCategoria(categoriaId);

            if (categoriaExiste == null)
            {
                return NotFound($"No se encontro la categoria con este id {categoriaId}");
            }

            if (!_IRepo.BorrarCategoria(categoriaExiste))
            {
                ModelState.AddModelError("", $"Algo salio mal borrando el registro {categoriaExiste.NombreCategoria}");
                return StatusCode(500, ModelState);
            }

            return NoContent();

        }



    }
}
