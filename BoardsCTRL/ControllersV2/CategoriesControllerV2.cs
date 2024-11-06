using BoardsProject.Data;
using BoardsProject.DTO;
using BoardsProject.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Swashbuckle.AspNetCore.Annotations;

namespace BoardsCTRL.ControllersV2
{
    [ApiVersion("2.0")]
    [ApiController]
    [Route("api/[controller]")]
    public class CategoriesControllerV2 : ControllerBase
    {
        // Contexto de la base de datos para interactuar con las categorias
        private readonly BoardsContext _context;

        // Constructor que inyecta el contexto de base de datos
        public CategoriesControllerV2(BoardsContext context)
        {
            _context = context;
        }

        // Metodo GET para obtener una lista de categorias con paginacion opcional
        // Se puede acceder a este endpoint con los roles "Admin" o "User"

        /// <summary>
        /// Obtiene una lista de categorias con opcion de paginacion
        /// </summary>
        /// <param name="isPaginated">Numero de la pagina a recuperar (Opcional).</param>
        /// <param name="pageNumber">Numero de la paginacionpara la paginacion</param>
        /// <param name="pageSize">Tamaño de la pagina para la paginacion</param>
        /// <returns>Lista de categorias y detalles de paginacion</returns>
        [Authorize(Roles = "Admin,User")]
        [HttpGet]
        public async Task<ActionResult<IEnumerable<CategoryDto>>> GetCategories([FromQuery] bool isPaginated = true, [FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 5)
        {
            // Obtiene el numero total de categorias disponibles en la base de datos
            var totalCategories = await _context.Categories.CountAsync();

            // Lista para almacenar las categorías obtenidas
            List<Category> categories;

            // Si 'isPaginated' es true, se aplica la paginación, de lo contrario, se devuelven todas las categorías
            if (isPaginated)
            {
                // Aplica paginacion
                categories = await _context.Categories
                    .Skip((pageNumber - 1) * pageSize) // Omite los primeros (pageNumber - 1) * pageSize elementos
                    .Take(pageSize) // Toma el numero de elementos correspondientes a pageSize
                    .ToListAsync(); // Ejecuta la consulta de forma asincrona y devuelve los resultados como una lista
            }
            else
            {
                // Si no se desea paginación, devuelve todas las categorías
                categories = await _context.Categories.ToListAsync();
            }

            // Mapeo de entidades Category a DTO
            var categoryDtos = categories.Select(c => new CategoryDto
            {
                categoryId = c.categoryId, // ID de la categoria
                categoryTitle = c.categoryTitle, // Titulo de la categoria
                categoryStatus = c.categoryStatus, // Estado de la categoria (habilitado/deshabilitado)
                createdCategoryById = c.createdCategoryById, // Usuario que creo la categoria
                modifiedCategoryById = c.modifiedCategoryById, // Usuario que modifico la categoria
                createdCategoryDate = c.createdCategoryDate, // Fecha de creacion
                modifiedCategoryDate = c.modifiedCategoryDate // Fecha de la ultima modificacion
            }).ToList();

            // Devuelve el resultado con o sin paginación según el valor de 'isPaginated'
            return Ok(new
            {
                TotalCategories = totalCategories, // Total de categorías
                Categories = categoryDtos, // Lista de categorías en formato DTO
                IsPaginated = isPaginated, // Indica si la paginación fue aplicada
                PageNumber = isPaginated ? pageNumber : 0, // Si no se aplicó paginación, devuelve 0
                PageSize = isPaginated ? pageSize : 0 // Si no se aplicó paginación, devuelve 0
            });
        }

        // Metodo GET para obtener una categoria especifica por su ID
        // Se puede acceder a este endpoint con los roles "Admin" o "User"

        /// <summary>
        /// Crea una nueva categoría.
        /// </summary>
        /// <param name="categoryDto">Datos de la nueva categoría.</param>
        /// <returns>Resultado de la operación.</returns>
        [Authorize(Roles = "Admin,User")]
        [HttpGet("{id}")]
        public async Task<ActionResult<CategoryDto>> GetCategoryById(int id)
        {
            // Busca la categoria por su ID en la base de datos
            var category = await _context.Categories.FindAsync(id);

            // Si no se encuentra la categoria, retorna un codigo 404 (Not Found)
            if (category == null)
            {
                return NotFound();
            }

            // Crea un objeto 'CategoryDto' con la informacion de la categoria encontrada
            var categoryDto = new CategoryDto
            {
                categoryId = category.categoryId, // ID de la categoria
                categoryTitle = category.categoryTitle, // Descripcion de la categoria
                categoryStatus = category.categoryStatus, // Estado de la categoria
                createdCategoryById = category.createdCategoryById, // Usuario que creo la categoria
                createdCategoryDate = category.createdCategoryDate, // Usuario que modifico la categoria por ultima vez
                modifiedCategoryById = category.modifiedCategoryById, // Usuario que modifico la categoria por ultima vez
                modifiedCategoryDate = category.modifiedCategoryDate // Fecha de la ultima modificacion
            };

            // Retorna la categoria en formato DTO
            return Ok(categoryDto);
        }

        /// <summary>
        /// Crea una nueva categoría.
        /// </summary>
        /// <param name="categoryDto">Datos de la nueva categoría.</param>
        /// <returns>Resultado de la operación.</returns>
        [Authorize(Roles = "Admin")]
        [HttpPost]
        public async Task<ActionResult<CategoryDto>> CreateCategory(CategoryDto categoryDto)
        {
            var userIdClaim = User.FindFirst("userId")?.Value;
            if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out int userId))
            {
                return BadRequest(new { Code = "InvalidInput", Message = "ID de usuario no encontrado." });
            }

