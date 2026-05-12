using APIBanca.Data;
using APIBanca.Dtos;
using APIBanca.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace APIBanca.Controllers;

[ApiController]
[Route("api/creditos")]
public class CreditosController(AppDbContext context) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IEnumerable<CreditoDto>>> GetAll(CancellationToken cancellationToken)
    {
        return Ok(await context.Creditos
            .AsNoTracking()
            .Select(credito => ToDto(credito))
            .ToListAsync(cancellationToken));
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<CreditoDto>> GetById(int id, CancellationToken cancellationToken)
    {
        var credito = await context.Creditos
            .AsNoTracking()
            .Where(credito => credito.IdCredito == id)
            .Select(credito => ToDto(credito))
            .FirstOrDefaultAsync(cancellationToken);

        return credito is null ? NotFound() : Ok(credito);
    }

    [HttpPost]
    public async Task<ActionResult<CreditoDto>> Post(CreateCreditoDto dto, CancellationToken cancellationToken)
    {
        var credito = new Credito
        {
            IdSolicitud = dto.IdSolicitud,
            IdCuenta = dto.IdCuenta,
            MontoOriginal = dto.MontoOriginal,
            SaldoPendiente = dto.SaldoPendiente ?? dto.MontoOriginal,
            TasaInteres = dto.TasaInteres,
            CuotaMensual = dto.CuotaMensual,
            Estado = dto.Estado,
            FechaInicio = dto.FechaInicio ?? DateTime.UtcNow
        };

        context.Creditos.Add(credito);
        await context.SaveChangesAsync(cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = credito.IdCredito }, ToDto(credito));
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Put(int id, UpdateCreditoDto dto, CancellationToken cancellationToken)
    {
        var credito = await context.Creditos.FindAsync([id], cancellationToken);

        if (credito is null)
        {
            return NotFound();
        }

        credito.IdSolicitud = dto.IdSolicitud;
        credito.IdCuenta = dto.IdCuenta;
        credito.MontoOriginal = dto.MontoOriginal;
        credito.SaldoPendiente = dto.SaldoPendiente ?? dto.MontoOriginal;
        credito.TasaInteres = dto.TasaInteres;
        credito.CuotaMensual = dto.CuotaMensual;
        credito.Estado = dto.Estado;
        credito.FechaInicio = dto.FechaInicio ?? credito.FechaInicio;

        await context.SaveChangesAsync(cancellationToken);
        return NoContent();
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken)
    {
        var credito = await context.Creditos.FindAsync([id], cancellationToken);
        if (credito is null)
        {
            return NotFound();
        }

        context.Creditos.Remove(credito);
        await context.SaveChangesAsync(cancellationToken);
        return NoContent();
    }

    private static CreditoDto ToDto(Credito credito) => new()
    {
        IdCredito = credito.IdCredito,
        IdSolicitud = credito.IdSolicitud,
        IdCuenta = credito.IdCuenta,
        MontoOriginal = credito.MontoOriginal,
        SaldoPendiente = credito.SaldoPendiente,
        TasaInteres = credito.TasaInteres,
        CuotaMensual = credito.CuotaMensual,
        Estado = credito.Estado,
        FechaInicio = credito.FechaInicio
    };
}