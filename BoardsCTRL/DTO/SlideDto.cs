namespace BoardsProject.DTO
{
    public class SlideDto
    {

        // Identificador unico de la Slide
        public int slideId { get; set; }

        // ID del tablero al que pertenece esta Slide
        public int boardId { get; set; }

        // Descripcion de la diapositiva
        public string slideTitle { get; set; }

        // URL de la diapositiva
        public string URL { get; set; }

        // Tiempo en segundos
        public int time { get; set; }

        public bool slideStatus { get; set; }

        // Nombre del usuario que creo la categoria.
        public int createdSlideById { get; set; }

        // Nombre del usuario que modifico la catergoria por ultima vez
        public int? modifiedSlideById { get; set; }

        // Decha en la que se creo la categoria
        public DateTime createdSlideDate { get; set; }

        // Fecha en la que se modifico la categoria por ultima vez.
        public DateTime? modifiedSlideDate { get; set; }
    }
}
