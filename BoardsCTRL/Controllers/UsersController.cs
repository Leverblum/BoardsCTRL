using BoardsProject.Data;
using BoardsProject.DTO;
using BoardsProject.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

[ApiController]
[Route("api/[controller]")]
public class UsersController : ControllerBase
{
    // Inyeccion de dependencia del contexto de la base de datos
    private readonly BoardsContext _context;

    // Constructor que inicializa el contexto de la base de datos
    public UsersController(BoardsContext context)
    {
        _context = context;
    }

    // GET: Obtener todos los usuarios con paginacion
    // Endpoint accesible por usuarios con rol Admin o User
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
    // Endpoint accesible por usuarios con rol Admin o User
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
    [Authorize(Roles = "Admin")]
    [HttpPost]
    public async Task<ActionResult<UserDto>> CreateUser(UserDto userDto)
    {
        // Obtiene el nombre de usuario actual del token JWT
        var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);

        // Verifica si el nombre de usuario ya existe
        var existingUser = await _context.Users.FirstOrDefaultAsync(u => u.username == userDto.username);
        if (existingUser != null)
        {
            return BadRequest("El nombre de usuario ya existe.");
        }

        // Hashear la contraseña antes de guardarla
        var hashedPassword = BCrypt.Net.BCrypt.HashPassword(userDto.passwordHash);

        // Crea un nuevo usuario con los datos del DTO
        var user = new User
        {
            username = userDto.username,
            passwordHash = hashedPassword, // Almacena el hash de la contraseña
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

    // PUT: Actualizar Usuario
    // Endpoint accesible solo por usuarios con rol Admin
    [Authorize(Roles = "Admin")]
    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateUser(int id, UserDto userDto)
    {
        // Obtiene el nombre de usuario actual del token JWT
        var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);

        // Buscar el usuario por ID en la base de datos
        var user = await _context.Users.FindAsync(id);
        if (user == null)
        {
            return NotFound();
        }
        
        // Actualiza los campos edl usuario con los datos del DTO
        user.username = userDto.username;
        user.roleId = userDto.roleId;
        user.userStatus = userDto.userStatus;
        user.modifiedUserById = userId;
        user.modifiedUserDate = DateTime.Now;

        // MArca el usuario como modificado en el contexto
        _context.Entry(user).State = EntityState.Modified;
        
        // Guarda los cambios en la base de datos
        await _context.SaveChangesAsync();

        // Retorna el estado 204 NoContent (Sin contenido)
        return NoContent();
    }

    // Delete Cambiar el estado del usuaraio (activar/desactivar)
    // Endpoint accesible solo por usuarios con rol Admin
    [Authorize(Roles = "Admin")]
    [HttpDelete("{id}")]
    public async Task<IActionResult> ToggleUserStatus(int id, [FromQuery] bool? activate = null)
    {
        // Busca el usuario por ID en la base de datos
        var user = await _context.Users.FindAsync(id);

        // Si no se encuentra el usuario, retorna NotFound
        if (user == null)
        {
            return NotFound();
        }

        // Cambiar el estado segun el valor del parametro activate
        if (activate.HasValue)
        {
            user.userStatus = activate.Value; // True o false segun el parametro
        }
        else
        {
            user.userStatus = !user.userStatus; // Alterna el estado si no se proporciona activate
        }

        var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);

        // Asigna el usuario que realiza la modificacion y la fecha actual
        user.modifiedUserById = userId;
        user.modifiedUserDate = DateTime.Now;

        // Marca el usuario como modificado en el contenido
        _context.Entry(user).State = EntityState.Modified;
        
        // Guarda los cambios en la base de datos
        await _context.SaveChangesAsync();
        
        // Retorna el estado 204 NoContent (sin contenido8)
        return NoContent();
    }

    [Authorize(Roles = "Admin")]
    [HttpDelete("delete/{id}")]
    public async Task<IActionResult> DeleteUser(int id)
    {
        var user = await _context.Users.FindAsync(id);
        if (user == null)
        {
            return NotFound();
        }

        _context.Users.Remove(user);
        _context.SaveChanges();
        return NoContent();
    }
}