            // Crea una nueva entidad 'Category' a partir del DTO proporcionado
            var category = new Category
            {
                categoryTitle = categoryDto.categoryTitle, // Establece la descripcion
                categoryStatus = categoryDto.categoryStatus, // Establece el estado de la categoria
                createdCategoryById = userId, // Registra el usuario que crea la categoria
                createdCategoryDate = DateTime.Now // Establece la fecha y hora como fecha de creacion
            };

            // Agrega la nueva categoria al contexto de la base de datos
            _context.Categories.Add(category);
            await _context.SaveChangesAsync(); // Guarda los cambios en la base de datos de forma asincrona

            // Retorna un codigo 201 (Created) con la categoria creada
            return CreatedAtAction(nameof(GetCategoryById), new { id = category.categoryId }, categoryDto);
        }

        // Metodo PUT para actualizar una categoria existente
        // Solo se permite el acceso a este endpoint con el rol "Admin"

        /// <summary>
        /// Activa o desactiva el estado de una categoría.
        /// </summary>
        /// <param name="id">ID de la categoría a actualizar.</param>
        /// <param name="activate">Valor opcional para definir el estado (true o false).</param>
        /// <returns>Confirmación de actualización de estado exitosa.</returns>
        [Authorize(Roles = "Admin")]
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateCategory(int id, CategoryDto categoryDto)
        {
            // Verifica si el ID proporcionado coincide con el ID del DTO de la categoria, si no coincide, retorna un codigo 400 (Bad Request)
            if (id != categoryDto.categoryId)
            {
                return BadRequest();
            }

            // Si no se encuentra la categoria, retorna un codigo 404 (Not Found)
            var category = await _context.Categories.FindAsync(id);

            if (category == null)
            {
                return NotFound();
            }

            var userIdClaim = User.FindFirst("userId")?.Value;
            if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out int userId))
            {
                return BadRequest(new { Code = "InvalidInput", Message = "ID de usuario no encontrado." });
            }

            // Actualiza las propiedades de la categoria con los valores proporcionados en el DTO
            category.categoryTitle = categoryDto.categoryTitle; // Actualiza la descripcion
            category.categoryStatus = categoryDto.categoryStatus; // Actualiza el estado de la categoria
            category.modifiedCategoryById = userId; // Registra el usuario que realizo la modificacion
            category.modifiedCategoryDate = DateTime.Now; // Establece la fecha actual como fecha de modificacion

            // Marca la entidad como modificada en el contexto de la base de datos
            _context.Entry(category).State = EntityState.Modified;

            // Intenta guardar los camvios en la base de datos
            try
            {
                await _context.SaveChangesAsync();
            }
            // Captura excepciones de concurrencia, por ejemplo, cuando dos usuarios intentan la misma entidad al tiempo
            catch (DbUpdateConcurrencyException)
            {
                if (!CategoryExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw; // Lanza la excepcion si ocurrio otro tipo de error
                }
            }

            // Retorna un codigo 204 (No content) si la actualizacion fue exitosa
            return NoContent();
        }

        // Metodo DELETE (en este caso se usa para alternar el estado) de una categoria
        // Se puede habilitar/deshabilitar una categoria sin eliminarla fisicamente
        // Solo se permite el acceso a este endpoint con el rol "Admin"

        /// <summary>
        /// Elimina una categoría.
        /// </summary>
        /// <param name="id">ID de la categoría a eliminar.</param>
        /// <returns>Confirmación de eliminación exitosa.</returns>
        [Authorize(Roles = "Admin")]
        [HttpDelete("{id}")]
        public async Task<IActionResult> ToggleCategoryStatus(int id, [FromQuery] bool? activate = null)
        {
            // Busca la categoria en la base de datos
            var category = await _context.Categories.FindAsync(id);

            // Si no se encuentra la categoria, retorna un codigo 404 (Not Found)
            if (category == null)
            {
                return NotFound();
            }

            // SI se proporciona el parametro 'activate', se establece el estado de la categoria segun su valor (true/false)
            if (activate.HasValue)
            {
                category.categoryStatus = activate.Value; // True o false segun el parametro
            }
            // Si no se proporciona, simplemente alterna el estado actual (true a false, o viceversa)
            else
            {
                category.categoryStatus = !category.categoryStatus;
            }

            var userIdClaim = User.FindFirst("userId")?.Value;
            if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out int userId))
            {
                return BadRequest(new { Code = "InvalidInput", Message = "ID de usuario no encontrado." });
            }

            // Actualiza las propiedades de modificacion
            category.modifiedCategoryById = userId; // Registra el usuario que relizo la modificacion
            category.modifiedCategoryDate = DateTime.Now; // Establece la fecha actual como fecha de modificacion

            // Marca la entidad como modificada y guarda los cambios
            _context.Entry(category).State = EntityState.Modified;
            await _context.SaveChangesAsync();

            // Retorna un codigo 204 (No content) si la operacion fue exitosa
            return NoContent();
        }

        // Metodo auxiliar para verificar si una categoria existe en la base de datos
        private bool CategoryExists(int id)
        {
            return _context.Categories.Any(e => e.categoryId == id);
        }
    }
}
