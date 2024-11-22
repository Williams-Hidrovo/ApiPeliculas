using System.ComponentModel.DataAnnotations;

namespace ApiPeliculas.Modelos
{
    public class Categoria
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string NombreCategoria { get; set; }
        [Required]
        public DateTime FechaCreacion { get; set; }


    }
}
