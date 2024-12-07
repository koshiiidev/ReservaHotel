using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using ReservaHotel.Areas.Identity.Data;
using ReservaHotel.Models;
using ReservaHotel.Services;



namespace ReservaHotel.Controllers
{
    public class ReservasController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IReservaService _reservaService;
        private readonly UserManager<Usuario> _userManager;
        private readonly IUserService _userService;

        public ReservasController(
            ApplicationDbContext context,
            IReservaService reservaService, 
            UserManager<Usuario> userManager,
            IUserService userService)
        {
            _context = context;
            _reservaService = reservaService ?? throw new ArgumentNullException(nameof(reservaService));
            _userManager = userManager ?? throw new ArgumentNullException(nameof(userManager));
            _userService = userService;
        }



        // GET: Reservas
        [Authorize(Roles = "Admin, Cliente")]
        public async Task<IActionResult> Index()
        {
            var userId = User?.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized();
            }

            var isAdmin = User?.IsInRole("Admin") ?? false;
            var reservas = await _reservaService.ObtenerTodasAsync();

            // Si no es admin, solo mostrar sus propias reservas
            if (!isAdmin)
            {
                reservas = reservas.Where(r => r.UserId == userId);
            }
            return View(reservas);
        }

        // GET: Reservas/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var reserva = await _reservaService.ObtenerPorIdAsync(id.Value);
            if (reserva == null)
            {
                return NotFound();
            }

            return View(reserva);
        }

        // GET: Reservas/Create

        [Authorize(Roles = "Admin, Cliente")]
        public IActionResult Create()
        {
            ViewBag.Estados = Enum.GetValues(typeof(EstadoReserva))
                .Cast<EstadoReserva>()
                .Select(e => new SelectListItem
                {
                    Text = e.ToString(),
                    Value = ((int)e).ToString()
            });
            return View();
        }

        // POST: Reservas/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin, Cliente")]
        public async Task<IActionResult> Create([Bind("NombreCliente,FechaInicio,FechaFin,NumeroHabitacion,Estado")] Reserva reserva)
        {

            try
            {
                // Limpiar el ModelState del UserId ya que lo vamos a asignar manualmente
                ModelState.Remove("UserId");
                ModelState.Remove("User");

                // Asignar el UserId
                reserva.UserId = User.FindFirstValue(ClaimTypes.NameIdentifier);

                if (ModelState.IsValid)
                {
                    reserva.Estado = EstadoReserva.Pendiente;
                    var success = await _reservaService.CrearAsync(reserva);

                    if (success)
                    {
                        TempData["Success"] = "Reserva creada exitosamente.";
                        return RedirectToAction(nameof(Index));
                    }

                    ModelState.AddModelError("", "No se pudo crear la reserva. Verifica que la habitación esté disponible para las fechas seleccionadas.");
                }
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", "Ocurrió un error al crear la reserva: " + ex.Message);
            }


            ViewBag.Estados = Enum.GetValues(typeof(EstadoReserva))
                .Cast<EstadoReserva>()
                .Select(e => new SelectListItem
                {
                    Text = e.ToString(),
                    Value = ((int)e).ToString()
                });
            return View(reserva);
        }

        private async Task<bool> VerificarRolUsuario()
        {
            if (User == null) return false;

            var user = await _userManager.GetUserAsync(User as ClaimsPrincipal);
            if (user == null) return false;

            var roles = await _userManager.GetRolesAsync(user);
            return roles.Contains("Cliente") || roles.Contains("Admin");
        }

        [Authorize]
        public async Task<IActionResult> VerificarRol()
        {
            var user = await _userManager.GetUserAsync(User as ClaimsPrincipal);
            if (user == null)
            {
                return Unauthorized();
            }

            var roles = await _userManager.GetRolesAsync(user);
            return Json(new
            {
                userId = user.Id,
                userName = user.UserName,
                roles = roles
            });
        }



            // GET: Reservas/Edit/5
            //[Authorize(Roles = "Cliente")]
        [Authorize(Roles = "Admin, Cliente")]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var reserva = await _context.Reserva
                .Include(r => r.User)
                .FirstOrDefaultAsync(r => r.Id == id);

            if (reserva == null)
            {
                return NotFound();
            }


            var userId = User?.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized();
            }

            var isAdmin = User?.IsInRole("Admin") ?? false;

            if (!isAdmin && reserva.UserId != userId)
            {
                return Forbid();
            }


            ViewBag.Estados = Enum.GetValues(typeof(EstadoReserva))
                .Cast<EstadoReserva>()
                .Select(e => new SelectListItem
                {
                    Text = e.ToString(),
                    Value = ((int)e).ToString()
                });
            return View(reserva);
        }

        // POST: Reservas/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin, Cliente")]
        public async Task<IActionResult> Edit(int id, [Bind("Id,NombreCliente,FechaInicio,FechaFin,NumeroHabitacion,Estado, UserId")] Reserva reserva)
        {
            System.Diagnostics.Debug.WriteLine($"Edit POST iniciado - ID: {id}");
            System.Diagnostics.Debug.WriteLine($"Datos de reserva: Inicio={reserva.FechaInicio}, Fin={reserva.FechaFin}, Cliente={reserva.NombreCliente}");

            System.Diagnostics.Debug.WriteLine($"Edit POST iniciado - ID: {id}");
            System.Diagnostics.Debug.WriteLine($"Datos de reserva: Inicio={reserva.FechaInicio}, Fin={reserva.FechaFin}, Cliente={reserva.NombreCliente}");

            try
            {
                // Obtener la reserva original primero
                var reservaOriginal = await _context.Reserva
                    .Include(r => r.User)
                    .FirstOrDefaultAsync(r => r.Id == id);

                if (reservaOriginal == null)
                {
                    return NotFound();
                }

                // Asignar el UserId ANTES de verificar ModelState
                reserva.UserId = reservaOriginal.UserId;
                reserva.User = reservaOriginal.User;

                // Limpiar y revalidar ModelState
                ModelState.Remove("UserId");
                ModelState.Remove("User");

                if (ModelState.IsValid)
                {
                    try
                    {
                        var success = await _reservaService.ActualizarAsync(reserva);
                        System.Diagnostics.Debug.WriteLine($"Resultado de ActualizarAsync: {success}");

                        if (success)
                        {
                            TempData["Success"] = "Reserva actualizada exitosamente.";
                            return RedirectToAction(nameof(Index));
                        }

                        ModelState.AddModelError("", "No se pudo actualizar la reserva. Verifica las fechas y disponibilidad de la habitación.");
                    }
                    catch (Exception ex)
                    {
                        System.Diagnostics.Debug.WriteLine($"Error en actualización: {ex.Message}");
                        ModelState.AddModelError("", $"Error al actualizar: {ex.Message}");
                    }
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine("ModelState no es válido:");
                    foreach (var modelState in ModelState.Values)
                    {
                        foreach (var error in modelState.Errors)
                        {
                            System.Diagnostics.Debug.WriteLine($"Error: {error.ErrorMessage}");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error general: {ex.Message}");
                ModelState.AddModelError("", $"Error general: {ex.Message}");
            }

            ViewBag.Estados = Enum.GetValues(typeof(EstadoReserva))
                .Cast<EstadoReserva>()
                .Select(e => new SelectListItem
                {
                    Text = e.ToString(),
                    Value = ((int)e).ToString()
                });
            return View(reserva);
        }



        // GET: Reservas/Delete/5
        [Authorize(Roles = "Admin, Cliente")]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var reserva = await _reservaService.ObtenerPorIdAsync(id.Value);
            if (reserva == null)
            {
                return NotFound();
            }

            var userId = User?.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized();
            }

            var isAdmin = User?.IsInRole("Admin") ?? false;
            if (!isAdmin && reserva.UserId != userId)
            {
                return Forbid();
            }

            return View(reserva);
        }

        // POST: Reservas/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin, Cliente")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            await _reservaService.EliminarAsync(id);
            TempData["Success"] = "Reserva eliminada exitosamente.";
            return RedirectToAction(nameof(Index));
        }


        
       
    }
}
