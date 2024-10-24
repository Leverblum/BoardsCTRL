using BoardsProject.Data;
using BoardsProject.DTO;
using BoardsProject.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Data;
using System.Security.Claims;

[ApiController]
[Route("api/[controller]")]
public class SlidesController : ControllerBase
{
    private readonly BoardsContext _context; // Contexto de la base de datos

    // Constructor que inyecta en el contexto de la base de datos
    public SlidesController(BoardsContext context)
    {
        _context = context;
    }

    // Metodo GET para obtener una lista paginada de diaposivas (slides)
    // Se permite el acceso a usuarios con roles "Admin" y "Users"
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

    [HttpGet("List-Slide-by-board")]
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
            slideId = s.slideId,
            slideTitle = s.slideTitle,
            URL = s.URL,
            time = s.time,
            slideStatus = s.slideStatus,
            Board = new
            {
                boardId = s.Board.boardId,
                boardTitle = s.Board.boardTitle,
                boardDescription = s.Board.boardDescription,
                boardStatus = s.Board.boardStatus,
                Category = new
                {
                    categoryId = s.Board.Category.categoryId,  // Acceso a categoryId a través de Board
                    categoryTitle = s.Board.Category.categoryTitle,
                    createdCategoryById = s.Board.Category.createdCategoryById,
                    modifiedCategoryById = s.Board.Category.modifiedCategoryById,
                    createdCategoryDate = s.Board.Category.createdCategoryDate,
                    modifiedCategoryDate = s.Board.Category.modifiedCategoryDate,
                    categoryStatus = s.Board.Category.categoryStatus
                }
            }
        }).ToList();
        return Ok(SlideDto);
    }

    // Metodo POST para crear una nueva diapositiva
    // Solo los usuarios con rol "Admin" pueden acceder a este endpoint
    [Authorize(Roles = "Admin")]
    [HttpPost]
    public async Task<ActionResult<SlideDto>> CreateSlide(SlideDto slideDto)
    {
        // Obtener el nombre de usuario del creador (quien hace la solicitud)
        var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);

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

    // Metodo PUT para actualizar una diapositiva existente
    // Solo los usuarios con rol "Admin" pueden acceder a este endpoint
    [Authorize(Roles = "Admin")]
    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateSlide(int id, SlideDto slideDto)
    {
        // Obtener el nombre de usuario de quien hace la solucitud
        var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);

        // Verifica si el ID en la URL coincide con el ID en el DTO
        if (id != slideDto.slideId)
        {
            return BadRequest();
        }

        // Busca la diapositiva por si ID en la base de datos
        var slide = await _context.Slides.FindAsync(id);

        // Si no se encuentra la diapositiva, retorna un codigo 404 (Not Found)
        if (slide == null)
        {
            return NotFound();
        }

        // Actualiza los campos de la diapositiva con los valores proporcionados en el DTO
        slide.slideTitle = slideDto.slideTitle;
        slide.URL = slideDto.URL;
        slide.time = slideDto.time;
        slide.boardId = slideDto.boardId;
        slide.slideStatus = slideDto.slideStatus;
        slide.modifiedSlideById = userId;
        slide.modifiedSlideDate = DateTime.Now;

        // Marca la diapositiva como modificada en el contexto de la base de datos
        _context.Entry(slide).State = EntityState.Modified;

        // Intenta guardar los cambios en la base de datos
        try
        {
            await _context.SaveChangesAsync();
        }
        catch (DbUpdateConcurrencyException)
        {
            // Si la diapositiva no exite, retorna un codigo 404 (Not Found)
            if (!SlideExists(id))
            {
                return NotFound();
            }
            else
            {
                throw; // Si ocurre otro error, lo lanza
            }
        }

        // Retorna un codigo 204 (No Content) si la actualizacion fue exitosa
        return NoContent();
    }

    // Metodo DELETE (toggle status) para alternar el estado de una diapositiva (activar/desactivar)
    // Solo los usuarios con rol "Admin" puede acceder a este endpoint
    [Authorize(Roles = "Admin")]
    [HttpDelete("{id}")]
    public async Task<IActionResult> ToggleSlideStatus(int id, [FromQuery] bool? activate = null)
    {
        // Busca la diapositiva por su ID en la base de datos
        var slide = await _context.Slides.FindAsync(id);

        // Si no se encuentra la diapositiva, retorna un codigo 404 (Not Found)
        if (slide == null)
        {
            return NotFound();
        }

        // Cambia el estado de la diapositiva segun el valor del parametro 'activate'
        if (activate.HasValue)
        {
            slide.slideStatus = activate.Value; // Activa o desactiva segun el valor proporcionado
        }
        else
        {
            slide.slideStatus = !slide.slideStatus; // Alterna el estado si no se proporciona un valor especifico
        }

        var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);

        // Actualiza los campos de modificacion
        slide.modifiedSlideById = userId;
        slide.modifiedSlideDate = DateTime.Now;

        // Marca la diapositiva como modificada en el contexto de la base de datos
        _context.Entry(slide).State = EntityState.Modified;
        await _context.SaveChangesAsync();

        // Retorna un codigo 204 (No Content) si la operacion fue exitosa
        return NoContent();
    }

    [Authorize(Roles = "Admin")]
    [HttpDelete("delete/{id}")]
    public async Task<IActionResult> DeleteSlide(int id)
    {
        var slide = await _context.Slides.FindAsync(id);
        if (slide == null)
        {
            return NotFound();
        }

        _context.Slides.Remove(slide);
        _context.SaveChanges();
        return NoContent();
    }

    // Metodo privado para verificar si una diapositiva existe por su ID
    private bool SlideExists(int id)
    {
        return _context.Slides.Any(e => e.slideId == id);
    }
}
