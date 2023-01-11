using ManejoPresupuestos.Models;

namespace ManejoPresupuestos.Servicios
{
    public interface IRepositorioTiposCuentas
    {
        Task Crear(TipoCuentaViewModel TipoCuenta);
        Task<bool> Existe(string nombre, int usuarioid, int id= 0);
        Task<IEnumerable<TipoCuentaViewModel>> Obtener(int usuarioid);
        Task<TipoCuentaViewModel> ObtenerId(int id, int usuarioid);
        Task Actualizar(TipoCuentaViewModel tipoCuentaViewModel);
        Task Borrar(int id);
        Task Ordenar(IEnumerable<TipoCuentaViewModel> tiposCuentasOrdenados);
    }
}
