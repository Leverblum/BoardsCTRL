using BoardsProject.Data;
using BoardsProject.DTO;
using BoardsProject.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Swashbuckle.AspNetCore.Annotations;
using System.Collections.Generic;
using System.Security.Claims;

namespace BoardsCTRL.ControllersV2
{
    [ApiVersion("2.0")]
    [ApiController]
    [Route("api/v{version:apiVersion}/[controller]")] // Define la ruta base para esta versión (api/v2/Boards)
    public class BoardsControllerV2 : ControllerBase
    {
        private readonly BoardsContext _context;

        public BoardsControllerV2(BoardsContext context)
        {
            _context = context;
        }

        // Metodo GET para obyener una lista de todos los tableros con paginacion
        // Requiere autorizacion para roles "Admin" y "User"

        /// <summary>
        /// Obtiene una lista de tableros con paginación.
        /// </summary>
        /// <param name="pageNumber">Número de la página solicitada (predeterminado = 1).</param>
        /// <param name="pageSize">Cantidad de elementos por página (predeterminado = 10).</param>
        /// <returns>Lista de tableros con detalles básicos.</returns>
        [HttpGet]
        [Authorize(Roles = "Admin,User")]
        [SwaggerOperation(
            Summary = "Obtener lista de tableros",
            Description = "Obtiene una lista de todos los tableros en el sistema con paginación.")]
        [SwaggerResponse(200, "Lista de tableros obtenida exitosamente", typeof(IEnumerable<BoardDto>))]
        public async Task<ActionResult<IEnumerable<BoardDto>>> GetBoards([FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 10)
        {
            // Obtiene el total de tableros en la base de datos
            var totalBoards = await _context.Boards.CountAsync();

            // Obtiene la lista de tableros paginada, incluyendo las categorias relacionadas
            var boards = await _context.Boards
                .Skip((pageNumber - 1) * pageSize) // Salta los registros anteriores segun la pagina
                .Take(pageSize) // Limita la cantidad de registros a pageSize
                .Include(b => b.Category) // Incluye las categorias relacionadas con los tableros
                .ToListAsync();

            // Convierte los tableros a su DTO correspondiente
            var boardDtos = boards.Select(b => new BoardDto
            {
                boardId = b.boardId,
                boardTitle = b.boardTitle,
                boardDescription = b.boardDescription,
                categoryId = b.categoryId,
                boardStatus = b.boardStatus,
                createdBoardById = b.createdBoardById,
                createdBoardDate = b.createdBoardDate,
                modifiedBoardById = b.modifiedBoardById,
                modifiedBoardDate = b.modifiedBoardDate
            }).ToList();

            // Retorna un objeto con el total de tableros, el numero de pagina y los tableros en formato DTO
            return Ok(new
            {
                TotalBoards = totalBoards,
                PageNumber = pageNumber,
                PageSize = pageSize,
                Boards = boardDtos
            });
        }

        // Metodo GET para obtener un tablero por su ID
        // Requiere autorizacion para roles "Admin" y "Users"

        /// <summary>
        /// Obtiene un tablero específico por su ID.
        /// </summary>
        /// <param name="id">ID del tablero</param>
        /// <returns>Detalles del tablero solicitado</returns>
        [HttpGet("{id}")]
        [Authorize(Roles = "Admin,User")]
        [SwaggerOperation(
            Summary = "Obtener un tablero",
            Description = "Obtiene los detalles de un tablero específico utilizando su ID.")]
        [SwaggerResponse(200, "Detalles del tablero obtenidos exitosamente", typeof(BoardDto))]
        [SwaggerResponse(404, "Tablero no encontrado")]
        public async Task<ActionResult<BoardDto>> GetBoardById(int id)
        {
            // Busca el tablero por su ID e incluye su categoria
            var board = await _context.Boards.Include(b => b.Category).FirstOrDefaultAsync(b => b.boardId == id);

            // Si no se encuentra el tablero, retorna un error 404 (Not Found)
            if (board == null)
            {
                return NotFound();
            }

            // Convierte el tablero a su DTO correspondiente
            var boardDto = new BoardDto
            {
                boardId = board.boardId,
                boardTitle = board.boardTitle,
                boardDescription = board.boardDescription,
                categoryId = board.categoryId,
                boardStatus = board.boardStatus,
                createdBoardById = board.createdBoardById,
                createdBoardDate = board.createdBoardDate,
                modifiedBoardById = board.modifiedBoardById,
                modifiedBoardDate = board.modifiedBoardDate
            };

            // Retorna el tablero en formato DTO
            return Ok(boardDto);
        }

        // Visualiza todos los tableros conectados a una categoría
        // Tanto usuarios como administrador pueden visualizar tableros por categoría

        /// <summary>
        /// Obtiene una lista de tableros pertenecientes a una categoría específica, con paginación.
        /// </summary>
        /// <param name="categoryId">ID de la categoría para filtrar los tableros.</param>
        /// <param name="pageNumber">Número de la página solicitada (predeterminado = 1).</param>
        /// <param name="pageSize">Cantidad de tableros por página (predeterminado = 20).</param>
        /// <returns>Una lista de tableros asociados a la categoría especificada, junto con información de paginación.</returns>
        [HttpGet("list-boards-by-category")]
        [Authorize(Roles = "Admin, User")]
        [SwaggerOperation(
            Summary = "Obtener tableros por categoría",
            Description = "Devuelve una lista de tableros que pertenecen a una categoría específica, con soporte para paginación.")]
        [SwaggerResponse(200, "Lista de tableros obtenida exitosamente para la categoría solicitada", typeof(object))] // Reemplaza `object` con tu DTO o modelo adecuado
        [SwaggerResponse(404, "No se encontraron tableros para la categoría especificada")]
        public IActionResult GetBoardsByCategory(int categoryId, int pageNumber = 1, int pageSize = 20)
        {
            // Calcula el número de tableros a omitir
            var skip = (pageNumber - 1) * pageSize;

            var boards = _context.Boards
                .Include(b => b.Category) // Incluye la categoría relacionada al board
                .Where(b => b.Category.categoryId == categoryId) // Filtra los boards por el categoryId
                .Skip(skip) // Omitir los tableros según el número de página
                .Take(pageSize) // Tomar solo la cantidad de tableros definida por pageSize
                .ToList();

            if (!boards.Any()) // Si no hay tableros respecto a la categoria
            {
                return NotFound($"No se encontraron tableros para la categoría con ID {categoryId}.");
            }

            // Objeto que devolverá con relación a la categoría
            var boardDtos = boards.Select(b => new
            {
                boardId = b.boardId,
                boardTitle = b.boardTitle,
                boardDescription = b.boardDescription,
                boardStatus = b.boardStatus,
                createdBoardById = b.createdBoardById,
                modifiedBoardById = b.modifiedBoardById,
                createdBoardDate = b.createdBoardDate,
                modifiedBoardDate = b.modifiedBoardDate,
                Category = new
                {
                    categoryId = b.Category.categoryId,
                    categoryTitle = b.Category.categoryTitle,
                    createdCategoryById = b.Category.createdCategoryById,
                    modifiedCategoryById = b.Category.modifiedCategoryById,
                    createdCategoryDate = b.Category.createdCategoryDate,
                    modifiedCategoryDate = b.Category.modifiedCategoryDate,
                    categoryStatus = b.Category.categoryStatus
                }
            }).ToList();

            // Obtener el total de tableros para la categoría para calcular la cantidad de páginas
            var totalBoards = _context.Boards.Count(b => b.Category.categoryId == categoryId);
            var totalPages = (int)Math.Ceiling((double)totalBoards / pageSize);

            return Ok(new
            {
                Boards = boardDtos,
                TotalBoards = totalBoards,
                TotalPages = totalPages,
                CurrentPage = pageNumber
            }); // Respuesta 200 con los tableros de la categoría
        }

        // Metodo POST para crear un nuevo tablero
        // Solo los usuarios con rol "Admin" pueden usar este metodo

        /// <summary>
        /// Crea un nuevo tablero.
        /// </summary>
        /// <param name="createBoardDto">Información del tablero a crear</param>
        /// <returns>El tablero recién creado</returns>
        [HttpPost]
        [Authorize(Roles = "Admin")]
        [SwaggerOperation(
            Summary = "Crear un nuevo tablero",
            Description = "Crea un nuevo tablero en el sistema con la información proporcionada.")]
        [SwaggerResponse(201, "Tablero creado exitosamente", typeof(BoardDto))]
        [SwaggerResponse(400, "Error en los datos proporcionados")]
        public async Task<IActionResult> PostBoard(BoardDto createBoardDto)
        {
            var userIdClaim = User.FindFirst("userId")?.Value;
            if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out int userId))
            {
                return BadRequest(new { Code = "InvalidInput", Message = "ID de usuario no encontrado." });
            }

