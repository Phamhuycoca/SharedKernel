using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SharedKernel.Commands;

public record GetByIdQueryCommand<P> where P : struct
{
    [FromRoute]
    public P id { get; set; }
}
