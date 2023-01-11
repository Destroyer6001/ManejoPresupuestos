namespace ManejoPresupuestos.Models
{
    public class PaginacionRespuestaViewModel
    {
        public int Pagina { get; set; } = 1;
        public int RecordsPorPagina { get; set; } = 10;
        public int CantidadTotalRecords { get; set; }
        public int CantidadTotalDePaginas => (int)Math.Ceiling((double)CantidadTotalRecords / RecordsPorPagina);
        public string BaseUrl { get; set; }
    }

    public class PaginacionRespuesta<t> : PaginacionRespuestaViewModel
    {
        public IEnumerable<t> Elementos { get; set; }
    }

}
