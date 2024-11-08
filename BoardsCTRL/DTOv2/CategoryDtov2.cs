using System.ComponentModel.DataAnnotations;

namespace BoardsCTRL.DTOv2
{
    public class CategoryDtov2
    {
        [MaxLength(100, ErrorMessage = "El titulo no puede superar los 100 caracteres.")]
        public string? categoryTitle { get; set; }
        public bool? categoryStatus { get; set; }
    }
}
