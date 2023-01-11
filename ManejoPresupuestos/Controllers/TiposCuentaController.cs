using Dapper;
using ManejoPresupuestos.Models;
using ManejoPresupuestos.Servicios;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using System.Globalization;

namespace ManejoPresupuestos.Controllers
{
    public class TiposCuentaController : Controller
    {
        private readonly IRepositorioTiposCuentas _repositorioTiposCuentas;
        private readonly IServicioUsuarios _serviciosUsuarios;
        public TiposCuentaController(IRepositorioTiposCuentas repositorioTiposCuentas, IServicioUsuarios servicioUsuarios)
        {
           _repositorioTiposCuentas = repositorioTiposCuentas;
            _serviciosUsuarios = servicioUsuarios;
        }

        public async Task<IActionResult> Index()
        {
            var usuarioId = _serviciosUsuarios.ObtenerUsuarioId(); 
            var tiposCuentas = await _repositorioTiposCuentas.Obtener(usuarioId);
            return View(tiposCuentas);
        }

        public IActionResult Crear()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Crear(TipoCuentaViewModel tipoCuenta)
        {
            if (!ModelState.IsValid)
            { 
                return View(tipoCuenta);
            }

            tipoCuenta.UsuarioId = _serviciosUsuarios.ObtenerUsuarioId();

            var Exite = await _repositorioTiposCuentas.Existe(tipoCuenta.Nombre, tipoCuenta.UsuarioId);

            if(Exite)
            {
                ModelState.AddModelError(nameof(tipoCuenta.Nombre), $"El nombre {tipoCuenta.Nombre} ya existe.");
                return View(tipoCuenta); 
            }

            await _repositorioTiposCuentas.Crear(tipoCuenta);
            return RedirectToAction("Index");
        }

        public async Task<ActionResult> Editar(int id)
        {
            var usuarioId = _serviciosUsuarios.ObtenerUsuarioId();
            var tipocuenta = await _repositorioTiposCuentas.ObtenerId(id, usuarioId);

            if (tipocuenta is null)
            {
                return RedirectToAction("NoEncontrado", "Home");
            }

            return View(tipocuenta);
        }

        [HttpPost]
        public async Task<ActionResult> Editar(TipoCuentaViewModel tipoCuenta)
        {
            var Usuarioid = _serviciosUsuarios.ObtenerUsuarioId();
            var tipocuentaexiste = await _repositorioTiposCuentas.ObtenerId(tipoCuenta.Id, Usuarioid);

            if(tipocuentaexiste is null)
            {
                return RedirectToAction("NoEncontrado","Home");
            }

            await _repositorioTiposCuentas.Actualizar(tipoCuenta);
            return RedirectToAction("Index");
        }

        [HttpGet]
        public async Task<IActionResult> VerificarExistenciaTipoCuenta(string nombre, int id)
        {
            var usuarioId = _serviciosUsuarios.ObtenerUsuarioId();
            var yaexiste = await _repositorioTiposCuentas.Existe(nombre, usuarioId, id);

            if(yaexiste)
            {
                return Json($"El nombre {nombre} ya existe");
            }

            return Json(true);
        }

        public async Task<IActionResult> Borrar(int id)
        { 
            var usuarioid = _serviciosUsuarios.ObtenerUsuarioId();
            var tipocuenta = await _repositorioTiposCuentas.ObtenerId(id, usuarioid);

            if (tipocuenta is null)
            {
                return RedirectToAction("NoEncontrado", "Home");
            }

            return View(tipocuenta);
        }

        [HttpPost]
        public async Task<IActionResult> BorrarTipoCuenta(int id)
        {

            var usuarioid = _serviciosUsuarios.ObtenerUsuarioId();
            var tipocuenta = await _repositorioTiposCuentas.ObtenerId(id, usuarioid);

            if (tipocuenta is null)
            {
                return RedirectToAction("NoEncontrado", "Home");
            }

            await _repositorioTiposCuentas.Borrar(id);
            return RedirectToAction("Index");
        }

        [HttpPost]
        public async Task<IActionResult> Ordenar([FromBody] int[] ids)
        {
            var usuarioId = _serviciosUsuarios.ObtenerUsuarioId();
            var tiposCuentas = await _repositorioTiposCuentas.Obtener(usuarioId);
            var idsTiposCuentas = tiposCuentas.Select(x => x.Id);

            var idsTiposCuentssNoPertenecenAlUsuario = ids.Except(idsTiposCuentas).ToList();

            if(idsTiposCuentssNoPertenecenAlUsuario.Count > 0)
            {
                return Forbid();
            }

            var tiposCuentasOrdenados = ids.Select((valor, indice)=> new TipoCuentaViewModel() 
            {Id=valor, Orden=indice + 1 }).AsEnumerable();

            await _repositorioTiposCuentas.Ordenar(tiposCuentasOrdenados);

            return Ok();
        }

    }
}
