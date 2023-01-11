using Dapper;
using ManejoPresupuestos.Models;
using Microsoft.Data.SqlClient;

namespace ManejoPresupuestos.Servicios
{
    public class RepositorioCategorias: IRepositorioCategorias
    {
        private readonly string ConnectionString;
        public RepositorioCategorias(IConfiguration configuration)
        {
            ConnectionString = configuration.GetConnectionString("DefaultConnection");
        }

        public async Task Crear(CategoriaViewModel categoria)
        { 
            using var connection = new SqlConnection(ConnectionString);
            var id = await connection.QuerySingleAsync<int>(@"INSERT INTO CATEGORIA  (CATEGORIA, TipoOperacionId, UsuarioId) VALUES 
                                                            (@CATEGORIA,@TIPOOPERACIONID,@UsuarioID);
                                                            SELECT SCOPE_IDENTITY();", categoria);

            categoria.Id = id;
        }

        public async Task<IEnumerable<CategoriaViewModel>> Obtener(int UsuarioId, PaginacionViewModel paginacion)
        {
            using var connection = new SqlConnection(ConnectionString);
            return await connection.QueryAsync<CategoriaViewModel>(@$"SELECT * FROM CATEGORIA 
                                                                    WHERE USUARIOID = @USUARIOID
                                                                    ORDER BY CATEGORIA
                                                                    OFFSET {paginacion.RecordsASaltar} ROWS FETCH NEXT 
                                                                    {paginacion.RecordsPorPagina} ROWS ONLY"
                                                                    , new {UsuarioId});
        }

        public async Task<int> Contar(int UsuarioId)
        {
            using var connection = new SqlConnection(ConnectionString);
            return await connection.ExecuteScalarAsync<int>(@"SELECT COUNT(*) FROM CATEGORIA WHERE UsuarioId = @USUARIOID", new {UsuarioId});
        }
        
        public async Task<CategoriaViewModel> ObtenerPorId(int id, int usuarioId)
        {
            using var connection = new SqlConnection(ConnectionString);
            return await connection.QueryFirstOrDefaultAsync<CategoriaViewModel>(@"SELECT * FROM CATEGORIA WHERE ID = @ID AND USUARIOID= @USUARIOID", new { id, usuarioId });
        }

        public async Task Actualizar(CategoriaViewModel categoria)
        {
            using var connection = new SqlConnection(ConnectionString);
            await connection.ExecuteAsync(@"UPDATE CATEGORIA SET
                                                Categoria = @CATEGORIA,
                                                TipoOperacionId = @TIPOOPERACIONID
                                                WHERE Id = @ID", categoria);
        }

        public async Task Borrar(int id)
        {
            using var connection = new SqlConnection(ConnectionString);
            await connection.ExecuteAsync(@"DELETE CATEGORIA WHERE ID = @ID", new {id});
        }

        public async Task<IEnumerable<CategoriaViewModel>> Obtener(int UsuarioId, TipoOperacion tipoOperacionId)
        {
            using var connection = new SqlConnection(ConnectionString);
            return await connection.QueryAsync<CategoriaViewModel>(@"SELECT * FROM CATEGORIA 
            WHERE USUARIOID = @USUARIOID AND 
            TipoOperacionId = @TIPOOPERACIONID", new { UsuarioId, tipoOperacionId });
        }


    }
}
