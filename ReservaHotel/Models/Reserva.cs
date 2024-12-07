using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using ReservaHotel.Areas.Identity.Data;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

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
    [DataType(DataType.Date)]
    public DateTime FechaInicio { get; set; }

    [Required(ErrorMessage = "La fecha de fin es obligatoria")]
    [Display(Name = "Fecha de Fin")]
    [DataType(DataType.Date)]
    public DateTime FechaFin { get; set; }

    [Required(ErrorMessage = "El numero de habitacion es obligatorio")]
    [Range(1, int.MaxValue, ErrorMessage = "El numero de habitacion debe ser mayor que 0")]
    [Display(Name = "Numero de habitacion")]
    [Column(TypeName = "int")]
    public int NumeroHabitacion { get; set; }

    [Required(ErrorMessage = "El estado es obligatorio")]
    [Display(Name = "Estado")]
    //[BindNever]
    public EstadoReserva Estado { get; set; }

    [Required]
    public string UserId { get; set; } = string.Empty;

    [ForeignKey("UserId")]
    public Usuario? User { get; set; } = null;

    public bool EsReservaValida()
    {
        if (FechaInicio == default || FechaFin == default)
        {
            return false;
        }

        
        if (FechaInicio.Date < DateTime.Today)
        {
            return false;
        }

        
        if (FechaFin.Date < FechaInicio.Date)
        {
            return false;
        }

        
        if (NumeroHabitacion <= 0)
        {
            return false;
        }

        return true;
    }
    
    
}
