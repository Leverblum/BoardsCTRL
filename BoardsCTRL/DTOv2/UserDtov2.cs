using System.ComponentModel.DataAnnotations;

namespace BoardsCTRL.DTOv2
{
    public class UserDtov2
    {
        public int? roleId { get; set; }
        [MaxLength(100, ErrorMessage = "El nombre de usuario no puede superar los 100 caracteres")]
        public string? username { get; set; }
        [MaxLength(255, ErrorMessage = "El correo no puede superar los 255 caracteres")]
        public string? email { get; set; }
        public bool? userStatus { get; set; }
    }
}
