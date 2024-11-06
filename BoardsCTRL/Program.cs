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
using BoardsCTRL.Extensions; // Extensi�n para versi�n de API;
using System.Reflection; // Para obtener informaci�n de ensamblados
using System.IO; // Para trabajar con rutas

var builder = WebApplication.CreateBuilder(args);

// Configuraci�n de la conexi�n a la base de datos con Entity Framework Core y SQL Server.
builder.Services.AddDbContext<BoardsContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// Configurar el servicio de autenticaci�n JWT.
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    // Configuraci�n de validaci�n de token JWT
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

// Configurar Swagger para generar documentaci�n de la API y agregar autenticaci�n JWT.
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "API BOARDSCTRL", Version = "v1" });
    c.SwaggerDoc("v2", new OpenApiInfo { Title = "API BOARDSCTRL v2", Version = "v2" });

    var xmlFile = "boardCtrl.xml"; // Ruta del archivo XML
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile); // Obtener la ruta completa
    c.IncludeXmlComments(xmlPath);

    // Definici�n del esquema de autenticaci�n JWT en Swagger.
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

// Configuraci�n de versionamiento de API
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddApiVersioning(options =>
{
    options.DefaultApiVersion = new ApiVersion(1, 0); // Versi�n predeterminada
    options.AssumeDefaultVersionWhenUnspecified = true; // Usa la versi�n predeterminada si no se especifica ninguna
    options.ReportApiVersions = true; // Informar versiones disponibles
});

// Configuraci�n de Swagger para versiones de API con `IApiVersionDescriptionProvider`
builder.Services.AddVersionedApiExplorer(options =>
{
    options.GroupNameFormat = "'v'VVV";  // Formato de nombre de versi�n
    options.SubstituteApiVersionInUrl = true;
});

builder.Services.AddAuthorization(); // A�adir servicio de autorizaci�n
builder.Services.AddControllers(); // A�adir controladores

var app = builder.Build();

// Middleware para habilitar CORS
app.UseCors("AllowAll");

// Middleware para redirigir a HTTPS
app.UseHttpsRedirection();

// Middleware para gesti�n de autenticaci�n y autorizaci�n
app.UseAuthentication();
app.UseAuthorization();

// Configura la aplicaci�n seg�n el entorno
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "BoardsCtrl v1"); // Configuraci�n del endpoint de Swagger
        c.SwaggerEndpoint("/swagger/v2/swagger.json", "BoardsCtrl v2");
        c.DefaultModelsExpandDepth(-1); // Opcional, oculta la expansi�n autom�tica de modelos en Swagger
    });
}

// Mapear los controladores de las rutas HTTP
app.MapControllers();

// Ejecuta la aplicaci�n
app.Run();
