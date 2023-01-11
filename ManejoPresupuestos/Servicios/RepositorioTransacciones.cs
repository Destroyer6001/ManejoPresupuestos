using Dapper;
using ManejoPresupuestos.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.Data.SqlClient;

namespace ManejoPresupuestos.Servicios
{
    public class RepositorioTransacciones : IRepositorioTransacciones
    {
        private readonly string connectionString;
        public RepositorioTransacciones(IConfiguration configuration)
        {
            connectionString = configuration.GetConnectionString("DefaultConnection");
        }

        public async Task Crear(TransaccionViewModel transaccion)
        {
            using var connection = new SqlConnection(connectionString);
            var id = await connection.QuerySingleAsync<int>("INSERTARTRANSACCION",
                new
                { transaccion.UsuarioId,
                    transaccion.FechaTransaccion,
                    transaccion.Monto,
                    transaccion.CategoriaId,
                    transaccion.CuentaId,
                    transaccion.Nota
                },
                commandType: System.Data.CommandType.StoredProcedure);

            transaccion.Id = id;
        }

        public async Task Actualizar(TransaccionViewModel transaccion, decimal montoAnterior, int cuentaAnteriorId)
        {
            using var connection = new SqlConnection(connectionString);
            await connection.ExecuteAsync("ACTUALIZARTRANSACCION",
                new
                {
                    transaccion.Id,
                    transaccion.FechaTransaccion,
                    transaccion.Monto,
                    transaccion.CategoriaId,
                    transaccion.CuentaId,
                    transaccion.Nota,
                    montoAnterior,
                    cuentaAnteriorId
                }, commandType: System.Data.CommandType.StoredProcedure);
        }

        public async Task<TransaccionViewModel> ObtenerPorId(int id, int usuarioId)
        {
            using var connection = new SqlConnection(connectionString);
            return await connection.QueryFirstOrDefaultAsync<TransaccionViewModel>(@"SELECT Transacciones.*,
            CAT.TipoOperacionId
            FROM Transacciones
            INNER JOIN CATEGORIA CAT
            ON CAT.Id = Transacciones.CategoriaId
            WHERE Transacciones.Id = @ID AND Transacciones.UsuarioId = @USUARIOID",
            new { id, usuarioId });
        }

        public async Task Borrar(int id)
        {
            using var connection = new SqlConnection(connectionString);
            await connection.ExecuteAsync("TRANSACCIONESBORRA", new { id },
                commandType: System.Data.CommandType.StoredProcedure);
        } 

        public async Task<IEnumerable<TransaccionViewModel>> ObtenerPorCuentaId(ObtenerTransaccionesPorCuenta modelo)
        {
            using var connection = new SqlConnection(connectionString);
            return await connection.QueryAsync<TransaccionViewModel>(@"SELECT T.Id, T.Monto, T.FechaTransaccion, 
            C.Categoria AS CATEGORIA, CU.Nombre AS CUENTA, C.TipoOperacionId FROM
            Transacciones T
            INNER JOIN CATEGORIA C
            ON C.Id = T.CategoriaId
            INNER JOIN CUENTAS CU
            ON CU.Id = T.Cuentaid
            WHERE T.Cuentaid = @CUENTAID AND T.UsuarioId = @USUARIOID AND FechaTransaccion BETWEEN @FECHAINICIO 
            AND @FECHAFIN", modelo);
        }

        public async Task<IEnumerable<TransaccionViewModel>> ObtenerPorUsuarioId(ParametroObtenerTransaccionesPorUsuario modelo)
        {
            using var connection = new SqlConnection(connectionString);
            return await connection.QueryAsync<TransaccionViewModel>(@"SELECT T.Id, T.Monto, T.FechaTransaccion, 
            C.Categoria AS CATEGORIA, CU.Nombre AS CUENTA, C.TipoOperacionId,Nota FROM
            Transacciones T
            INNER JOIN CATEGORIA C
            ON C.Id = T.CategoriaId
            INNER JOIN CUENTAS CU
            ON CU.Id = T.Cuentaid
            WHERE T.UsuarioId = @USUARIOID AND FechaTransaccion BETWEEN @FECHAINICIO AND @FECHAFIN
            ORDER BY T.FechaTransaccion DESC", modelo);
        }

        public async Task<IEnumerable<ResultadoObtenerPorSemana>> ObtenerPorSemana 
            (ParametroObtenerTransaccionesPorUsuario modelo)
        {
            using var connection = new SqlConnection(connectionString);
            return await connection.QueryAsync<ResultadoObtenerPorSemana>(@"SELECT DATEDIFF(d, @fechaInicio, 
            FechaTransaccion)/7+1 as Semana,
            SUM(Monto) as Monto, CAT.TipoOperacionId
            FROM Transacciones
            INNER JOIN CATEGORIA CAT
            ON CAT.Id = Transacciones.CategoriaId
            WHERE TRANSACCIONES.UsuarioId = @USUARIOID AND
            FechaTransaccion BETWEEN @fechaInicio and @fechaFin
            GROUP BY DATEDIFF(d, @fechaInicio, FechaTransaccion)/7, CAT.TipoOperacionId",modelo);
        }

        public async Task<IEnumerable<ResultadoObtenerPorMes>> ObtenerPorMes(int usuarioId, int año)
        {
           using var connection = new SqlConnection(connectionString);
           return await connection.QueryAsync<ResultadoObtenerPorMes>(@"SELECT MONTH(FechaTransaccion) as Mes,
                                                    SUM(Monto) as Monto, CAT.TipoOperacionId
                                                    FROM Transacciones
                                                    INNER JOIN CATEGORIA CAT
                                                    ON CAT.Id = Transacciones.CategoriaId
                                                    WHERE Transacciones.UsuarioId = @USUARIOID AND YEAR(FechaTransaccion) = @AÑO
                                                    GROUP BY MONTH(FechaTransaccion), CAT.TipoOperacionId", new {usuarioId,año});

        }
    }
}
