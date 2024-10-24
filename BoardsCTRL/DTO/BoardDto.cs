﻿using System.Text.Json.Serialization;

namespace BoardsProject.DTO
{
    public class BoardDto
    {
        // Identificador unico del tablero
        public int boardId { get; set; }

        // Identificador de la categoria a la que pertenece el tablero
        public int categoryId { get; set; }

        // Titulo del tablero
        public string boardTitle { get; set; }

        // Descripcion del tablero
        public string? boardDescription { get; set; }

        // Estado del tablero (Habilitado o deshabilitado)
        public bool boardStatus { get; set; }

        // Usuario que creo el tablero
        public int? createdBoardById { get; set; }

        // Usuario que realizo la ultima modificacion
        public int? modifiedBoardById { get; set; }

        // Fecha de la creacion del tablero
        public DateTime createdBoardDate { get; set; }

        // Fecha de la ultima modificacion del tablero
        public DateTime? modifiedBoardDate { get; set; }
    }
}
