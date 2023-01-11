using ManejoPresupuestos.Models;

namespace ManejoPresupuestos.Servicios
{
    public interface IRepositorioTransacciones
    {
        Task Actualizar(TransaccionViewModel transaccion, decimal montoAnterior, int cuentaAnteriorId);
        Task Borrar(int id);
        Task Crear(TransaccionViewModel transaccion);
        Task<IEnumerable<TransaccionViewModel>> ObtenerPorCuentaId(ObtenerTransaccionesPorCuenta modelo);
        Task<TransaccionViewModel> ObtenerPorId(int id, int usuarioId);
        Task<IEnumerable<ResultadoObtenerPorMes>> ObtenerPorMes(int usuarioId, int año);
        Task<IEnumerable<ResultadoObtenerPorSemana>> ObtenerPorSemana(ParametroObtenerTransaccionesPorUsuario modelo);
        Task<IEnumerable<TransaccionViewModel>> ObtenerPorUsuarioId(ParametroObtenerTransaccionesPorUsuario modelo);
    }
}
