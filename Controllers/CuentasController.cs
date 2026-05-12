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
}