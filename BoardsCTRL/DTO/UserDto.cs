namespace BoardsProject.DTO
{
    public class UserDto
    {
        public int userId { get; set; }
        public int roleId { get; set; }
        public string username { get; set; }
        public string passwordHash { get; set; } // Cambiado a PascalCase
        public string email { get; set; }
        public bool userStatus { get; set; }
        public string createdUserBy { get; set; } // Añadido
        public int? modifiedUserById { get; set; } // Añadido
        public DateTime createdUserDate { get; set; }
        public DateTime? modifiedUserDate { get; set; }
    }
}
