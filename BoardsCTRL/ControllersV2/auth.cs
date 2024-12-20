﻿using BoardsCTRL.Models;
using BoardsProject.Data;
using BoardsProject.DTO;
using BoardsProject.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Swashbuckle.AspNetCore.Annotations;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace BoardsCTRL.ControllersV2
{

    /// <summary>
    /// Controlador de autenticacion para la version 2 de la API.
    /// Proporciona endpoints para registrar usuarios y autenticar mediante JWT.
    /// </summary>

    [ApiVersion("2.0")]
    [ApiController]
    [Route("api/v{version:apiVersion}/[controller]")]
    public class auth : ControllerBase
    {
        private readonly BoardsContext _context; // Contexto de la base de datos
        private readonly IConfiguration _configuration; // Configuración de JWT
        private readonly IHttpClientFactory _httpClientFactory; // Para hacer solicitudes HTTP

        /// <summary>
        /// Constructor del controlador de autenticacion.
        /// Inicializa el contexto de base de datos, la configuracion y el cliente HTTP.
        /// </summary>
        /// <param name="context">Contexto de la base de datos.</param>
        /// <param name="configuration">Condiguracion para generar el token JWT.</param>
        /// <param name="httpClientFactory">Fabrica de clientes HTTP para autenticacion externa.</param>
        public auth(BoardsContext context, IConfiguration configuration, IHttpClientFactory httpClientFactory)
        {
            _context = context;
            _configuration = configuration;
            _httpClientFactory = httpClientFactory ?? throw new ArgumentNullException(nameof(httpClientFactory));
        }

        /// <summary>
        /// Autentica un usuario y genera un token JWT en caso de exito.
        /// </summary>
        /// <param name="userLoginDto">Datos de inicio de sesion del usuario.</param>
        /// <returns>Token JWT y detalles del usuario autenticado</returns>
        [HttpPost("login")]
        [SwaggerOperation(
            Summary = "Iniciar sesion",
            Description = "Permite a un usuario autenticarse con un nombre de usuario y contraseña.")]
        [SwaggerResponse(200, "Inicio de sesion exitoso")]
        [SwaggerResponse(401, "Credenciales invalidas o usuario inactivo")]
        public async Task<IActionResult> Login(UserLoginDto userLoginDto)
        {
            // Autenticación contra el dominio externo
            var client = _httpClientFactory.CreateClient();
            var companyLoginRequest = new
            {
                User = userLoginDto.username,
                Passwd = userLoginDto.password,
                IdAplicativo = 3,
                Firma = "KdNESJeIadQ+U+Q5Qs+8BQ=="
            };

            // Enviar solicitud al servicio externo
            var response = await client.PostAsJsonAsync("https://www.finanzauto.com.co/Services/ApiWeb/api/RespuestaLg", companyLoginRequest);

            if (!response.IsSuccessStatusCode)
            {
                return Unauthorized("Credenciales inválidas del servidor externo");
            }

            // Verificar respuesta del servicio externo
            var externalResponse = await response.Content.ReadFromJsonAsync<ExternalResponse>();
            if (externalResponse == null || externalResponse.Mensaje.CodigoMensaje != 0)
            {
                return Unauthorized("Credenciales inválidas del servidor externo");
            }

            // Obtener el usuario después de la autenticación externa
            var user = await _context.Users.Include(u => u.Role).FirstOrDefaultAsync(u => u.username == userLoginDto.username && u.userStatus == true);

            if (user == null || user.Role == null)
            {
                return Unauthorized("Usuario no encontrado o rol no disponible");
            }

            // Generar token JWT
            var token = GenerateJwtToken(user);

            return Ok(new
            {
                token,
                userName = user.username,
                role = user.Role.roleName,
                userId = user.userId,
                Message = "Inicio de sesión exitoso."
            });
        }

        /// <summary>
        /// Genera un token JWT basado en el usuario autenticado.
        /// </summary>
        /// <param name="user">Usuario autenticado.</param>
        /// <returns>Token JWT generado.</returns>

        // Generar el token JWT
        private string GenerateJwtToken(User user)
        {
            var claims = new[]
            {
            new Claim(JwtRegisteredClaimNames.Sub, user.username),
            new Claim("userId", user.userId.ToString()),
            new Claim(ClaimTypes.Role, user.Role.roleName)
            };

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: _configuration["Jwt:Issuer"],
                audience: _configuration["Jwt:Audience"],
                claims: claims,
                expires: DateTime.Now.AddHours(24),
                signingCredentials: creds);

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        /// <summary>
        /// Obtiene el ID de un rol basado en su nombre.
        /// </summary>
        /// <param name="roleName">Nombre del rol.</param>
        /// <returns>ID del rol.</returns>
        /// <exception cref="Exception">Si el rol no es encontrado</exception>

        // Obtener el Id del rol basado en el nombre del rol
        private async Task<int> GetRoleId(string roleName)
        {
            var role = await _context.Roles.FirstOrDefaultAsync(r => r.roleName == roleName);
            if (role == null)
            {
                throw new Exception("Rol no encontrado"); // Mensaje de error
            }

            return role.roleId;
        }
    }
}
