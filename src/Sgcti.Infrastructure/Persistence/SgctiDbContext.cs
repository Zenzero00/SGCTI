using Microsoft.EntityFrameworkCore;
using Sgcti.Core.Entities;

namespace Sgcti.Infrastructure.Persistence;

public class SgctiDbContext : DbContext
{
    public SgctiDbContext(DbContextOptions<SgctiDbContext> options)
        : base(options)
    {
    }

    public DbSet<Impresora> Impresoras => Set<Impresora>();

    public DbSet<Consumible> Consumibles => Set<Consumible>();

    public DbSet<HistorialConsumo> HistorialConsumo => Set<HistorialConsumo>();

    public DbSet<Ticket> Tickets => Set<Ticket>();

    public DbSet<BitacoraActividad> BitacoraActividades => Set<BitacoraActividad>();

    public DbSet<MetricaRed> MetricasRed => Set<MetricaRed>();

    public DbSet<Suministro> Suministros => Set<Suministro>();

    public DbSet<RegistroConsumoDiario> RegistrosConsumoDiario => Set<RegistroConsumoDiario>();

    public DbSet<MantenimientoImpresora> MantenimientosImpresora => Set<MantenimientoImpresora>();

    public DbSet<Usuario> Usuarios => Set<Usuario>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Impresora>(entity =>
        {
            entity.ToTable("Impresoras");
            entity.HasIndex(i => i.Ip).IsUnique();
            entity.Property(i => i.Ip).HasMaxLength(45).IsRequired();
            entity.Property(i => i.Modelo).HasMaxLength(100).IsRequired();
            entity.Property(i => i.Fabricante).HasMaxLength(100).IsRequired();
            entity.Property(i => i.Departamento).HasMaxLength(60).IsRequired();
            entity.Property(i => i.EstadoPing).HasConversion<string>().HasMaxLength(20);
            entity.Property(i => i.ComputadorasVinculadas).HasMaxLength(500);

            entity.Property(i => i.RutaManual).HasMaxLength(500);
            entity.Property(i => i.NivelTonnerNegro).HasDefaultValue(0);
            entity.Property(i => i.PromedioPaginasDiario).HasPrecision(10, 2);
            entity.Property(i => i.PromedioTonnerDiario).HasPrecision(10, 2);
            entity.Property(i => i.AlertaStockCritico).HasDefaultValue(false);
            entity.HasIndex(i => i.Departamento);
            entity.HasIndex(i => i.NivelTonnerNegro);
            entity.HasIndex(i => i.DiasEstimadosAgotamientoTonner);
            entity.HasIndex(i => i.DiasEstimadosMantenimiento);
            entity.HasIndex(i => i.ConsumibleTonerId);
            entity.HasIndex(i => i.AlertaStockCritico);
            entity.HasOne(i => i.ConsumibleToner)
                .WithMany()
                .HasForeignKey(i => i.ConsumibleTonerId)
                .OnDelete(DeleteBehavior.SetNull);
        });

        modelBuilder.Entity<Consumible>(entity =>
        {
            entity.ToTable("Consumibles");
            entity.Property(c => c.Tipo).HasMaxLength(50).IsRequired();
            entity.Property(c => c.ModeloCompatible).HasMaxLength(100).IsRequired();
            entity.Property(c => c.CostoUnitario).HasColumnType("decimal(18,2)");
            entity.Property(c => c.StockMinimo).HasDefaultValue(1);
            entity.Property(c => c.CantidadActual).HasDefaultValue(0);
            entity.Property(c => c.DiasAntelacionPedido).HasDefaultValue(0);
        });

