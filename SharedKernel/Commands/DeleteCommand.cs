using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SharedKernel.Commands;

public record DeleteCommand<TKey> where TKey : struct
{
    [FromRoute]
    public TKey id { get; set; }
}
