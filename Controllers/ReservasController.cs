using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using GestionReservas.Models;
using GestionReservas.Services;

namespace GestionReservas.Controllers
{
    public class ReservasController : Controller
    {
        // Servicio que contiene la lógica de negocio relacionada con las reservas.
        private readonly IReservaService _reservaService;

        // ASP.NET Core inyecta aquí el servicio configurado en Program.cs.
        public ReservasController(IReservaService reservaService)
        {
            _reservaService = reservaService;
        }

        // Muestra la vista principal de reservas.
        // También carga los espacios disponibles para el formulario y los filtros.
        public async Task<IActionResult> Index()
        {
            ViewBag.Espacios = await _reservaService.ObtenerEspaciosAsync();
            return View();
        }

        // Devuelve las reservas en formato JSON.
        // Este método es llamado desde AJAX para cargar o filtrar la tabla.
        [HttpGet]
        public async Task<IActionResult> GetReservas(DateTime? fecha, int? espacioId, string estado)
        {
            var result = await _reservaService.ObtenerReservasAsync(fecha, espacioId, estado);
            return Json(result);
        }

        // Registra una nueva reserva.
        // Recibe los datos enviados desde el formulario mediante AJAX.
        [HttpPost]
        public async Task<IActionResult> Create(Reserva reserva)
        {
            if (!ModelState.IsValid)
            {
                return Json(new
                {
                    success = false,
                    message = "Por favor, complete todos los campos requeridos correctamente."
                });
            }

            var result = await _reservaService.RegistrarReservaAsync(reserva);

            return Json(new
            {
                success = result.Success,
                message = result.Message
            });
        }

        // Cancela una reserva vigente.
        // No elimina el registro; solo cambia su estado a Cancelada.
        [HttpPost]
        public async Task<IActionResult> Cancelar(int id)
        {
            var result = await _reservaService.CancelarReservaAsync(id);

            return Json(new
            {
                success = result.Success,
                message = result.Message
            });
        }

        // Elimina definitivamente una reserva cancelada.
        // Esta acción solo debe usarse después de cancelar la reserva.
        [HttpPost]
        public async Task<IActionResult> Eliminar(int id)
        {
            var result = await _reservaService.EliminarReservaAsync(id);

            return Json(new
            {
                success = result.Success,
                message = result.Message
            });
        }
    }
}