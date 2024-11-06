using BoardsProject.Data;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Security.Claims;
using System.Text;
using Microsoft.Extensions.DependencyInjection; // Para AddVersionedApiExplorer y AddApiVersioning
using Microsoft.AspNetCore.Mvc.Versioning; // Para opciones de versionado
using BoardsCTRL.Extensions; // Extensión para versión de API;
using System.Reflection; // Para obtener información de ensamblados
using System.IO; // Para trabajar con rutas

var builder = WebApplication.CreateBuilder(args);

// Configuración de la conexión a la base de datos con Entity Framework Core y SQL Server.
builder.Services.AddDbContext<BoardsContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// Configurar el servicio de autenticación JWT.
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    // Configuración de validación de token JWT
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = builder.Configuration["Jwt:Issuer"],
        ValidAudience = builder.Configuration["Jwt:Audience"],
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"])),
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

// Configurar Swagger para generar documentación de la API y agregar autenticación JWT.
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "API BOARDSCTRL", Version = "v1" });
    c.SwaggerDoc("v2", new OpenApiInfo { Title = "API BOARDSCTRL v2", Version = "v2" });

    var xmlFile = "boardCtrl.xml"; // Ruta del archivo XML
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile); // Obtener la ruta completa
    c.IncludeXmlComments(xmlPath);

    // Definición del esquema de autenticación JWT en Swagger.
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        In = ParameterLocation.Header,
        Description = "JWT Authorization header using the Bearer scheme. Just enter the token, no need to include 'Bearer ' prefix.",
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT"
    });

    c.EnableAnnotations();

    // Requisito de seguridad para todas las operaciones en Swagger
    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            new string[] {}
        }
    });

    // Incluir comentarios XML en Swagger
    c.IncludeXmlComments(xmlPath);
});

// Configuración de versionamiento de API
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddApiVersioning(options =>
{
    options.DefaultApiVersion = new ApiVersion(1, 0); // Versión predeterminada
    options.AssumeDefaultVersionWhenUnspecified = true; // Usa la versión predeterminada si no se especifica ninguna
    options.ReportApiVersions = true; // Informar versiones disponibles
});

// Configuración de Swagger para versiones de API con `IApiVersionDescriptionProvider`
builder.Services.AddVersionedApiExplorer(options =>
{
    options.GroupNameFormat = "'v'VVV";  // Formato de nombre de versión
    options.SubstituteApiVersionInUrl = true;
});

builder.Services.AddAuthorization(); // Añadir servicio de autorización
builder.Services.AddControllers(); // Añadir controladores

var app = builder.Build();

// Middleware para habilitar CORS
app.UseCors("AllowAll");

// Middleware para redirigir a HTTPS
app.UseHttpsRedirection();

// Middleware para gestión de autenticación y autorización
app.UseAuthentication();
app.UseAuthorization();

// Configura la aplicación según el entorno
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "BoardsCtrl v1"); // Configuración del endpoint de Swagger
        c.SwaggerEndpoint("/swagger/v2/swagger.json", "BoardsCtrl v2");
        c.DefaultModelsExpandDepth(-1); // Opcional, oculta la expansión automática de modelos en Swagger
    });
}

// Mapear los controladores de las rutas HTTP
app.MapControllers();

// Ejecuta la aplicación
app.Run();
