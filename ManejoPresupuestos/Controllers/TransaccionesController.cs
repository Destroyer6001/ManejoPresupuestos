using AutoMapper;
using ClosedXML.Excel;
using DocumentFormat.OpenXml.Drawing.Charts;
using ManejoPresupuestos.Models;
using ManejoPresupuestos.Servicios;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Data;
using System.Reflection;
using DataTable = System.Data.DataTable;

namespace ManejoPresupuestos.Controllers
{
  
    public class TransaccionesController : Controller
    {
        private readonly IServicioUsuarios usuarios;
        private readonly IRepositorioCuentas cuentas;
        private readonly IRepositorioCategorias categorias;
        private readonly IRepositorioTransacciones transacciones;
        private readonly IMapper mapper;
        private readonly IServicioReportes reportes;
        public TransaccionesController(IServicioUsuarios servicioUsuarios, IRepositorioCuentas repositorioCuentas, IRepositorioCategorias repositorioCategorias, IRepositorioTransacciones repositorioTransacciones, IMapper imapper, IServicioReportes servicioReportes)
        {
            usuarios = servicioUsuarios;
            cuentas = repositorioCuentas;
            categorias = repositorioCategorias;
            transacciones = repositorioTransacciones;
            reportes = servicioReportes;
            mapper = imapper;
        }

        
        public async Task<IActionResult> Index(int mes, int año)
        {
            var usuarioId = usuarios.ObtenerUsuarioId();

            var modelo = await reportes.ObtenerReporteTransaccionesDetalladas(usuarioId, mes, año, ViewBag);
            

            return View(modelo);
        }

        public async Task<IActionResult> Semanal(int mes, int año)
        {
            var usuarioId = usuarios.ObtenerUsuarioId();
            IEnumerable<ResultadoObtenerPorSemana>transaccionesPorSemana =
                await reportes.ObtenerReporteSemanal(usuarioId, mes, año, ViewBag);

            var agrupado = transaccionesPorSemana.GroupBy(x => x.Semana).Select
                (x => new ResultadoObtenerPorSemana()
                {
                    Semana = x.Key,
                    Ingresos = x.Where(x => x.TipoOperacionId == TipoOperacion.Ingreso).Select(x => x.Monto).FirstOrDefault(),
                    Egresos = x.Where(x => x.TipoOperacionId == TipoOperacion.Egreso).Select(x => x.Monto).FirstOrDefault()

                }).ToList();

            if(año == 0 || mes == 0)
            {
                var hoy = DateTime.Today;
                año = hoy.Year;
                mes = hoy.Month;
            }

            var fechaReferencia = new DateTime(año, mes, 1);
            var diasDelMes = Enumerable.Range(1, fechaReferencia.AddMonths(1).AddDays(-1).Day);

            var diasSegmentados = diasDelMes.Chunk(7).ToList();

            for (int i = 0; i < diasSegmentados.Count(); i++)
            {
                var semana = i + 1;
                var fechaInicio = new DateTime(año, mes, diasSegmentados[i].First());
                var fechaFin = new DateTime(año, mes, diasSegmentados[i].Last());
                var grupoSemana = agrupado.FirstOrDefault(x => x.Semana == semana);

                if(grupoSemana is null)
                {
                    agrupado.Add(new ResultadoObtenerPorSemana()
                    {
                        Semana = semana,
                        FechaInicio = fechaInicio,
                        FechaFin = fechaFin

                    });
                }
                else
                {
                    grupoSemana.FechaInicio = fechaInicio;
                    grupoSemana.FechaFin = fechaFin;
                }
            }

            agrupado = agrupado.OrderByDescending(x => x.Semana).ToList();
            var modelo = new ReporteSemanalViewModel();
            modelo.TransaccionesPorSemana = agrupado;
            modelo.FechaReferencia = fechaReferencia;
            return View(modelo);
        }

