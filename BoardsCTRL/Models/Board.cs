using System.Text.Json.Serialization;

namespace BoardsProject.Models
{
    public class Board
    {
        public int boardId { get; set; }
        public int categoryId { get; set; }
        public string boardTitle { get; set; }
        public string boardDescription { get; set; }
        public bool boardStatus { get; set; }
        public int? createdBoardById { get; set; }
        public int? modifiedBoardById { get; set; }
        public DateTime createdBoardDate { get; set; }
        public DateTime? modifiedBoardDate { get; set; }

        // Relación con Category
        public Category Category { get; set; }

        // Relación con Slides
        public ICollection<Slide> Slides { get; set; }
    }

}
