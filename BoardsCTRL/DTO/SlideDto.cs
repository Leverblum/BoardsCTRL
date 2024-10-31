namespace BoardsProject.DTO
{
    public class SlideDto
    {
        public int slideId { get; set; }
        public int boardId { get; set; }
        public string slideTitle { get; set; }
        public string URL { get; set; }
        public int time { get; set; }
        public bool slideStatus { get; set; }
        public int? createdSlideById { get; set; } // Añadido
        public int? modifiedSlideById { get; set; } // Añadido
        public DateTime createdSlideDate { get; set; }
        public DateTime? modifiedSlideDate { get; set; }
    }
}
