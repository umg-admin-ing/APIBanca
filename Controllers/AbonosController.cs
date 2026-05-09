using APIBanca.Data;
using APIBanca.Dtos;
using APIBanca.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace APIBanca.Controllers;

[ApiController]
[Route("api/abonos")]
public class AbonosController(AppDbContext context) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IEnumerable<Abono>>> GetAll(CancellationToken cancellationToken)
    {
        return Ok(await context.Abonos.AsNoTracking().ToListAsync(cancellationToken));
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<Abono>> GetById(int id, CancellationToken cancellationToken)
    {
        var abono = await context.Abonos.FindAsync([id], cancellationToken);
        return abono is null ? NotFound() : Ok(abono);
    }

    [HttpPost]
    public async Task<ActionResult<Abono>> Post(CreateAbonoDto dto, CancellationToken cancellationToken)
    {
        var abono = new Abono
        {
            IdCredito = dto.IdCredito,
            Monto = dto.Monto,
            SaldoAnterior = dto.SaldoAnterior,
            SaldoNuevo = dto.SaldoNuevo ?? dto.SaldoAnterior - dto.Monto
        };

        context.Abonos.Add(abono);
        await context.SaveChangesAsync(cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = abono.IdAbono }, abono);
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Put(int id, UpdateAbonoDto dto, CancellationToken cancellationToken)
    {
        var abono = await context.Abonos.FindAsync([id], cancellationToken);

        if (abono is null)
        {
            return NotFound();
        }

        abono.IdCredito = dto.IdCredito;
        abono.Monto = dto.Monto;
        abono.SaldoAnterior = dto.SaldoAnterior;
        abono.SaldoNuevo = dto.SaldoNuevo ?? dto.SaldoAnterior - dto.Monto;

        await context.SaveChangesAsync(cancellationToken);
        return NoContent();
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken)
    {
        var abono = await context.Abonos.FindAsync([id], cancellationToken);
        if (abono is null)
        {
            return NotFound();
        }

        context.Abonos.Remove(abono);
        await context.SaveChangesAsync(cancellationToken);
        return NoContent();
    }
}