using Dapper;
using ManejoPresupuestos.Models;
using Microsoft.Data.SqlClient;
using System.Collections;

namespace ManejoPresupuestos.Servicios
{
    public class RepositorioCuentas : IRepositorioCuentas
    {
        private readonly string connectionString;
        public RepositorioCuentas(IConfiguration configuration)
        {
            connectionString = configuration.GetConnectionString("DefaultConnection");
        }

        public async Task Crear(CuentaViewModel cuentaViewModel)
        {
            using var connection = new SqlConnection(connectionString);
            var id = await connection.QuerySingleAsync<int>(@"INSERT INTO CUENTAS (Nombre, TipoCuentaId, Descripcio, Balance) VALUES
                                                      (@NOMBRE, @TIPOCUENTAID, @DESCRIPCION,@BALANCE);
                                                      SELECT SCOPE_IDENTITY();", cuentaViewModel);

            cuentaViewModel.Id = id;
        }

        public async Task<IEnumerable<CuentaViewModel>> Buscar (int usuarioId)
        {
            using var connection = new SqlConnection(connectionString);
            return await connection.QueryAsync<CuentaViewModel>(@"SELECT CUENTAS.Id, CUENTAS.Nombre, 
                                                                CUENTAS.Balance, TC.Nombre AS TIPOCUENTA FROM CUENTAS
                                                                INNER JOIN TIPODECUENTA TC
                                                                ON TC.Id = CUENTAS.TipoCuentaId
                                                                WHERE TC.UsuarioId = @USUARIOID
                                                                ORDER BY TC.Orden", new { usuarioId });
            
        }

        public async Task <CuentaViewModel> ObtenerPorId(int id, int UsuarioId)
        {
            using var connection = new SqlConnection(connectionString);
            return await connection.QueryFirstOrDefaultAsync<CuentaViewModel>(
                @"SELECT CUENTAS.Id, CUENTAS.Nombre,CUENTAS.Balance,Descripcio,CUENTAS.TipoCuentaId
                FROM CUENTAS
                INNER JOIN TIPODECUENTA TC
                ON TC.Id = CUENTAS.TipoCuentaId
                WHERE TC.UsuarioId = @USUARIOID AND Cuentas.id = @Id", new {id,UsuarioId});
        }

        public async Task Actualizar(CuentaCreacionViewModel cuenta)
        { 
            using var connection = new SqlConnection(connectionString);
            await connection.ExecuteAsync(@"UPDATE CUENTAS SET
            Nombre = @NOMBRE,Balance = @BALANCE, Descripcio = @DESCRIPCION, TipoCuentaId = @TIPOCUENTAID
            WHERE Id = @ID;", cuenta);
        }

        public async Task Borrar(int id)
        {
            using var connection = new SqlConnection(connectionString);
            await connection.ExecuteAsync(@"DELETE CUENTAS WHERE ID = @ID", new {id});
        }
    }
}
