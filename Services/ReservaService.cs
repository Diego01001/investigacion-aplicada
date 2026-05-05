using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using GestionReservas.Data;
using GestionReservas.Models;

namespace GestionReservas.Services
{
    public class ReservaService : IReservaService
    {
        // Contexto de base de datos usado para acceder a reservas y espacios.
        private readonly AppDbContext _context;

        // El contexto se recibe mediante inyección de dependencias.
        public ReservaService(AppDbContext context)
        {
            _context = context;
        }

        // Obtiene las reservas registradas e incluye la información del espacio.
        // También aplica filtros por fecha, espacio y estado cuando el usuario los selecciona.
        public async Task<IEnumerable<Reserva>> ObtenerReservasAsync(DateTime? fecha, int? espacioId, string estado)
        {
            var query = _context.Reservas
                .Include(r => r.Espacio)
                .AsQueryable();

            if (espacioId.HasValue)
            {
                query = query.Where(r => r.EspacioId == espacioId.Value);
            }

            if (!string.IsNullOrWhiteSpace(estado))
            {
                query = query.Where(r => r.Estado == estado);
            }

            var reservas = await query
                .OrderByDescending(r => r.Id)
                .ToListAsync();

            if (fecha.HasValue)
            {
                var fechaFiltro = fecha.Value.Date;

                reservas = reservas
                    .Where(r => r.Fecha.Date == fechaFiltro)
                    .ToList();
            }

            return reservas;
        }

        // Registra una reserva nueva.
        // Antes de guardar, valida que no exista choque de horario con reservas vigentes.
        public async Task<(bool Success, string Message)> RegistrarReservaAsync(Reserva reserva)
        {
            reserva.Fecha = reserva.Fecha.Date;

            var reservasExistentes = await _context.Reservas
                .Where(r => r.EspacioId == reserva.EspacioId &&
                            r.Fecha == reserva.Fecha &&
                            r.Estado == "Vigente")
                .ToListAsync();

            bool hayChoque = reservasExistentes.Any(r =>
                (reserva.HoraInicio >= r.HoraInicio && reserva.HoraInicio < r.HoraFin) ||
                (reserva.HoraFin > r.HoraInicio && reserva.HoraFin <= r.HoraFin) ||
                (reserva.HoraInicio <= r.HoraInicio && reserva.HoraFin >= r.HoraFin));

            if (hayChoque)
            {
                return (false, "El espacio ya se encuentra reservado en ese horario.");
            }

            reserva.Estado = "Vigente";

            _context.Reservas.Add(reserva);
            await _context.SaveChangesAsync();

            return (true, "Reserva registrada correctamente.");
        }

        // Cancela una reserva sin eliminarla de la base de datos.
        // La reserva queda visible, pero con estado Cancelada.
        public async Task<(bool Success, string Message)> CancelarReservaAsync(int id)
        {
            var reserva = await _context.Reservas.FindAsync(id);

            if (reserva == null)
            {
                return (false, "La reserva no existe.");
            }

            if (reserva.Estado == "Cancelada")
            {
                return (false, "La reserva ya se encuentra cancelada.");
            }

            reserva.Estado = "Cancelada";

            await _context.SaveChangesAsync();

            return (true, "Reserva cancelada correctamente.");
        }

        // Elimina definitivamente una reserva.
        // Solo se permite eliminar si la reserva ya está cancelada.
        public async Task<(bool Success, string Message)> EliminarReservaAsync(int id)
        {
            var reserva = await _context.Reservas.FindAsync(id);

            if (reserva == null)
            {
                return (false, "La reserva no existe.");
            }

            if (reserva.Estado != "Cancelada")
            {
                return (false, "Solo se pueden eliminar reservas canceladas.");
            }

            _context.Reservas.Remove(reserva);
            await _context.SaveChangesAsync();

            return (true, "Reserva eliminada correctamente.");
        }

        // Obtiene los espacios académicos para llenar los select del formulario y filtros.
        public async Task<IEnumerable<Espacio>> ObtenerEspaciosAsync()
        {
            return await _context.Espacios.ToListAsync();
        }
    }
}