        public async Task<IActionResult> Mensual(int año)
        {
            var usuarioid = usuarios.ObtenerUsuarioId();

            if(año == 0)
            {
                año = DateTime.Today.Year;
            }

            var transaccionesPorMes = await transacciones.ObtenerPorMes(usuarioid, año);
            var transaccionesAgrupada = transaccionesPorMes.GroupBy(x => x.Mes).Select(x => new ResultadoObtenerPorMes()
            {
                Mes = x.Key,
                Ingreso = x.Where(x => x.TipoOperacionId == TipoOperacion.Ingreso).Select(x => x.Monto).FirstOrDefault(),
                Egreso = x.Where(x => x.TipoOperacionId == TipoOperacion.Egreso).Select(x => x.Monto).FirstOrDefault()

            }).ToList();

            for(int mes = 1; mes <= 12; ++mes)
            {
                var transaccion  = transaccionesAgrupada.FirstOrDefault(x => x.Mes == mes);
                var fechaReferencia = new DateTime(año, mes, 1);
                if(transaccion is null)
                {
                    transaccionesAgrupada.Add(new ResultadoObtenerPorMes()
                    {
                        Mes = mes,
                        FechaReferencia = fechaReferencia
                    });
                }
                else
                {
                    transaccion.FechaReferencia = fechaReferencia;
                }
            }

            transaccionesAgrupada = transaccionesAgrupada.OrderByDescending(x => x.Mes).ToList();

            var modelo = new ReporteMensualViewModel();
            modelo.Año = año;
            modelo.TransaccionesPorMes = transaccionesAgrupada;

            return View(modelo);
        }

        public IActionResult ExcelReporte()
        {
            return View();
        }

        [HttpGet]
        public async Task<FileResult> ExportarExcelPorMes(int mes, int año)
        {
            var fechaInicio = new DateTime(año, mes, 1);
            var fechaFin = fechaInicio.AddMonths(1).AddDays(-1);
            var usuarioId = usuarios.ObtenerUsuarioId();

            var Transacciones = await transacciones.ObtenerPorUsuarioId(new ParametroObtenerTransaccionesPorUsuario
            {
                UsuarioId = usuarioId,
                FechaInicio = fechaInicio,
                FechaFin = fechaFin
            });

            var nombreArchivo = $"Manejo Presupuesto - {fechaInicio.ToString("MMM yyyy")}.xlsx";
            return GenerarExcel(nombreArchivo, Transacciones);
        }

        [HttpGet]
        public async Task<FileResult> ExportarExcelPorAño(int año)
        {
            var fechaInicio = new DateTime(año, 1, 1);
            var fechaFin = fechaInicio.AddYears(1).AddDays(-1);
            var usuarioId = usuarios.ObtenerUsuarioId();

            var Transacciones = await transacciones.ObtenerPorUsuarioId(new ParametroObtenerTransaccionesPorUsuario
            {
                FechaInicio = fechaInicio,
                FechaFin = fechaFin,
                UsuarioId = usuarioId
            });

            var nombreArchivo = $"Manejo Presupuesto - {fechaInicio.ToString("yyyy")}.xlsx";
            return GenerarExcel(nombreArchivo, Transacciones);
        }

        [HttpGet]
        public async Task<FileResult> ExportarTodo()
        {
            var fechaInicio = DateTime.Today.AddYears(-100);
            var fechaFin = DateTime.Today.AddYears(1000);
            var usuarioId = usuarios.ObtenerUsuarioId();

            var Transacciones = await transacciones.ObtenerPorUsuarioId(new ParametroObtenerTransaccionesPorUsuario
            {
                FechaInicio = fechaInicio,
                FechaFin = fechaFin,
                UsuarioId = usuarioId
            }) ;

            var nombreArchivo = $"Manejo Presupuesto - {DateTime.Today.ToString("dd-MM-yyyy")}.xlsx";
            return GenerarExcel(nombreArchivo, Transacciones);
        }

