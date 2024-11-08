using System.ComponentModel.DataAnnotations;

namespace BoardsCTRL.DTOv2
{
    public class RoleDtov2
    {
        [MaxLength(100, ErrorMessage = "El nombre del rol no puede exeder los 100 caracteres")]
        public string? roleName { get; set; }
        public bool? roleStatus { get; set; }
    }
}
