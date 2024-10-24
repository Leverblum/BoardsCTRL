namespace BoardsProject.DTO
{
    public class UserRegisterDto
    {
        // Nombre de usuario que se utilizara para el registro
        public string username { get; set; }

        // Contraseña proporcionada por el usuario para el registro
        public string password { get; set; }

        public string email { get; set; }

        // Rol del usuario (Ej. Admin, User) durante el proceso de registro
        public int RoleId { get; set; } // Rol del usuario (Admin, User.)
    }
}
