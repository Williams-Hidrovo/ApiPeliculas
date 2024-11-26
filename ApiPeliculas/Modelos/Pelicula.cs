﻿using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ApiPeliculas.Modelos
{
    //siempre agregar los modelos al dbcontext
    public class Pelicula
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string Nombre { get; set; }

        public string Descripcion { get; set; }

        public int Duracion { get; set; }

        public string RutaImagen { get; set; }

        public enum TipoClasificacion { Siete, Trece, Dieciseis, Dieciocho }
        public TipoClasificacion Clasificacion{get;set;}
        public DateTime FechaCreacion { get; set; }

        //Relacion con categoria
        public int categoriaId { get; set; }
        
        [ForeignKey("categoriaId")]
        public Categoria Categoria { get; set; }

    }
}