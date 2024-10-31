namespace BoardsProject.Models
{
    public class User
    {
        public int userId { get; set; }
        public int roleId { get; set; }
        public string username { get; set; }
        public string passwordHash { get; set; }
        public string email { get; set; }
        public bool userStatus { get; set; }
        public string createdUserBy { get; set; }
        public int? modifiedUserById { get; set; }
        public DateTime createdUserDate { get; set; }
        public DateTime? modifiedUserDate { get; set; }

        // Relación con Role
        public Role Role { get; set; }
    }

}
