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
                INSERT INTO SOS.AlertasEvacuacion (IdUsuario, FechaHoraActivacion, IdTipoAlerta, MensajeAlerta, LatitudActivacion, LongitudActivacion, DescripcionUbicacionActivacion, IdEstadoAlerta, UsuarioRegistro, Activo)
                VALUES (@IdUsuario, @FechaHoraActivacion, @IdTipoAlerta, @MensajeAlerta, @LatitudActivacion, @LongitudActivacion, @DescripcionUbicacionActivacion, @IdEstadoAlerta, @UsuarioRegistro, @Activo);
                SELECT CAST(SCOPE_IDENTITY() as int);";

            var id = await cn.QuerySingleAsync<int>(sql, alerta);
            return id;
  
    }

    public async Task<AlertaEvacuacion?> ObtenerUltimaAlertaActivaAsync()
    {
          using var cn = await _db.CreateConnectionAsync();
            var sql = @"
                SELECT TOP 1 al.*,lv.ValorTexto as TipoAlerta, lv1.ValorTexto as EstadoAlerta FROM SOS.AlertasEvacuacion as al inner join
                   EDI.ADM.ListaValores lv  on  al.IdTipoAlerta=lv.ValorNumerico  and lv.IdLista = 1091 inner join
                   EDI.ADM.ListaValores lv1  on  al.IdEstadoAlerta=lv1.ValorNumerico  and lv1.IdLista = 1092          
                WHERE al.IdEstadoAlerta = 1
                ORDER BY al.FechaHoraActivacion DESC";

            return await cn.QueryFirstOrDefaultAsync<AlertaEvacuacion?>(sql);
        
    }
}