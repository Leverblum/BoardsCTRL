using System.Text.Json.Serialization;

namespace BoardsProject.Models
{
    public class Category
    {
        // Identificador unico de la categoria.
        public int categoryId { get; set; }

        // Descripcion de la categoria.
        public string categoryTitle { get; set; }

        // Estado de la categoria (Habilitada o deshabilitada)
        public bool categoryStatus { get; set; }

        // Nombre del usuario que creo la categoria.
        public int createdCategoryById { get; set; }

        // Nombre del usuario que modifico la catergoria por ultima vez
        public int? modifiedCategoryById { get; set; }

        // Decha en la que se creo la categoria
        public DateTime createdCategoryDate { get; set; }

        // Fecha en la que se modifico la categoria por ultima vez.
        public DateTime? modifiedCategoryDate { get; set; }

        // Coleccion de tableros que perteneces a esta categoria
        public ICollection<Board> Boards { get; set; }
    }
}
