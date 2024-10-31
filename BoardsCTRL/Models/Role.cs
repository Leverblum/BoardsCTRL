using System.ComponentModel.DataAnnotations;

namespace BoardsProject.Models
{
    public class Role
    {
        public int roleId { get; set; }
        public string roleName { get; set; }
        public bool roleStatus { get; set; }
        public int? createdRoleById { get; set; }
        public int? modifiedRoleById { get; set; }
        public DateTime createdRoleDate { get; set; }
        public DateTime? modifiedRoleDate { get; set; }

        // Relación con Users
        public ICollection<User> Users { get; set; }
    }
}