        private FileResult GenerarExcel (string NombreArchivo, IEnumerable<TransaccionViewModel> Transacciones)
        {
            DataTable dataTable = new DataTable("Transacciones");
            dataTable.Columns.AddRange(new DataColumn[]
            {
                new DataColumn("Fecha"),
                new DataColumn("Cuenta"),
                new DataColumn("Categoria"),
                new DataColumn("Nota"),
                new DataColumn("Monto"),
                new DataColumn("Ingreso/Gasto")
            });

            foreach (var transaccion in Transacciones)
            {
                dataTable.Rows.Add(
                    transaccion.FechaTransaccion,
                    transaccion.Cuenta,
                    transaccion.Categoria,
                    transaccion.Nota,
                    transaccion.Monto,
                    transaccion.TipoOperacionId
                    );
            }

            using (XLWorkbook wb = new XLWorkbook())
            {
                wb.Worksheets.Add(dataTable);

                using(MemoryStream stream = new MemoryStream())
                {
                    wb.SaveAs(stream);
                    return File(stream.ToArray(), "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", NombreArchivo);
                }
            }

        }

        public IActionResult Calendario()
        {
            return View();
        }

        public async Task<JsonResult> ObtenerTransaccionesCalendario(DateTime start, DateTime end)
        {
            var usuarioId = usuarios.ObtenerUsuarioId();

            var Transacciones = await transacciones.ObtenerPorUsuarioId(new ParametroObtenerTransaccionesPorUsuario
            {
                FechaFin = end,
                FechaInicio = start,
                UsuarioId = usuarioId
            });

            var EventosCalendario = Transacciones.Select(transaccion => new EventoCalendario()
            {
                Title = transaccion.Monto.ToString("N"),
                Start = transaccion.FechaTransaccion.ToString("yyyy-MM-dd"),
                End = transaccion.FechaTransaccion.ToString("yyyy-MM-dd"),
                Color = (transaccion.TipoOperacionId == TipoOperacion.Egreso) ? "Red" : null
            });

            return Json(EventosCalendario);
        }

        public async Task<JsonResult> ObtenerTransaccionesPorFecha (DateTime fecha)
        {
            var usuarioId = usuarios.ObtenerUsuarioId();

            var Transacciones = await transacciones.ObtenerPorUsuarioId(new ParametroObtenerTransaccionesPorUsuario 
            { 
                FechaFin = fecha,
                FechaInicio = fecha,
                UsuarioId = usuarioId
            });

            return Json(Transacciones);
        }

        public async Task<IActionResult> Crear()
        {
            var usuarioId = usuarios.ObtenerUsuarioId();
            var modelo = new TransaccionCreacionViewModel();
            modelo.Cuentas = await ObtenerCuentas(usuarioId);
            modelo.Categorias = await ObtenerCategorias(usuarioId, modelo.TipoOperacionId);
            return View(modelo);
        }

        [HttpPost]
        public async Task<IActionResult> Crear(TransaccionCreacionViewModel modelo)
        {
            var usuarioId = usuarios.ObtenerUsuarioId();

            if(!ModelState.IsValid)
            {
                modelo.Cuentas = await ObtenerCuentas(usuarioId);
                modelo.Categorias = await ObtenerCategorias(usuarioId, modelo.TipoOperacionId);

                return View(modelo);
            }

            var Cuenta = await cuentas.ObtenerPorId(modelo.CuentaId, usuarioId);

            if(Cuenta is null)
            {
                return RedirectToAction("NoEncontrado", "Home");
            }

            var Categoria = await categorias.ObtenerPorId(modelo.CategoriaId, usuarioId);

            if (Categoria is null)
            {
                return RedirectToAction("NoEncontrado", "Home");
            }

            modelo.UsuarioId = usuarioId;

            if(modelo.TipoOperacionId == TipoOperacion.Egreso)
            {
                modelo.Monto *= -1;
            }

            await transacciones.Crear(modelo);
            return RedirectToAction("Index");
        }

