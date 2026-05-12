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