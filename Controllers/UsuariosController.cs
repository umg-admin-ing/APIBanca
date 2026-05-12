using APIBanca.Data;
using APIBanca.Dtos;
using APIBanca.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace APIBanca.Controllers;

[ApiController]
[Route("api/usuarios")]
public class UsuariosController(AppDbContext context) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IEnumerable<UsuarioDto>>> GetAll(CancellationToken cancellationToken)
    {
        return Ok(await context.Usuarios
            .AsNoTracking()
            .Select(usuario => ToDto(usuario))
            .ToListAsync(cancellationToken));
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<UsuarioDto>> GetById(int id, CancellationToken cancellationToken)
    {
        var usuario = await context.Usuarios
            .AsNoTracking()
            .Where(usuario => usuario.IdUsuario == id)
            .Select(usuario => ToDto(usuario))
            .FirstOrDefaultAsync(cancellationToken);

        return usuario is null ? NotFound() : Ok(usuario);
    }

    [HttpPost]
    public async Task<ActionResult<UsuarioDto>> Post(CreateUsuarioDto dto, CancellationToken cancellationToken)
    {
        var usuario = new Usuario
        {
            IdCliente = dto.IdCliente,
            Username = dto.Username,
            PasswordHash = dto.PasswordHash,
            Rol = dto.Rol,
            Estado = dto.Estado
        };

        context.Usuarios.Add(usuario);
        await context.SaveChangesAsync(cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = usuario.IdUsuario }, ToDto(usuario));
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Put(int id, UpdateUsuarioDto dto, CancellationToken cancellationToken)
    {
        var usuario = await context.Usuarios.FindAsync([id], cancellationToken);

        if (usuario is null)
        {
            return NotFound();
        }

        usuario.IdCliente = dto.IdCliente;
        usuario.Username = dto.Username;
        usuario.PasswordHash = dto.PasswordHash;
        usuario.Rol = dto.Rol;
        usuario.Estado = dto.Estado;

        await context.SaveChangesAsync(cancellationToken);
        return NoContent();
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken)
    {
        var usuario = await context.Usuarios.FindAsync([id], cancellationToken);
        if (usuario is null)
        {
            return NotFound();
        }

        context.Usuarios.Remove(usuario);
        await context.SaveChangesAsync(cancellationToken);
        return NoContent();
    }

    private static UsuarioDto ToDto(Usuario usuario) => new()
    {
        IdUsuario = usuario.IdUsuario,
        IdCliente = usuario.IdCliente,
        Username = usuario.Username,
        Rol = usuario.Rol,
        Estado = usuario.Estado,
        CreatedAt = usuario.CreatedAt
    };
}