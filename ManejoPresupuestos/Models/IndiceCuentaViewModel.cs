namespace ManejoPresupuestos.Models
{
    public class IndiceCuentaViewModel
    {
        public string TipoCuenta { get; set; }

        public IEnumerable<CuentaViewModel> Cuentas { get; set; }

        public decimal Balance => Cuentas.Sum(x => x.Balance);

    }
}
