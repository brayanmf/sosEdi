using Dapper;
using System.Data;
using Microsoft.Data.SqlClient;
using SOS.Models;
using SOS.Data;

public class ConfirmacionesRepository
{
    private readonly IConexion _db;
    public ConfirmacionesRepository(IConexion db)
    {
        _db = db;
    
    }

    public async Task<int> InsertarConfirmacionAsync(ConfirmacionSeguridad confirmacion)
    {
        using (IDbConnection db = await _db.CreateConnectionAsync())
        {
            var sql = @"
                INSERT INTO SOS.ConfirmacionesSeguridad (IdUsuario, AlertaEvacuacionId, FechaHoraConfirmacion, Latitud, Longitud, EstadoReportado, Comentario, UsuarioRegistro, FechaRegistro, Activo)
                VALUES (@IdUsuario, @AlertaEvacuacionId, @FechaHoraConfirmacion, @Latitud, @Longitud, @EstadoReportado, @Comentario, @UsuarioRegistro, @FechaRegistro, @Activo);
                SELECT CAST(SCOPE_IDENTITY() as int);";

            var id = await db.QuerySingleAsync<int>(sql, confirmacion);
            return id;
        }
    }
}