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

        // El servicio se recibe por inyección de dependencias.
        public ReservasController(IReservaService reservaService)
        {
            _reservaService = reservaService;
        }

        // Muestra la vista principal de reservas.
        // También carga los espacios disponibles para mostrarlos en los select del formulario y filtros.
        public async Task<IActionResult> Index()
        {
            ViewBag.Espacios = await _reservaService.ObtenerEspaciosAsync();
            return View();
        }

        // Devuelve las reservas en formato JSON.
        // Este método es llamado desde jQuery/AJAX para cargar o filtrar la tabla sin recargar la página.
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
            // Verifica que el modelo cumpla las validaciones definidas en la clase Reserva.
            if (!ModelState.IsValid)
            {
                return Json(new 
                { 
                    success = false, 
                    message = "Por favor, complete todos los campos requeridos correctamente." 
                });
            }

            // Envía la reserva al servicio, donde se valida y se guarda en la base de datos.
            var result = await _reservaService.RegistrarReservaAsync(reserva);

            // Devuelve una respuesta JSON para que jQuery muestre el resultado en la página.
            return Json(new 
            { 
                success = result.Success, 
                message = result.Message 
            });
        }

        // Cancela una reserva existente.
        // Este método se llama desde AJAX cuando el usuario confirma la cancelación.
        [HttpPost]
        public async Task<IActionResult> Cancelar(int id)
        {
            // La lógica de cancelación se maneja en el servicio.
            var result = await _reservaService.CancelarReservaAsync(id);

            // Devuelve el resultado para actualizar la interfaz sin recargar la página.
            return Json(new 
            { 
                success = result.Success, 
                message = result.Message 
            });
        }
    }
}