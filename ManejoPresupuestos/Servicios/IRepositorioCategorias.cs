using ManejoPresupuestos.Models;

namespace ManejoPresupuestos.Servicios
{
    public interface IRepositorioCategorias
    {
        Task Crear(CategoriaViewModel categoria);

        Task<IEnumerable<CategoriaViewModel>> Obtener(int UsuarioId, PaginacionViewModel paginacion);

        Task<IEnumerable<CategoriaViewModel>> Obtener(int UsuarioId, TipoOperacion tipoOperacionId);

        Task<CategoriaViewModel> ObtenerPorId(int id, int usuarioId);

        Task Actualizar(CategoriaViewModel categoria);

        Task Borrar(int id);
        Task<int> Contar(int UsuarioId);
    }
}
