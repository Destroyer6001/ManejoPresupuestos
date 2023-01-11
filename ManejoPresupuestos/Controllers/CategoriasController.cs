using ManejoPresupuestos.Models;
using ManejoPresupuestos.Servicios;
using Microsoft.AspNetCore.Mvc;

namespace ManejoPresupuestos.Controllers
{
    public class CategoriasController : Controller
    {
        private readonly IRepositorioCategorias categorias;
        private readonly IServicioUsuarios servicio;
        public CategoriasController(IRepositorioCategorias repositorioCategorias, IServicioUsuarios servicioUsuarios)
        {
            categorias = repositorioCategorias;
            servicio = servicioUsuarios;
        }

        public async Task<IActionResult> Index(PaginacionViewModel paginacion)
        {
            var usuarioId = servicio.ObtenerUsuarioId();
            var Categorias = await categorias.Obtener(usuarioId,paginacion);
            var totalCategorias = await categorias.Contar(usuarioId);

            var respuestaVM = new PaginacionRespuesta<CategoriaViewModel>
            {
                Elementos = Categorias,
                Pagina = paginacion.Pagina,
                RecordsPorPagina = paginacion.RecordsPorPagina,
                CantidadTotalRecords = totalCategorias,
                BaseUrl = Url.Action()
            };
            return View(respuestaVM);
        }


        [HttpGet]
        public IActionResult Crear()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Crear(CategoriaViewModel categoriaVM)
        {
            if (!ModelState.IsValid)
            {
                return View(categoriaVM);
            }

            var UsuarioId = servicio.ObtenerUsuarioId();
            categoriaVM.UsuarioId = UsuarioId;
            await categorias.Crear(categoriaVM);
            return RedirectToAction("Index");

        }

        [HttpGet]
        public async Task<IActionResult> Editar(int id)
        {
            var usuarioId = servicio.ObtenerUsuarioId();
            var categoria = await categorias.ObtenerPorId(id, usuarioId);

            if (categoria is null)
            {
                return RedirectToAction("NoEncontrado", "Home");
            }

            return View(categoria);
        }

        [HttpPost]
        public async Task<IActionResult> Editar(CategoriaViewModel categoriaVM)
        {
            if (!ModelState.IsValid)
            {
                return View(categoriaVM);
            }

            var usuarioId = servicio.ObtenerUsuarioId();
            var categoria = await categorias.ObtenerPorId(categoriaVM.Id, usuarioId);

            if (categoria is null)
            {
                return RedirectToAction("NoEncontrado", "Home");
            }

            categoriaVM.UsuarioId = usuarioId;
            await categorias.Actualizar(categoriaVM);
            return RedirectToAction("Index");
        }

        [HttpGet]
        public async Task<IActionResult> Borrar(int id)
        {
            var usuarioId = servicio.ObtenerUsuarioId();
            var categoria = await categorias.ObtenerPorId(id, usuarioId);

            if(categoria is null)
            {
                return RedirectToAction("NoEncontrado", "Index");
            }

            return View(categoria);
        }

        [HttpPost]
        public async Task<IActionResult> BorrarCategoria(int id)
        {
            var usuarioId = servicio.ObtenerUsuarioId();
            var categoria = await categorias.ObtenerPorId(id, usuarioId);

            if(categoria is null)
            {
                return RedirectToAction("NoEncontrado", "Index");
            }

            await categorias.Borrar(id);
            return RedirectToAction("Index");
        }
    }
}
