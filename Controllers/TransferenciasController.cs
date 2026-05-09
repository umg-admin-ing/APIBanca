using APIBanca.Data;
using APIBanca.Dtos;
using APIBanca.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace APIBanca.Controllers;

[ApiController]
[Route("api/transferencias")]
public class TransferenciasController(AppDbContext context) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IEnumerable<Transferencia>>> GetAll(CancellationToken cancellationToken)
    {
        return Ok(await context.Transferencias.AsNoTracking().ToListAsync(cancellationToken));
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<Transferencia>> GetById(int id, CancellationToken cancellationToken)
    {
        var transferencia = await context.Transferencias.FindAsync([id], cancellationToken);
        return transferencia is null ? NotFound() : Ok(transferencia);
    }

    [HttpPost]
    public async Task<ActionResult<Transferencia>> Post(CreateTransferenciaDto dto, CancellationToken cancellationToken)
    {
        var transferencia = new Transferencia
        {
            CuentaOrigenId = dto.CuentaOrigenId,
            CuentaDestinoId = dto.CuentaDestinoId,
            CuentaOrigenExterna = dto.CuentaOrigenExterna,
            CuentaDestinoExterna = dto.CuentaDestinoExterna,
            SwiftOrigen = dto.SwiftOrigen,
            SwiftDestino = dto.SwiftDestino,
            Monto = dto.Monto,
            Tipo = dto.Tipo,
            Direccion = dto.Direccion,
            Estado = dto.Estado
        };

        context.Transferencias.Add(transferencia);
        await context.SaveChangesAsync(cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = transferencia.IdTransferencia }, transferencia);
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Put(int id, UpdateTransferenciaDto dto, CancellationToken cancellationToken)
    {
        var transferencia = await context.Transferencias.FindAsync([id], cancellationToken);

        if (transferencia is null)
        {
            return NotFound();
        }

        transferencia.CuentaOrigenId = dto.CuentaOrigenId;
        transferencia.CuentaDestinoId = dto.CuentaDestinoId;
        transferencia.CuentaOrigenExterna = dto.CuentaOrigenExterna;
        transferencia.CuentaDestinoExterna = dto.CuentaDestinoExterna;
        transferencia.SwiftOrigen = dto.SwiftOrigen;
        transferencia.SwiftDestino = dto.SwiftDestino;
        transferencia.Monto = dto.Monto;
        transferencia.Tipo = dto.Tipo;
        transferencia.Direccion = dto.Direccion;
        transferencia.Estado = dto.Estado;

        await context.SaveChangesAsync(cancellationToken);
        return NoContent();
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken)
    {
        var transferencia = await context.Transferencias.FindAsync([id], cancellationToken);
        if (transferencia is null)
        {
            return NotFound();
        }

        context.Transferencias.Remove(transferencia);
        await context.SaveChangesAsync(cancellationToken);
        return NoContent();
    }
}