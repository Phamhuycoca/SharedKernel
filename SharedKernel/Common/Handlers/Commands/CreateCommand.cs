using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SharedKernel.Common.Handlers.Commands;

public record CreateCommand<T> where T : class
{
    [FromBody]
    public T data { get; set; }
}