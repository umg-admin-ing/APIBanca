using Microsoft.EntityFrameworkCore;
using APIBanca.Models;

namespace APIBanca.Data;

public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<Cliente> Clientes => Set<Cliente>();

    public DbSet<Usuario> Usuarios => Set<Usuario>();

    public DbSet<Cuenta> Cuentas => Set<Cuenta>();

    public DbSet<Movimiento> Movimientos => Set<Movimiento>();

    public DbSet<Transferencia> Transferencias => Set<Transferencia>();

    public DbSet<SolicitudCredito> SolicitudesCredito => Set<SolicitudCredito>();

    public DbSet<Credito> Creditos => Set<Credito>();

    public DbSet<Abono> Abonos => Set<Abono>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Cliente>()
            .HasOne(cliente => cliente.Usuario)
            .WithOne(usuario => usuario.Cliente)
            .HasForeignKey<Usuario>(usuario => usuario.IdCliente)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Cliente>()
            .HasMany(cliente => cliente.Cuentas)
            .WithOne(cuenta => cuenta.Cliente)
            .HasForeignKey(cuenta => cuenta.IdCliente)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Cliente>()
            .HasMany(cliente => cliente.SolicitudesCredito)
            .WithOne(solicitud => solicitud.Cliente)
            .HasForeignKey(solicitud => solicitud.IdCliente)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Cuenta>()
            .HasIndex(cuenta => cuenta.NumeroCuenta)
            .IsUnique();

        modelBuilder.Entity<Cuenta>()
            .HasMany(cuenta => cuenta.Movimientos)
            .WithOne(movimiento => movimiento.Cuenta)
            .HasForeignKey(movimiento => movimiento.IdCuenta)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Cuenta>()
            .HasMany(cuenta => cuenta.TransferenciasOrigen)
            .WithOne(transferencia => transferencia.CuentaOrigen)
            .HasForeignKey(transferencia => transferencia.CuentaOrigenId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Cuenta>()
            .HasMany(cuenta => cuenta.TransferenciasDestino)
            .WithOne(transferencia => transferencia.CuentaDestino)
            .HasForeignKey(transferencia => transferencia.CuentaDestinoId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Cuenta>()
            .HasMany(cuenta => cuenta.Creditos)
            .WithOne(credito => credito.Cuenta)
            .HasForeignKey(credito => credito.IdCuenta)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<SolicitudCredito>()
            .HasOne(solicitud => solicitud.Credito)
            .WithOne(credito => credito.SolicitudCredito)
            .HasForeignKey<Credito>(credito => credito.IdSolicitud)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Credito>()
            .HasMany(credito => credito.Abonos)
            .WithOne(abono => abono.Credito)
            .HasForeignKey(abono => abono.IdCredito)
            .OnDelete(DeleteBehavior.Restrict);

        base.OnModelCreating(modelBuilder);
    }
}