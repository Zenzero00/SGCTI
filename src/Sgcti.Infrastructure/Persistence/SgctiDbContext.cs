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
            entity.Property(b => b.DescripcionActividad).HasMaxLength(2000).IsRequired();
            entity.Property(b => b.EquipoIntervenido).HasMaxLength(150);
            entity.Property(b => b.Etiqueta).HasMaxLength(30);
            entity.Property(b => b.Estado).HasConversion<string>().HasMaxLength(15);
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
    }
}