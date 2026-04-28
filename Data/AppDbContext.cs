using Microsoft.EntityFrameworkCore;
using GestionReservas.Models;
using System;
using System.Linq;

namespace GestionReservas.Data
{
    public class AppDbContext : DbContext
    {
        // Constructor del contexto. Recibe las opciones de configuración de la base de datos
        // desde Program.cs, por ejemplo si se usa SQLite o SQL Server.
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        // Representa la tabla de espacios en la base de datos.
        public DbSet<Espacio> Espacios { get; set; }

        // Representa la tabla de reservas en la base de datos.
        public DbSet<Reserva> Reservas { get; set; }

        // Configuración adicional del modelo de base de datos.
        // Aquí se definen conversiones especiales y datos iniciales.
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configuración necesaria para SQLite.
            // SQLite no maneja TimeSpan de la misma forma que otros motores,
            // por eso HoraInicio y HoraFin se guardan como Ticks.
            if (Database.ProviderName == "Microsoft.EntityFrameworkCore.Sqlite")
            {
                modelBuilder.Entity<Reserva>()
                    .Property(r => r.HoraInicio)
                    .HasConversion(v => v.Ticks, v => new TimeSpan(v));

                modelBuilder.Entity<Reserva>()
                    .Property(r => r.HoraFin)
                    .HasConversion(v => v.Ticks, v => new TimeSpan(v));
            }

            // Datos iniciales de espacios.
            // Estos registros se crean automáticamente en la base de datos
            // para que el sistema tenga espacios disponibles al iniciar.
            modelBuilder.Entity<Espacio>().HasData(
                new Espacio 
                { 
                    Id = 1, 
                    Nombre = "Auditorio Magno", 
                    Capacidad = 200, 
                    Ubicacion = "Edificio A", 
                    Estado = "Disponible" 
                },
                new Espacio 
                { 
                    Id = 2, 
                    Nombre = "Laboratorio Alfa", 
                    Capacidad = 30, 
                    Ubicacion = "Edificio B", 
                    Estado = "Disponible" 
                },
                new Espacio 
                { 
                    Id = 3, 
                    Nombre = "Sala de Juntas", 
                    Capacidad = 12, 
                    Ubicacion = "Edificio C", 
                    Estado = "Disponible" 
                }
            );
        }
    }
}