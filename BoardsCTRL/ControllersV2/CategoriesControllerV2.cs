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
    [Route("api/v{version:apiVersion}/[controller]")]
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

        // Método PATCH para actualizar parcialmente una categoría
        [HttpPatch("{id}")]
        [Authorize(Roles = "Admin")]
        [SwaggerOperation(
            Summary = "Actualizar parcialmente una categoría",
            Description = "Actualiza campos específicos de una categoría sin necesidad de enviar todo el objeto.")]
        [SwaggerResponse(204, "Categoría actualizada exitosamente")]
        [SwaggerResponse(404, "Categoría no encontrada")]
        public async Task<IActionResult> ActualizarCategoryPatch(int id, [FromBody] CategoryDto categoryDTO)
        {
            // Verifica si el ID es inválido o si el DTO es nulo
            if (id <= 0 || categoryDTO == null)
            {
                return BadRequest(new { message = "Por favor, ingrese todos los campos correctamente." });
            }

            // Busca la categoría en la base de datos
            var existingCategory = await _context.Categories.FindAsync(id);
            if (existingCategory == null)
            {
                return NotFound(new { message = "El ID no es válido." });
            }

            // Verifica el ID del usuario
            var userIdClaim = User.FindFirst("userId")?.Value;
            if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out int userId))
            {
                return BadRequest(new { message = "ID de usuario no válido." });
            }

            // Valida la longitud del título de la categoría
            if (!string.IsNullOrWhiteSpace(categoryDTO.categoryTitle) && categoryDTO.categoryTitle.Length > 100)
            {
                return BadRequest(new { Code = "InvalidInput", Message = "El título de la categoría no puede tener más de 100 caracteres." });
            }

            // Solo actualiza los campos que han sido proporcionados
            if (!string.IsNullOrWhiteSpace(categoryDTO.categoryTitle))
            {
                // Verifica si ya existe una categoría con el mismo título
                bool categoryExists = await _context.Categories.AnyAsync(c => c.categoryTitle == categoryDTO.categoryTitle
                                                                              && c.categoryId != id);
                if (categoryExists)
                {
                    return BadRequest(new { message = "Ya existe una categoría con este nombre." });
                }
                existingCategory.categoryTitle = categoryDTO.categoryTitle;
            }

            if (categoryDTO.categoryStatus != null)
            {
                existingCategory.categoryStatus = categoryDTO.categoryStatus; // Aquí ya está bien si es un bool.
            }

            // Asigna el usuario que hizo la modificación
            existingCategory.modifiedCategoryById = userId;
            existingCategory.modifiedCategoryDate = DateTime.Now;

            // Marca la categoría como modificada en el contexto
            _context.Entry(existingCategory).State = EntityState.Modified;

            try
            {
                // Guarda los cambios en la base de datos
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                // Maneja la excepción si hay problemas de concurrencia
                if (!CategoryExists(id))
                {
                    return NotFound(new { message = "La categoría no existe." });
                }

                throw; // Re-lanza la excepción si no se ha manejado
            }

            // Responde con el mensaje de éxito
            return Ok(new { message = "Categoría actualizada correctamente" });
        }


        // Metodo auxiliar para verificar si una categoria existe en la base de datos
        private bool CategoryExists(int id)
        {
            return _context.Categories.Any(e => e.categoryId == id);
        }
    }
}
