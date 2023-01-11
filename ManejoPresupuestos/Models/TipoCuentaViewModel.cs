using ManejoPresupuestos.Validaciones;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;

namespace ManejoPresupuestos.Models
{
    public class TipoCuentaViewModel
    {
        public int Id { get; set; }

        [Remote(action:"VerificarExistenciaTipoCuenta",controller:"TiposCuenta", AdditionalFields =nameof(Id))]
        [Required(ErrorMessage ="El campo {0} es requerido")]
        [PrimeraLetraMayuscula]
        public string Nombre { get; set; }
        public int UsuarioId { get; set; }
        public int Orden { get; set; }

        
    }
}
