using ManejoPresupuestos.Validaciones;
using System.ComponentModel.DataAnnotations;

namespace ManejoPresupuestos.Models
{
    public class CuentaViewModel
    {
        public int Id { get; set; }

        [Required(ErrorMessage ="El campo {0} es un campo requerido")]
        [StringLength(maximumLength:50)]
        [PrimeraLetraMayuscula]
        public string Nombre { get; set; }

        [Display(Name ="Tipo Cuenta")]
        public int TipoCuentaID { get; set; }

        public decimal Balance { get; set; }

        [StringLength(maximumLength:1000)]
        public string Descripcion { get; set; }

        public string TipoCuenta { get; set; }
    }
}
