using Dapper;
using ManejoPresupuestos.Models;
using Microsoft.Data.SqlClient;

namespace ManejoPresupuestos.Servicios
{
    public class RepositorioTiposCuentas : IRepositorioTiposCuentas
    {
        private readonly string ConnectionString;
        public RepositorioTiposCuentas(IConfiguration configuration)
        {
            ConnectionString = configuration.GetConnectionString("DefaultConnection");
        }

        public async Task Crear(TipoCuentaViewModel TipoCuenta)
        { 
            using var connection = new SqlConnection(ConnectionString);
            var id = await connection.QuerySingleAsync<int>("TiposCuentas_Insertar",new {usuarioId = TipoCuenta.UsuarioId, nombre = TipoCuenta.Nombre }, 
                                                            commandType: System.Data.CommandType.StoredProcedure);
            TipoCuenta.Id = id;
        }

        public async Task<bool> Existe(string nombre, int usuarioid, int id= 0)
        {
            using var connection = new SqlConnection(ConnectionString);
            var existe = await connection.QueryFirstOrDefaultAsync<int>(@"SELECT 1
                                                                        FROM TIPODECUENTA
                                                                        WHERE Nombre = @NOMBRE AND UsuarioId = @USUARIOID AND ID <> @ID;", new {nombre,usuarioid,id});

            return existe == 1; 
        }

        public async Task<IEnumerable<TipoCuentaViewModel>> Obtener(int usuarioid)
        { 
            using var connection = new SqlConnection(ConnectionString);
            return await connection.QueryAsync<TipoCuentaViewModel>(@"SELECT Id, Nombre, Orden FROM TIPODECUENTA
                                                                    WHERE UsuarioId = @USUARIOID 
                                                                    ORDER BY Orden", new {usuarioid});
        }

        public async Task Actualizar(TipoCuentaViewModel tipoCuentaViewModel)
        {
            using var connection = new SqlConnection(ConnectionString);
            await connection.ExecuteAsync(@" UPDATE TIPODECUENTA SET Nombre = @NOMBRE WHERE ID = @ID", tipoCuentaViewModel);

        }

        public async Task<TipoCuentaViewModel> ObtenerId(int id, int usuarioid)
        {
            using var connection = new SqlConnection(ConnectionString);
            return await connection.QueryFirstOrDefaultAsync<TipoCuentaViewModel>(@"SELECT ID, NOMBRE, ORDEN FROM TIPODECUENTA 
                                                                        WHERE ID = @ID AND UsuarioId = @USUARIOID", new {id, usuarioid });
        }

        public async Task Borrar(int id)
        {
            using var connection = new SqlConnection(ConnectionString);
            await connection.ExecuteAsync("DELETE TIPODECUENTA WHERE ID = @iD", new {id});

        
        }

        public async Task Ordenar(IEnumerable<TipoCuentaViewModel> tiposCuentasOrdenados)
        {
            var query = "UPDATE TIPODECUENTA SET ORDEN = @ORDEN WHERE ID = @ID;";
            using var connection = new SqlConnection(ConnectionString);
            await connection.ExecuteAsync(query, tiposCuentasOrdenados);
        }
    }
}
