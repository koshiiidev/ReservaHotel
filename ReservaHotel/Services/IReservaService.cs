using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Conventions;
using ReservaHotel.Areas.Identity.Data;
using ReservaHotel.Models;


namespace ReservaHotel.Services
{
    public interface IReservaService
    {
        Task<IEnumerable<Reserva>> ObtenerTodasAsync();
        Task<Reserva?> ObtenerPorIdAsync(int id);
        Task<bool> CrearAsync(Reserva reserva);
        Task<bool> ActualizarAsync(Reserva reserva);
        Task<bool> EliminarAsync(int id);
        Task<bool> HabitacionDisponible(int numeroHabitacion, DateOnly fechaInicio, DateOnly fechaFin, int? excluirReservaID = null);
    }


    public class ReservaService : IReservaService
    {
        private readonly ApplicationDbContext _context;

        public ReservaService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Reserva>> ObtenerTodasAsync()
        {
            return await _context.Reserva.ToListAsync();
        }

        public async Task<Reserva?> ObtenerPorIdAsync(int id)
        {
            return await _context.Reserva.FindAsync(id);
        }

        public async Task<bool> CrearAsync(Reserva reserva)
        {
            if (!reserva.EsReservaValida())
            {
                return false;
            }

            if (!await HabitacionDisponible(reserva.NumeroHabitacion, reserva.FechaInicio, reserva.FechaFin))
            {
                return false;
            }

            _context.Reserva.Add(reserva);
            await _context.SaveChangesAsync();
            return true;

        }

        public async Task<bool> ActualizarAsync(Reserva reserva)
        {
            if (!reserva.EsReservaValida())
            {
                return false;
            }

            if (!await HabitacionDisponible(reserva.NumeroHabitacion, reserva.FechaInicio, reserva.FechaFin, reserva.Id))
            {
                return false;
            }

            _context.Entry(reserva).State = EntityState.Modified;
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> EliminarAsync(int id)
        {
            var reserva = await _context.Reserva.FindAsync(id);
            if (reserva == null)
            {
                return false;
            }

            _context.Reserva.Remove(reserva);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> HabitacionDisponible(int numeroHabitacion, DateOnly fechaInicio, DateOnly fechaFin, int? excluirReservaId = null)
        {
            var query = _context.Reserva
                .Where(r => r.NumeroHabitacion == numeroHabitacion &&
                    r.Estado != EstadoReserva.Cancelada &&
                    ((fechaInicio >= r.FechaInicio && fechaInicio < r.FechaFin) ||
                    (fechaFin > r.FechaInicio && fechaFin <= r.FechaFin) ||
                    (fechaInicio <= r.FechaInicio && fechaFin >= r.FechaFin)));

            if (excluirReservaId.HasValue)
            {
                query = query.Where(r => r.Id != excluirReservaId.Value);
            }

            return !await query.AnyAsync();

        }
    }
}
