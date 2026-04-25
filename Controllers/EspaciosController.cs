using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using GestionReservas.Data;

namespace GestionReservas.Controllers
{
    public class EspaciosController : Controller
    {
        private readonly AppDbContext _context;

        public EspaciosController(AppDbContext context)
        {
            _context = context;
        }

        // GET: Espacios
        public async Task<IActionResult> Index()
        {
            var espacios = await _context.Espacios.ToListAsync();
            return View(espacios);
        }
    }
}
