using System;
using System.Collections.Generic;

namespace Ace_Admin.Models;

public partial class Instrument
{
    public int Id { get; set; }

    public string Exchange { get; set; } = null!;

    public string Token { get; set; } = null!;

    public string Symbol { get; set; } = null!;

    public string Name { get; set; } = null!;

    public string Isin { get; set; } = null!;

    public string InstrumentType { get; set; } = null!;

    public decimal TickSize { get; set; }
}
