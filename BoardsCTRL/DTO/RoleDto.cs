﻿namespace BoardsProject.DTO
{
    public class RoleDto
    {

        public int ? roleId { get; set; }

        // Nombre del rol
        public string roleName { get; set; }

        public bool roleStatus { get; set; }

        // Nombre del usuario que creo la categoria.
        public int createdRoleById { get; set; }

        // Nombre del usuario que modifico la catergoria por ultima vez
        public int? modifiedRoleById { get; set; }

        // Decha en la que se creo la categoria
        public DateTime createdRoleDate { get; set; }

        // Fecha en la que se modifico la categoria por ultima vez.
        public DateTime? modifiedRoleDate { get; set; }
    }
}
