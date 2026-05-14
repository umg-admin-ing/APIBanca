using APIBanca.Data;
using APIBanca.Dtos;
using APIBanca.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace APIBanca.Controllers;

[ApiController]
[Route("api/solicitudes-credito")]
public class SolicitudesCreditoController(AppDbContext context) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IEnumerable<SolicitudCreditoDto>>> GetAll(CancellationToken cancellationToken)
    {
        return Ok(await context.SolicitudesCredito
            .AsNoTracking()
            .Select(solicitudCredito => ToDto(solicitudCredito))
            .ToListAsync(cancellationToken));
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<SolicitudCreditoDto>> GetById(int id, CancellationToken cancellationToken)
    {
        var solicitudCredito = await context.SolicitudesCredito
            .AsNoTracking()
            .Where(solicitudCredito => solicitudCredito.IdSolicitud == id)
            .Select(solicitudCredito => ToDto(solicitudCredito))
            .FirstOrDefaultAsync(cancellationToken);

        return solicitudCredito is null ? NotFound() : Ok(solicitudCredito);
    }

    [HttpPost]
    public async Task<ActionResult<SolicitudCreditoDto>> Post(CreateSolicitudCreditoDto dto, CancellationToken cancellationToken)
    {
        var solicitudCredito = new SolicitudCredito
        {
            IdCliente = dto.IdCliente,
            MontoSolicitado = dto.MontoSolicitado,
            PlazoMeses = dto.PlazoMeses,
            Estado = dto.Estado,
            Score = dto.Score
        };

        context.SolicitudesCredito.Add(solicitudCredito);
        await context.SaveChangesAsync(cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = solicitudCredito.IdSolicitud }, ToDto(solicitudCredito));
    }

    // POST api/solicitudes-credito/{id}/aprobar
    [HttpPost("{id:int}/aprobar")]
    public async Task<ActionResult<CreditoDto>> Aprobar(
        int id,
        AprobarSolicitudDto dto,
        CancellationToken cancellationToken)
    {
        var solicitud = await context.SolicitudesCredito
            .FindAsync([id], cancellationToken);

        if (solicitud is null)
            return NotFound("Solicitud no encontrada.");

        if (solicitud.Estado != "PENDIENTE")
            return Conflict($"La solicitud ya fue procesada (estado: {solicitud.Estado}).");

        var cuentaExiste = await context.Cuentas
            .AnyAsync(c => c.IdCuenta == dto.IdCuenta, cancellationToken);

        if (!cuentaExiste)
            return BadRequest("La cuenta destino no existe.");

        // Cálculo cuota nivelada (sistema francés)
        // i = tasa mensual, n = plazo en meses
        decimal tasaMensual = dto.TasaInteresAnual / 100m / 12m;
        decimal n = solicitud.PlazoMeses;
        decimal monto = solicitud.MontoSolicitado;

        // Fórmula: C = P * [i(1+i)^n] / [(1+i)^n - 1]
        decimal factor = (decimal)Math.Pow((double)(1 + tasaMensual), (double)n);
        decimal cuotaMensual = monto * (tasaMensual * factor) / (factor - 1);
        cuotaMensual = Math.Round(cuotaMensual, 2);

        // Actualizar solicitud
        solicitud.Estado = "APROBADA";

        // Crear crédito
        var credito = new Credito
        {
            IdSolicitud = solicitud.IdSolicitud,
            IdCuenta = dto.IdCuenta,
            MontoOriginal = monto,
            SaldoPendiente = monto,
            TasaInteres = dto.TasaInteresAnual,
            CuotaMensual = cuotaMensual,
            Estado = "ACTIVO",
            FechaInicio = DateTime.UtcNow
        };

        context.Creditos.Add(credito);
        await context.SaveChangesAsync(cancellationToken);

        // Log
        Console.WriteLine(
            $"[CREDITO APROBADO] SolicitudId={id} | CreditoId={credito.IdCredito} | " +
            $"Monto={monto} | Cuota={cuotaMensual} | Tasa={dto.TasaInteresAnual}% | " +
            $"Cuenta={dto.IdCuenta} | At={DateTime.UtcNow:O}");

        return Ok(CreditosController.ToDto(credito));

    }

    // POST api/solicitudes-credito/{id}/denegar
    [HttpPost("{id:int}/denegar")]
    public async Task<IActionResult> Denegar(
        int id,
        DenegarSolicitudDto dto,
        CancellationToken cancellationToken)
    {
        var solicitud = await context.SolicitudesCredito
            .FindAsync([id], cancellationToken);

        if (solicitud is null)
            return NotFound("Solicitud no encontrada.");

        if (solicitud.Estado != "PENDIENTE")
            return Conflict($"La solicitud ya fue procesada (estado: {solicitud.Estado}).");

        solicitud.Estado = "DENEGADA";
        await context.SaveChangesAsync(cancellationToken);

        // Log
        Console.WriteLine(
            $"[CREDITO DENEGADO] SolicitudId={id} | " +
            $"Motivo=\"{dto.Motivo}\" | At={DateTime.UtcNow:O}");

        return Ok(new { mensaje = "Solicitud denegada.", motivo = dto.Motivo });
    }




    [HttpPut("{id:int}")]
    public async Task<IActionResult> Put(int id, UpdateSolicitudCreditoDto dto, CancellationToken cancellationToken)
    {
        var solicitudCredito = await context.SolicitudesCredito.FindAsync([id], cancellationToken);

        if (solicitudCredito is null)
        {
            return NotFound();
        }

        solicitudCredito.IdCliente = dto.IdCliente;
        solicitudCredito.MontoSolicitado = dto.MontoSolicitado;
        solicitudCredito.PlazoMeses = dto.PlazoMeses;
        solicitudCredito.Estado = dto.Estado;
        solicitudCredito.Score = dto.Score;

        await context.SaveChangesAsync(cancellationToken);
        return NoContent();
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken)
    {
        var solicitudCredito = await context.SolicitudesCredito.FindAsync([id], cancellationToken);
        if (solicitudCredito is null)
        {
            return NotFound();
        }

        context.SolicitudesCredito.Remove(solicitudCredito);
        await context.SaveChangesAsync(cancellationToken);
        return NoContent();
    }

    private static SolicitudCreditoDto ToDto(SolicitudCredito solicitudCredito) => new()
    {
        IdSolicitud = solicitudCredito.IdSolicitud,
        IdCliente = solicitudCredito.IdCliente,
        MontoSolicitado = solicitudCredito.MontoSolicitado,
        PlazoMeses = solicitudCredito.PlazoMeses,
        Estado = solicitudCredito.Estado,
        Score = solicitudCredito.Score,
        CreatedAt = solicitudCredito.CreatedAt
    };
}