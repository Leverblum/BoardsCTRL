namespace BoardsProject.Models
{
    public class Slide
    {
        public int slideId { get; set; }
        public int boardId { get; set; }
        public string slideTitle { get; set; }
        public string URL { get; set; }
        public int time { get; set; }
        public bool slideStatus { get; set; }
        public int? createdSlideById { get; set; }
        public int? modifiedSlideById { get; set; }
        public DateTime createdSlideDate { get; set; }
        public DateTime? modifiedSlideDate { get; set; }

        // Relación con Board
        public Board Board { get; set; }
    }


}
