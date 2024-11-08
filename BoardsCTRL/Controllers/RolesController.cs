using BoardsProject.Data;
using BoardsProject.DTO;
using BoardsProject.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace BoardsProject.Controllers
{
    [ApiVersion("1.0")]
    [ApiController]
    [Route("api/[controller]")]
    public class RolesController : ControllerBase
    {
        private readonly BoardsContext _context; // Contexto de la base de datos

        // Constructor que intecta el contexto de la base de datos
        public RolesController(BoardsContext context)
        {
            _context = context;
        }

        // Metodo GET para obtener una lista paginada de roles
        // Se puede acceder a este endpoint con los role "Admin" o "User"
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

        // PUT: api/Roles/{id}
        [Authorize(Roles = "Admin")]
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateRole(int id, RoleDto updateRoleDto)
        {
            var username = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            var role = await _context.Roles.FindAsync(id);
            if (role == null)
            {
                return NotFound();
            }

            // Actualizar las propiedades del rol
            role.roleName = updateRoleDto.roleName;

            _context.Entry(role).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!RoleExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return NoContent(); // Retornar 204 No Content si la actualización es exitosa
        }

        private bool RoleExists(int id)
        {
            return _context.Roles.Any(e => e.roleId == id);
        }


        [Authorize(Roles = "Admin")]
        [HttpDelete("delete/{id}")]
        public async Task<IActionResult> DeleteRole(int id)
        {
            var role = await _context.Roles.FindAsync(id);
            if (role == null)
            {
                return NotFound();
            }

            _context.Roles.Remove(role);
            _context.SaveChanges();
            return NoContent();
        }
    }
}
