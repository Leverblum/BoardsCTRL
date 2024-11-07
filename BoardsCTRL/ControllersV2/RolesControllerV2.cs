using BoardsCTRL.DTOv2;
using BoardsProject.Data;
using BoardsProject.DTO;
using BoardsProject.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace BoardsCTRL.ControllersV2
{
    [ApiVersion("2.0")]
    [ApiController]
    [Route("api/v{version:apiVersion}/[controller]")]
    public class RolesControllerV2 : ControllerBase
    {
        /// <summary>
        /// Controlador para la gestion de roles en la version 2.0 de la API.
        /// Incluye endpoints para obtener, crear, y actualizar roles.
        /// </summary>
        private readonly BoardsContext _context; // Contexto de la base de datos

        // Constructor que intecta el contexto de la base de datos

        /// <summary>
        /// Constructor que inicializa el contexto de la base de datos.
        /// </summary>
        /// <param name="context">Contexto de la base de datos.</param>
        public RolesControllerV2(BoardsContext context)
        {
            _context = context;
        }

        // Metodo GET para obtener una lista paginada de roles
        // Se puede acceder a este endpoint con los role "Admin" o "User"

        /// <summary>
        /// Obtiene una lista paginada de roles.
        /// </summary>
        /// <param name="pageNumber">Numero de la pagina a consultar (valor predeterminado: 1).</param>
        /// <param name="pageSize">Numero de roles por pagina (valor predeterminado: 10).</param>
        /// <returns>Una lista paginada de roles, junto con el numero total de roles.</returns>
        [Authorize(Roles = "Admin,User")]
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Role>>> GetRoles([FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 10)
        {
            // Obtiene el numero total de toles en base de datos
            var totalRoles = await _context.Roles.CountAsync();

            // Aplica paginacion a la consulta de roles, basado en 'pageNumber' y 'pageSize'
            var roles = await _context.Roles
                .Skip((pageNumber - 1) * pageSize) // Omite los roles de paginas anteriores
                .Take(pageSize) // Toma solo el numero de roles especificados por 'pageSize'
                .ToListAsync(); // Ejecuta la consulta y devuelve una lusta de roles

            // Retorna los resultados paginados, incluyendo el numero total de roles, la pagina actual y el tamaño de la pagina
            return Ok(new
            {
                TotalRoles = totalRoles, // Total de roles disponibles en la base de datos
                PageNumber = pageNumber, // Numero de pagina actual
                PageSize = pageSize, // Tamaño de la pagina
                Roles = roles // Lista de roles en la pagina actual
            });
        }

        // Metodo GET para obtener un rol especifico por su ID
        // Se puede accefer a este metodo endpoint con los roles "Admin" o "User"

        /// <summary>
        /// Obtiene un rol especifico por su ID.
        /// </summary>
        /// <param name="id">Id del rol a obtener.</param>
        /// <returns>El rol solucitado.</returns>
        [Authorize(Roles = "Admin,User")]
        [HttpGet("{id}")]
        public async Task<ActionResult<Role>> GetRole(int id)
        {
            // Busca el rol por su ID en la base de datos
            var role = await _context.Roles.FindAsync(id);

            // Si no se encuentra el rol, retorna un codigo 404 (Not Found)
            if (role == null)
            {
                return NotFound();
            }

            // Retorna el rol encontrado
            return role;
        }

        // Metodo POST para crear un nuevo rol
        // Solo se permite el acceso a este endpoint con el rol "Admin"

        /// <summary>
        /// Crea un nuevo rol
        /// </summary>
        /// <param name="createRoleDto">Objeto DTO con la informacion del rol a crear</param>
        /// <returns>El rol creado con ID asignado</returns>
        [Authorize(Roles = "Admin")]
        [HttpPost]
        public async Task<ActionResult<RoleDto>> CreateRole(RoleDto createRoleDto)
        {
            // Crea una nueva entidad 'Role' a partir del DTO proporcionado
            var role = new Role
            {
                roleName = createRoleDto.roleName // Establece el nombre del rol
            };

            // Agrega el nuevo rol al contexto de la base de datos
            _context.Roles.Add(role);
            await _context.SaveChangesAsync(); // Guarda los cambios en la base de datos de forma asincrona

            // Crea un objeto 'RoleDto' para devolver en la respuesta
            var roleDto = new RoleDto
            {
                roleId = role.roleId, // ID del rol reciente creado
                roleName = role.roleName // Nombre del rol recien creado
            };

            // Retorna un codigo 201 (Created) con el rol recien creado y la ruta para obtener el rol por su ID
            return CreatedAtAction(nameof(GetRole), new { id = role.roleId }, roleDto);
        }

        // Metodo PATCH para actualizar parcialmente un rol existente
        // Solo se permite el acceso a este endpoint con el rol "Admin"

        /// <summary>
        /// Actualiza parcialmente un rol existente por su ID.
        /// </summary>
        /// <param name="id">ID del rol a actualizar.</param>
        /// <param name="updateRoleDto">Objeto DTO con la informacion actualizada del rol.</param>
        /// <returns>Una respuesta vacía con código 204 si la actualización es exitosa.</returns>
        [Authorize(Roles = "Admin")]
        [HttpPatch("{id}")]
        public async Task<IActionResult> PatchRole(int id, [FromBody] RoleDtov2 roleDTO)
        {
            // Verifica si el ID es inválido o si el DTO es nulo
            if (id <= 0 || roleDTO == null)
            {
                return BadRequest(new { message = "Por favor, ingrese todos los campos correctamente." });
            }

            // Busca el rol en la base de datos
            var existingRole = await _context.Roles.FindAsync(id);
            if (existingRole == null)
            {
                return NotFound(new { message = "El ID no es válido." });
            }

            // Verifica el ID del usuario
            var userIdClaim = User.FindFirst("userId")?.Value;
            if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out int userId))
            {
                return BadRequest(new { message = "ID de usuario no válido." });
            }

            // Validación de la longitud de 'roleName' si es proporcionado
            if (roleDTO.roleName != null && roleDTO.roleName.Length > 50)
            {
                return BadRequest(new { Code = "InvalidInput", Message = "El nombre del rol no puede tener más de 50 caracteres." });
            }

            // Solo actualiza los campos que han sido proporcionados
            if (!string.IsNullOrWhiteSpace(roleDTO.roleName))
            {
                // Verifica si ya existe un rol con el mismo nombre
                bool roleExists = await _context.Roles.AnyAsync(r => r.roleName == roleDTO.roleName && r.roleId != id);
                if (roleExists)
                {
                    return BadRequest(new { message = "Ya existe un rol con este nombre." });
                }
                existingRole.roleName = roleDTO.roleName;
            }

            if (roleDTO.roleStatus.HasValue)
            {
                existingRole.roleStatus = roleDTO.roleStatus.Value;
            }

            // Asigna el usuario que hizo la modificación
            existingRole.modifiedRoleById = userId;
            existingRole.modifiedRoleDate = DateTime.Now;

            // Marca el rol como modificado en el contexto
            _context.Entry(existingRole).State = EntityState.Modified;

            try
            {
                // Guarda los cambios en la base de datos
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!RoleExists(id))
                {
                    return NotFound(new { message = "El rol no existe." });
                }
                throw;
            }

            return Ok(new { message = "Rol actualizado correctamente" });
        }


        private bool RoleExists(int id)
        {
            return _context.Roles.Any(e => e.roleId == id);
        }
    }
}
