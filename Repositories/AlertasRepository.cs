using Dapper;
using System.Data;
using Microsoft.Data.SqlClient;
using SOS.Models;
using SOS.Data;

public class AlertasRepository
{ 
    private readonly IConexion _db;
    public AlertasRepository(IConexion db)
    {
        _db = db;
    }

    public async Task<int> InsertarAlertaAsync(AlertaEvacuacion alerta)
    {
 

       using var cn = await _db.CreateConnectionAsync();
            var sql = @"
                INSERT INTO SOS.AlertasEvacuacion (IdUsuario, FechaHoraActivacion, TipoAlerta, MensajeAlerta, LatitudActivacion, LongitudActivacion, DescripcionUbicacionActivacion, EstadoAlerta, UsuarioRegistro, FechaRegistro, Activo)
                VALUES (@IdUsuario, @FechaHoraActivacion, @TipoAlerta, @MensajeAlerta, @LatitudActivacion, @LongitudActivacion, @DescripcionUbicacionActivacion, @EstadoAlerta, @UsuarioRegistro, @FechaRegistro, @Activo);
                SELECT CAST(SCOPE_IDENTITY() as int);";

            var id = await cn.QuerySingleAsync<int>(sql, alerta);
            return id;
  
    }

    public async Task<AlertaEvacuacion?> ObtenerUltimaAlertaActivaAsync()
    {
          using var cn = await _db.CreateConnectionAsync();
            var sql = @"
                SELECT TOP 1 * FROM SOS.AlertasEvacuacion
                WHERE EstadoAlerta = 'Activa'
                ORDER BY FechaHoraActivacion DESC;";

            return await cn.QueryFirstOrDefaultAsync<AlertaEvacuacion>(sql);
        
    }
}