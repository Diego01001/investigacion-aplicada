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
        private readonly AppDbContext _context;

        public ReservaService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Reserva>> ObtenerReservasAsync(DateTime? fecha, int? espacioId, string estado)
        {
            var query = _context.Reservas.Include(r => r.Espacio).AsQueryable();

            if (espacioId.HasValue)
                query = query.Where(r => r.EspacioId == espacioId.Value);

            if (!string.IsNullOrEmpty(estado))
                query = query.Where(r => r.Estado == estado);

            var result = await query.OrderByDescending(r => r.Id).ToListAsync();

            if (fecha.HasValue)
            {
                var f = fecha.Value.Date;
                result = result.Where(r => r.Fecha.Date == f).ToList();
            }

            return result;
        }

        public async Task<(bool Success, string Message)> RegistrarReservaAsync(Reserva reserva)
        {
            // Normalizar fecha
            reserva.Fecha = reserva.Fecha.Date;

            // Validar choque de horarios
            var reservasExistentes = await _context.Reservas
                .Where(r => r.EspacioId == reserva.EspacioId && 
                            r.Fecha == reserva.Fecha)
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

        public async Task<(bool Success, string Message)> CancelarReservaAsync(int id)
        {
            var reserva = await _context.Reservas.FindAsync(id);
            if (reserva == null) return (false, "La reserva no existe.");

            _context.Reservas.Remove(reserva);
            await _context.SaveChangesAsync();
            return (true, "Reserva eliminada con éxito.");
        }

        public async Task<IEnumerable<Espacio>> ObtenerEspaciosAsync()
        {
            return await _context.Espacios.ToListAsync();
        }
    }
}
