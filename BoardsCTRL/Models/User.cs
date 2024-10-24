namespace BoardsProject.Models
{
    public class User
    {
        // Identificador unico del usuario
        public int userId { get; set; }
        
        // Id del rol asignado al usuario
        public int roleId { get; set; }

        // Nombre de usuario
        public string username { get; set; }

        // Hash de la contraseña del usuario (Almacena la contraseña en formato seguro)
        public string passwordHash { get; set; }

        public string email { get; set; }

        public bool userStatus { get; set; }

        // Nombre del usuario que creo la categoria.
        public string createdUserBy { get; set; }

        // Nombre del usuario que modifico la catergoria por ultima vez
        public int? modifiedUserById { get; set; }

        // Decha en la que se creo la categoria
        public DateTime createdUserDate { get; set; }

        // Fecha en la que se modifico la categoria por ultima vez.
        public DateTime? modifiedUserDate { get; set; }

        // Relacion con la entidad Role que define el rol del usuario
        public Role Role { get; set; }
    }
}
