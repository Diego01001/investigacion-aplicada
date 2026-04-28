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
        // Contexto de base de datos usado para acceder a las tablas de reservas y espacios.
        private readonly AppDbContext _context;

        
        // Esto permite usar la base de datos sin crear manualmente una instancia de AppDbContext.
        public ReservaService(AppDbContext context)
        {
            _context = context;
        }

        // Obtiene las reservas registradas, incluyendo la información del espacio asociado.
        // También permite aplicar filtros por fecha, espacio y estado.
        public async Task<IEnumerable<Reserva>> ObtenerReservasAsync(DateTime? fecha, int? espacioId, string estado)
        {
            // Se prepara la consulta incluyendo la relación con Espacio.
            var query = _context.Reservas.Include(r => r.Espacio).AsQueryable();

            // Filtra por espacio si el usuario seleccionó uno.
            if (espacioId.HasValue)
                query = query.Where(r => r.EspacioId == espacioId.Value);

            // Filtra por estado si el usuario seleccionó uno.
            if (!string.IsNullOrEmpty(estado))
                query = query.Where(r => r.Estado == estado);

            // Ejecuta la consulta ordenando las reservas más recientes primero.
            var result = await query.OrderByDescending(r => r.Id).ToListAsync();

            // Filtra por fecha después de obtener los datos para comparar solo la parte de la fecha.
            if (fecha.HasValue)
            {
                var f = fecha.Value.Date;
                result = result.Where(r => r.Fecha.Date == f).ToList();
            }

            return result;
        }

        // Registra una nueva reserva en la base de datos.
        // Antes de guardar, valida que no exista un choque de horario para el mismo espacio.
        public async Task<(bool Success, string Message)> RegistrarReservaAsync(Reserva reserva)
        {
            // Normaliza la fecha para evitar diferencias por hora interna.
            reserva.Fecha = reserva.Fecha.Date;

            // Busca reservas existentes del mismo espacio y en la misma fecha.
            var reservasExistentes = await _context.Reservas
                .Where(r => r.EspacioId == reserva.EspacioId && 
                            r.Fecha == reserva.Fecha)
                .ToListAsync();

            // Verifica si el horario solicitado se cruza con alguna reserva existente.
            bool hayChoque = reservasExistentes.Any(r =>
                (reserva.HoraInicio >= r.HoraInicio && reserva.HoraInicio < r.HoraFin) ||
                (reserva.HoraFin > r.HoraInicio && reserva.HoraFin <= r.HoraFin) ||
                (reserva.HoraInicio <= r.HoraInicio && reserva.HoraFin >= r.HoraFin));

            // Si hay choque, no se guarda la reserva.
            if (hayChoque)
            {
                return (false, "El espacio ya se encuentra reservado en ese horario.");
            }

            // Si no hay choque, se marca como vigente y se guarda en la base de datos.
            reserva.Estado = "Vigente";
            _context.Reservas.Add(reserva);
            await _context.SaveChangesAsync();

            return (true, "Reserva registrada correctamente.");
        }

        // Cancela una reserva existente.
        // En esta versión, la reserva se elimina de la base de datos.
        public async Task<(bool Success, string Message)> CancelarReservaAsync(int id)
        {
            // Busca la reserva por su identificador.
            var reserva = await _context.Reservas.FindAsync(id);

            // Si no existe, devuelve un mensaje de error.
            if (reserva == null)
                return (false, "La reserva no existe.");

            // Elimina la reserva y guarda los cambios.
            _context.Reservas.Remove(reserva);
            await _context.SaveChangesAsync();

            return (true, "Reserva eliminada con éxito.");
        }

        // Obtiene todos los espacios disponibles registrados en la base de datos.
        // Se usa para llenar los select del formulario y los filtros.
        public async Task<IEnumerable<Espacio>> ObtenerEspaciosAsync()
        {
            return await _context.Espacios.ToListAsync();
        }
    }
}