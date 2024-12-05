using ApiPeliculas.Data;
using ApiPeliculas.PeliculasMapper;
using ApiPeliculas.Repositorio;
using ApiPeliculas.Repositorio.IRepositorio;
using Asp.Versioning;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.


// 1----------------------------------- Configurar la cadena de conexion en el Program.cs
string connectionString = Environment.GetEnvironmentVariable("DB_CONNECTION_STRING") ?? builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddDbContext<AplicationDbContext>(opciones =>
opciones.UseSqlServer(connectionString));


//9. Soporte para cache

//soporte para versionamiento
var apiVersioningBuilder = builder.Services.AddApiVersioning(options =>
{
    options.AssumeDefaultVersionWhenUnspecified = true;
    options.DefaultApiVersion = new ApiVersion(1, 0);
    options.ReportApiVersions = true;
    //options.ApiVersionReader = ApiVersionReader.Combine(
    //    new QueryStringApiVersionReader("api-version")//? api-version=1.0
    //    );
});

//14 configurar api version
apiVersioningBuilder.AddApiExplorer(options =>
{
    options.GroupNameFormat = "'v'VVV";
    options.SubstituteApiVersionInUrl = true;
});


// 2 ----------------------------------- Agregar los repositorios
builder.Services.AddScoped<ICategoriaRepositorio, CategoriaRepositorio>();
builder.Services.AddScoped<IPeliculaRepositorio, PeliculaRepositorio>();
builder.Services.AddScoped<IUsuarioRepositorio, UsuarioRepositorio>();

var key = builder.Configuration.GetValue<string>("ApiSettings:Secreta");


// 3 ----------------------------------- Agregar Automapper
builder.Services.AddAutoMapper(typeof(PeliculasMapper));

// 4 ----------------------------------- Agregar Cors
// se usa (*) para todos los dominios
builder.Services.AddCors(p => p.AddPolicy("PoliticaCors", build => {
    build.WithOrigins("http://localhost:3223").AllowAnyMethod().AllowAnyHeader();
}
)) ;

//5 ------------------------------------ Activar la autenticazion
//aqui se configura la autenticacion
builder.Services.AddAuthentication(
    x =>
    {
        x.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
        x.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    }
    ).AddJwtBearer(x =>
    {
        x.RequireHttpsMetadata = false;
        x.SaveToken = true;
        x.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(key)),
            ValidateIssuer = false,
            ValidateAudience = false
        };

    });


//10. agregar perfil de cache global para usarlo en los controladores
builder.Services.AddControllers(options =>
{
    options.CacheProfiles.Add("PorDefecto20Sec",new CacheProfile() { Duration=20});
});


//11. instalar paquetes de versionamiento de api | Asp.Versioning.Mvc | Asp.Versioning.Mvc.ApiExplorer
//12. soporte para versionamiento
builder.Services.AddApiVersioning();


// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();


//8. agregar autenticacion Bearer a swagger
builder.Services.AddSwaggerGen(options => {
    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description=
        "Autenticacion jwt usando el esquema Bearer. \r\n\r\n"+
        "Ingresa la palabra 'Bearer' seguido de un [espacio] y despues su token en el campo de abajo. \r\n\r\n" +
        "Ejemplo: \"Bearer tkssdTfddfa5421\"",
        Name="Authorization",
        In=ParameterLocation.Header,
        Scheme= "Bearer"
    });
    options.AddSecurityRequirement(new OpenApiSecurityRequirement()
    {
        {
            new OpenApiSecurityScheme
            {
                Reference=new OpenApiReference
                {
                    Type=ReferenceType.SecurityScheme,
                    Id="Bearer"
                },
                Scheme="oauth2",
                Name="Bearer",
                In=ParameterLocation.Header
            },
            new List<string>()
        }

    });
    //16 agregar info de la api y su version
    
    options.SwaggerDoc("v1",new OpenApiInfo
    {
        Version = "v1.0", // Versión de la API
        Title = "ApiPeliculas", // Título de la API
        Description = "API para manejar películas, incluyendo la gestión de películas, actores y géneros.", // Descripción de lo que hace la API
        TermsOfService=new Uri("https://will-antonio.com"),
        Contact = new OpenApiContact
        {
            Name = "williams Hidrovo",
            Url = new Uri("https://tu-website.com") // Página web o repositorio
        },
        License = new OpenApiLicense
        {
            Name = "licencia personal",
            Url = new Uri("https://opensource.org/licenses/MIT") // Licencia de uso
        }
    });

    options.SwaggerDoc("v2", new OpenApiInfo
    {
        Version = "v2.0", // Versión de la API
        Title = "ApiPeliculas", // Título de la API
        Description = "API para manejar películas, incluyendo la gestión de películas, actores y géneros.", // Descripción de lo que hace la API
        TermsOfService = new Uri("https://will-antonio.com"),
        Contact = new OpenApiContact
        {
            Name = "williams Hidrovo",
            Url = new Uri("https://tu-website.com") // Página web o repositorio
        },
        License = new OpenApiLicense
        {
            Name = "licencia personal",
            Url = new Uri("https://opensource.org/licenses/MIT") // Licencia de uso
        }
    });


}
);



var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    //15. documentar swagger
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/swagger/v1/swagger.json","ApiPeliculasV1");
        options.SwaggerEndpoint("/swagger/v2/swagger.json", "ApiPeliculasV2");
    });
}

app.UseHttpsRedirection();

//6 ------------------------------------ SOPORTE PARA CORS habilitarlos
app.UseCors("PoliticaCors");





//7 ------------------------------------ Activar la autenticazion
//soporte para autenticaacion
app.UseAuthentication();

app.UseAuthorization();

app.MapControllers();

app.Run();
