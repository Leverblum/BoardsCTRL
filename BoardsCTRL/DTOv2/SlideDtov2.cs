using System.ComponentModel.DataAnnotations;

namespace BoardsCTRL.DTOv2
{
    public class SlideDtov2
    {
        [MaxLength(100, ErrorMessage = "El titulo no puede superar los 100 caracteres")]
        public string? slideTitle { get; set; }
        [MaxLength(255, ErrorMessage = "La Url no puede superar los 255 caracteres")]
        public string? URL { get; set; }
        [Range(1, 1000, ErrorMessage = "El tiempo tiene que estar en un rango entre 1 y 1.000")]
        public int? time { get; set; }
        public bool? slideStatus { get; set; }
    }
}


 