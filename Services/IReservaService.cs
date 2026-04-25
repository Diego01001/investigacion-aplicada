using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using GestionReservas.Models;

namespace GestionReservas.Services
{
    public interface IReservaService
    {
        Task<IEnumerable<Reserva>> ObtenerReservasAsync(DateTime? fecha, int? espacioId, string estado);
        Task<(bool Success, string Message)> RegistrarReservaAsync(Reserva reserva);
        Task<(bool Success, string Message)> CancelarReservaAsync(int id);
        Task<IEnumerable<Espacio>> ObtenerEspaciosAsync();
    }
}
