using Microsoft.AspNetCore.Mvc.Rendering;

namespace ManejoPresupuestos.Models
{
    public class CuentaCreacionViewModel : CuentaViewModel
    {
        public IEnumerable<SelectListItem> TipoCuentaViewModel { get; set; }
    }
}
