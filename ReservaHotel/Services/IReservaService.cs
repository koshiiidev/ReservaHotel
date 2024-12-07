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
        Task<bool> HabitacionDisponible(int numeroHabitacion, DateTime fechaInicio, DateTime fechaFin, int? excluirReservaID = null);
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
            try
            {
                if (!reserva.EsReservaValida())
                {
                    return false;
                }

                if (!await HabitacionDisponible(reserva.NumeroHabitacion, reserva.FechaInicio, reserva.FechaFin))
                {
                    return false;
                }

                reserva.Estado = EstadoReserva.Pendiente;

                _context.Reserva.Add(reserva);
                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception)
            {

            }
            return false;
        }

        public async Task<bool> ActualizarAsync(Reserva reserva)
        {
            try
            {
                System.Diagnostics.Debug.WriteLine($"Iniciando ActualizarAsync para reserva ID: {reserva.Id}");

                var reservaExistente = await _context.Reserva.FindAsync(reserva.Id);
                if (reservaExistente == null)
                {
                    System.Diagnostics.Debug.WriteLine("Reserva existente no encontrada");
                    return false;
                }

                if (!await HabitacionDisponible(
                    reserva.NumeroHabitacion,
                    reserva.FechaInicio.Date,
                    reserva.FechaFin.Date,
                    reserva.Id))
                {
                    System.Diagnostics.Debug.WriteLine("Habitación no disponible para las fechas seleccionadas");
                    return false;
                }

                // Actualizar campos
                reservaExistente.NombreCliente = reserva.NombreCliente;
                reservaExistente.FechaInicio = reserva.FechaInicio.Date;
                reservaExistente.FechaFin = reserva.FechaFin.Date;
                reservaExistente.NumeroHabitacion = reserva.NumeroHabitacion;
                reservaExistente.Estado = reserva.Estado;

                await _context.SaveChangesAsync();
                System.Diagnostics.Debug.WriteLine("Actualización completada exitosamente");
                return true;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error en ActualizarAsync: {ex.Message}");
                System.Diagnostics.Debug.WriteLine($"Stack trace: {ex.StackTrace}");
                return false;
            }
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

        public async Task<bool> HabitacionDisponible(int numeroHabitacion, DateTime fechaInicio, DateTime fechaFin, int? excluirReservaId = null)
        {
            var query = _context.Reserva
                .Where(r => r.NumeroHabitacion == numeroHabitacion &&
                    r.Estado != EstadoReserva.Cancelada &&
                    ((fechaInicio.Date >= r.FechaInicio.Date && fechaInicio.Date < r.FechaFin.Date) ||
                    (fechaFin.Date > r.FechaInicio.Date && fechaFin.Date <= r.FechaFin.Date) ||
                    (fechaInicio.Date <= r.FechaInicio.Date && fechaFin.Date >= r.FechaFin.Date)));

            if (excluirReservaId.HasValue)
            {
                query = query.Where(r => r.Id != excluirReservaId.Value);
            }

            return !await query.AnyAsync();

        }
    }
}
