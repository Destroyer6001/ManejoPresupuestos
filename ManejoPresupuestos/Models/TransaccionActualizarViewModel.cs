namespace ManejoPresupuestos.Models
{
    public class TransaccionActualizarViewModel : TransaccionCreacionViewModel
    {
        public decimal MontoAnterior { get; set; }

        public int CuentaAnteriorId { get; set; }

        public string UrlRetorno { get; set; }
    }
}