            // Crea un nuevo objeto Board basado en el DTO recibido
            var board = new Board
            {
                boardTitle = createBoardDto.boardTitle,
                boardDescription = createBoardDto.boardDescription,
                categoryId = createBoardDto.categoryId,
                boardStatus = createBoardDto.boardStatus,
                createdBoardById = userId, // Asigna el usuario creador
                createdBoardDate = DateTime.Now // Asigna la fecha de creacion
            };

            // Añade el nuevo tablero a la base de datos
            _context.Boards.Add(board);
            await _context.SaveChangesAsync();

            // Retorna el tablero recien creado con codigo 201 (Created)
            return CreatedAtAction("GetBoards", new { id = board.boardId }, board);
        }

        // Metodo PUT para actualizar un tablero existente
        // Solo los usuarios con rol "Admin" pueden usar este metodo

        /// <summary>
        /// Actualiza un tablero existente.
        /// </summary>
        /// <param name="id">ID del tablero a actualizar</param>
        /// <param name="updateBoardDto">Información del tablero actualizada</param>
        /// <returns>Respuesta 204 si la actualización es exitosa</returns>
        [HttpPut("{id}")]
        [Authorize(Roles = "Admin")]
        [SwaggerOperation(
            Summary = "Actualizar un tablero",
            Description = "Actualiza los detalles de un tablero existente utilizando su ID.")]
        [SwaggerResponse(204, "Tablero actualizado exitosamente")]
        [SwaggerResponse(404, "Tablero no encontrado")]
        public async Task<IActionResult> PutBoard(int id, BoardDto updateBoardDto)
        {
            var userIdClaim = User.FindFirst("userId")?.Value;
            if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out int userId))
            {
                return BadRequest(new { Code = "InvalidInput", Message = "ID de usuario no encontrado." });
            }

            // Busca el tablero por su ID
            var board = await _context.Boards.FindAsync(id);

            // Si no se encuentra el tablero, retorna un error 404 (Not Found)
            if (board == null) return NotFound();

            // Actualiza los valores del tablero con la informacion proporcionada en el DTO
            board.boardTitle = updateBoardDto.boardTitle;
            board.boardDescription = updateBoardDto.boardDescription;
            board.categoryId = updateBoardDto.categoryId;
            board.boardStatus = updateBoardDto.boardStatus;
            board.modifiedBoardById = userId;  // Asigna el usuario que realizo la modificacion
            board.modifiedBoardDate = DateTime.Now; // Asigna la fecha de modificacion

            try
            {
                // Guarda los cambios en la base de datos
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                // Verifica si el tablero aun existe antes de lanzar una excepcion
                if (!BoardExists(id)) return NotFound();
                else throw;
            }

            // Retorna un codigo 204 (No Content) para indicar que la operacion fue exitosa
            return NoContent();
        }

        // Metodo DELETE (o Toggle Status) para activar o desactivar un tablero
        // Solo los usuarios con rol "Admin" pueden usar este metodo

        /// <summary>
        /// Cambia el estado de un tablero (activar/desactivar).
        /// </summary>
        /// <param name="id">ID del tablero</param>
        /// <param name="activate">Opcional: valor booleano para activar o desactivar.</param>
        /// <returns>Respuesta 204 si el cambio es exitoso</returns>
        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        [SwaggerOperation(
            Summary = "Cambiar estado de un tablero",
            Description = "Activa o desactiva el estado de un tablero. Si no se especifica un valor, se alternará el estado actual.")]
        [SwaggerResponse(204, "Estado del tablero cambiado exitosamente")]
        [SwaggerResponse(404, "Tablero no encontrado")]
        public async Task<IActionResult> ToggleBoardsStatus(int id, [FromQuery] bool? activate = null)
        {
            // Busca el tablero por su ID
            var board = await _context.Boards.FindAsync(id);

            // Si no se encuentra el tablero, retorna un error 404 (Not Found)
            if (board == null)
            {
                return NotFound();
            }

            // Si se proporciona el parametro activate, establece el estado del tablero segun el valor de activate
            // Si no se proporciona, alterna el estado actual del tablero
            if (activate.HasValue)
            {
                board.boardStatus = activate.Value; // True o false segun el parametro
            }
            else
            {
                // Si no se proporciona, simplemente alterna el estado actual
                board.boardStatus = !board.boardStatus;
            }

            var userIdClaim = User.FindFirst("userId")?.Value;
            if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out int userId))
            {
                return BadRequest(new { Code = "InvalidInput", Message = "ID de usuario no encontrado." });
            }

            // Actualiza la informacion del usuario que realizo la modificacion
            board.modifiedBoardById = userId;
            board.modifiedBoardDate = DateTime.Now;

            // Marca el objeto como modificado en el contexto de datos
            _context.Entry(board).State = EntityState.Modified;
            await _context.SaveChangesAsync();

            // Retorna un codigo 204 (No Content) para indicar que la operacion fue exitosa
            return NoContent();
        }

        // Metodo privado para verificar si un tablero existe por su ID
        private bool BoardExists(int id)
        {
            return _context.Boards.Any(e => e.boardId == id);
        }
    }
}
