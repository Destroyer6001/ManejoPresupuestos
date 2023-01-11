using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace ManejoPresupuestos.Models
{
    public class CategoriaViewModel
    {
        public int Id { get; set; }
        [Required(ErrorMessage ="El campo {0} es requerido")]
        [StringLength(50, ErrorMessage ="No puede ser mayor a {1} caracteres")]
        [Display(Name ="Nombre")]
        public string Categoria { get; set; }

        [Display(Name = "Tipo Operacion")]
        public TipoOperacion TipoOperacionId { get; set; }

        public int UsuarioId { get; set; }
    }
}
