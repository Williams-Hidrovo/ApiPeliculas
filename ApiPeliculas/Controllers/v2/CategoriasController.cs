using ApiPeliculas.Modelos;
using ApiPeliculas.Modelos.Dtos;
using ApiPeliculas.Repositorio.IRepositorio;
using Asp.Versioning;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;

namespace ApiPeliculas.Controllers.v2
{
    //authorize protejera todas las rutas
    [Authorize(Roles = "admin")]
    [ApiVersion("2.0")]    //decir que la version esta deprecada
    [Route("api/v{version:ApiVersion}/categorias")]
    [ApiController]
    
    public class CategoriasController : ControllerBase
    {
        private readonly ICategoriaRepositorio _IRepo;
        private readonly IMapper _mapper;
        public CategoriasController(ICategoriaRepositorio ctRepo, IMapper imapper)
        {
            _IRepo = ctRepo;
            _mapper = imapper;

        }

        [HttpGet("GetString")]
        [AllowAnonymous]
        [MapToApiVersion("2.0")]
        public IEnumerable<string> Get()
        {
            return new string[] { "Williams", "Programando c#" };
        }








    }
}
