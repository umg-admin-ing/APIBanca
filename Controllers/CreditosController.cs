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
    public async Task<ActionResult<IEnumerable<Credito>>> GetAll(CancellationToken cancellationToken)
    {
        return Ok(await context.Creditos.AsNoTracking().ToListAsync(cancellationToken));
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<Credito>> GetById(int id, CancellationToken cancellationToken)
    {
        var credito = await context.Creditos.FindAsync([id], cancellationToken);
        return credito is null ? NotFound() : Ok(credito);
    }

    [HttpPost]
    public async Task<ActionResult<Credito>> Post(CreateCreditoDto dto, CancellationToken cancellationToken)
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
        return CreatedAtAction(nameof(GetById), new { id = credito.IdCredito }, credito);
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
}