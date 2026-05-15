using APIBanca.Data;
using APIBanca.Dtos;
using APIBanca.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Net.Http.Json;

namespace APIBanca.Controllers;

[ApiController]
[Route("api/transferencias")]
public class TransferenciasController(
    AppDbContext context,
    IHttpClientFactory httpClientFactory,
    IConfiguration configuration) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IEnumerable<TransferenciaDto>>> GetAll(CancellationToken cancellationToken)
    {
        return Ok(await context.Transferencias
            .AsNoTracking()
            .Select(transferencia => ToDto(transferencia))
            .ToListAsync(cancellationToken));
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<TransferenciaDto>> GetById(int id, CancellationToken cancellationToken)
    {
        var transferencia = await context.Transferencias
            .AsNoTracking()
            .Where(transferencia => transferencia.IdTransferencia == id)
            .Select(transferencia => ToDto(transferencia))
            .FirstOrDefaultAsync(cancellationToken);

        return transferencia is null ? NotFound() : Ok(transferencia);
    }

    [HttpPost]
    public async Task<ActionResult<TransferenciaDto>> Post(CreateTransferenciaDto dto, CancellationToken cancellationToken)
    {
        if (dto.Monto <= 0)
        {
            return BadRequest("El monto debe ser mayor que cero.");
        }

        var transferencia = new Transferencia
        {
            CuentaOrigenId = dto.CuentaOrigenId,
            CuentaDestinoId = dto.CuentaDestinoId,
            CuentaOrigenExterna = dto.CuentaOrigenExterna,
            NombreCuentaOrigenExterna = dto.NombreCuentaOrigenExterna,
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

        return CreatedAtAction(nameof(GetById), new { id = transferencia.IdTransferencia }, ToDto(transferencia));
    }

    [HttpPost("interna")]
    public async Task<ActionResult<TransferenciaDto>> PostInterna(CreateTransferenciaInternaDto dto, CancellationToken cancellationToken)
    {
        return await ProcesarTransferenciaInternaAsync(dto, cancellationToken);
    }

    [HttpPost("interbancaria/saliente")]
    public async Task<ActionResult<TransferenciaDto>> PostInterbancariaSaliente(CreateTransferenciaInterbancariaSalienteDto dto, CancellationToken cancellationToken)
    {
        return await ProcesarTransferenciaExternaSalienteAsync(dto, cancellationToken);
    }

    [HttpPost("interbancaria/entrante")]
    public async Task<ActionResult<TransferenciaDto>> PostInterbancariaEntrante(CreateTransferenciaInterbancariaEntranteDto dto, CancellationToken cancellationToken)
    {
        return await ProcesarTransferenciaExternaEntranteAsync(dto, cancellationToken);
    }

    [HttpPost("/api/transferencia/validar")]
    public async Task<ActionResult<string>> ValidarTransferencia(ValidarTransferenciaDto dto, CancellationToken cancellationToken)
    {
        if (dto.Monto <= 0 || string.IsNullOrWhiteSpace(dto.CuentaDestino))
        {
            return Ok("RECHAZADO");
        }

        var cuentaDestino = await context.Cuentas
            .AsNoTracking()
            .Include(cuenta => cuenta.Cliente)
            .FirstOrDefaultAsync(cuenta => cuenta.NumeroCuenta == dto.CuentaDestino, cancellationToken);

        if (cuentaDestino is null)
        {
            return Ok("RECHAZADO");
        }

        if (cuentaDestino.Cliente is null)
        {
            return Ok("RECHAZADO");
        }

        if (!string.Equals(cuentaDestino.Estado, "ACTIVA", StringComparison.OrdinalIgnoreCase))
        {
            return Ok("RECHAZADO");
        }

        if (!string.Equals(cuentaDestino.Cliente.Estado, "ACTIVO", StringComparison.OrdinalIgnoreCase))
        {
            return Ok("RECHAZADO");
        }

        return Ok("APROBADO");
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
        transferencia.NombreCuentaOrigenExterna = dto.NombreCuentaOrigenExterna;
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

    private static TransferenciaDto ToDto(Transferencia transferencia) => new()
    {
        IdTransferencia = transferencia.IdTransferencia,
        CuentaOrigenId = transferencia.CuentaOrigenId,
        CuentaDestinoId = transferencia.CuentaDestinoId,
        CuentaOrigenExterna = transferencia.CuentaOrigenExterna,
        NombreCuentaOrigenExterna = transferencia.NombreCuentaOrigenExterna,
        CuentaDestinoExterna = transferencia.CuentaDestinoExterna,
        SwiftOrigen = transferencia.SwiftOrigen,
        SwiftDestino = transferencia.SwiftDestino,
        NombreClienteDestino = null,
        Monto = transferencia.Monto,
        Tipo = transferencia.Tipo,
        Direccion = transferencia.Direccion,
        Estado = transferencia.Estado,
        CreatedAt = transferencia.CreatedAt
    };

    private static TransferenciaDto ToDto(Transferencia transferencia, string? nombreClienteDestino) => new()
    {
        IdTransferencia = transferencia.IdTransferencia,
        CuentaOrigenId = transferencia.CuentaOrigenId,
        CuentaDestinoId = transferencia.CuentaDestinoId,
        CuentaOrigenExterna = transferencia.CuentaOrigenExterna,
        NombreCuentaOrigenExterna = transferencia.NombreCuentaOrigenExterna,
        CuentaDestinoExterna = transferencia.CuentaDestinoExterna,
        SwiftOrigen = transferencia.SwiftOrigen,
        SwiftDestino = transferencia.SwiftDestino,
        NombreClienteDestino = nombreClienteDestino,
        Monto = transferencia.Monto,
        Tipo = transferencia.Tipo,
        Direccion = transferencia.Direccion,
        Estado = transferencia.Estado,
        CreatedAt = transferencia.CreatedAt
    };

    private async Task<ActionResult<TransferenciaDto>> ProcesarTransferenciaInternaAsync(CreateTransferenciaInternaDto dto, CancellationToken cancellationToken)
    {
        if (dto.Monto <= 0)
        {
            return BadRequest("El monto debe ser mayor que cero.");
        }

        if (string.IsNullOrWhiteSpace(dto.NumeroCuentaDestino))
        {
            return BadRequest("El numero de cuenta destino es requerido.");
        }

        var numeroCuentaDestino = dto.NumeroCuentaDestino.Trim();

        await using var transaction = await context.Database.BeginTransactionAsync(cancellationToken);

        var cuentaOrigen = await context.Cuentas
            .FirstOrDefaultAsync(cuenta => cuenta.IdCuenta == dto.CuentaOrigenId, cancellationToken);

        if (cuentaOrigen is null)
        {
            return BadRequest("La cuenta origen no existe.");
        }

        if (string.Equals(cuentaOrigen.NumeroCuenta, numeroCuentaDestino, StringComparison.OrdinalIgnoreCase))
        {
            return BadRequest("La cuenta origen y destino no pueden ser la misma.");
        }

        var cuentaDestino = await context.Cuentas
            .FirstOrDefaultAsync(cuenta => cuenta.NumeroCuenta == numeroCuentaDestino, cancellationToken);

        if (cuentaDestino is null)
        {
            return BadRequest("La cuenta destino no existe.");
        }

        if (!string.Equals(cuentaOrigen.Estado, "ACTIVA", StringComparison.OrdinalIgnoreCase))
        {
            return BadRequest("La cuenta origen no esta activa.");
        }

        if (!string.Equals(cuentaDestino.Estado, "ACTIVA", StringComparison.OrdinalIgnoreCase))
        {
            return BadRequest("La cuenta destino no esta activa.");
        }

        if (cuentaOrigen.Saldo < dto.Monto)
        {
            return BadRequest("La cuenta origen no tiene saldo suficiente.");
        }

        var transferencia = new Transferencia
        {
            CuentaOrigenId = dto.CuentaOrigenId,
            CuentaDestinoId = cuentaDestino.IdCuenta,
            Monto = dto.Monto,
            Tipo = "INTERNA",
            Direccion = "INTERNA",
            Estado = "COMPLETADA"
        };

        cuentaOrigen.Saldo -= dto.Monto;
        cuentaDestino.Saldo += dto.Monto;

        context.Transferencias.Add(transferencia);
        await context.SaveChangesAsync(cancellationToken);

        var referencia = $"TRF-INT-{transferencia.IdTransferencia}";

        context.Movimientos.AddRange(
            new Movimiento
            {
                IdCuenta = cuentaOrigen.IdCuenta,
                Tipo = "DEBITO_TRANSFERENCIA",
                Monto = dto.Monto,
                Descripcion = $"Transferencia interna enviada a {cuentaDestino.NumeroCuenta}",
                Referencia = referencia,
                SaldoResultante = cuentaOrigen.Saldo
            },
            new Movimiento
            {
                IdCuenta = cuentaDestino.IdCuenta,
                Tipo = "CREDITO_TRANSFERENCIA",
                Monto = dto.Monto,
                Descripcion = $"Transferencia interna recibida de {cuentaOrigen.NumeroCuenta}",
                Referencia = referencia,
                SaldoResultante = cuentaDestino.Saldo
            });

        await context.SaveChangesAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);

        return CreatedAtAction(nameof(GetById), new { id = transferencia.IdTransferencia }, ToDto(transferencia));
    }

    private async Task<ActionResult<TransferenciaDto>> ProcesarTransferenciaExternaSalienteAsync(CreateTransferenciaInterbancariaSalienteDto dto, CancellationToken cancellationToken)
    {
        var cuentaOrigen = await context.Cuentas
            .FirstOrDefaultAsync(cuenta => cuenta.IdCuenta == dto.CuentaOrigenId, cancellationToken);

        if (cuentaOrigen is null)
        {
            return BadRequest("La cuenta origen no existe.");
        }

        if (dto.Monto <= 0)
        {
            return BadRequest("El monto debe ser mayor que cero.");
        }

        if (!string.Equals(cuentaOrigen.Estado, "ACTIVA", StringComparison.OrdinalIgnoreCase))
        {
            return BadRequest("La cuenta origen no esta activa.");
        }

        if (cuentaOrigen.Saldo < dto.Monto)
        {
            return BadRequest("La cuenta origen no tiene saldo suficiente.");
        }

        var swiftDestino = dto.SwiftDestino.Trim();
        var bankSwiftMap = configuration.GetSection("BankSwiftMap").Get<Dictionary<string, string>>() ?? [];

        if (!bankSwiftMap.TryGetValue(swiftDestino, out var bankUrl) || string.IsNullOrWhiteSpace(bankUrl))
        {
            return BadRequest("No existe configuracion para el banco destino.");
        }

        await using var transaction = await context.Database.BeginTransactionAsync(cancellationToken);

        var transferencia = new Transferencia
        {
            CuentaOrigenId = dto.CuentaOrigenId,
            CuentaOrigenExterna = cuentaOrigen.NumeroCuenta,
            CuentaDestinoExterna = dto.CuentaDestinoExterna,
            SwiftOrigen = cuentaOrigen.SwiftBanco,
            SwiftDestino = swiftDestino,
            Monto = dto.Monto,
            Tipo = "INTERBANCARIA",
            Direccion = "SALIENTE",
            Estado = "PENDIENTE"
        };

        context.Transferencias.Add(transferencia);
        await context.SaveChangesAsync(cancellationToken);

        var referencia = $"TRF-EXT-{transferencia.IdTransferencia}";

        cuentaOrigen.Saldo -= dto.Monto;
        context.Movimientos.Add(new Movimiento
        {
            IdCuenta = cuentaOrigen.IdCuenta,
            Tipo = "RETENCION_TRANSFERENCIA_EXTERNA",
            Monto = dto.Monto,
            Descripcion = $"Monto retenido para transferencia interbancaria a {dto.CuentaDestinoExterna}",
            Referencia = referencia,
            SaldoResultante = cuentaOrigen.Saldo
        });

        await context.SaveChangesAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);

        try
        {
            var httpClient = httpClientFactory.CreateClient();
            var response = await httpClient.PostAsJsonAsync(
                $"{bankUrl.TrimEnd('/')}/api/transferencia/validar",
                new ValidarTransferenciaDto
                {
                    TransactionId = transferencia.IdTransferencia.ToString(),
                    CuentaDestino = dto.CuentaDestinoExterna,
                    Monto = dto.Monto
                },
                cancellationToken);

            if (response.IsSuccessStatusCode)
            {
                var estadoRespuesta = await response.Content.ReadAsStringAsync(cancellationToken);
                var estadoNormalizado = estadoRespuesta.Trim().Trim('"').ToUpperInvariant();

                if (estadoNormalizado == "APROBADO")
                {
                    transferencia.Estado = "APROBADA";
                }
                else if (estadoNormalizado == "RECHAZADO")
                {
                    RevertirTransferenciaExterna(cuentaOrigen, transferencia, dto.Monto, dto.CuentaDestinoExterna, referencia);
                }
                else
                {
                    RevertirTransferenciaExterna(cuentaOrigen, transferencia, dto.Monto, dto.CuentaDestinoExterna, referencia);
                }
            }
            else
            {
                RevertirTransferenciaExterna(cuentaOrigen, transferencia, dto.Monto, dto.CuentaDestinoExterna, referencia);
            }
        }
        catch
        {
            RevertirTransferenciaExterna(cuentaOrigen, transferencia, dto.Monto, dto.CuentaDestinoExterna, referencia);
        }

        await context.SaveChangesAsync(cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = transferencia.IdTransferencia }, ToDto(transferencia));
    }

    private void RevertirTransferenciaExterna(Cuenta cuentaOrigen, Transferencia transferencia, decimal monto, string cuentaDestinoExterna, string referencia)
    {
        if (transferencia.Estado == "RECHAZADA")
        {
            return;
        }

        cuentaOrigen.Saldo += monto;
        transferencia.Estado = "RECHAZADA";

        context.Movimientos.Add(new Movimiento
        {
            IdCuenta = cuentaOrigen.IdCuenta,
            Tipo = "REVERSO_TRANSFERENCIA_EXTERNA",
            Monto = monto,
            Descripcion = $"Reverso de transferencia interbancaria hacia {cuentaDestinoExterna}",
            Referencia = referencia,
            SaldoResultante = cuentaOrigen.Saldo
        });
    }

    private async Task<ActionResult<TransferenciaDto>> ProcesarTransferenciaExternaEntranteAsync(CreateTransferenciaInterbancariaEntranteDto dto, CancellationToken cancellationToken)
    {
        if (dto.Monto <= 0)
        {
            return BadRequest("El monto debe ser mayor que cero.");
        }

        if (string.IsNullOrWhiteSpace(dto.NumeroCuentaDestino))
        {
            return BadRequest("El numero de cuenta destino es requerido.");
        }

        if (string.IsNullOrWhiteSpace(dto.NombreCuentaOrigenExterna))
        {
            return BadRequest("El nombre de la cuenta origen es requerido.");
        }

        await using var transaction = await context.Database.BeginTransactionAsync(cancellationToken);

        var cuentaDestino = await context.Cuentas
            .Include(cuenta => cuenta.Cliente)
            .FirstOrDefaultAsync(cuenta => cuenta.NumeroCuenta == dto.NumeroCuentaDestino.Trim(), cancellationToken);

        if (cuentaDestino is null)
        {
            return BadRequest("La cuenta destino no existe.");
        }

        if (cuentaDestino.Cliente is null)
        {
            return BadRequest("La cuenta destino no tiene cliente asociado.");
        }

        if (!string.Equals(cuentaDestino.Estado, "ACTIVA", StringComparison.OrdinalIgnoreCase))
        {
            return BadRequest("La cuenta destino no esta activa.");
        }

        if (!string.Equals(cuentaDestino.Cliente.Estado, "ACTIVO", StringComparison.OrdinalIgnoreCase))
        {
            return BadRequest("El cliente de la cuenta destino no esta activo.");
        }

        cuentaDestino.Saldo += dto.Monto;

        var transferencia = new Transferencia
        {
            CuentaDestinoId = cuentaDestino.IdCuenta,
            CuentaOrigenExterna = dto.CuentaOrigenExterna,
            NombreCuentaOrigenExterna = dto.NombreCuentaOrigenExterna.Trim(),
            CuentaDestinoExterna = cuentaDestino.NumeroCuenta,
            SwiftOrigen = dto.SwiftOrigen,
            SwiftDestino = cuentaDestino.SwiftBanco,
            Monto = dto.Monto,
            Tipo = "INTERBANCARIA",
            Direccion = "ENTRANTE",
            Estado = "APROBADA"
        };

        context.Transferencias.Add(transferencia);
        await context.SaveChangesAsync(cancellationToken);

        context.Movimientos.Add(new Movimiento
        {
            IdCuenta = cuentaDestino.IdCuenta,
            Tipo = "CREDITO_TRANSFERENCIA_EXTERNA",
            Monto = dto.Monto,
            Descripcion = $"Transferencia interbancaria recibida de {dto.NombreCuentaOrigenExterna.Trim()} ({dto.CuentaOrigenExterna})",
            Referencia = $"TRF-EXT-IN-{transferencia.IdTransferencia}",
            SaldoResultante = cuentaDestino.Saldo
        });

        await context.SaveChangesAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);

        return CreatedAtAction(
            nameof(GetById),
            new { id = transferencia.IdTransferencia },
            ToDto(transferencia, cuentaDestino.Cliente.Nombre));
    }
}