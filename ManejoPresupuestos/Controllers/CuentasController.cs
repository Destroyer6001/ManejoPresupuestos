using AutoMapper;
using ManejoPresupuestos.Models;
using ManejoPresupuestos.Servicios;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Reflection;

namespace ManejoPresupuestos.Controllers
{
    public class CuentasController : Controller
    {
        private readonly IRepositorioTiposCuentas repositorioTiposCuentas;
        private readonly IServicioUsuarios servicio;
        private readonly IRepositorioCuentas cuentas;
        private readonly IRepositorioTransacciones transacciones;
        private readonly IMapper _mapper;
        private readonly IServicioReportes reportes;
        public CuentasController(IRepositorioTiposCuentas tiposCuentas, IServicioUsuarios servicioUsuarios, IRepositorioCuentas repositorioCuentas, IMapper mapper, IRepositorioTransacciones repositorioTransacciones, IServicioReportes servicioReportes)
        {
            repositorioTiposCuentas = tiposCuentas;
            servicio = servicioUsuarios;
            cuentas = repositorioCuentas;
            _mapper = mapper;
            transacciones = repositorioTransacciones; 
            reportes = servicioReportes;
        }


        public async Task<IActionResult> Index()
        {
            var usuarioId = servicio.ObtenerUsuarioId();
            var cuentascontipocuenta = await cuentas.Buscar(usuarioId);

            var modelo = cuentascontipocuenta
                .GroupBy(x => x.TipoCuenta)
                .Select(grupo => new IndiceCuentaViewModel
                {
                    TipoCuenta = grupo.Key,
                    Cuentas = grupo.AsEnumerable()
                }).ToList();

            return View(modelo);
        }

        [HttpGet]
        public async Task <IActionResult> Crear()
        {
            var usuarioId = servicio.ObtenerUsuarioId();
            var modelo = new CuentaCreacionViewModel();
            modelo.TipoCuentaViewModel = await ObtenerTiposCuentas(usuarioId);
            return View(modelo);
        }

        [HttpPost]
        public async Task<IActionResult> Crear(CuentaCreacionViewModel cuentaCreacionViewModel)
        {
            var usuarioId = servicio.ObtenerUsuarioId();
            var tipocuenta = await repositorioTiposCuentas.ObtenerId(cuentaCreacionViewModel.TipoCuentaID, usuarioId);

            if(tipocuenta is null)
            {
                return RedirectToAction("NoEncontrado", "Home");
            }

            if(!ModelState.IsValid)
            {
                cuentaCreacionViewModel.TipoCuentaViewModel = await ObtenerTiposCuentas(usuarioId);
                return View(cuentaCreacionViewModel);
            }

            await cuentas.Crear(cuentaCreacionViewModel);
            return RedirectToAction("Index");
        }

        public async Task<IActionResult> Editar(int id)
        {
            var usuarioId = servicio.ObtenerUsuarioId();
            var cuenta = await cuentas.ObtenerPorId(id, usuarioId);

            if(cuenta is null)
            {
                return RedirectToAction("NoEncontrado", "Home");
            }

            var modelo = _mapper.Map<CuentaCreacionViewModel>(cuenta);

            modelo.TipoCuentaViewModel = await ObtenerTiposCuentas(usuarioId);
            return View(modelo);
        }

        [HttpPost]
        public async Task<IActionResult> Editar(CuentaCreacionViewModel cuentaCreacionViewModel)
        {
            var usuarioId = servicio.ObtenerUsuarioId();
            var cuenta = await cuentas.ObtenerPorId(cuentaCreacionViewModel.Id, usuarioId);

            if(cuenta is null)
            {
                return RedirectToAction("NoEncontrado", "Home");
            }

            var tipoCuenta = await repositorioTiposCuentas.ObtenerId(cuentaCreacionViewModel.TipoCuentaID, usuarioId);

            if(tipoCuenta is null)
            {
                return RedirectToAction("NoEncontrado", "Home");
            }

            await cuentas.Actualizar(cuentaCreacionViewModel);
            return RedirectToAction("Index");
        }

        [HttpGet]
        public async Task<IActionResult> Borrar(int id)
        {
            var usuarioId = servicio.ObtenerUsuarioId();
            var cuenta = await cuentas.ObtenerPorId(id,usuarioId);

            if (cuenta is null)
            {
                return RedirectToAction("NoEncontrado", "Home");
            }

            return View(cuenta);
        
        }

        [HttpPost]
        public async Task<IActionResult> BorrarCuenta(int id)
        {
            var usuarioId = servicio.ObtenerUsuarioId();
            var cuenta = await cuentas.ObtenerPorId(id, usuarioId);

            if (cuenta is null)
            {
                return RedirectToAction("NoEncontrado", "Home");
            }

            await cuentas.Borrar(id);
            return RedirectToAction("Index");
        }

        private async Task<IEnumerable<SelectListItem>> ObtenerTiposCuentas(int usuarioId)
        {
            var tipoCuentas = await repositorioTiposCuentas.Obtener(usuarioId);
            return tipoCuentas.Select(x => new SelectListItem(x.Nombre, x.Id.ToString()));
        }

        public async Task<IActionResult> Detalle(int id, int mes, int año)
        {
            var usurioId = servicio.ObtenerUsuarioId();
            var cuenta = await cuentas.ObtenerPorId(id,usurioId);

            if (cuenta is null)
            {
                return RedirectToAction("NoEncontrado", "Home");
            }

            ViewBag.Cuenta = cuenta.Nombre;

            var modelo = await reportes.ObtenerReporteTransaccionesDetalladasPorCuenta(usurioId, id, mes, año, ViewBag);

            return View(modelo);
        }
    }
}
