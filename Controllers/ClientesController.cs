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
    [HttpGet]
    public async Task<ActionResult<IEnumerable<Cliente>>> GetAll(CancellationToken cancellationToken)
    {
        return Ok(await context.Clientes.AsNoTracking().ToListAsync(cancellationToken));
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<Cliente>> GetById(int id, CancellationToken cancellationToken)
    {
        var cliente = await context.Clientes.FindAsync([id], cancellationToken);
        return cliente is null ? NotFound() : Ok(cliente);
    }

    [HttpPost]
    public async Task<ActionResult<Cliente>> Post(CreateClienteDto dto, CancellationToken cancellationToken)
    {
        var cliente = new Cliente
        {
            Nombre = dto.Nombre,
            Dpi = dto.Dpi,
            Email = dto.Email,
            Telefono = dto.Telefono,
            Estado = dto.Estado
        };

        context.Clientes.Add(cliente);
        await context.SaveChangesAsync(cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = cliente.IdCliente }, cliente);
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
        cliente.Estado = dto.Estado;

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
}