using APIBanca.Data;
using APIBanca.Dtos;
using APIBanca.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.IdentityModel.Tokens;
using System.Text;

namespace APIBanca.Controllers;

[ApiController]
[Route("api/usuarios")]
public class UsuariosController(AppDbContext context, IConfiguration configuration) : ControllerBase
{
    [Authorize]
    [HttpGet]
    public async Task<ActionResult<IEnumerable<UsuarioDto>>> GetAll(CancellationToken cancellationToken)
    {
        return Ok(await context.Usuarios
            .AsNoTracking()
            .Select(usuario => ToDto(usuario))
            .ToListAsync(cancellationToken));
    }

    [Authorize]
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

    [Authorize(Roles = "ADMIN")]
    [HttpPost]
public async Task<ActionResult<UsuarioDto>> Post(CreateUsuarioDto dto, CancellationToken cancellationToken)
{
    if (string.IsNullOrWhiteSpace(dto.Username))
    {
        return BadRequest("Username invalido.");
    }

    dto.Username = dto.Username.Trim().ToLower();

    var existeCliente = await context.Clientes
        .AsNoTracking()
        .AnyAsync(c => c.IdCliente == dto.IdCliente, cancellationToken);

    if (!existeCliente)
    {
        return BadRequest("El cliente asociado no existe.");
    }

    var existeUsuario = await context.Usuarios
        .AsNoTracking()
        .AnyAsync(u => u.Username == dto.Username, cancellationToken);

    if (existeUsuario)
    {
        return BadRequest("El nombre de usuario ya esta en uso.");
    }

    var usuario = new Usuario
    {
        IdCliente = dto.IdCliente,
        Username = dto.Username,
        PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password),
        Rol = dto.Rol,
        Estado = dto.Estado
    };

    context.Usuarios.Add(usuario);
    await context.SaveChangesAsync(cancellationToken);
    return CreatedAtAction(nameof(GetById), new { id = usuario.IdUsuario }, ToDto(usuario));
}

[HttpPost("login")]
public async Task<ActionResult<UsuarioDto>> Login(LoginDto dto, CancellationToken cancellationToken)
{

    dto.Username = dto.Username.Trim().ToLower();

    var usuario = await context.Usuarios
        .AsNoTracking()
        .Where(u => u.Username == dto.Username)
        .FirstOrDefaultAsync(cancellationToken);

    if (usuario is null)
    {
        return Unauthorized("Usuario no encontrado o contraseña incorrecta.");
    }

    var validPassword = BCrypt.Net.BCrypt.Verify(dto.Password, usuario.PasswordHash);
    if (!validPassword)
    {
        return Unauthorized("Contraseña incorrecta.");
    }
    if (usuario.Estado != "ACTIVO")
    {
        return BadRequest("Usuario inactivo.");
    }
    var permisos = new []
    {
        new Claim(ClaimTypes.NameIdentifier, usuario.IdUsuario.ToString()),
        new Claim(ClaimTypes.Name, usuario.Username),
        new Claim(ClaimTypes.Role, usuario.Rol)
    };
    var key = new SymmetricSecurityKey( Encoding.UTF8.GetBytes(configuration["Jwt:Key"]!));
    var creditos = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

    var token = new JwtSecurityToken(
        issuer: configuration["Jwt:Issuer"],
        audience: configuration["Jwt:Audience"],
        claims: permisos,
        expires: DateTime.UtcNow.AddHours(1),
        signingCredentials: creditos
    );

    var jwt = new JwtSecurityTokenHandler().WriteToken(token);

    return Ok(new
    {
        Token = jwt,
        Usuario = ToDto(usuario),
        Role = usuario.Rol
    });
}

[Authorize(Roles = "ADMIN")]
[HttpPut("{id:int}")]
public async Task<IActionResult> Put(int id, UpdateUsuarioDto dto, CancellationToken cancellationToken)
{
    var usuario = await context.Usuarios.FindAsync(id, cancellationToken);

    if (usuario is null)
    {
        return NotFound();
    }

    var existeUsuario = await context.Usuarios
        .AsNoTracking()
        .AnyAsync(u => u.Username == dto.Username && u.IdUsuario != id, cancellationToken);
    if (existeUsuario)        {
        return BadRequest("El nombre de usuario ya esta en uso.");
    }

    var existeCliente = await context.Clientes
        .AsNoTracking()
        .AnyAsync(c => c.IdCliente == dto.IdCliente, cancellationToken);
    if (!existeCliente)        {
        return BadRequest("El nombre de cliente no existe.");
    }

    if (string.IsNullOrWhiteSpace(dto.Username))
    {
        return BadRequest("Username invalido.");
    }

    dto.Username = dto.Username.Trim().ToLower();

    usuario.IdCliente = dto.IdCliente;
    usuario.Username = dto.Username;
    usuario.PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password);
    usuario.Rol = dto.Rol;
    usuario.Estado = dto.Estado;

    await context.SaveChangesAsync(cancellationToken);
    return NoContent();
}

[Authorize(Roles = "ADMIN")]
[HttpDelete("{id:int}")]
public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken)
{
    var usuario = await context.Usuarios.FindAsync(id, cancellationToken);
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