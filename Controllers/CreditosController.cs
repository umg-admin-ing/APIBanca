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

    // GET api/creditos/{id}/amortizacion
    [HttpGet("{id:int}/amortizacion")]
    public async Task<ActionResult<IEnumerable<CuotaAmortizacionDto>>> GetAmortizacion(
        int id,
        CancellationToken cancellationToken)
    {
        var credito = await context.Creditos
            .AsNoTracking()
            .FirstOrDefaultAsync(c => c.IdCredito == id, cancellationToken);

        if (credito is null)
            return NotFound();

        var tabla = GenerarAmortizacion(
            credito.SaldoPendiente,   // parte desde el saldo actual, no el original
            credito.TasaInteres,
            credito.CuotaMensual,
            credito.FechaInicio);

        return Ok(tabla);
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

    [HttpPost("{id:int}/abonos")]
    public async Task<ActionResult<AbonoResultadoDto>> PostAbono(
    int id,
    CreateAbonoDto dto,
    CancellationToken cancellationToken)
    {
        // Cargar crédito CON su cuenta asociada
        var credito = await context.Creditos
            .Include(c => c.Cuenta)              // ← NUEVO: trae la cuenta
            .FirstOrDefaultAsync(c => c.IdCredito == id, cancellationToken);

        if (credito is null)
            return NotFound("Crédito no encontrado.");

        if (credito.Estado != "ACTIVO")
            return Conflict("Solo se pueden hacer abonos a créditos ACTIVOS.");

        if (dto.Monto <= 0)
            return BadRequest("El monto del abono debe ser mayor a 0.");

        // Validar saldo suficiente en la cuenta bancaria
        if (credito.Cuenta.Saldo < dto.Monto)
            return BadRequest($"Saldo insuficiente en la cuenta. Saldo disponible: {credito.Cuenta.Saldo:F2}.");

        decimal saldoAnterior = credito.SaldoPendiente;
        decimal saldoNuevo;
        decimal? nuevaCuota = null;

        if (dto.TipoAbono == "CUOTA")
        {
            if (dto.Monto != credito.CuotaMensual)
                return BadRequest($"El abono a cuota debe ser exactamente {credito.CuotaMensual:F2}.");

            decimal tasaMensual = credito.TasaInteres / 100m / 12m;
            decimal interesMes = Math.Round(saldoAnterior * tasaMensual, 2);
            decimal capitalMes = Math.Round(credito.CuotaMensual - interesMes, 2);

            saldoNuevo = Math.Round(saldoAnterior - capitalMes, 2);
        }
        else // CAPITAL
        {
            if (dto.Monto > credito.SaldoPendiente)
                return BadRequest("El monto excede el saldo pendiente.");

            saldoNuevo = Math.Round(saldoAnterior - dto.Monto, 2);

            if (saldoNuevo > 0)
            {
                int cuotasRestantes = CalcularCuotasRestantes(saldoAnterior, credito.TasaInteres, credito.CuotaMensual);
                if (cuotasRestantes > 1)
                {
                    decimal tasaMensual = credito.TasaInteres / 100m / 12m;
                    decimal factor = (decimal)Math.Pow((double)(1 + tasaMensual), cuotasRestantes);
                    nuevaCuota = Math.Round(saldoNuevo * (tasaMensual * factor) / (factor - 1), 2);
                    credito.CuotaMensual = nuevaCuota.Value;
                }
            }
        }

        if (saldoNuevo < 0) saldoNuevo = 0;

        // Descontar de la cuenta bancaria
        credito.Cuenta.Saldo = Math.Round(credito.Cuenta.Saldo - dto.Monto, 2);  // ← NUEVO

        var abono = new Abono
        {
            IdCredito = id,
            Monto = dto.Monto,
            SaldoAnterior = saldoAnterior,
            SaldoNuevo = saldoNuevo,
            TipoAbono = dto.TipoAbono,
            CreatedAt = DateTime.UtcNow
        };

        credito.SaldoPendiente = saldoNuevo;

        if (saldoNuevo == 0)
            credito.Estado = "PAGADO";

        context.Abonos.Add(abono);
        await context.SaveChangesAsync(cancellationToken);  // guarda abono + crédito + cuenta en una sola transacción

        Console.WriteLine(
            $"[ABONO {dto.TipoAbono}] CreditoId={id} | Monto={dto.Monto} | " +
            $"SaldoAnterior={saldoAnterior} | SaldoNuevo={saldoNuevo} | " +
            $"NuevaCuota={nuevaCuota?.ToString() ?? "-"} | At={DateTime.UtcNow:O}");

        return Ok(new AbonoResultadoDto
        {
            IdAbono = abono.IdAbono,
            MontoAplicado = abono.Monto,
            SaldoAnterior = saldoAnterior,
            SaldoNuevo = saldoNuevo,
            TipoAbono = dto.TipoAbono,
            NuevaCuotaMensual = nuevaCuota,
            CreatedAt = abono.CreatedAt
        });
    }

    // Helper privado — tabla de amortización francesa
    private static List<CuotaAmortizacionDto> GenerarAmortizacion(
        decimal saldo,
        decimal tasaAnual,
        decimal cuota,
        DateTime fechaInicio)
    {
        decimal tasaMensual = tasaAnual / 100m / 12m;
        var tabla = new List<CuotaAmortizacionDto>();
        int cuotaNum = 1;

        while (saldo > 0.01m)
        {
            decimal interes = Math.Round(saldo * tasaMensual, 2);
            decimal capital = Math.Round(cuota - interes, 2);

            // Última cuota: ajusta para no quedar saldo residual
            if (capital > saldo)
            {
                capital = saldo;
                cuota = capital + interes;
            }

            saldo = Math.Round(saldo - capital, 2);

            tabla.Add(new CuotaAmortizacionDto
            {
                Numero = cuotaNum,
                FechaVencimiento = fechaInicio.AddMonths(cuotaNum),
                CuotaMensual = Math.Round(capital + interes, 2),
                Capital = capital,
                Interes = interes,
                SaldoPendiente = saldo
            });

            cuotaNum++;
        }

        return tabla;
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

    internal static CreditoDto ToDto(Credito credito) => new()
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

    private static int CalcularCuotasRestantes(decimal saldo, decimal tasaAnual, decimal cuota)
    {
        decimal tasaMensual = tasaAnual / 100m / 12m;
        int cuotas = 0;

        while (saldo > 0.01m && cuotas < 600)
        {
            decimal interes = Math.Round(saldo * tasaMensual, 2);
            decimal capital = Math.Round(cuota - interes, 2);
            if (capital <= 0) break;
            saldo = Math.Round(saldo - capital, 2);
            cuotas++;
        }

        return cuotas;
    }


}