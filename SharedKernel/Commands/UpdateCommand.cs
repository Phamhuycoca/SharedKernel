using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SharedKernel.Commands;

public record UpdateCommand<T, P> where T : class where P : struct
{
    [FromRoute]
    public P id { get; set; }
    [FromBody]
    public T data { get; set; }
}