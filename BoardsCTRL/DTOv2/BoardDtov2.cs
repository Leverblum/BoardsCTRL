using System.ComponentModel.DataAnnotations;

namespace BoardsCTRL.Dtov2
{
    public class BoardDtov2
    {
        [MaxLength(100, ErrorMessage = "El título no puede superar los 100 caracteres.")]
        public string? boardTitle { get; set; }
        [MaxLength(255, ErrorMessage = "La descripcion no puede superar los 255 caracteres.")]
        public string? boardDescription { get; set; }
        public int? categoryId { get; set; }
        public bool? boardStatus { get; set; }
    }
}
