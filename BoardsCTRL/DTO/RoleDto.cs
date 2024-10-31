namespace BoardsProject.DTO
{
    public class RoleDto
    {
        public int roleId { get; set; }
        public string roleName { get; set; }
        public bool roleStatus { get; set; }
        public int? createdRoleById { get; set; }
        public int? modifiedRoleById { get; set; }
        public DateTime createdRoleDate { get; set; }
        public DateTime? modifiedRoleDate { get; set; }
    }
}
