using Microsoft.EntityFrameworkCore;
using GestionReservas.Models;
using System;
using System.Linq;

namespace GestionReservas.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<Espacio> Espacios { get; set; }
        public DbSet<Reserva> Reservas { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // SQLite TimeSpan conversion fix
            if (Database.ProviderName == "Microsoft.EntityFrameworkCore.Sqlite")
            {
                modelBuilder.Entity<Reserva>()
                    .Property(r => r.HoraInicio)
                    .HasConversion(v => v.Ticks, v => new TimeSpan(v));

                modelBuilder.Entity<Reserva>()
                    .Property(r => r.HoraFin)
                    .HasConversion(v => v.Ticks, v => new TimeSpan(v));
            }

            // Seed inicial
            modelBuilder.Entity<Espacio>().HasData(
                new Espacio { Id = 1, Nombre = "Auditorio Magno", Capacidad = 200, Ubicacion = "Edificio A", Estado = "Disponible" },
                new Espacio { Id = 2, Nombre = "Laboratorio Alfa", Capacidad = 30, Ubicacion = "Edificio B", Estado = "Disponible" },
                new Espacio { Id = 3, Nombre = "Sala de Juntas", Capacidad = 12, Ubicacion = "Edificio C", Estado = "Disponible" }
            );
        }
    }
}
