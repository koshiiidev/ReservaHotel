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

        private readonly IReservaService _reservaService;
        private readonly UserManager<Usuario> _userManager;
        private readonly IUserService _userService;

        public ReservasController(
            IReservaService reservaService, 
            UserManager<Usuario> userManager,
            IUserService userService)
        {
            _reservaService = reservaService ?? throw new ArgumentNullException(nameof(reservaService));
            _userManager = userManager ?? throw new ArgumentNullException(nameof(userManager));
            _userService = userService;
        }



        // GET: Reservas
        [Authorize(Roles = "Admin, Cliente")]
        public async Task<IActionResult> Index()
        {
            var reservas = await _reservaService.ObtenerTodasAsync();
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
           if (ModelState.IsValid)
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if(string.IsNullOrEmpty(userId))
                {
                    return Unauthorized();
                }

                reserva.UserId = userId;
                

                var succes = await _reservaService.CrearAsync(reserva);
                if (succes) 
                {
                    TempData["Success"] = "Reserva creada exitosamente";
                    return RedirectToAction(nameof(Index));
                }

                ModelState.AddModelError("", "La habitacion no esta disponible para las fechas seleccionadas o las fechas son invalidas.");
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

            var reserva = await _reservaService.ObtenerPorIdAsync(id.Value);
            if (reserva == null)
            {
                return NotFound();
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
        public async Task<IActionResult> Edit(int id, [Bind("Id,NombreCliente,FechaInicio,FechaFin,NumeroHabitacion,Estado")] Reserva reserva)
        {
            if (id != reserva.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (string.IsNullOrEmpty(userId))
                {
                    return Unauthorized();
                }
                reserva.UserId = userId;

                var success = await _reservaService.ActualizarAsync(reserva);
                if (success) 
                {
                    TempData["Success"] = "Reserva actualizada exitosamente. ";
                    return RedirectToAction(nameof(Index));
                }

                ModelState.AddModelError("", "La habitacion no esta disponible para las fechas seleccionadas o las fechas son invalidas. ");
               
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

        [Authorize(Roles = "Admin, Cliente")]
        // GET: Reservas/Delete/5
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

            return View(reserva);
        }

        // POST: Reservas/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            await _reservaService.EliminarAsync(id);
            TempData["Success"] = "Reserva eliminada exitosamente.";
            return RedirectToAction(nameof(Index));
        }


        
       
    }
}
