using Dapper;
using ManejoPresupuestos.Models;
using Microsoft.Data.SqlClient;

namespace ManejoPresupuestos.Servicios
{
    public class RepositorioUsuarios : IRepositorioUsuarios
    {
        private readonly string ConnectionString;
        public RepositorioUsuarios(IConfiguration configuration)
        {
            ConnectionString = configuration.GetConnectionString("DefaultConnection");
        }

        public async Task<int> CrearUsuario(Usuario usuario)
        {
            using var connection = new SqlConnection(ConnectionString);
            var Usuarioid = await connection.QuerySingleAsync<int>(@"INSERT INTO USUARIO (Email, EmailNormalizado,
                                                            PasswordHash) VALUES
                                                            (@Email,@EMAILNORMALIZADO,@PasswordHash);
                                                            SELECT SCOPE_IDENTITY();", usuario);

            await connection.ExecuteAsync("CREARDATOSUSUARIONUEVO", new {Usuarioid }, commandType:System.Data.CommandType.StoredProcedure);
            return Usuarioid;
        }

        public async Task<Usuario> BuscarUsuarioPorEmail(string emailNormalizado)
        {
            using var connection = new SqlConnection(ConnectionString);
            return await connection.QuerySingleOrDefaultAsync<Usuario>(@"SELECT * FROM USUARIO
                            WHERE EmailNormalizado = @EMAILNORMALIZADO", new {emailNormalizado});
        }

        
    }
}
