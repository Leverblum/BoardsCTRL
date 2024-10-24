using BoardsProject.Data;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// Configuración de la conexión a la base de datos con Entity Framework Core y SQL Server.
// La cadena de conexion se extrae de la configuracion del archivo appsettings.json.
builder.Services.AddDbContext<BoardsContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// Configurar el servicio de autenticacion JWT.
// Aqui se define el esquema de autenticacion por defecto como JWT Bearer
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    // Configuracion de validacion de token JWT
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true, // Valida que el emisor del token coincida.
        ValidateAudience = true, // Valida que el destinatario del token coincida.
        ValidateLifetime = true, // Verifica que el token no haya expirado.
        ValidateIssuerSigningKey = true, // Verifica que el token este firmado correctamente.
        ValidIssuer = builder.Configuration["Jwt:Issuer"], // Emisor valido
        ValidAudience = builder.Configuration["Jwt:Audience"], // Audiencia valida
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"])) // Clave de firma del token
    };
});

// Agregar CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll",
        builder =>
        {
            builder.AllowAnyOrigin()
                   .AllowAnyMethod()
                   .AllowAnyHeader();
        });
});

// Registrar IHttpClientFactory
builder.Services.AddHttpClient();

// Configuracion de Swagger para generar la documentacion de la API.
// Tambien se añade soporte para la autenticacion con JWT en la interfaz de Swagger.
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "API BOARDSCTRL", Version = "v1" });

    // Definicion del esquema de autenticacion JWT en Swagger.
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        In = ParameterLocation.Header, // El token JWT se enviara en el encabezado HTTP.
        Description = "JWT Authorization header using the Bearer scheme. Just enter the token, no need to include 'Bearer ' prefix.", // Instrucciones para el uso del token.
        Name = "Authorization", // Nombre del parametro del encabenzado donde se espera el token.
        Type = SecuritySchemeType.Http, // Tipo de esquema de seguridad
        Scheme = "bearer",  // Esquema de autenticacion utilizado
        BearerFormat = "JWT" // Formato del token
    });

    // Requisito de seguridad para que todas las operaciones en Swagger requieran el token JWT.
    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme, // Referencia al esquema de seguridad
                    Id = "Bearer" // Identificador del esquema de seguridad
                }
            },
            new string[] {}
        }
    });
});

builder.Services.AddAuthorization(); // Se añade el servicio de autorizacion.
builder.Services.AddControllers(); // Se añaden los controladores al servicio de inyeccion de dependencias.

var app = builder.Build();

// Middleware para habilitar CORS
app.UseCors("AllowAll"); // Agregando el middleware de CORS aquí

// Middleware que redirige todo el trafico HTTP a HTTPS para asegurar las comunicaciones.
app.UseHttpsRedirection();

// Middleware para gestionar la autenticacion.
app.UseAuthentication();

// Middleware para gestionar la autorizacion basada en roles y politicas.
app.UseAuthorization();

// Middleware para habilitar la interfaz de Swagger.
app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "Boards API v1"); // Configuracion del endpoint de Swagger
    c.DefaultModelsExpandDepth(-1); // Oculta la expancion automatica de los modelos en Swagger
});

// Mapea los controladores de las rutas HTTP.
app.MapControllers();

app.Run(); // Ejecuta la aplicacion
