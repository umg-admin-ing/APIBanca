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
    private const string SwiftBancoLocal = "GTB666";
    private const string EstadoCuentaInicial = "ACTIVA";
    private const string EstadoCuentaInactiva = "INACTIVA";
    private const string EstadoClienteActivo = "ACTIVO";
    private const string EstadoUsuarioActivo = "ACTIVO";
    private const string RolUsuarioCliente = "CLIENTE";

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
            NumeroCuenta = await GenerateNumeroCuentaAsync(cancellationToken),
            IdCliente = dto.IdCliente,
            Saldo = dto.Saldo,
            SwiftBanco = SwiftBancoLocal,
            Tipo = dto.Tipo,
            Estado = EstadoCuentaInicial
        };

        context.Cuentas.Add(cuenta);
        await context.SaveChangesAsync(cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = cuenta.IdCuenta }, ToDto(cuenta));
    }

    [HttpPost("apertura")]
    public async Task<ActionResult<AperturaCuentaResponseDto>> AperturarCuenta(AperturaCuentaDto dto, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(dto.Cliente.Dpi))
        {
            return BadRequest("El DPI del cliente es requerido.");
        }

        if (string.IsNullOrWhiteSpace(dto.Tipo))
        {
            return BadRequest("El tipo de cuenta es requerido.");
        }

        var dpiNormalizado = dto.Cliente.Dpi.Trim();
        var emailNormalizado = dto.Cliente.Email.Trim().ToLowerInvariant();

        await using var transaction = await context.Database.BeginTransactionAsync(cancellationToken);

        var cliente = await context.Clientes
            .FirstOrDefaultAsync(cliente => cliente.Dpi == dpiNormalizado, cancellationToken);

        if (cliente is null)
        {
            cliente = new Cliente
            {
                Nombre = dto.Cliente.Nombre.Trim(),
                Dpi = dpiNormalizado,
                Email = dto.Cliente.Email.Trim(),
                Telefono = dto.Cliente.Telefono.Trim(),
                Estado = EstadoClienteActivo
            };

            context.Clientes.Add(cliente);
            await context.SaveChangesAsync(cancellationToken);
        }
        else if (!string.Equals(cliente.Estado, EstadoClienteActivo, StringComparison.OrdinalIgnoreCase))
        {
            return BadRequest("El cliente existe pero no esta activo.");
        }

        CredencialesTemporalesDto? credencialesTemporales = null;
        var usuario = await context.Usuarios
            .FirstOrDefaultAsync(usuario => usuario.IdCliente == cliente.IdCliente, cancellationToken);

        if (usuario is null)
        {
            var usernameDisponible = !await context.Usuarios
                .AsNoTracking()
                .AnyAsync(usuario => usuario.Username == emailNormalizado, cancellationToken);

            if (!usernameDisponible)
            {
                return BadRequest("El correo del cliente ya esta asociado a otro usuario.");
            }

            var passwordTemporal = GenerateTemporaryPassword();

            usuario = new Usuario
            {
                IdCliente = cliente.IdCliente,
                Username = emailNormalizado,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(passwordTemporal),
                Rol = RolUsuarioCliente,
                Estado = EstadoUsuarioActivo,
                RequiereCambioPassword = true,
                PasswordTemporal = true,
                FechaCambioPassword = null
            };

            context.Usuarios.Add(usuario);
            await context.SaveChangesAsync(cancellationToken);

            credencialesTemporales = new CredencialesTemporalesDto
            {
                Username = usuario.Username,
                PasswordTemporal = passwordTemporal,
                RequiereCambioPassword = true
            };
        }

        var cuenta = new Cuenta
        {
            NumeroCuenta = await GenerateNumeroCuentaAsync(cancellationToken),
            IdCliente = cliente.IdCliente,
            Saldo = dto.Saldo,
            SwiftBanco = SwiftBancoLocal,
            Tipo = dto.Tipo.Trim(),
            Estado = EstadoCuentaInicial
        };

        context.Cuentas.Add(cuenta);
        await context.SaveChangesAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);

        return CreatedAtAction(nameof(GetById), new { id = cuenta.IdCuenta }, new AperturaCuentaResponseDto
        {
            Cuenta = ToDto(cuenta),
            NombreCliente = cliente.Nombre,
            UsuarioCreado = credencialesTemporales is not null,
            CredencialesTemporales = credencialesTemporales
        });
    }

    [HttpPost("numero/{numeroCuenta}/deposito")]
    public async Task<ActionResult<DepositoCuentaResponseDto>> Depositar(string numeroCuenta, DepositoCuentaDto dto, CancellationToken cancellationToken)
    {
        if (dto.Monto <= 0)
        {
            return BadRequest("El monto debe ser mayor que cero.");
        }

        if (string.IsNullOrWhiteSpace(numeroCuenta))
        {
            return BadRequest("Numero de cuenta invalido.");
        }

        var numeroCuentaNormalizado = numeroCuenta.Trim();

        await using var transaction = await context.Database.BeginTransactionAsync(cancellationToken);

        var cuenta = await context.Cuentas
            .Include(cuenta => cuenta.Cliente)
            .FirstOrDefaultAsync(cuenta => cuenta.NumeroCuenta == numeroCuentaNormalizado, cancellationToken);

        if (cuenta is null)
        {
            return NotFound();
        }

        if (!string.Equals(cuenta.Estado, EstadoCuentaInicial, StringComparison.OrdinalIgnoreCase))
        {
            return BadRequest("La cuenta no esta activa.");
        }

        if (cuenta.Cliente is null)
        {
            return BadRequest("La cuenta no tiene cliente asociado.");
        }

        if (!string.Equals(cuenta.Cliente.Estado, "ACTIVO", StringComparison.OrdinalIgnoreCase))
        {
            return BadRequest("El cliente de la cuenta no esta activo.");
        }

        cuenta.Saldo += dto.Monto;

        var movimiento = new Movimiento
        {
            IdCuenta = cuenta.IdCuenta,
            Tipo = "CREDITO_DEPOSITO",
            Monto = dto.Monto,
            Descripcion = "Deposito a cuenta",
            Referencia = $"DEP-{Guid.NewGuid():N}",
            SaldoResultante = cuenta.Saldo
        };

        context.Movimientos.Add(movimiento);
        await context.SaveChangesAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);

        return Ok(new DepositoCuentaResponseDto
        {
            Monto = dto.Monto,
            NumeroCuenta = cuenta.NumeroCuenta,
            NombreCliente = cuenta.Cliente.Nombre
        });
    }

    [HttpPost("numero/{numeroCuenta}/retiro")]
    public async Task<ActionResult<DepositoCuentaResponseDto>> Retirar(string numeroCuenta, DepositoCuentaDto dto, CancellationToken cancellationToken)
    {
        if (dto.Monto <= 0)
        {
            return BadRequest("El monto debe ser mayor que cero.");
        }

        if (string.IsNullOrWhiteSpace(numeroCuenta))
        {
            return BadRequest("Numero de cuenta invalido.");
        }

        var numeroCuentaNormalizado = numeroCuenta.Trim();

        await using var transaction = await context.Database.BeginTransactionAsync(cancellationToken);

        var cuenta = await context.Cuentas
            .Include(cuenta => cuenta.Cliente)
            .FirstOrDefaultAsync(cuenta => cuenta.NumeroCuenta == numeroCuentaNormalizado, cancellationToken);

        if (cuenta is null)
        {
            return NotFound();
        }

        if (!string.Equals(cuenta.Estado, EstadoCuentaInicial, StringComparison.OrdinalIgnoreCase))
        {
            return BadRequest("La cuenta no esta activa.");
        }

        if (cuenta.Cliente is null)
        {
            return BadRequest("La cuenta no tiene cliente asociado.");
        }

        if (!string.Equals(cuenta.Cliente.Estado, "ACTIVO", StringComparison.OrdinalIgnoreCase))
        {
            return BadRequest("El cliente de la cuenta no esta activo.");
        }

        if (cuenta.Saldo < dto.Monto)
        {
            return BadRequest("La cuenta no tiene saldo suficiente.");
        }

        cuenta.Saldo -= dto.Monto;

        var movimiento = new Movimiento
        {
            IdCuenta = cuenta.IdCuenta,
            Tipo = "DEBITO_RETIRO",
            Monto = dto.Monto,
            Descripcion = "Retiro de cuenta",
            Referencia = $"RET-{Guid.NewGuid():N}",
            SaldoResultante = cuenta.Saldo
        };

        context.Movimientos.Add(movimiento);
        await context.SaveChangesAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);

        return Ok(new DepositoCuentaResponseDto
        {
            Monto = dto.Monto,
            NumeroCuenta = cuenta.NumeroCuenta,
            NombreCliente = cuenta.Cliente.Nombre
        });
    }

    [HttpPatch("numero/{numeroCuenta}/desactivar")]
    public async Task<IActionResult> Desactivar(string numeroCuenta, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(numeroCuenta))
        {
            return BadRequest("Numero de cuenta invalido.");
        }

        var numeroCuentaNormalizado = numeroCuenta.Trim();

        var cuenta = await context.Cuentas
            .FirstOrDefaultAsync(cuenta => cuenta.NumeroCuenta == numeroCuentaNormalizado, cancellationToken);

        if (cuenta is null)
        {
            return NotFound();
        }

        if (!string.Equals(cuenta.Estado, EstadoCuentaInicial, StringComparison.OrdinalIgnoreCase))
        {
            return BadRequest("La cuenta ya no esta activa.");
        }

        if (cuenta.Saldo != 0)
        {
            return BadRequest("Solo se puede desactivar una cuenta con saldo 0.");
        }

        cuenta.Estado = EstadoCuentaInactiva;

        await context.SaveChangesAsync(cancellationToken);
        return NoContent();
    }

    [HttpPatch("numero/{numeroCuenta}/reactivar")]
    public async Task<IActionResult> Reactivar(string numeroCuenta, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(numeroCuenta))
        {
            return BadRequest("Numero de cuenta invalido.");
        }

        var numeroCuentaNormalizado = numeroCuenta.Trim();

        var cuenta = await context.Cuentas
            .FirstOrDefaultAsync(cuenta => cuenta.NumeroCuenta == numeroCuentaNormalizado, cancellationToken);

        if (cuenta is null)
        {
            return NotFound();
        }

        if (!string.Equals(cuenta.Estado, EstadoCuentaInactiva, StringComparison.OrdinalIgnoreCase))
        {
            return BadRequest("La cuenta no esta inactiva.");
        }

        cuenta.Estado = EstadoCuentaInicial;

        await context.SaveChangesAsync(cancellationToken);
        return NoContent();
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Put(int id, UpdateCuentaDto dto, CancellationToken cancellationToken)
    {
        var cuenta = await context.Cuentas.FindAsync([id], cancellationToken);

        if (cuenta is null)
        {
            return NotFound();
        }

        cuenta.IdCliente = dto.IdCliente;
        cuenta.Saldo = dto.Saldo;
        cuenta.Tipo = dto.Tipo;

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

    private async Task<string> GenerateNumeroCuentaAsync(CancellationToken cancellationToken)
    {
        const int maxAttempts = 20;

        for (var attempt = 0; attempt < maxAttempts; attempt++)
        {
            var numeroCuenta = $"1{Random.Shared.Next(0, 100_000_000):D8}";

            var exists = await context.Cuentas
                .AsNoTracking()
                .AnyAsync(cuenta => cuenta.NumeroCuenta == numeroCuenta, cancellationToken);

            if (!exists)
            {
                return numeroCuenta;
            }
        }

        throw new InvalidOperationException("No fue posible generar un numero de cuenta unico.");
    }

    private static string GenerateTemporaryPassword()
    {
        const string uppercase = "ABCDEFGHJKLMNPQRSTUVWXYZ";
        const string lowercase = "abcdefghijkmnopqrstuvwxyz";
        const string digits = "23456789";
        const string symbols = "!@$%*";

        Span<char> passwordChars = stackalloc char[12];
        passwordChars[0] = uppercase[Random.Shared.Next(uppercase.Length)];
        passwordChars[1] = lowercase[Random.Shared.Next(lowercase.Length)];
        passwordChars[2] = digits[Random.Shared.Next(digits.Length)];
        passwordChars[3] = symbols[Random.Shared.Next(symbols.Length)];

        var allChars = string.Concat(uppercase, lowercase, digits, symbols);
        for (var index = 4; index < passwordChars.Length; index++)
        {
            passwordChars[index] = allChars[Random.Shared.Next(allChars.Length)];
        }

        for (var index = passwordChars.Length - 1; index > 0; index--)
        {
            var swapIndex = Random.Shared.Next(index + 1);
            (passwordChars[index], passwordChars[swapIndex]) = (passwordChars[swapIndex], passwordChars[index]);
        }

        return new string(passwordChars);
    }
}