        modelBuilder.Entity<HistorialConsumo>(entity =>
        {
            entity.ToTable("HistorialConsumo");
            entity.Property(h => h.Costo).HasColumnType("decimal(18,2)");
            entity.HasIndex(h => h.FechaCambio);
            entity.HasOne(h => h.Impresora)
                .WithMany(i => i.HistorialConsumo)
                .HasForeignKey(h => h.ImpresoraId)
                .OnDelete(DeleteBehavior.Cascade);
            entity.HasOne(h => h.Consumible)
                .WithMany(c => c.HistorialConsumo)
                .HasForeignKey(h => h.ConsumibleId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<Ticket>(entity =>
        {
            entity.ToTable("Tickets");
            entity.Property(t => t.Titulo).HasMaxLength(200).IsRequired();
            entity.Property(t => t.Descripcion).HasMaxLength(2000).IsRequired();
            entity.Property(t => t.Estado).HasConversion<string>().HasMaxLength(20);
            entity.Property(t => t.Prioridad).HasConversion<string>().HasMaxLength(20);
            entity.Property(t => t.FechaCreacion).IsRequired();
            entity.HasIndex(t => t.FechaCreacion);
            entity.HasIndex(t => t.Estado);
        });

        modelBuilder.Entity<BitacoraActividad>(entity =>
        {
            entity.ToTable("BitacoraActividades");
            entity.Property(b => b.AnalistaId).HasMaxLength(50).IsRequired();
            entity.Property(b => b.DescripcionActividad).HasMaxLength(2000).IsRequired();
            entity.Property(b => b.EquipoIntervenido).HasMaxLength(150);
            entity.Property(b => b.Etiqueta).HasMaxLength(30).IsRequired();
            entity.Property(b => b.Estado).HasMaxLength(15).IsRequired();
            entity.HasIndex(b => b.Estado);
            entity.HasIndex(b => b.AnalistaId);
        });

        modelBuilder.Entity<MetricaRed>(entity =>
        {
            entity.ToTable("MetricasRed");
            entity.Property(m => m.DispositivoIP).HasMaxLength(45).IsRequired();
            entity.Property(m => m.LatenciaMS).HasPrecision(10, 2);
            entity.HasIndex(m => m.FechaHora);
            entity.HasIndex(m => m.DispositivoIP);
        });

        modelBuilder.Entity<Suministro>(entity =>
        {
            entity.ToTable("Suministros");
            entity.Property(s => s.Nombre).HasMaxLength(150).IsRequired();
            entity.Property(s => s.Tipo).HasMaxLength(50).IsRequired();
            entity.Property(s => s.CantidadActual).HasDefaultValue(0);
            entity.Property(s => s.StockMinimo).HasDefaultValue(0);
            entity.Property(s => s.Observacion).HasMaxLength(500);
            entity.HasIndex(s => s.Nombre);
            entity.HasIndex(s => s.Tipo);
        });

        modelBuilder.Entity<RegistroConsumoDiario>(entity =>
        {
            entity.ToTable("RegistrosConsumoDiario");
            entity.Property(r => r.Observacion).HasMaxLength(500);
            entity.HasIndex(r => r.Fecha);
            entity.HasIndex(r => r.ImpresoraId);
            entity.HasOne(r => r.Impresora)
                .WithMany(i => i.RegistrosConsumoDiario)
                .HasForeignKey(r => r.ImpresoraId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<MantenimientoImpresora>(entity =>
        {
            entity.ToTable("MantenimientosImpresora");
            entity.Property(m => m.Tipo).HasMaxLength(20).IsRequired();
            entity.Property(m => m.Descripcion).HasMaxLength(2000).IsRequired();
            entity.Property(m => m.RealizadoPor).HasMaxLength(100).IsRequired();
            entity.HasIndex(m => m.ImpresoraId);
            entity.HasIndex(m => m.Fecha);
            entity.HasOne(m => m.Impresora)
                .WithMany(i => i.MantenimientosImpresora)
                .HasForeignKey(m => m.ImpresoraId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<Usuario>(entity =>
        {
            entity.ToTable("Usuarios");
            entity.Property(u => u.NombreCompleto).HasMaxLength(150).IsRequired();
            entity.Property(u => u.Username).HasMaxLength(50).IsRequired();
            entity.Property(u => u.PasswordHash).HasMaxLength(200).IsRequired();
            entity.Property(u => u.Rol).HasMaxLength(20).IsRequired();
            entity.Property(u => u.Activo).HasDefaultValue(true);
            entity.HasIndex(u => u.Username).IsUnique();

            entity.HasData(new Usuario
            {
                Id = 1,
                NombreCompleto = "Administrador del Sistema",
                Username = "admin",
                PasswordHash = "admin123",
                Rol = Usuario.RolAdmin,
                Activo = true
            });
        });
    }
}