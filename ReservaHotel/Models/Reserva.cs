using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace ReservaHotel.Models;

    public enum EstadoReserva
{
    Pendiente,
    Confirmada,
    Cancelada

}
   


public partial class Reserva
{
    [Key]
    public int Id { get; set; }

    [Required(ErrorMessage = "El nombre del cliente es obligatorio")]
    [StringLength(100, MinimumLength = 3,
        ErrorMessage = "El nombre de usuario debe tener entre 3 y 100 caracteres")]
    [Display(Name = "Nombre del Cliente")]
    public string NombreCliente { get; set; } = null!;

    [Required(ErrorMessage ="La fecha de inicio es obligatoria")]
    [Display(Name = "Fecha de Inicio")]
    public DateOnly FechaInicio { get; set; }

    [Required(ErrorMessage = "La fecha de fin es obligatoria")]
    [Display(Name = "Fecha de Fin")]
    public DateOnly FechaFin { get; set; }

    [Required(ErrorMessage = "El numero de habitacion es obligatorio")]
    [Range(1, int.MaxValue, ErrorMessage = "El numero de habitacion debe ser mayor que 0")]
    [Display(Name = "Numero de habitacion")]
    public int NumeroHabitacion { get; set; }

    [Required(ErrorMessage = "El estado es obligatorio")]
    [Display(Name = "Estado")]
    public EstadoReserva Estado { get; set; }

    public bool EsReservaValida()
    {
        if (FechaFin < FechaInicio)
        {
            return false;
        }

        if (FechaInicio < DateOnly.FromDateTime(DateTime.Today))
        {
            return false;
        }

        return true;

    }
    
    
}
