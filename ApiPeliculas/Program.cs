using ApiPeliculas.Data;
using ApiPeliculas.PeliculasMapper;
using ApiPeliculas.Repositorio;
using ApiPeliculas.Repositorio.IRepositorio;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Text;
using XAct;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.


// 1----------------------------------- Configurar la cadena de conexion en el Program.cs
string connectionString = Environment.GetEnvironmentVariable("DB_CONNECTION_STRING") ?? builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddDbContext<AplicationDbContext>(opciones =>
opciones.UseSqlServer(connectionString));


//9. Soporte para cache
builder.Services.AddResponseCaching();


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

}
);



var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
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
