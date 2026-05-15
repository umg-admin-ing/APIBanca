using APIBanca.Data;
using APIBanca.Dtos;
using APIBanca.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace APIBanca.Controllers;

[ApiController]
[Route("api/cuentas")]
public class CuentasController(AppDbContext context) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IEnumerable<CuentaDto>>> GetAll(CancellationToken cancellationToken)
    {
        return Ok(await context.Cuentas
            .AsNoTracking()
            .Select(cuenta => ToDto(cuenta))
            .ToListAsync(cancellationToken));
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<CuentaDto>> GetById(int id, CancellationToken cancellationToken)
    {
        var cuenta = await context.Cuentas
            .AsNoTracking()
            .Where(cuenta => cuenta.IdCuenta == id)
            .Select(cuenta => ToDto(cuenta))
            .FirstOrDefaultAsync(cancellationToken);

        return cuenta is null ? NotFound() : Ok(cuenta);
    }

    [HttpGet("numero/{numeroCuenta}")]
    public async Task<ActionResult<CuentaVerificacionDto>> GetByNumeroCuenta(string numeroCuenta, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(numeroCuenta))
        {
            return BadRequest("Numero de cuenta invalido.");
        }

        var numeroCuentaNormalizado = numeroCuenta.Trim();

        var cuenta = await context.Cuentas
            .AsNoTracking()
            .Include(cuenta => cuenta.Cliente)
            .Where(cuenta => cuenta.NumeroCuenta == numeroCuentaNormalizado)
            .Select(cuenta => ToVerificacionDto(cuenta))
            .FirstOrDefaultAsync(cancellationToken);

        return cuenta is null ? NotFound() : Ok(cuenta);
    }

    [HttpGet("{id:int}/monto-flotante")]
    public async Task<ActionResult<CuentaMontoFlotanteDto>> GetMontoFlotante(int id, CancellationToken cancellationToken)
    {
        var cuenta = await context.Cuentas
            .AsNoTracking()
            .Where(cuenta => cuenta.IdCuenta == id)
            .Select(cuenta => new
            {
                cuenta.IdCuenta,
                cuenta.NumeroCuenta
            })
            .FirstOrDefaultAsync(cancellationToken);

        if (cuenta is null)
        {
            return NotFound();
        }

        var transferenciasPendientes = await context.Transferencias
            .AsNoTracking()
            .Where(transferencia => transferencia.CuentaOrigenId == id
                && transferencia.Tipo == "INTERBANCARIA"
                && transferencia.Direccion == "SALIENTE"
                && transferencia.Estado == "PENDIENTE")
            .OrderByDescending(transferencia => transferencia.CreatedAt)
            .Select(transferencia => new MontoFlotanteTransferenciaDto
            {
                IdTransferencia = transferencia.IdTransferencia,
                CuentaDestinoExterna = transferencia.CuentaDestinoExterna ?? string.Empty,
                SwiftDestino = transferencia.SwiftDestino ?? string.Empty,
                Monto = transferencia.Monto,
                Estado = transferencia.Estado,
                CreatedAt = transferencia.CreatedAt
            })
            .ToListAsync(cancellationToken);

        var response = new CuentaMontoFlotanteDto
        {
            IdCuenta = cuenta.IdCuenta,
            NumeroCuenta = cuenta.NumeroCuenta,
            MontoFlotante = transferenciasPendientes.Sum(transferencia => transferencia.Monto),
            CantidadTransferenciasPendientes = transferenciasPendientes.Count,
            Transferencias = transferenciasPendientes
        };

        return Ok(response);
    }

    [HttpPost]
    public async Task<ActionResult<CuentaDto>> Post(CreateCuentaDto dto, CancellationToken cancellationToken)
    {
        var cuenta = new Cuenta
        {
            NumeroCuenta = dto.NumeroCuenta,
            IdCliente = dto.IdCliente,
            Saldo = dto.Saldo,
            SwiftBanco = dto.SwiftBanco,
            Tipo = dto.Tipo,
            Estado = dto.Estado
        };

        context.Cuentas.Add(cuenta);
        await context.SaveChangesAsync(cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = cuenta.IdCuenta }, ToDto(cuenta));
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Put(int id, UpdateCuentaDto dto, CancellationToken cancellationToken)
    {
        var cuenta = await context.Cuentas.FindAsync([id], cancellationToken);

        if (cuenta is null)
        {
            return NotFound();
        }

        cuenta.NumeroCuenta = dto.NumeroCuenta;
        cuenta.IdCliente = dto.IdCliente;
        cuenta.Saldo = dto.Saldo;
        cuenta.SwiftBanco = dto.SwiftBanco;
        cuenta.Tipo = dto.Tipo;
        cuenta.Estado = dto.Estado;

        await context.SaveChangesAsync(cancellationToken);
        return NoContent();
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken)
    {
        var cuenta = await context.Cuentas.FindAsync([id], cancellationToken);
        if (cuenta is null)
        {
            return NotFound();
        }

        context.Cuentas.Remove(cuenta);
        await context.SaveChangesAsync(cancellationToken);
        return NoContent();
    }

    private static CuentaDto ToDto(Cuenta cuenta) => new()
    {
        IdCuenta = cuenta.IdCuenta,
        NumeroCuenta = cuenta.NumeroCuenta,
        IdCliente = cuenta.IdCliente,
        Saldo = cuenta.Saldo,
        SwiftBanco = cuenta.SwiftBanco,
        Tipo = cuenta.Tipo,
        Estado = cuenta.Estado,
        CreatedAt = cuenta.CreatedAt
    };

    private static CuentaVerificacionDto ToVerificacionDto(Models.Cuenta cuenta) => new()
    {
        NumeroCuenta = cuenta.NumeroCuenta,
        NombreCliente = cuenta.Cliente.Nombre
    };
}