        [HttpGet]
        public async Task<IActionResult> Editar(int id, string urlRetorno = null)
        {
            var usuarioId = usuarios.ObtenerUsuarioId();
            var trasaccion = await transacciones.ObtenerPorId(id, usuarioId);

            if(trasaccion is null)
            {
                return RedirectToAction("NoEncontrado", "Home");
            }
            
            var modelo = mapper.Map<TransaccionActualizarViewModel>(trasaccion);

            modelo.MontoAnterior = modelo.Monto;

            if(modelo.TipoOperacionId == TipoOperacion.Egreso)
            {
                modelo.MontoAnterior = modelo.Monto * -1;
            }

            modelo.CuentaAnteriorId = trasaccion.CuentaId;
            modelo.Categorias = await ObtenerCategorias(usuarioId, trasaccion.TipoOperacionId);
            modelo.Cuentas = await ObtenerCuentas(usuarioId);
            modelo.UrlRetorno = urlRetorno;

            return View(modelo);
        }

        [HttpPost]
        public async Task<IActionResult> Editar(TransaccionActualizarViewModel modelo)
        {
            var usuarioId = usuarios.ObtenerUsuarioId();

            if(!ModelState.IsValid)
            {
                modelo.Cuentas = await ObtenerCuentas(usuarioId);
                modelo.Categorias = await ObtenerCategorias(usuarioId, modelo.TipoOperacionId);
                return View(modelo);
            }

            var cuenta = await cuentas.ObtenerPorId(modelo.CuentaId, usuarioId);

            if(cuenta is null)
            {
                return RedirectToAction("NoEncontrado", "Home");
            }

            var categoria = await categorias.ObtenerPorId(modelo.CategoriaId, usuarioId);

            if(categoria is null)
            {
                return RedirectToAction("NoEncontrado", "Home");
            }

            var transaccion = mapper.Map<TransaccionViewModel>(modelo);

            if (modelo.TipoOperacionId == TipoOperacion.Egreso)
            {
                transaccion.Monto *= -1;
            }

            await transacciones.Actualizar(transaccion, modelo.MontoAnterior, modelo.CuentaAnteriorId);

            if(string.IsNullOrEmpty(modelo.UrlRetorno))
            {
                return RedirectToAction("Index");
            }
            else
            {
                return LocalRedirect(modelo.UrlRetorno);
            }
        }

        private async Task<IEnumerable<SelectListItem>> ObtenerCuentas(int UsuarioId)
        { 
            var Cuentas = await cuentas.Buscar(UsuarioId);
            return Cuentas.Select(x => new SelectListItem(x.Nombre, x.Id.ToString()));
        }

        private async Task<IEnumerable<SelectListItem>> ObtenerCategorias(int usuarioId, TipoOperacion tipoOperacion)
        {
            var Categorias = await categorias.Obtener(usuarioId, tipoOperacion);
            var resultado = Categorias.Select(x => new SelectListItem(x.Categoria, x.Id.ToString())).ToList();

            var OpcionPorDefecto = new SelectListItem("--Seleccione Una Categoria--","0",true);
            resultado.Insert(0,OpcionPorDefecto);

            return resultado;
        }

        [HttpPost]
        public async Task<IActionResult> ObtenerCategorias([FromBody] TipoOperacion tipoOperacion)
        {
            var usuarioId = usuarios.ObtenerUsuarioId();
            var Categorias = await ObtenerCategorias(usuarioId, tipoOperacion);
            return Ok(Categorias);

        }

        [HttpPost]

        public async Task<IActionResult> Borrar(int id, string urlRetorno = null)
        {
            var usuarioId = usuarios.ObtenerUsuarioId();

            var transaccion = transacciones.ObtenerPorId(id,usuarioId);

            if (transaccion is null)
            {
                RedirectToAction("NoEncontrado", "Home");
            }

            await transacciones.Borrar(id);
            if (string.IsNullOrEmpty(urlRetorno))
            {
                return RedirectToAction("Index");
            }
            else
            {
                return LocalRedirect(urlRetorno);
            }
        }

    }
}
