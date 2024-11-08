namespace BoardsProject.DTO
{
    public class CategoryDto
    {
        public int categoryId { get; set; }
        public string categoryTitle { get; set; }
        public bool categoryStatus { get; set; }
        public int? createdCategoryById { get; set; } // Añadido
        public int? modifiedCategoryById { get; set; } // Añadido
        public DateTime createdCategoryDate { get; set; }
        public DateTime? modifiedCategoryDate { get; set; }
    }
}
