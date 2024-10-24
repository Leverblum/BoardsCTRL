namespace BoardsCTRL.Models
{
    public class Mensaje
    {
        public int CodigoMensaje { get; set; }
        public string DescMensaje { get; set; }
    }

    public class ExternalResponse
    {
        public Mensaje Mensaje { get; set; }
        public List<Rol> Roles { get; set; }
    }

    public class Rol
    {
        public string Descripcion { get; set; }
    }
}
