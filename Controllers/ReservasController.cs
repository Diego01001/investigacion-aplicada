using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using GestionReservas.Models;
using GestionReservas.Services;

namespace GestionReservas.Controllers
{
    public class ReservasController : Controller
    {
        private readonly IReservaService _reservaService;

        public ReservasController(IReservaService reservaService)
        {
            _reservaService = reservaService;
        }

        // GET: Reservas
        public async Task<IActionResult> Index()
        {
            ViewBag.Espacios = await _reservaService.ObtenerEspaciosAsync();
            return View();
        }

        // GET: Reservas/GetReservas (AJAX)
        [HttpGet]
        public async Task<IActionResult> GetReservas(DateTime? fecha, int? espacioId, string estado)
        {
            var result = await _reservaService.ObtenerReservasAsync(fecha, espacioId, estado);
            return Json(result);
        }

        // POST: Reservas/Create (AJAX)
        [HttpPost]
        public async Task<IActionResult> Create(Reserva reserva)
        {
            if (!ModelState.IsValid)
            {
                return Json(new { success = false, message = "Por favor, complete todos los campos requeridos correctamente." });
            }

            var result = await _reservaService.RegistrarReservaAsync(reserva);
            return Json(new { success = result.Success, message = result.Message });
        }

        // POST: Reservas/Cancelar (AJAX)
        [HttpPost]
        public async Task<IActionResult> Cancelar(int id)
        {
            var result = await _reservaService.CancelarReservaAsync(id);
            return Json(new { success = result.Success, message = result.Message });
        }
    }
}
