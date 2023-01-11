using System.Security.Claims;

namespace ManejoPresupuestos.Servicios
{
    public class ServicioUsuarios : IServicioUsuarios
    {
        private readonly HttpContext accessor;
        public ServicioUsuarios(IHttpContextAccessor contextAccessor)
        {
            accessor = contextAccessor.HttpContext;
        }
        public int ObtenerUsuarioId()
        {
            if(accessor.User.Identity.IsAuthenticated)
            {
                var idClaim = accessor.User.Claims.Where(x => x.Type == ClaimTypes.NameIdentifier).FirstOrDefault();
                var id = int.Parse(idClaim.Value);
                return id;
            }
            else
            {
                throw new ApplicationException("El usuario no esta autenticado");
            }
        }
    }
}
