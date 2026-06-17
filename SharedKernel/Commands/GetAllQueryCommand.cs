using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SharedKernel.Commands;

public record GetAllQueryCommand
{
    [FromQuery]
    public int page { get; set; } = 1;

    [FromQuery]
    public int page_size { get; set; } = 20;

    [FromQuery]
    public string? search { get; set; }

    [FromQuery]
    public string? filter { get; set; }

    [FromQuery]
    public string? sort { get; set; }
}
