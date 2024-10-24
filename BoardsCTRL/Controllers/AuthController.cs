﻿using BoardsCTRL.Models;
using BoardsProject.Data;
using BoardsProject.DTO;
using BoardsProject.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Net.Http;
using System.Net.Http.Json;
using System.Security.Claims;
using System.Text;
using System.Text.Json;

namespace BoardsProject.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly BoardsContext _context; // Contexto de la base de datos
        private readonly IConfiguration _configuration; // Configuración de JWT
        private readonly IHttpClientFactory _httpClientFactory; // Para hacer solicitudes HTTP

        public AuthController(BoardsContext context, IConfiguration configuration, IHttpClientFactory httpClientFactory)
        {
            _context = context;
            _configuration = configuration;
            _httpClientFactory = httpClientFactory ?? throw new ArgumentNullException(nameof(httpClientFactory));
        }

        // POST: api/Auth/register
        [HttpPost("register")]
        public async Task<IActionResult> Register(UserRegisterDto userRegisterDto)
        {
            // Verificar si el usuario ya existe
            if (await _context.Users.AnyAsync(u => u.username == userRegisterDto.username))
            {
                return BadRequest("El usuario ya existe"); // Mensaje de error
            }

            // Crear el nuevo usuario
            var user = new User
            {
                username = userRegisterDto.username,
                passwordHash = BCrypt.Net.BCrypt.HashPassword(userRegisterDto.password),
                email = userRegisterDto.email,
                roleId = userRegisterDto.RoleId,
                userStatus = true,
                createdUserBy = Environment.MachineName,
                createdUserDate = DateTime.Now
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            return Ok("Usuario registrado exitosamente"); // Mensaje de éxito
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login(UserLoginDto userLoginDto)
        {
            // Verificar si el usuario existe, validar la contraseña y comprobar si está activo
            var user = await _context.Users
    .Include(u => u.Role)
    .FirstOrDefaultAsync(u => u.username == userLoginDto.Username && u.userStatus == true);


            // Verifica si el usuario existe o está inactivo
            if (user == null )
            {
                return Unauthorized("Usuario no encontrado o inactivo"); // Mensaje de error
            }

            if (user.userStatus == false)
            {
                return Unauthorized(" inactivo"); // Mensaje de error
            }
            else
            {
                return Unauthorized("Usuario no encontrado o inactivo"); // Mensaje de error

            }

            // Verifica la contraseña
            if (!BCrypt.Net.BCrypt.Verify(userLoginDto.Password, user.passwordHash))
            {
                return Unauthorized("Credenciales inválidas"); // Mensaje de error
            }

            if (user.Role == null)
            {
                return Unauthorized("El rol del usuario no está disponible"); // Mensaje de error
            }

            // Autenticación contra el dominio
            var client = _httpClientFactory.CreateClient();
            var companyLoginRequest = new
            {
                User = userLoginDto.Username,
                Passwd = userLoginDto.Password,
                IdAplicativo = 3,
                Firma = "KdNESJeIadQ+U+Q5Qs+8BQ=="
            };

            // Enviar la solicitud al servicio externo
            var response = await client.PostAsJsonAsync("https://www.finanzauto.com.co/Services/ApiWeb/api/RespuestaLg", companyLoginRequest);

            // Verifica la respuesta del servidor externo
            if (!response.IsSuccessStatusCode)
            {
                // Log de error en caso de que la llamada al servidor externo falle
                return Unauthorized("Credenciales inválidas del servidor externo"); // Mensaje de error
            }

            // Verificar la respuesta del servicio externo
            var externalResponse = await response.Content.ReadFromJsonAsync<ExternalResponse>();
            if (externalResponse == null || externalResponse.Mensaje.CodigoMensaje != 0)
            {
                return Unauthorized("Credenciales inválidas del servidor externo"); // Mensaje de error
            }
            // Generar token JWT
            var token = GenerateJwtToken(user);

            return Ok(new
            {
                token,
                userName = user.username,
                role = user.Role.roleName,
                Message = "Inicio de sesión exitoso." // Mensaje de éxito
            });
        }


        // Generar el token JWT
        private string GenerateJwtToken(User user)
        {
            var claims = new[]
            {
                new Claim(JwtRegisteredClaimNames.Sub, user.username),
                new Claim(ClaimTypes.Role, user.Role.roleName)
            };

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: _configuration["Jwt:Issuer"],
                audience: _configuration["Jwt:Audience"],
                claims: claims,
                expires: DateTime.Now.AddHours(1),
                signingCredentials: creds);

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

       
    }
}