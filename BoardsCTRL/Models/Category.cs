using System.Text.Json.Serialization;

namespace BoardsProject.Models
{
    public class Category
    {
        public int categoryId { get; set; }
        public string categoryTitle { get; set; }
        public bool categoryStatus { get; set; }
        public int? createdCategoryById { get; set; }
        public int? modifiedCategoryById { get; set; }
        public DateTime createdCategoryDate { get; set; }
        public DateTime? modifiedCategoryDate { get; set; }

        // Relación con Boards
        public ICollection<Board> Boards { get; set; }
    }

}
