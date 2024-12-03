using System;
using System.Collections.Generic;

namespace ReservaHotel.Models;

public partial class Reserva
{
    public int Id { get; set; }

    public string NombreCliente { get; set; } = null!;

    public DateOnly FechaInicio { get; set; }

    public DateOnly FechaFin { get; set; }

    public int NumeroHabitacion { get; set; }

    public string Estado { get; set; } = null!;
}
