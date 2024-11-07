using BoardsCTRL.DTOv2;
using BoardsProject.Data;
using BoardsProject.DTO;
using BoardsProject.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BoardsCTRL.ControllersV2
{
    [ApiVersion("2.0")]
    [ApiController]
    [Route("api/v{version:apiVersion}/[controller]")]
    public class UsersControllerV2 : ControllerBase
    {
        // Inyeccion de dependencia del contexto de la base de datos
        private readonly BoardsContext _context;

        // Constructor que inicializa el contexto de la base de datos
        public UsersControllerV2(BoardsContext context)
        {
            _context = context;
        }

        // GET: Obtener todos los usuarios con paginacion
        // Endpoint accesible por usuarios con rol Admin o User

        /// <summary>
        /// Obtiene una lista de usuarios con paginacion.
        /// </summary>
        /// <param name="pageNumber">Numero de la pagina (por defecto 1).</param>
        /// <param name="pageSize">Tamaño de la pagina (por defecto 10).</param>
        [Authorize(Roles = "Admin,User")]
        [HttpGet]
        public async Task<ActionResult<IEnumerable<UserDto>>> GetUsers([FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 10)
        {
            // Obtiene los usuarios de la base de datos, aplicando paginacion
            var users = await _context.Users
                .Skip((pageNumber - 1) * pageSize) // Omite usuarios de paginas anteriores
                .Take(pageSize) // Toma la cantidad de usuarios especificada por pageSize
                .ToListAsync();

            // Convierte los usuarios a objetos UserDto
            var userDtos = users.Select(u => new UserDto
            {
                userId = u.userId,
                username = u.username,
                roleId = u.roleId,
                userStatus = u.userStatus,
                createdUserBy = u.createdUserBy,
                modifiedUserById = u.modifiedUserById,
                createdUserDate = u.createdUserDate,
                modifiedUserDate = u.modifiedUserDate
            }).ToList();

            // Retorna los usuarios con la informacion de paginacion
            return Ok(new
            {
                TotalUsers = await _context.Users.CountAsync(), // Total de usuarios
                PageNumber = pageNumber, // Pagina actual
                PageSize = pageSize, // Tamaño de la pagina
                Users = userDtos // Lista de usuarios en esta pagina
            });
        }

        // GET: Obtener usuario por ID
        // Endpoint accesible por usuarios con rol Admin o User.

        /// <summary>
        /// Obtiene un usuario especifico por su ID.
        /// </summary>
        /// <param name="id">ID del usuario a buscar.</param>
        /// <returns>Informacion del usuario encontrado.</returns>
        [Authorize(Roles = "Admin,User")]
        [HttpGet("{id}")]
        public async Task<ActionResult<UserDto>> GetUserById(int id)
        {
            // Busca al usuario por ID en la base de datos
            var user = await _context.Users.FindAsync(id);

            // Si no se encuentra el usuario, retorna NotFound
            if (user == null)
            {
                return NotFound();
            }

            // Convierte el usuario a UserDto
            var userDto = new UserDto
            {
                userId = user.userId,
                username = user.username,
                roleId = user.roleId,
                userStatus = user.userStatus,
                createdUserBy = user.createdUserBy,
                modifiedUserById = user.modifiedUserById,
                createdUserDate = user.createdUserDate,
                modifiedUserDate = user.modifiedUserDate
            };

            // Retorna el usuario encontrado
            return Ok(userDto);
        }

        // POST: Crear Usuario
        // Endpoint accesible solo por usuarios con rol Admin

        /// <summary>
        /// Crea un nuevo usuario.
        /// </summary>
        /// <param name="userDto">Objeto DTO con los datos del nuevo usuario.</param>
        /// <returns></returns>
        [Authorize(Roles = "Admin")]
        [HttpPost]
        public async Task<ActionResult<UserDto>> CreateUser(UserDto userDto)
        {
            var userIdClaim = User.FindFirst("userId")?.Value;
            if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out int userId))
            {
                return BadRequest(new { Code = "InvalidInput", Message = "ID de usuario no encontrado." });
            }

            // Verifica si el nombre de usuario ya existe
            var existingUser = await _context.Users.FirstOrDefaultAsync(u => u.username == userDto.username);
            if (existingUser != null)
            {
                return BadRequest("El nombre de usuario ya existe.");
            }


            // Crea un nuevo usuario con los datos del DTO
            var user = new User
            {
                username = userDto.username,
                roleId = userDto.roleId,
                userStatus = userDto.userStatus,
                modifiedUserById = userId, // Asigna el usuario que lo creó
                createdUserDate = DateTime.Now // Establece la fecha actual como fecha de creación
            };

            // Añade el nuevo usuario a la base de datos
            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            // Retorna el usuario creado y el estado 201 Created
            return CreatedAtAction(nameof(GetUserById), new { id = user.userId }, userDto);
        }

        // PATCH: Actualizar parcialmente un usuario
        // Endpoint accesible solo por usuarios con rol Admin

        /// <summary>
        /// Actualiza parcialmente los datos de un usuario.
        /// </summary>
        /// <param name="id">ID del usuario a actualizar.</param>
        /// <param name="patchDoc">Documento de parche con los campos a actualizar.</param>
        /// <returns></returns>
        [Authorize(Roles = "Admin")]
        [HttpPatch("{id}")]
        public async Task<IActionResult> PartialUpdateUser(int id, [FromBody] UserDtov2 userDTO)
        {
            // Verifica si el ID es inválido o si el DTO es nulo
            if (id <= 0 || userDTO == null)
            {
                return BadRequest(new { message = "Por favor, ingrese todos los campos correctamente." });
            }

            // Busca al usuario por ID en la base de datos
            var user = await _context.Users.FindAsync(id);
            if (user == null)
            {
                return NotFound(new { message = "El usuario no fue encontrado." });
            }

            // Verifica el ID del usuario que está haciendo la modificacion
            var userIdClaim = User.FindFirst("userId")?.Value;
            if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out int modifiedById))
            {
                return BadRequest(new { message = "ID de usuario no válido. " });
            }

            // Valida que el nombre de usuario no tenga más de 50 caracteres si es proporcionado
            if (userDTO.username != null && userDTO.username.Length >50)
            {
                return BadRequest(new { message = "El nombre de usuario no puede tener mas de 50 caracteres. " });
            }

            // Valida que el correo no tenga más de 100 caracteres si es proporcionado
            if (userDTO.email != null && userDTO.email.Length > 100)
            {
                return BadRequest(new { message = "El correo no puede tener más de 100 caracteres." });
            }

            // Valida que el estado del usuario sea un valor booleano válido si es proporcionado
            if (userDTO.userStatus.HasValue && !userDTO.userStatus.HasValue)
            {
                return BadRequest(new { message = "El estado del usuarioes inválido." });
            }

            // Actualiza los campos del usuario solo si vienen en el DTO
            if (!string.IsNullOrWhiteSpace(userDTO.username))
            {
                user.username = userDTO.username;
            }

            if (!string.IsNullOrWhiteSpace(userDTO.email))
            {
                user.email = userDTO.email;
            }

            if (userDTO.roleId.HasValue)
            {
                user.roleId = userDTO.roleId.Value;
            }

            if (userDTO.userStatus.HasValue)
            {
                user.userStatus = userDTO.userStatus.Value;
            }

            // Asigna el ID del usuario que hizo la modificacion y la fecha
            user.modifiedUserById = modifiedById;
            user.modifiedUserDate = DateTime.Now;

            // Marca el usuario como modificado en el conteto de la base de datos
            _context.Entry(user).State = EntityState.Modified;

            try
            {
                // Guarda los cambios en la base de datos
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!UserExists(id))
                {
                    return NotFound(new { message = "El usuario no existe." });
                }
                throw;
            }

            return NoContent();
        }


        private bool UserExists(int id)
        {
            return _context.Users.Any(u => u.userId == id);
        }

    }
}
