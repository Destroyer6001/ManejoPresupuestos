using ManejoPresupuestos.Models;

namespace ManejoPresupuestos.Servicios
{
    public interface IRepositorioCuentas
    {
        Task Crear(CuentaViewModel cuentaViewModel);
        Task<IEnumerable<CuentaViewModel>> Buscar(int usuarioId);
        Task<CuentaViewModel> ObtenerPorId(int id, int UsuarioId);
        Task Actualizar(CuentaCreacionViewModel cuenta);
        Task Borrar(int id);
    }
}
