using Microsoft.AspNetCore.Identity;

namespace ApiPeliculas.Modelos
{
    public class AppUsuario : IdentityUser
    {
        //hereda de IdentityUser para usar el modelo de identity
        public string Nombre { get; set; }

    }
}
