using APIBanca.Data;
using APIBanca.Dtos;
using APIBanca.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace APIBanca.Controllers;

[ApiController]
[Route("api/clientes")]
public class ClientesController(AppDbContext context) : ControllerBase
{
    private const string EstadoClienteActivo = "ACTIVO";
    private const string EstadoClienteInactivo = "INACTIVO";

    [HttpGet]
    public async Task<ActionResult<IEnumerable<ClienteDto>>> GetAll(CancellationToken cancellationToken)
    {
        return Ok(await context.Clientes
            .AsNoTracking()
            .Select(cliente => ToDto(cliente))
            .ToListAsync(cancellationToken));
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<ClienteDto>> GetById(int id, CancellationToken cancellationToken)
    {
        var cliente = await context.Clientes
            .AsNoTracking()
            .Where(cliente => cliente.IdCliente == id)
            .Select(cliente => ToDto(cliente))
            .FirstOrDefaultAsync(cancellationToken);

        return cliente is null ? NotFound() : Ok(cliente);
    }

    [HttpPost]
    public async Task<ActionResult<ClienteDto>> Post(CreateClienteDto dto, CancellationToken cancellationToken)
    {
        var cliente = new Cliente
        {
            Nombre = dto.Nombre,
            Dpi = dto.Dpi,
            Email = dto.Email,
            Telefono = dto.Telefono,
            Estado = EstadoClienteActivo
        };

        context.Clientes.Add(cliente);
        await context.SaveChangesAsync(cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = cliente.IdCliente }, ToDto(cliente));
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Put(int id, UpdateClienteDto dto, CancellationToken cancellationToken)
    {
        var cliente = await context.Clientes.FindAsync([id], cancellationToken);

        if (cliente is null)
        {
            return NotFound();
        }

        cliente.Nombre = dto.Nombre;
        cliente.Dpi = dto.Dpi;
        cliente.Email = dto.Email;
        cliente.Telefono = dto.Telefono;

        await context.SaveChangesAsync(cancellationToken);
        return NoContent();
    }

    [HttpPatch("{id:int}/desactivar")]
    public async Task<IActionResult> Desactivar(int id, CancellationToken cancellationToken)
    {
        var cliente = await context.Clientes.FindAsync([id], cancellationToken);

        if (cliente is null)
        {
            return NotFound();
        }

        if (!string.Equals(cliente.Estado, EstadoClienteActivo, StringComparison.OrdinalIgnoreCase))
        {
            return BadRequest("El cliente ya no esta activo.");
        }

        cliente.Estado = EstadoClienteInactivo;

        await context.SaveChangesAsync(cancellationToken);
        return NoContent();
    }

    [HttpPatch("{id:int}/reactivar")]
    public async Task<IActionResult> Reactivar(int id, CancellationToken cancellationToken)
    {
        var cliente = await context.Clientes.FindAsync([id], cancellationToken);

        if (cliente is null)
        {
            return NotFound();
        }

        if (!string.Equals(cliente.Estado, EstadoClienteInactivo, StringComparison.OrdinalIgnoreCase))
        {
            return BadRequest("El cliente no esta inactivo.");
        }

        cliente.Estado = EstadoClienteActivo;

        await context.SaveChangesAsync(cancellationToken);
        return NoContent();
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken)
    {
        var cliente = await context.Clientes.FindAsync([id], cancellationToken);
        if (cliente is null)
        {
            return NotFound();
        }

        context.Clientes.Remove(cliente);
        await context.SaveChangesAsync(cancellationToken);
        return NoContent();
    }

    private static ClienteDto ToDto(Cliente cliente) => new()
    {
        IdCliente = cliente.IdCliente,
        Nombre = cliente.Nombre,
        Dpi = cliente.Dpi,
        Email = cliente.Email,
        Telefono = cliente.Telefono,
        Estado = cliente.Estado,
        CreatedAt = cliente.CreatedAt
    };
}