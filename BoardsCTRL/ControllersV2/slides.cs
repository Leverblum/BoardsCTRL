﻿using BoardsCTRL.DTOv2;
using BoardsProject.Data;
using BoardsProject.DTO;
using BoardsProject.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BoardsCTRL.ControllersV2
{
    [ApiVersion("2.0")]
    [ApiController]
    [Route("api/v{version:apiVersion}/[controller]")]
    public class slides : ControllerBase
    {
        private readonly BoardsContext _context; // Contexto de la base de datos

        // Constructor que inyecta en el contexto de la base de datos
        public slides(BoardsContext context)
        {
            _context = context;
        }

        // Metodo GET para obtener una lista paginada de diaposivas (slides)
        // Se permite el acceso a usuarios con roles "Admin" y "Users"

        /// <summary>
        /// Obtiene una lista paginada de Slides
        /// </summary>
        /// <param name="pageNumber">Numero de pagina a recuperar (por defecto 1).</param>
        /// <param name="pageSize">Cantidad de diapositivas por pagina (por defecto 10).</param>
        /// <returns>Lista paginada de diapositivas.</returns>
        [Authorize(Roles = "Admin,User")]
        [HttpGet]
        public async Task<ActionResult<IEnumerable<SlideDto>>> GetSlides([FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 10)
        {
            // Calcular el número total de diapositivas en la base de datos
            var totalSlides = await _context.Slides.CountAsync();

            // Obtener las diapositivas con paginación, segun el numero de pagina y el tamaño de pagina
            var slides = await _context.Slides
                .Skip((pageNumber - 1) * pageSize) // Omite las diapositivas de las paginas anteriores
                .Take(pageSize) // Toma solo el numero de diapositivas indicando por 'pageSize'
                .ToListAsync(); // Ejecuta la consulta y obtiene la lista de diapositivas

            // Convertir las diapositivas a objetos DTO para devolver en la respuesta
            var slideDtos = slides.Select(s => new SlideDto
            {
                slideId = s.slideId,
                slideTitle = s.slideTitle,
                URL = s.URL,
                time = s.time,
                boardId = s.boardId,
                slideStatus = s.slideStatus,
                createdSlideById = s.createdSlideById,
                createdSlideDate = s.createdSlideDate,
                modifiedSlideById = s.modifiedSlideById,
                modifiedSlideDate = s.modifiedSlideDate
            }).ToList();

            // Retorna la lista de diapositivas paginada junto con la informacion de la paginacion
            return Ok(new
            {
                TotalSlides = totalSlides, // Numero total de diapositivas
                PageNumber = pageNumber, // Numero de la pagina actual
                PageSize = pageSize, // Tamaño de la pagina
                Slides = slideDtos // Lista de diapositivas en formato DTO
            });
        }

        // Metodo GET para obtener una diapositiva especifica por su ID
        // Se permite el acceso a usuarios con roles "Admin" y "Users"

        /// <summary>
        /// Obtiene una Slide especifica por su ID.
        /// </summary>
        /// <param name="id">ID de la diapositiva a recuperar</param>
        /// <returns>La Slide solicitada</returns>
        [Authorize(Roles = "Admin,User")]
        [HttpGet("{id}")]
        public async Task<ActionResult<SlideDto>> GetSlideById(int id)
        {
            // Busca la diapositiva por su ID en la base de datos
            var slide = await _context.Slides.FindAsync(id);

            // Si no se encuentra la diapositiva, retorna un codigo 404 (Not Found)
            if (slide == null)
            {
                return NotFound();
            }

            // Crea un objeto DTO con los datos de la diapositiva encontrada
            var slideDto = new SlideDto
            {
                slideId = slide.slideId,
                slideTitle = slide.slideTitle,
                URL = slide.URL,
                time = slide.time,
                boardId = slide.boardId,
                slideStatus = slide.slideStatus,
                createdSlideById = slide.createdSlideById,
                createdSlideDate = slide.createdSlideDate,
                modifiedSlideById = slide.modifiedSlideById,
                modifiedSlideDate = slide.modifiedSlideDate
            };

            // Retorna el objeto DTO de la diapositiva
            return Ok(slideDto);
        }


        /// <summary>
        /// Obtiene una lista de Slides para un tablero especifico.
        /// </summary>
        /// <param name="boardId">ID del tablero asociado.</param>
        /// <param name="page">Numero de pagina.</param>
        /// <param name="size">Tamaño de la pagina</param>
        /// <returns>Lista de Slides asociadas al tablero</returns>
        [HttpGet("list-slide-by-board")]
        [Authorize(Roles = "Admin, User")] // Tanto usuarios como administrador pueden visualizar a gusto
        public IActionResult GetSlidesByBoard(int boardId, int page = 1, int size = 20)
        {
            var slides = _context.Slides
                .Include(s => s.Board)
                .ThenInclude(b => b.Category)
                .Where(s => s.Board.boardId == boardId)
                .ToList();
            // Verificar si no se encontraron slides
            if (!slides.Any())
            {
                return NotFound($"No se encontraron slides para el tablero con ID {boardId}.");
            }
            var SlideDto = slides.Select(s => new
            {
                s.slideId,
                s.slideTitle,
                s.URL,
                s.time,
                s.slideStatus,
                Board = new
                {
                    s.Board.boardId,
                    s.Board.boardTitle,
                    s.Board.boardDescription,
                    s.Board.boardStatus,
                    Category = new
                    {
                        s.Board.Category.categoryId,  // Acceso a categoryId a través de Board
                        s.Board.Category.categoryTitle,
                        s.Board.Category.createdCategoryById,
                        s.Board.Category.modifiedCategoryById,
                        s.Board.Category.createdCategoryDate,
                        s.Board.Category.modifiedCategoryDate,
                        s.Board.Category.categoryStatus
                    }
                }
            }).ToList();
            return Ok(SlideDto);
        }

        // Metodo POST para crear una nueva diapositiva
        // Solo los usuarios con rol "Admin" pueden acceder a este endpoint

        /// <summary>
        /// Crea una nueva Slide
        /// </summary>
        /// <param name="slideDto">Objeto DTO de la diapositiva a crear.</param>
        /// <returns></returns>
        [Authorize(Roles = "Admin")]
        [HttpPost]
        public async Task<ActionResult<SlideDto>> CreateSlide(SlideDto slideDto)
        {
            var userIdClaim = User.FindFirst("userId")?.Value;
            if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out int userId))
            {
                return BadRequest(new { Code = "InvalidInput", Message = "ID de usuario no encontrado." });
            }

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            // Crear una nueva instancia de la entidad Slide con los datos proporcionados
            var slide = new Slide
            {
                slideTitle = slideDto.slideTitle,
                URL = slideDto.URL,
                time = slideDto.time,
                boardId = slideDto.boardId,
                slideStatus = slideDto.slideStatus,
                createdSlideById = userId, // El usuario que crea la diapositiva
                createdSlideDate = DateTime.Now // Fecha de creacion actual
            };

            // Agrega la diapositiva al contexto de la base de datos
            _context.Slides.Add(slide);

            // Guardar los cambios en la base de datos
            await _context.SaveChangesAsync();

            // Retorna un codigo 201 (Created) con la diapositiva creada y la ruta para obtenerla
            return CreatedAtAction(nameof(GetSlideById), new { id = slide.slideId }, slideDto);
        }

        // Metodo PATCH para actualizar parcialmente una diapositiva existente
        // Solo los usuarios con rol "Admin" pueden acceder a este endpoint

        /// <summary>
        /// Actualiza parcialmente una diapositiva existente.
        /// </summary>
        /// <param name="id">ID de la diapositiva a actualizar.</param>
        /// <param name="slideDtov2">Objeto DTO con los campos a actualizar.</param>
        /// <returns>Una respuesta vacia con codigo 204 si la actualizacion es exitosa.</returns>
        [Authorize(Roles = "Admin")]
        [HttpPatch]
        public async Task<IActionResult> PartialUpdateSlide(int id, [FromBody] SlideDtov2 slideDTO)
        {
            // Verifica si el ID es inválido o si el DTO es nulo
            if (id <= 0 || slideDTO == null)
            {
                return BadRequest(new { message = "Por favor, ingrese todos los campos correctamente." });
            }

            // Busca la Slide en la base de datos
            var existingSlide = await _context.Slides.FindAsync(id);
            if (existingSlide == null)
            {
                return NotFound(new { message = "El ID no es válido." });
            }

            // Verifica el ID del usuario
            var userIdClaim = User.FindFirst("userId")?.Value;
            if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out int userId))
            {
                return BadRequest(new { message = "ID del usuario no válido." });
            }

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            // Valida el tiempo solo si es propocionado y no negativo
            if (slideDTO.time.HasValue && slideDTO.time < 0)
            {
                return BadRequest(new { Code = "InvalidInput", Message = "El tiempo de la diapositiva no puede ser negativo." });
            }

            // Solo actualiza los campos que han sido proporcionados
            if (!string.IsNullOrWhiteSpace(slideDTO.slideTitle))
            {
                existingSlide.slideTitle = slideDTO.slideTitle;
            }

            if (!string.IsNullOrWhiteSpace(slideDTO.URL))
            {
                existingSlide.URL = slideDTO.URL;
            }

            if (slideDTO.time.HasValue)
            {
                existingSlide.time = slideDTO.time.Value;
            }

            if (slideDTO.slideStatus.HasValue)
            {
                existingSlide.slideStatus = slideDTO.slideStatus.Value;
            }

            // Asigna el usuario que hizo la modificación
            existingSlide.modifiedSlideById = userId;
            existingSlide.modifiedSlideDate = DateTime.Now;

            // Marca la Slide como modificada en el contexto
            _context.Entry(existingSlide).State = EntityState.Modified;

            try
            {
                // Guarda los cambios en la base de datos
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!SlideExists(id))
                {
                    return NotFound(new { message = "La Slide no existe" });
                }
                throw;
            }

            return Ok(new { message = "Slide actualizada correctamente" });
        }


        // Metodo privado para verificar si una diapositiva existe por su ID
        private bool SlideExists(int id)
        {
            return _context.Slides.Any(e => e.slideId == id);
        }
    }
}
