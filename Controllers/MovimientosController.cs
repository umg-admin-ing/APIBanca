using APIBanca.Data;
using APIBanca.Dtos;
using APIBanca.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace APIBanca.Controllers;

[ApiController]
[Route("api/movimientos")]
public class MovimientosController(AppDbContext context) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IEnumerable<Movimiento>>> GetAll(CancellationToken cancellationToken)
    {
        return Ok(await context.Movimientos.AsNoTracking().ToListAsync(cancellationToken));
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<Movimiento>> GetById(int id, CancellationToken cancellationToken)
    {
        var movimiento = await context.Movimientos.FindAsync([id], cancellationToken);
        return movimiento is null ? NotFound() : Ok(movimiento);
    }

    [HttpPost]
    public async Task<ActionResult<Movimiento>> Post(CreateMovimientoDto dto, CancellationToken cancellationToken)
    {
        var movimiento = new Movimiento
        {
            IdCuenta = dto.IdCuenta,
            Tipo = dto.Tipo,
            Monto = dto.Monto,
            Descripcion = dto.Descripcion,
            Referencia = dto.Referencia,
            SaldoResultante = dto.SaldoResultante
        };

        context.Movimientos.Add(movimiento);
        await context.SaveChangesAsync(cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = movimiento.IdMovimiento }, movimiento);
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Put(int id, UpdateMovimientoDto dto, CancellationToken cancellationToken)
    {
        var movimiento = await context.Movimientos.FindAsync([id], cancellationToken);

        if (movimiento is null)
        {
            return NotFound();
        }

        movimiento.IdCuenta = dto.IdCuenta;
        movimiento.Tipo = dto.Tipo;
        movimiento.Monto = dto.Monto;
        movimiento.Descripcion = dto.Descripcion;
        movimiento.Referencia = dto.Referencia;
        movimiento.SaldoResultante = dto.SaldoResultante;

        await context.SaveChangesAsync(cancellationToken);
        return NoContent();
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken)
    {
        var movimiento = await context.Movimientos.FindAsync([id], cancellationToken);
        if (movimiento is null)
        {
            return NotFound();
        }

        context.Movimientos.Remove(movimiento);
        await context.SaveChangesAsync(cancellationToken);
        return NoContent();
    }
}